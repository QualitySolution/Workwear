using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using workwear.Tools;

namespace workwear.ViewModels.Tools
{
	public class DataBaseSettingsViewModel : UowDialogViewModelBase
	{
		private readonly BaseParameters baseParameters;

		public DataBaseSettingsViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, BaseParameters baseParameters) : base(unitOfWorkFactory, navigation)
		{
			Title = "Настройки учёта";
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			DefaultAutoWriteoff = baseParameters.DefaultAutoWriteoff;
			EmployeeSizeRanges = baseParameters.EmployeeSizeRanges;
			CheckBalances = baseParameters.CheckBalances;
			ColDayAheadOfShedule = baseParameters.ColDayAheadOfShedule;
			ShiftExpluatacion = baseParameters.ShiftExpluatacion;
			ExtendPeriod = baseParameters.ExtendPeriod;
		}

		public override bool HasChanges => DefaultAutoWriteoff != baseParameters.DefaultAutoWriteoff
			|| EmployeeSizeRanges != baseParameters.EmployeeSizeRanges
			|| CheckBalances != baseParameters.CheckBalances
			|| ColDayAheadOfShedule != baseParameters.ColDayAheadOfShedule
			|| ShiftExpluatacion != baseParameters.ShiftExpluatacion
			|| ExtendPeriod != baseParameters.ExtendPeriod;

		#region Parameters
		public bool DefaultAutoWriteoff { get; set; }
		public bool EmployeeSizeRanges { get; set; }
		public bool CheckBalances { get; set; }
		public int ColDayAheadOfShedule { get; set; }
		public AnswerOptions ShiftExpluatacion { get; set; }
		public AnswerOptions ExtendPeriod { get; set; }
		#endregion

		public override bool Save()
		{
			if(DefaultAutoWriteoff != baseParameters.DefaultAutoWriteoff)
				baseParameters.DefaultAutoWriteoff = DefaultAutoWriteoff;
			if(EmployeeSizeRanges != baseParameters.EmployeeSizeRanges)
				baseParameters.EmployeeSizeRanges = EmployeeSizeRanges;
			if(CheckBalances != baseParameters.CheckBalances)
				baseParameters.CheckBalances = CheckBalances;
			if(ColDayAheadOfShedule != baseParameters.ColDayAheadOfShedule)
				baseParameters.ColDayAheadOfShedule = ColDayAheadOfShedule;
			if(ShiftExpluatacion != baseParameters.ShiftExpluatacion)
				baseParameters.ShiftExpluatacion = ShiftExpluatacion;
			if(ExtendPeriod != baseParameters.ExtendPeriod)
				baseParameters.ExtendPeriod = ExtendPeriod;
			return true;
		}
	}
}
