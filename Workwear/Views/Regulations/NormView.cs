using System;
using Gamma.ColumnConfig;
using QS.Views.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.ViewModels.Regulations;

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
			yentryRegulationDoc.SubjectType = typeof(RegulationDoc);
			yentryRegulationDoc.Binding.AddBinding(Entity, e => e.Document, w => w.Subject).InitializeFromSource();
			ycomboAnnex.Binding.AddBinding(Entity, e => e.Annex, w => w.SelectedItem).InitializeFromSource();
			datefrom.Binding.AddBinding(Entity, e => e.DateFrom, w => w.DateOrNull).InitializeFromSource();
			dateto.Binding.AddBinding(Entity, e => e.DateTo, w => w.DateOrNull).InitializeFromSource();

			yentryTonParagraph.Binding.AddBinding (Entity, e => e.TONParagraph, w => w.Text).InitializeFromSource ();
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ytreeProfessions.ColumnsConfig = FluentColumnsConfig<Post>.Create ()
				.AddColumn ("Должность").AddTextRenderer (p => p.Name)
				.Finish ();
			ytreeProfessions.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeProfessions.ItemsDataSource = Entity.ObservableProfessions;
			ytreeProfessions.Selection.Changed += YtreeProfessions_Selection_Changed;

			ytreeItems.ColumnsConfig = FluentColumnsConfig<NormItem>.Create ()
				.AddColumn ("Наименование").AddTextRenderer (p => p.Item != null ? p.Item.Name : null)
				.AddColumn ("Количество")
				.AddNumericRenderer (i => i.Amount).WidthChars (9).Editing ().Adjustment (new Gtk.Adjustment(1, 0, 1000000, 1, 10, 10))
				.AddTextRenderer (i => i.Item != null && i.Item.Units != null ? i.Item.Units.Name : String.Empty)
				.AddColumn ("Период")
				.AddNumericRenderer (i => i.PeriodCount).WidthChars(6).Editing ().Adjustment (new Gtk.Adjustment(1, 0, 100, 1, 10, 10))
				.AddEnumRenderer (i => i.NormPeriod).Editing ()
				.AddColumn(String.Empty)
				.Finish ();
			ytreeItems.ItemsDataSource = Entity.ObservableItems;
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;

			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.SaveSensitive))
				buttonSave.Sensitive = ViewModel.SaveSensitive;
			if(e.PropertyName == nameof(ViewModel.CancelSensitive))
				buttonCancel.Sensitive = ViewModel.CancelSensitive;
		}

		void YtreeItems_Selection_Changed (object sender, EventArgs e)
		{
			buttonRemoveItem.Sensitive = ytreeItems.Selection.CountSelectedRows () > 0;
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

		#endregion

		protected void OnYentryRegulationDocChanged(object sender, EventArgs e)
		{
			ycomboAnnex.ItemsList = Entity.Document?.Annexess;
		}
	}
}

