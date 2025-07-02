using System;
using FluentNHibernate.Data;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace Workwear.Views.Company.EmployeeChildren {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeDutyNormsView : Gtk.Bin {
		public EmployeeDutyNormsView() {
			this.Build();
		}
		private EmployeeDutyNormsViewModel viewModel;

		public EmployeeDutyNormsViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				viewModel.PropertyChanged += ViewModel_PropertyChanged;
				ConfigureTable();
			}
		}
		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.ObservableDutyNormItems)) {
				ytreeview.ItemsDataSource = ViewModel.ObservableDutyNormItems;
			}
		}

		void ytreeView_Selection_Changed(object sender, EventArgs e)
		{
			ViewModel.SelectedItem = ytreeview.GetSelectedObject<DutyNormItem>();
		}
		private void ConfigureTable() 
		{
			ytreeview.ColumnsConfig = FluentColumnsConfig<DutyNormItem>.Create()
				.AddColumn("Наименование").Resizable()
					.AddTextRenderer(i  => i.ProtectionTools != null ? i.ProtectionTools.Name : null).WrapWidth(700)
				.AddColumn("По норме")
					.AddNumericRenderer(i => i.Amount)
					.AddTextRenderer(i => i.AmountUnitText(i.Amount))
				.AddColumn("Срок службы").Resizable()
					.AddNumericRenderer(i=>i.PeriodCount)
					.AddSetter((c, n) => c.Visible = n.NormPeriod != DutyNormPeriodType.Wearout)
					.AddEnumRenderer(i=>i.NormPeriod)
					.AddSetter((c,n) => c.Text = n.PeriodText )
				.AddColumn("Числится").Resizable()
					.AddTextRenderer(i => i.Issued(DateTime.Now).ToString())
					.AddSetter((w, i) => w.Foreground = i.AmountColor)
					.AddTextRenderer(i => i.AmountUnitText(i.Issued(DateTime.Now)))
				.AddColumn("След. получение").Resizable()
					.AddTextRenderer(i => i.NextIssueText)
					.AddSetter((w, i) => w.Foreground = i.NextIssueColor)
				.AddColumn("Пункт норм").AddTextRenderer(x => x.NormParagraph)
				.AddColumn("Комментарий").AddTextRenderer(x => x.Comment)
				.Finish ();
			ytreeview.Selection.Changed += ytreeView_Selection_Changed;
		}
		
		#region Кнопки

		protected void OnButtonGiveWearByDutyNormClicked(object sender, EventArgs e) {
			ybuttonGiveWearByDutyNorm.Sensitive = false;
			ViewModel.GiveWearByDutyNorm();
			ybuttonGiveWearByDutyNorm.Sensitive = true;
		}

		protected void OnButtonOpenDutyNormClicked(object sender, EventArgs e) {
			ybuttonOpenDutyNorm.Sensitive = false;
			ViewModel.OpenDutyNorm(ytreeview.GetSelectedObject<DutyNormItem>());
			ybuttonOpenDutyNorm.Sensitive = true;

		}
		#endregion
	}
}
