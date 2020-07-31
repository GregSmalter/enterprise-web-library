﻿using System.Collections.Generic;
using System.Linq;
using Humanizer;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// EWL use only.
	/// </summary>
	public class TableCssElementCreator: ControlCssElementCreator {
		internal static readonly ElementClass StandardLayoutOnlyStyleClass = new ElementClass( "ewfStandardLayoutOnly" );
		internal static readonly ElementClass StandardExceptLayoutStyleClass = new ElementClass( "ewfTblSel" );
		internal static readonly ElementClass StandardStyleClass = new ElementClass( "ewfStandard" );

		// This class allows the cell selectors to have the same specificity as the text alignment and cell alignment rules in the EWF CSS files.
		internal static readonly ElementClass AllCellAlignmentsClass = new ElementClass( "ewfTc" );

		internal static readonly ElementClass ItemLimitingAndGeneralActionContainerClass = new ElementClass( "ewfTblIlga" );
		internal static readonly ElementClass ItemLimitingControlContainerClass = new ElementClass( "ewfTblIl" );
		internal static readonly ElementClass ItemGroupNameAndGeneralActionContainerClass = new ElementClass( "ewfTblIgnga" );
		internal static readonly ElementClass ActionListContainerClass = new ElementClass( "ewfTblAl" );

		internal static readonly ElementClass ContrastClass = new ElementClass( "ewfContrast" );

		/// <summary>
		/// EWL use only.
		/// </summary>
		public static readonly IReadOnlyCollection<string> Selectors = new[]
			{
				"table", "table." + StandardLayoutOnlyStyleClass.ClassName, "table." + StandardExceptLayoutStyleClass.ClassName,
				"table." + StandardStyleClass.ClassName
			};

		internal static readonly IReadOnlyCollection<string> CellSelectors =
			( from e in new[] { "th", "td" } select e + "." + AllCellAlignmentsClass.ClassName ).Materialize();

		IReadOnlyCollection<CssElement> ControlCssElementCreator.CreateCssElements() {
			var elements = new[]
				{
					new CssElement( "TableAllStyles", Selectors.ToArray() ),
					new CssElement(
						"TableStandardAndStandardLayoutOnlyStyles",
						"table." + StandardStyleClass.ClassName,
						"table." + StandardLayoutOnlyStyleClass.ClassName ),
					new CssElement(
						"TableStandardAndStandardExceptLayoutStyles",
						"table." + StandardStyleClass.ClassName,
						"table." + StandardExceptLayoutStyleClass.ClassName ),
					new CssElement( "TableStandardStyle", "table." + StandardStyleClass.ClassName ),
					new CssElement( "TheadAndTfootAndTbody", "thead", "tfoot", "tbody" ), new CssElement( "ThAndTd", CellSelectors.ToArray() ),
					new CssElement( "Th", "th." + AllCellAlignmentsClass.ClassName ), new CssElement( "Td", "td." + AllCellAlignmentsClass.ClassName ),
					new CssElement( "TableItemLimitingAndGeneralActionContainer", "div.{0}".FormatWith( ItemLimitingAndGeneralActionContainerClass.ClassName ) ),
					new CssElement( "TableItemLimitingControlContainer", "div.{0}".FormatWith( ItemLimitingControlContainerClass.ClassName ) ),
					new CssElement( "TableItemGroupNameAndGeneralActionContainer", "div.{0}".FormatWith( ItemGroupNameAndGeneralActionContainerClass.ClassName ) ),
					new CssElement( "TableActionListContainer", "div.{0}".FormatWith( ActionListContainerClass.ClassName ) )
				}.ToList();


			// Add row elements.

			const string tr = "tr";
			var noActionSelector = ":not(." + ElementActivationBehavior.ActivatableClass.ClassName + ")";
			var actionSelector = "." + ElementActivationBehavior.ActivatableClass.ClassName;
			const string noHoverSelector = ":not(:hover)";
			const string hoverSelector = ":hover";
			var contrastSelector = "." + ContrastClass.ClassName;

			var trNoAction = tr + noActionSelector;
			var trNoActionContrast = tr + noActionSelector + contrastSelector;
			var trActionNoHover = tr + actionSelector + noHoverSelector;
			var trActionNoHoverContrast = tr + actionSelector + noHoverSelector + contrastSelector;
			var trActionHover = tr + actionSelector + hoverSelector;
			var trActionHoverContrast = tr + actionSelector + hoverSelector + contrastSelector;

			// all rows
			elements.Add(
				new CssElement( "TrAllStates", trNoAction, trNoActionContrast, trActionNoHover, trActionNoHoverContrast, trActionHover, trActionHoverContrast ) );
			elements.Add( new CssElement( "TrStatesWithContrast", trNoActionContrast, trActionNoHoverContrast, trActionHoverContrast ) );

			// all rows except the one being hovered, if it's an action row
			elements.Add( new CssElement( "TrStatesWithNoActionHover", trNoAction, trNoActionContrast, trActionNoHover, trActionNoHoverContrast ) );
			elements.Add( new CssElement( "TrStatesWithNoActionHoverAndWithContrast", trNoActionContrast, trActionNoHoverContrast ) );

			// non action rows
			elements.Add( new CssElement( "TrStatesWithNoAction", trNoAction, trNoActionContrast ) );
			elements.Add( new CssElement( "TrStatesWithNoActionAndWithContrast", trNoActionContrast ) );

			// action rows
			elements.Add( new CssElement( "TrStatesWithAction", trActionNoHover, trActionNoHoverContrast, trActionHover, trActionHoverContrast ) );
			elements.Add( new CssElement( "TrStatesWithActionAndWithContrast", trActionNoHoverContrast, trActionHoverContrast ) );

			// action rows except the one being hovered
			elements.Add( new CssElement( "TrStatesWithActionAndWithNoHover", trActionNoHover, trActionNoHoverContrast ) );
			elements.Add( new CssElement( "TrStatesWithActionAndWithNoHoverAndWithContrast", trActionNoHoverContrast ) );

			// the action row being hovered
			elements.Add( new CssElement( "TrStatesWithActionAndWithHover", trActionHover, trActionHoverContrast ) );

			return elements.ToArray();
		}
	}
}