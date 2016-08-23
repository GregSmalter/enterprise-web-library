﻿using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	public sealed class PageElement: WebControl, ElementNode, FlowComponent, EtherealComponent, ControlTreeDataLoader, FormValueControl, ControlWithJsInitLogic,
		EtherealControl {
		internal readonly Func<ElementContext, ElementData> ElementDataGetter;
		internal readonly FormValue FormValue;

		// Web Forms compatibility. Remove when EnduraCode goal 790 is complete.
		private Func<ElementLocalData> webFormsLocalDataGetter;
		private ElementLocalData webFormsLocalData;

		public PageElement( Func<ElementContext, ElementData> elementDataGetter, FormValue formValue = null ): base( HtmlTextWriterTag.Unknown ) {
			ElementDataGetter = elementDataGetter;
			FormValue = formValue;
		}

		IEnumerable<PageNode> FlowComponent.GetNodes() {
			return this.ToSingleElementArray();
		}

		IEnumerable<ElementNode> EtherealComponent.GetElements() {
			return this.ToSingleElementArray();
		}


		// Web Forms compatibility. Remove when EnduraCode goal 790 is complete.

		void ControlTreeDataLoader.LoadData() {
			var elementData = ElementDataGetter( new ElementContext( ClientID ) );
			this.AddControlsReturnThis( elementData.Children.GetControls() );
			elementData.EtherealChildren.AddEtherealControls( this );
			webFormsLocalDataGetter = elementData.LocalDataGetter;
		}

		WebControl EtherealControl.Control { get { return this; } }

		string ControlWithJsInitLogic.GetJsInitStatements() {
			return getJsInitStatements();
		}

		string EtherealControl.GetJsInitStatements() {
			return getJsInitStatements();
		}

		private string getJsInitStatements() {
			if( webFormsLocalDataGetter == null )
				throw new ApplicationException( "webFormsLocalDataGetter not set" );
			webFormsLocalData = webFormsLocalDataGetter();
			return webFormsLocalData.JsInitStatements;
		}

		FormValue FormValueControl.FormValue { get { return FormValue; } }

		protected override void AddAttributesToRender( HtmlTextWriter writer ) {
			if( webFormsLocalData == null )
				throw new ApplicationException( "webFormsLocalData not set" );
			foreach( var i in webFormsLocalData.Attributes )
				writer.AddAttribute( i.Item1, i.Item2 );
			if( webFormsLocalData.IncludeIdAttribute )
				writer.AddAttribute( HtmlTextWriterAttribute.Id, ClientID );
		}

		protected override string TagName {
			get {
				if( webFormsLocalData == null )
					throw new ApplicationException( "webFormsLocalData not set" );
				return webFormsLocalData.ElementName;
			}
		}
	}
}