﻿using System.Collections.Generic;

namespace EnterpriseWebLibrary.EnterpriseWebFramework.Ui {
	/// <summary>
	/// EWL use only. One big reason for this interface is that ReSharper gets confused when code in a web app contains calls to the copy of the master page in
	/// that web app.
	/// </summary>
	public interface AppEwfUiMasterPage {
		/// <summary>
		/// EWL use only.
		/// </summary>
		void OmitContentBox();

		/// <summary>
		/// EWL use only.
		/// </summary>
		void SetPageActions( IReadOnlyCollection<ActionComponentSetup> actions );

		/// <summary>
		/// EWL use only.
		/// </summary>
		void SetContentFootActions( IReadOnlyCollection<ButtonSetup> actions );

		/// <summary>
		/// EWL use only.
		/// </summary>
		void SetContentFootComponents( IReadOnlyCollection<FlowComponent> components );
	}
}