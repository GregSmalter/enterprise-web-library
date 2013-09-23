﻿using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RedStapler.StandardLibrary.IO;

namespace RedStapler.StandardLibrary.EnterpriseWebFramework {
	/// <summary>
	/// A file upload control.
	/// </summary>
	public class EwfFileUpload: WebControl, ControlTreeDataLoader, FormControl {
		private readonly FormValue<HttpPostedFile> formValue;
		private RsFile postBackValue;

		public EwfFileUpload() {
			formValue = new FormValue<HttpPostedFile>( () => null,
			                                           () => this.IsOnPage() ? UniqueID : "",
			                                           v => "",
			                                           PostBackValueValidationResult<HttpPostedFile>.CreateValidWithValue );
		}

		void ControlTreeDataLoader.LoadData() {
			Attributes.Add( "type", "file" );
			Attributes.Add( "name", UniqueID );

			EwfPage.Instance.Form.Enctype = "multipart/form-data";
		}

		FormValue FormControl.FormValue { get { return formValue; } }

		/// <summary>
		/// Gets the post back value.
		/// </summary>
		public RsFile GetPostBackValue( PostBackValueDictionary postBackValues ) {
			if( postBackValue == null ) {
				var value = formValue.GetValue( postBackValues );
				if( value == null || value.ContentLength == 0 )
					return null;

				using( var ms = new MemoryStream() ) {
					IoMethods.CopyStream( value.InputStream, ms );
					postBackValue = new RsFile( ms.ToArray(), Path.GetFileName( value.FileName ), contentType: value.ContentType );
				}
			}
			return postBackValue;
		}

		/// <summary>
		/// Returns true if the value changed on this post back.
		/// </summary>
		public bool ValueChangedOnPostBack( PostBackValueDictionary postBackValues ) {
			return formValue.ValueChangedOnPostBack( postBackValues );
		}

		/// <summary>
		/// Returns the tag that represents this control in HTML.
		/// </summary>
		protected override HtmlTextWriterTag TagKey { get { return HtmlTextWriterTag.Input; } }
	}
}