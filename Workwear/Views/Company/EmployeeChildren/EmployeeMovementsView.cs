using System.Reflection;
using Gtk;
using QSWidgetLib;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace Workwear.Views.Company.EmployeeChildren
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeMovementsView : Gtk.Bin
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private EmployeeMovementsViewModel viewModel;

		public EmployeeMovementsView()
		{
			this.Build();
		}

		public EmployeeMovementsViewModel ViewModel {
			get => viewModel; 
			set {
				viewModel = value;
				viewModel.PropertyChanged += ViewModel_PropertyChanged;
				CreateTable();
			}
		}

		#region События
		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(viewModel.Movements))
				ytreeviewMovements.ItemsDataSource = viewModel.Movements;
		}

		public void YtreeviewMovements_RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if(args.Column.Title == "Документ") {
				var item = ytreeviewMovements.GetSelectedObject<EmployeeMovementItem>();
				ViewModel.OpenDoc(item);
			}
		}

		void YtreeviewMovements_ButtonReleaseEvent(object o, Gtk.ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeviewMovements.GetSelectedObject<EmployeeMovementItem>();

				var itemOpenLastIssue = new MenuItemId<EmployeeMovementItem>("Редактировать");
				itemOpenLastIssue.ID = selected;
				itemOpenLastIssue.Sensitive = selected?.EmployeeIssueReference?.DocumentType != null 
				                              || selected?.Operation.ManualOperation == true;
				
				itemOpenLastIssue.Activated += (sender, e) => ViewModel.OpenDoc(((MenuItemId<EmployeeMovementItem>)sender).ID);
				menu.Add(itemOpenLastIssue);

				var itemRemoveOperation = new MenuItemId<EmployeeMovementItem>("Удалить операцию");
				itemRemoveOperation.ID = selected;
				itemRemoveOperation.Sensitive = selected?.EmployeeIssueReference?.DocumentType == null;
				itemRemoveOperation.Activated += (sender, e) => ViewModel.RemoveOperation(((MenuItemId<EmployeeMovementItem>)sender).ID);
				menu.Add(itemRemoveOperation);

				var itemMakeEmptyProtectionTools = new MenuItemId<EmployeeMovementItem>("Очистить номенклатуру нормы");
				itemMakeEmptyProtectionTools.ID = selected;
				itemMakeEmptyProtectionTools.Activated += (sender, e) => ViewModel.MakeEmptyProtectionTools(((MenuItemId<EmployeeMovementItem>)sender).ID);
				menu.Add(itemMakeEmptyProtectionTools);

				var itemChangeProtectionTools = new MenuItem("Изменить номенклатуру нормы");
				var subItemChangeProtectionTools = new Menu();
					var menuItemChangePT = new MenuItem("Из списка потребностей");
					var submenuChangePT = new Menu();
					foreach(ProtectionTools protectionTools in ViewModel.ProtectionToolsForChange) {
						var ptItem = new MenuItem(protectionTools.Name);
						ptItem.ButtonPressEvent += (sender, e) => ViewModel.ChangeProtectionTools(selected,protectionTools);
						submenuChangePT.Append(ptItem);
					}
					menuItemChangePT.Submenu = submenuChangePT;
					subItemChangeProtectionTools.Append(menuItemChangePT);
					
					var menuItemChangePtFull = new MenuItem("Из полного списка");
					var submenuChangePtFull = new Menu();
					foreach(ProtectionTools protectionTools in ViewModel.ProtectionToolsFullList) {
						var ptItem = new MenuItem(protectionTools.Name);
						ptItem.ButtonPressEvent += (sender, e) => ViewModel.ChangeProtectionTools(selected,protectionTools);
						submenuChangePtFull.Append(ptItem);
					}
					menuItemChangePtFull.Submenu = submenuChangePtFull;
					subItemChangeProtectionTools.Append(menuItemChangePtFull);
					
				itemChangeProtectionTools.Submenu = subItemChangeProtectionTools;
				menu.Add(itemChangeProtectionTools);
				
				menu.ShowAll();
				menu.Popup();
			}
		}
		#endregion

		private void CreateTable()
		{
			var cardIcon = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "Workwear.icon.buttons.smart-card.png");
			ytreeviewMovements.CreateFluentColumnsConfig<EmployeeMovementItem>()
				.AddColumn("Дата").ToolTipText(x => $"ИД операции: {x.Operation.Id}")
				.AddTextRenderer(e => e.Date.ToShortDateString())
				//Заголовок колонки используется в методе YtreeviewMovements_RowActivated
				.AddColumn("Документ").AddTextRenderer(e => e.DocumentTitle)
				.AddColumn("Номенклатура").Resizable().AddTextRenderer(e => e.NomenclatureName).WrapWidth(1000)
				.AddColumn("Номенклатура нормы").Resizable().AddTextRenderer(e => e.ProtectionTools).WrapWidth(1000)
				.AddColumn("% износа").AddTextRenderer(e => e.WearPercentText)
				.AddColumn("Стоимость").AddTextRenderer(e => e.CostText)
				.AddColumn("Получено").AddTextRenderer(e => e.AmountReceivedText)
				.AddColumn("Сдано\\списано").AddTextRenderer(e => e.AmountReturnedText)
				.AddColumn("Автосписание").AddToggleRenderer(e => e.UseAutoWriteOff, false)
					.AddSetter((c, e) => c.Visible = e.AmountReceived > 0)
					.AddSetter((c, e) => c.Activatable = e.Operation.ExpiryByNorm.HasValue)
					.AddTextRenderer(e => e.AutoWriteOffDateTextColored, useMarkup: true)
				.AddColumn("Отметка о выдаче").Visible(ViewModel.VisibleSignColumn)
					.AddPixbufRenderer(x => x.IsSigned ? cardIcon : null)
					.AddTextRenderer(x => x.SingText)
				.Finish();

			ytreeviewMovements.RowActivated += YtreeviewMovements_RowActivated;
			ytreeviewMovements.ButtonReleaseEvent += YtreeviewMovements_ButtonReleaseEvent;
		}
	}
}
