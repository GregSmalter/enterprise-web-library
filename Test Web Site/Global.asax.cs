using System;
using System.Collections.Generic;
using RedStapler.StandardLibrary;
using RedStapler.StandardLibrary.EnterpriseWebFramework;
using RedStapler.TestWebSite.TestPages;

namespace RedStapler.TestWebSite {
	public class Global: EwfApp {
		// These methods exist because there is no way to hook into these events from within EWF.
		protected void Application_Start( object sender, EventArgs e ) {
			ewfApplicationStart( new GlobalLogic() );
		}

		protected void Application_End( object sender, EventArgs e ) {
			ewfApplicationEnd();
		}

		protected override void initializeWebApp() {}

		protected override IEnumerable<ShortcutUrlResolver> GetShortcutUrlResolvers() {
			yield return new ShortcutUrlResolver( "", ConnectionSecurity.SecureIfPossible, () => ActionControls.GetInfo() );
		}

		protected override List<CssInfo> GetStyleSheets() {
			return new List<CssInfo> { new Test.Info() };
		}

		public override string AppDisplayName { get { return "Test Web Site"; } }
	}
}