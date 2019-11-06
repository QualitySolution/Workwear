using System;
using Gamma.ColumnConfig;
using QS.Journal.GtkUI;
using workwear.JournalViewModels.Company;

namespace workwear.JournalViewModels
{
	public static class JournalsColumnsConfigs
	{
		public static void RegisterColumns()
		{
			TreeViewColumnsConfigFactory.Register<OrganizationJournalViewModel>(
				() => FluentColumnsConfig<ClientJournalNode>.Create()
					.AddColumn("Код").AddTextRenderer(node => node.Id.ToString()).SearchHighlight()
					.AddColumn("Название").AddTextRenderer(node => node.Name).SearchHighlight()
					.AddColumn("Адрес").AddTextRenderer(node => node.Address).SearchHighlight()
					.Finish()
			);
		}
	}
}
