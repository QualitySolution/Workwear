using System;
using System.ComponentModel;
using Gamma.Binding.Converters;
using Gamma.GtkWidgets;
using QS.Views.Dialog;
using Workwear.Domain.Sizes;
using Workwear.ViewModels.Sizes;

namespace Workwear.Views.Sizes
{
	public partial class SizeView : EntityDialogViewBase<SizeViewModel, Size>
	{
		public SizeView(SizeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
			Entity.ObservableSuitableSizes.PropertyOfElementChanged += ObservableSuitableSizesOnPropertyOfElementChanged;
		}

		private void ConfigureDlg()
		{
			CreateSuitableTable();
			entityname.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, v => v.Sensitive)
				.InitializeFromSource();
			labelId.Binding
				.AddBinding(Entity, e => e.Id, w => w.Text, new IdToStringConverter())
				.InitializeFromSource();
			specllistcomSizeType.SetRenderTextFunc<SizeType>(x => x.Name);
			specllistcomSizeType.ItemsList = ViewModel.SizeService.GetSizeType(ViewModel.UoW);
			specllistcomSizeType.Binding
				.AddBinding(Entity, e => e.SizeType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.IsNew, v => v.Sensitive)
				.InitializeFromSource();
			ycheckbuttonShowInEmployee.Binding
				.AddBinding(Entity, e => e.ShowInEmployee, w => w.Active)
				.InitializeFromSource();
			ycheckbuttonShowInNomenclature.Binding
				.AddBinding(Entity, e => e.ShowInNomenclature, w => w.Active)
				.InitializeFromSource();
			entityAlterName.Binding
				.AddBinding(Entity, e => e.AlternativeName, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, v => v.Sensitive)
				.InitializeFromSource();

			ybuttonAddSuitable.Binding.AddBinding(ViewModel, v => v.CanEditAnalogs, w => w.Sensitive).InitializeFromSource();
			ybuttonAddSuitable.Clicked += AddAnalog;
			ybuttonRemoveSuitable.Sensitive = false;
			ybuttonRemoveSuitable.Clicked += RemoveAnalog;
			ytreeviewSuitableSizes.Selection.Changed += SelectionOnChanged;
		}

		private void CreateSuitableTable()
		{
			ytreeviewSuitableSizes.ColumnsConfig = ColumnsConfigFactory.Create<Size>()
				.AddColumn("Значение").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeviewSuitableSizes.HeadersVisible = false;
			ytreeviewSuitableSizes.Binding
				.AddSource(ViewModel)
				.AddBinding(e => e.AllSuitable, w => w.ItemsDataSource)
				.InitializeFromSource();
		}
		private void AddAnalog(object sender, EventArgs eventArgs) => ViewModel.AddAnalog();
		private void RemoveAnalog(object sender, EventArgs eventArgs)
		{
			var analog = ytreeviewSuitableSizes.GetSelectedObject<Size>();
			ViewModel.RemoveAnalog(analog);
		}
		void SelectionOnChanged(object sender, EventArgs e) =>
			ybuttonRemoveSuitable.Sensitive = ytreeviewSuitableSizes.Selection.CountSelectedRows() > 0 && ViewModel.CanEditAnalogs;
		private void ObservableSuitableSizesOnPropertyOfElementChanged(object sender, PropertyChangedEventArgs e) => 
			ytreeviewSuitableSizes.YTreeModel.EmitModelChanged();
	}
}
