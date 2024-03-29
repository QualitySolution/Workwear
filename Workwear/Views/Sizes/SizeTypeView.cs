﻿using System;
using System.Linq;
using Gamma.Binding.Converters;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.Entity;
using QS.Views.Dialog;
using Workwear.Domain.Sizes;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Sizes;

namespace Workwear.Views.Sizes
{
	public partial class SizeTypeView : EntityDialogViewBase<SizeTypeViewModel, SizeType>
	{
		public SizeTypeView(SizeTypeViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			ybuttonRemoveSize.Sensitive = false;
			ybuttonAddSize.Binding.AddBinding(ViewModel, v => v.CanAdd, w => w.Sensitive).InitializeFromSource();
			ybuttonAddSize.Clicked += AddSize;
			ybuttonRemoveSize.Clicked += RemoveSize;
			entityname.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, v => v.Sensitive)
				.InitializeFromSource();
			labelId.Binding
				.AddBinding(Entity, e => e.Id, w => w.Text, new IdToStringConverter())
				.InitializeFromSource();
			specllistcomCategory.ItemsEnum = typeof(CategorySizeType);
			specllistcomCategory.Binding
				.AddBinding(Entity, e => e.CategorySizeType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.IsNew, v => v.Sensitive)
				.InitializeFromSource();
			yspinPosition.Binding
			.AddBinding(Entity, e => e.Position, w => w.ValueAsInt)
				.InitializeFromSource();
			ycheckbuttonUseInEmployee.Binding
				.AddBinding(Entity, e => e.UseInEmployee, v => v.Active)
				.InitializeFromSource();
			ytreeviewSizes.ColumnsConfig = ColumnsConfigFactory.Create<Size>()
				.AddColumn("Значение").AddTextRenderer(x => x.Name)
				.AddColumn("Использовать в сотрудника").AddToggleRenderer(x => x.ShowInEmployee)
				.AddColumn("Использовать в номенклатуры").AddToggleRenderer(x => x.ShowInNomenclature)
				.AddColumn("Аналоги").AddTextRenderer(x =>
					String.Join(", ", x.SuitableSizes.Union(x.SizesWhereIsThisSizeAsSuitable).Select(z => z.Name)))
				.Finish();
			ytreeviewSizes.Binding.CleanSources();
			ytreeviewSizes.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Sizes, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeviewSizes.Selection.Changed += SelectionOnChanged;
			ytreeviewSizes.RowActivated += OpenSize;
		}

		private void OpenSize(object o, RowActivatedArgs args) =>
			ViewModel.OpenSize(ytreeviewSizes.SelectedRow.GetId());
		private void RemoveSize(object sender, EventArgs e) =>
			ViewModel.RemoveSize(ytreeviewSizes.GetSelectedObject<Size>());
		private void SelectionOnChanged(object sender, EventArgs e) =>
			ybuttonRemoveSize.Sensitive = ytreeviewSizes.GetSelectedObject<Size>()?.Id > SizeService.MaxStandardSizeId;
		private void AddSize(object sender, EventArgs e) => ViewModel.AddSize();
	}
}
