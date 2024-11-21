using System;
using Gamma.ColumnConfig;
using Gtk;
using QS.Utilities.Text;
using QS.Utilities;
using QS.Views.Dialog;
using QS.Views.Resolve;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Regulations;

namespace Workwear.Views.Regulations
{
	public partial class NormView : EntityDialogViewBase<NormViewModel, Norm>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public NormView(NormViewModel viewModel, IGtkViewResolver viewResolver) : base(viewModel)
		{
			this.Build();
			ConfigureDlg(viewResolver);
			CommonButtonSubscription();
		}

		private void ConfigureDlg(IGtkViewResolver viewResolver)
		{
			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new Gamma.Binding.Converters.IdToStringConverter()).InitializeFromSource ();

			ycomboAnnex.SetRenderTextFunc<RegulationDocAnnex>(x => StringManipulationHelper.EllipsizeMiddle(x.Title,160));
			yentryRegulationDoc.SetRenderTextFunc<RegulationDoc>(x => StringManipulationHelper.EllipsizeMiddle(x.Title,160));
			yentryRegulationDoc.ItemsList = ViewModel.RegulationDocs;
			yentryRegulationDoc.WidthRequest = 1; //Минимальное не нулевое значение, чтобы элемент не участвовал в расчёте минимальной ширины окна
			ycomboAnnex.WidthRequest = 1;  
			yentryRegulationDoc.Binding.AddBinding(Entity, e => e.Document, w => w.SelectedItem).InitializeFromSource();
			yentryRegulationDoc.Changed += OnYentryRegulationDocChanged;
			ycomboAnnex.Binding.AddBinding(Entity, e => e.Annex, w => w.SelectedItem).InitializeFromSource();
			datefrom.Binding.AddBinding(Entity, e => e.DateFrom, w => w.DateOrNull).InitializeFromSource();
			dateto.Binding.AddBinding(Entity, e => e.DateTo, w => w.DateOrNull).InitializeFromSource();
			ylabellastupdate.Binding.AddBinding(ViewModel, e=>e.LastUpdate, w=>w.LabelProp).InitializeFromSource();
			ycheckArchival.Binding.AddBinding(Entity, e => e.Archival, w => w.Active).InitializeFromSource();
			
			yentryTonParagraph.Binding.AddBinding (Entity, e => e.TONParagraph, w => w.Text).InitializeFromSource ();
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ytreeItems.ColumnsConfig = FluentColumnsConfig<NormItem>.Create()
				.AddColumn("ИД").AddTextRenderer(p => p.ProtectionTools.Id.ToString())
				.AddColumn("Наименование").AddTextRenderer(p => p.ProtectionTools != null ? p.ProtectionTools.Name : null).WrapWidth(700)
				.AddColumn("Смывающие").AddTextRenderer(p => p.ProtectionTools.Dispenser ? "Дозатор" : p.ProtectionTools.WashingPpe ? "Да" : String.Empty)
				.AddColumn("Количество")
					.AddNumericRenderer(i => i.Amount).WidthChars(9).Editing().Adjustment(new Gtk.Adjustment(1, 1, int.MaxValue, 1, 10, 10))
						.AddSetter((c, n) => c.Visible = !n.ProtectionTools.Dispenser)
					.AddTextRenderer(i => i.ProtectionTools != null && i.ProtectionTools.Type.Units != null ? i.ProtectionTools.Type.Units.Name : String.Empty)
						.AddSetter((c, n) => c.Visible = !n.ProtectionTools.Dispenser)
				.AddColumn("Период")
				.AddNumericRenderer(i => i.PeriodCount).WidthChars(6).Editing().Adjustment(new Gtk.Adjustment(1, 1, 100, 1, 10, 10))
					.AddSetter((c, n) => c.Visible = n.NormPeriod != NormPeriodType.Wearout && n.NormPeriod != NormPeriodType.Duty && !n.ProtectionTools.Dispenser)
				.AddEnumRenderer(i => i.NormPeriod).Editing()
					.AddSetter((c, n) => c.Visible = !n.ProtectionTools.Dispenser)
				.AddColumn("Условие нормы").Visible(ViewModel.VisibleNormCondition)
					.AddComboRenderer(i => i.NormCondition)
						.SetDisplayFunc(x => x?.Name)
						.SetDisplayListFunc(x => x?.Name ?? "нет")
					.FillItems(ViewModel.NormConditions)
					.Editing()
				.AddColumn("Пункт норм").AddTextRenderer(x => x.NormParagraph).Editable()
				.AddColumn("Комментарий").AddTextRenderer(x => x.Comment).Editable()
				.Finish ();
			ytreeItems.ItemsDataSource = Entity.Items;
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += TreeItems_ButtonReleaseEvent;
			ytreeItems.Binding.AddSource(ViewModel)
				.AddBinding(v => v.SelectedItem, w => w.SelectedRow);
			
			tabs.AppendPage(viewResolver.Resolve(ViewModel.PostsViewModel), "Должности");
			tabs.AppendPage(viewResolver.Resolve(ViewModel.EmployeesViewModel), "Сотрудники");
			tabs.Binding.AddBinding(ViewModel, v => v.CurrentTab, w => w.CurrentPage).InitializeFromSource();

			buttonSave.Binding.AddBinding(ViewModel, v => v.SaveSensitive, w => w.Sensitive).InitializeFromSource();
			buttonCancel.Binding.AddBinding(ViewModel, v => v.CancelSensitive, w => w.Sensitive).InitializeFromSource();
		}

		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonRemoveItem.Sensitive = buttonReplaceNomeclature.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}

		#region Строки
		protected void OnButtonAddItemClicked (object sender, EventArgs e)
		{
			ViewModel.AddItem();
		}

		protected void OnButtonRemoveItemClicked (object sender, EventArgs e)
		{
			ViewModel.RemoveItem(ytreeItems.GetSelectedObject<NormItem>());
		}

		protected void OnButtonReplaceNomeclatureClicked(object sender, EventArgs e)
		{
			ViewModel.ReplaceNomenclature(ytreeItems.GetSelectedObject<NormItem>());
		}
		#endregion

		protected void OnYentryRegulationDocChanged(object sender, EventArgs e)
		{
			ycomboAnnex.ItemsList = Entity.Document?.Annexes;
			ycomboAnnex.Sensitive = Entity.Document?.Annexes.Count > 0;
		}
		
		#region PopupMenu

		private void TreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeItems.GetSelectedObject<NormItem>();
				
				var menuItem = new MenuItem("Открыть номенклатуру нормы");
				menuItem.Sensitive = selected != null;
				menuItem.Activated += (sender, e) => ViewModel.OpenProtectionTools(selected);
				menu.Add(menuItem);
				
				menu.Add(new SeparatorMenuItem());
				
				menuItem = new MenuItem("Пересчитать сроки носки в документах выдачи");
				menuItem.Sensitive = selected != null;
				menuItem.Activated += (sender, e) => ViewModel.ReSaveLastIssue(selected);
				menu.Add(menuItem);
				
				menu.ShowAll();
				menu.Popup();
			}
		}
		#endregion
	}
}

