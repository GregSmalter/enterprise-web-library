using System;
using System.Linq;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.WebSessionState;
using Humanizer;
using Tewl.Tools;

namespace EnterpriseWebLibrary.WebSite.TestPages {
	partial class ColumnPrimaryTableDemo: EwfPage {
		partial class Info {
			public override string ResourceName => "Column Primary";
		}

		protected override void loadData() {
			var itemGroups = Enumerable.Range( 1, 2 )
				.Select(
					group => ColumnPrimaryItemGroup.Create(
						"Group {0}".FormatWith( group ).ToComponents(),
						groupActions:
						new ButtonSetup(
								"Action 1",
								behavior: new PostBackBehavior(
									postBack: PostBack.CreateIntermediate(
										null,
										id: PostBack.GetCompositeId( group.ToString(), "action1" ),
										firstModificationMethod: () => AddStatusMessage( StatusMessageType.Info, "Action 1" ) ) ) ).Append(
								new ButtonSetup(
									"Action 2",
									behavior: new PostBackBehavior(
										postBack: PostBack.CreateIntermediate(
											null,
											id: PostBack.GetCompositeId( group.ToString(), "action2" ),
											firstModificationMethod: () => AddStatusMessage( StatusMessageType.Info, "Action 2" ) ) ) ) )
							.Materialize(),
						selectedItemActions: group == 1
							                     ? SelectedItemAction.CreateWithIntermediatePostBackBehavior<int>(
									                     "Echo group IDs",
									                     null,
									                     ids => AddStatusMessage(
										                     StatusMessageType.Info,
										                     StringTools.GetEnglishListPhrase( ids.Select( i => i.ToString() ), true ) ) )
								                     .ToCollection()
							                     : Enumerable.Empty<SelectedItemAction<int>>().Materialize(),
						items: Enumerable.Range( 1, 5 )
							.Select(
								i => EwfTableItem.Create(
									EwfTableItemSetup.Create(
										activationBehavior: ElementActivationBehavior.CreateRedirectScript( ActionControls.GetInfo() ),
										id: new SpecifiedValue<int>( group * 10 + i ) ),
									i.ToString().ToCell(),
									( ( i * 2 ) + Environment.NewLine + "extra stuff" ).ToCell() ) ) ) )
				.Materialize();

			place.AddControlsReturnThis(
				ColumnPrimaryTable.Create(
						caption: "My table",
						subCaption: "A new table implementation",
						allowExportToExcel: true,
						tableActions: new ButtonSetup(
							"Action",
							behavior: new PostBackBehavior(
								postBack: PostBack.CreateIntermediate(
									null,
									id: "action",
									firstModificationMethod: () => AddStatusMessage( StatusMessageType.Info, "You clicked action." ) ) ) ).ToCollection(),
						selectedItemActions: SelectedItemAction
							.CreateWithIntermediatePostBackBehavior<int>(
								"Echo IDs",
								null,
								ids => AddStatusMessage( StatusMessageType.Info, StringTools.GetEnglishListPhrase( ids.Select( i => i.ToString() ), true ) ) )
							.Append(
								SelectedItemAction.CreateWithIntermediatePostBackBehavior<int>(
									"With confirmation",
									null,
									ids => AddStatusMessage( StatusMessageType.Info, StringTools.GetEnglishListPhrase( ids.Select( i => i.ToString() ), true ) ),
									confirmationDialogContent: "Are you sure?".ToComponents() ) )
							.Materialize(),
						fields: new[] { new EwfTableField( size: 1.ToPercentage() ), new EwfTableField( size: 2.ToPercentage() ) } )
					.AddItemGroups( itemGroups )
					.ToCollection()
					.GetControls() );
		}
	}
}