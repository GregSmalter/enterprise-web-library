﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EnterpriseWebLibrary.DevelopmentUtility.Operations.CodeGeneration.DataAccess.Subsystems;
using RedStapler.StandardLibrary;
using RedStapler.StandardLibrary.Collections;
using RedStapler.StandardLibrary.DataAccess;
using RedStapler.StandardLibrary.DataAccess.CommandWriting;
using RedStapler.StandardLibrary.DatabaseSpecification;
using RedStapler.StandardLibrary.DatabaseSpecification.Databases;
using RedStapler.StandardLibrary.InstallationSupportUtility.DatabaseAbstraction;

namespace EnterpriseWebLibrary.DevelopmentUtility.Operations.CodeGeneration.DataAccess {
	internal static class DataAccessStatics {
		internal const string CSharpTemplateFileExtension = ".ewlt.cs";

		/// <summary>
		/// Given a string, returns all instances of @abc in an ordered set containing abc (the token without the @ sign). If a token is used more than once, it
		/// only appears in the list once. A different prefix may be used for certain databases.
		/// </summary>
		internal static ListSet<string> GetNamedParamList( DatabaseInfo info, string statement ) {
			// We don't want to find parameters in quoted text.
			statement = statement.RemoveTextBetweenStrings( "'", "'" ).RemoveTextBetweenStrings( "\"", "\"" );

			var parameters = new ListSet<string>();
			foreach( Match match in Regex.Matches( statement, getParamRegex( info ) ) )
				parameters.Add( match.Value.Substring( 1 ) );

			return parameters;
		}

		private static string getParamRegex( DatabaseInfo info ) {
			// Matches spaced followed by @abc. The space prevents @@identity, etc. from getting matched.
			return @"(?<!{0}){0}\w*\w".FormatWith( info.ParameterPrefix );
		}

		/// <summary>
		/// Given raw query text such as that from Development.xml, returns a command that has had all of its parameters filled in with
		/// good dummy values and is ready to safely execute using schema only or key info behavior.
		/// </summary>
		internal static DbCommand GetCommandFromRawQueryText( DBConnection cn, string commandText ) {
			// This replacement is necessary because SQL Server chooses to care about the type of the parameter passed to TOP.
			commandText = Regex.Replace( commandText, @"TOP\( *@\w+ *\)", "TOP 0", RegexOptions.IgnoreCase );

			var cmd = cn.DatabaseInfo.CreateCommand();
			cmd.CommandText = commandText;
			foreach( var param in GetNamedParamList( cn.DatabaseInfo, cmd.CommandText ) )
				cmd.Parameters.Add( new DbCommandParameter( param, new DbParameterValue( "0" ) ).GetAdoDotNetParameter( cn.DatabaseInfo ) );
			return cmd;
		}

		internal static void WriteRowClass( TextWriter writer, IEnumerable<Column> columns, Action<TextWriter> toModificationMethodWriter ) {
			CodeGenerationStatics.AddSummaryDocComment( writer, "Holds data for a row of this result." );
			writer.WriteLine( "public partial class Row: System.IEquatable<Row> {" );

			foreach( var column in columns )
				writer.WriteLine( "private readonly " + column.DataTypeName + " " + getMemberVariableName( column ) + ";" );

			writer.WriteLine( "internal Row( DbDataReader reader ) {" );
			var cnt = 0;
			foreach( var column in columns ) {
				if( column.AllowsNull ) {
					writer.WriteLine(
						"if( reader.IsDBNull( " + cnt + " ) ) " + getMemberVariableName( column ) +
						" = {0};".FormatWith( column.NullValueExpression.Any() ? column.NullValueExpression : "null" ) );
					writer.WriteLine( "else" );
				}
				writer.WriteLine( "" + getMemberVariableName( column ) + " = " + ( "(" + column.DataTypeName + ")" ) + "reader.GetValue( " + cnt + " );" );
				cnt++;
			}
			writer.WriteLine( "}" ); // constructor

			foreach( var column in columns )
				writeColumnProperty( writer, column );

			// NOTE: Being smarter about the hash code could make searches of the collection faster.
			writer.WriteLine( "public override int GetHashCode() { " );
			// NOTE: Catch an exception generated by not having any uniquely identifying columns and rethrow it as a UserCorrectableException.
			writer.WriteLine( "return " + getMemberVariableName( columns.First( c => c.UseToUniquelyIdentifyRow ) ) + ".GetHashCode();" );
			writer.WriteLine( "}" ); // Object override of GetHashCode

			writer.WriteLine( @"	public static bool operator == (Row row1, Row row2 ) {
				return Equals( row1, row2 );
			}

			public static bool operator !=( Row row1, Row row2 ) {
				return !Equals( row1, row2 );
			}" );

			writer.WriteLine( "public override bool Equals( object obj ) {" );
			writer.WriteLine( "return Equals( obj as Row );" );
			writer.WriteLine( "}" ); // Object override of Equals

			writer.WriteLine( "public bool Equals( Row other ) {" );
			writer.WriteLine( "if( other == null ) return false;" );

			var condition = "";
			foreach( var column in columns.Where( ( c, index ) => c.UseToUniquelyIdentifyRow ) )
				condition = StringTools.ConcatenateWithDelimiter( " && ", condition, getMemberVariableName( column ) + " == other." + getMemberVariableName( column ) );
			writer.WriteLine( "return " + condition + ";" );
			writer.WriteLine( "}" ); // Equals method

			toModificationMethodWriter( writer );

			writer.WriteLine( "}" ); // class
		}

