using System.Collections.Generic;
using System.Data.Bindings.Utilities;
using System.Linq;
using QS.Dialog.Gtk;
using workwear.Domain.Organization;
using workwear.DTO;
using workwear.Repository.Operations;

namespace workwear.Dialogs.Organization
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeMovementsView : WidgetOnEntityDialogBase<EmployeeCard>
	{
		public EmployeeMovementsView()
		{
			this.Build();

			ytreeviewMovements.CreateFluentColumnsConfig<EmployeeCardMovements>()
				.AddColumn("Дата").AddTextRenderer(e => e.Date.ToShortDateString())
				.AddColumn("Документ").AddTextRenderer(e => e.DocumentName)
				.AddColumn("Номенклатура").AddTextRenderer(e => e.NomenclatureName)
				.AddColumn("% износа").AddTextRenderer(e => e.WearPercentText)
				.AddColumn("Стоимость").AddTextRenderer(e => e.CostText)
				.AddColumn("Получено").AddTextRenderer(e => e.AmountReceivedText)
				.AddColumn("Сдано\\списано").AddTextRenderer(e => e.AmountReturnedText)
				.AddColumn("Автосписание").AddToggleRenderer(e => e.UseAutoWriteOff, false).Editing()
					.AddSetter( (c, e) => c.Visible = e.ReferencedDocument?.DocType == EmployeeIssueOpReferenceDoc.ReceivedFromStock)
					.AddTextRenderer(e => e.AutoWriteOffDateTextColored, useMarkup: true)
				.AddColumn("")
				.Finish();
		}

		public bool MovementsLoaded { get; private set; }

		public virtual void LoadMovements()
		{
			MovementsLoaded = true;
			UpdateMovements();
		}

		public virtual void UpdateMovements()
		{
			if(!MovementsLoaded)
				return;

			var displayList = new List<EmployeeCardMovements>();

			var list = EmployeeIssueRepository.AllOperationsForEmployee(UoW, RootEntity, query => query.Fetch(x => x.Nomenclature));
			var docs = EmployeeIssueRepository.GetReferencedDocuments(UoW, list.Select(x => x.Id).ToArray());
			foreach(var operation in list) {
				var item = new EmployeeCardMovements();
				item.Operation = operation;
				item.ReferencedDocument = docs.FirstOrDefault(x => x.OpId == operation.Id);
				item.PropertyChanged += Item_PropertyChanged;
				displayList.Add(item);
			}
			ytreeviewMovements.ItemsDataSource = displayList;
		}

		void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == PropertyUtil.GetName<EmployeeCardMovements>(x => x.UseAutoWriteOff)) {
				var item = sender as EmployeeCardMovements;
				UoW.Save(item.Operation);
			}
		}
	}
}
