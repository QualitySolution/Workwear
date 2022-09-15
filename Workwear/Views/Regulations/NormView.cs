using System;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.Dialog;
using QSWidgetLib;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.ViewModels.Regulations;
using Workwear.Domain.Regulations;

namespace workwear.Views.Regulations
{
	public partial class NormView : EntityDialogViewBase<NormViewModel, Norm>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public NormView(NormViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			ylabelId.Binding.AddBinding (Entity, e => e.Id, w => w.LabelProp, new Gamma.Binding.Converters.IdToStringConverter()).InitializeFromSource ();

			ycomboAnnex.SetRenderTextFunc<RegulationDocAnnex>(x => x.Title);
			yentryRegulationDoc.ItemsList = ViewModel.UoW.GetAll<RegulationDoc>();
			yentryRegulationDoc.Binding.AddBinding(Entity, e => e.Document, w => w.SelectedItem).InitializeFromSource();
			yentryRegulationDoc.Changed += OnYentryRegulationDocChanged;
			ycomboAnnex.Binding.AddBinding(Entity, e => e.Annex, w => w.SelectedItem).InitializeFromSource();
			datefrom.Binding.AddBinding(Entity, e => e.DateFrom, w => w.DateOrNull).InitializeFromSource();
			dateto.Binding.AddBinding(Entity, e => e.DateTo, w => w.DateOrNull).InitializeFromSource();

			yentryTonParagraph.Binding.AddBinding (Entity, e => e.TONParagraph, w => w.Text).InitializeFromSource ();
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ytreeProfessions.ColumnsConfig = FluentColumnsConfig<Post>.Create ()
				.AddColumn("Должность").AddTextRenderer (p => p.Name).WrapWidth(300)
				.AddColumn("Подразделение").AddTextRenderer(p => p.Subdivision != null ? p.Subdivision.Name : null).WrapWidth(300)
				.Finish ();
			ytreeProfessions.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeProfessions.ItemsDataSource = Entity.ObservablePosts;
			ytreeProfessions.Selection.Changed += YtreeProfessions_Selection_Changed;

			ytreeItems.ColumnsConfig = FluentColumnsConfig<NormItem>.Create()
				.AddColumn("Наименование").AddTextRenderer(p => p.ProtectionTools != null ? p.ProtectionTools.Name : null).WrapWidth(700)
				.AddColumn("Количество")
				.AddNumericRenderer(i => i.Amount).WidthChars(9).Editing().Adjustment(new Gtk.Adjustment(1, 1, 1000000, 1, 10, 10))
				.AddTextRenderer(i => i.ProtectionTools != null && i.ProtectionTools.Type.Units != null ? i.ProtectionTools.Type.Units.Name : String.Empty)
				.AddColumn("Период")
				.AddNumericRenderer(i => i.PeriodCount).WidthChars(6).Editing().Adjustment(new Gtk.Adjustment(1, 1, 100, 1, 10, 10))
					.AddSetter((c, n) => c.Visible = n.NormPeriod != NormPeriodType.Wearout && n.NormPeriod != NormPeriodType.Duty)
				.AddEnumRenderer(i => i.NormPeriod).Editing()
				.AddColumn("Условие нормы")
					.AddComboRenderer(i => i.NormCondition)
						.SetDisplayFunc(x => x?.Name)
						.SetDisplayListFunc(x => x?.Name ?? "нет")
					.FillItems(ViewModel.NormConditions)
					.Editing()
				.Finish ();
			ytreeItems.ItemsDataSource = Entity.ObservableItems;
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += TreeItems_ButtonReleaseEvent;

			buttonSave.Binding.AddBinding(ViewModel, v => v.SaveSensitive, w => w.Sensitive).InitializeFromSource();
			buttonCancel.Binding.AddBinding(ViewModel, v => v.CancelSensitive, w => w.Sensitive).InitializeFromSource();
		}

		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonRemoveItem.Sensitive = buttonReplaceNomeclature.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
		}

		#region Профессии
		void YtreeProfessions_Selection_Changed (object sender, EventArgs e)
		{
			buttonRemoveProfession.Sensitive = ytreeProfessions.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonAddProfessionClicked(object sender, EventArgs e)
		{
			ViewModel.AddProfession();
		}

		protected void OnButtonRemoveProfessionClicked (object sender, EventArgs e)
		{
			ViewModel.RemoveProfession(ytreeProfessions.GetSelectedObjects<Post> ());
		}

		protected void OnButtonNewProfessionClicked(object sender, EventArgs e)
		{
			ViewModel.NewProfession();
		}

		#endregion

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
			ycomboAnnex.ItemsList = Entity.Document?.Annexess;
			ycomboAnnex.Sensitive = Entity.Document?.Annexess.Count > 0;
		}
		
		#region PopupMenu

		private void TreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3) {
				var menu = new Menu();
				var selected = ytreeItems.GetSelectedObject<NormItem>();
				var menuItem = 
					new MenuItemId<NormItem>("пересчитать сроки носки в документах выдачи");
				menuItem.ID = selected;
				menuItem.Activated += (sender, e) => ViewModel.ReSaveLastIssue(((MenuItemId<NormItem>)sender).ID);
				menu.Add(menuItem);
				menu.ShowAll();
				menu.Popup();
			}
		}
		#endregion
	}
}