		private static void writeColumnProperty( TextWriter writer, Column column ) {
			CodeGenerationStatics.AddSummaryDocComment(
				writer,
				"This object will " + ( column.AllowsNull && !column.NullValueExpression.Any() ? "sometimes" : "never" ) + " be null." );
			writer.WriteLine(
				"public " + column.DataTypeName + " " + StandardLibraryMethods.GetCSharpIdentifierSimple( column.PascalCasedNameExceptForOracle ) + " { get { return " +
				getMemberVariableName( column ) + "; } }" );
		}

		private static string getMemberVariableName( Column column ) {
			// A single underscore is a pretty common thing for other code generators and even some developers to use, so two is more unique and avoids problems.
			return StandardLibraryMethods.GetCSharpIdentifierSimple( "__" + column.CamelCasedName );
		}

		internal static string GetMethodParamsFromCommandText( DatabaseInfo info, string commandText ) {
			return StringTools.ConcatenateWithDelimiter( ", ", GetNamedParamList( info, commandText ).Select( i => "object " + i ).ToArray() );
		}

		internal static void WriteAddParamBlockFromCommandText( TextWriter writer, string commandVariable, DatabaseInfo info, string commandText, Database database ) {
			foreach( var param in GetNamedParamList( info, commandText ) ) {
				writer.WriteLine(
					commandVariable + ".Parameters.Add( new DbCommandParameter( \"" + param + "\", new DbParameterValue( " + param + " ) ).GetAdoDotNetParameter( " +
					GetConnectionExpression( database ) + ".DatabaseInfo ) );" );
			}
		}

		internal static bool IsRevisionHistoryTable( string table, RedStapler.StandardLibrary.Configuration.SystemDevelopment.Database configuration ) {
			return configuration.revisionHistoryTables != null &&
			       configuration.revisionHistoryTables.Any( revisionHistoryTable => revisionHistoryTable.EqualsIgnoreCase( table ) );
		}

		internal static string GetTableConditionInterfaceName( DBConnection cn, Database database, string table ) {
			return database.SecondaryDatabaseName + "CommandConditions." + CommandConditionStatics.GetTableConditionInterfaceName( cn, table );
		}

		internal static string GetEqualityConditionClassName( DBConnection cn, Database database, string tableName, Column column ) {
			return database.SecondaryDatabaseName + "CommandConditions." + CommandConditionStatics.GetTableEqualityConditionsClassName( cn, tableName ) + "." +
			       CommandConditionStatics.GetConditionClassName( column );
		}

		internal static void WriteGetLatestRevisionsConditionMethod( TextWriter writer, string revisionIdColumn ) {
			writer.WriteLine( "private static InlineDbCommandCondition getLatestRevisionsCondition() {" );
			writer.WriteLine( "var provider = (RevisionHistoryProvider)DataAccessStatics.SystemProvider;" );
			writer.WriteLine( "return new InCondition( \"" + revisionIdColumn + "\", provider.GetLatestRevisionsQuery() );" );
			writer.WriteLine( "}" );
		}

		internal static string TableNameToPascal( this string tableName, DBConnection cn ) {
			return cn.DatabaseInfo is MySqlInfo ? tableName.OracleToEnglish().EnglishToPascal() : tableName;
		}

		internal static string GetConnectionExpression( Database database ) {
			return
				"DataAccessState.Current.{0}".FormatWith(
					database.SecondaryDatabaseName.Any()
						? "GetSecondaryDatabaseConnection( SecondaryDatabaseNames.{0} )".FormatWith( database.SecondaryDatabaseName )
						: "PrimaryDatabaseConnection" );
		}
	}
}