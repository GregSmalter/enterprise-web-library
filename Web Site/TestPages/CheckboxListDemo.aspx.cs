using System.Linq;
using EnterpriseWebLibrary.EnterpriseWebFramework;

namespace EnterpriseWebLibrary.WebSite.TestPages {
	partial class CheckboxListDemo: EwfPage {
		partial class Info {
			public override string ResourceName => "Checkbox List";
		}

		protected override void loadData() {
			ph.AddControlsReturnThis(
				new CheckboxList<int>(
						CheckboxListSetup.Create(
							from i in Enumerable.Range( 1, 20 ) select SelectListItem.Create( i, "Item " + i ),
							includeSelectAndDeselectAllButtons: true,
							minColumnWidth: 20.ToEm() ),
						new[] { 3, 9, 19 } ).ToFormItem()
					.ToComponentCollection()
					.GetControls() );
		}

		public override bool IsAutoDataUpdater => true;
	}
}