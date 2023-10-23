using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Workwear.Tools;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.Tools
{
	public class DataBaseSettingsViewModel : UowDialogViewModelBase
	{
		private readonly BaseParameters baseParameters;

		#region Ограниения версии
		public bool CollectiveIssueWithPersonalVisible = true;

		#endregion
		
		public DataBaseSettingsViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, BaseParameters baseParameters, FeaturesService featuresService) : base(unitOfWorkFactory, navigation)
		{
			Title = "Настройки учёта";
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			DefaultAutoWriteoff = baseParameters.DefaultAutoWriteoff;
			CheckBalances = baseParameters.CheckBalances;
			ColDayAheadOfShedule = baseParameters.ColDayAheadOfShedule;
			ShiftExpluatacion = baseParameters.ShiftExpluatacion;
			ExtendPeriod = baseParameters.ExtendPeriod;
			CollectiveIssueWithPersonal = baseParameters.CollectiveIssueWithPersonal;
			CollapseDuplicateIssuanceSheet = baseParameters.CollapseDuplicateIssuanceSheet;
			CollectiveIssueWithPersonalVisible = featuresService.Available(WorkwearFeature.CollectiveExpense);;
		}

		public override bool HasChanges => DefaultAutoWriteoff != baseParameters.DefaultAutoWriteoff
		                                   || CheckBalances != baseParameters.CheckBalances
		                                   || ColDayAheadOfShedule != baseParameters.ColDayAheadOfShedule
		                                   || ShiftExpluatacion != baseParameters.ShiftExpluatacion
		                                   || CollectiveIssueWithPersonal != baseParameters.CollectiveIssueWithPersonal
		                                   || CollapseDuplicateIssuanceSheet != baseParameters.CollapseDuplicateIssuanceSheet
		                                   || ExtendPeriod != baseParameters.ExtendPeriod;

		#region Parameters
		public bool DefaultAutoWriteoff { get; set; }
		public bool CheckBalances { get; set; }
		public int ColDayAheadOfShedule { get; set; }
		public AnswerOptions ShiftExpluatacion { get; set; }
		public AnswerOptions ExtendPeriod { get; set; }

		public bool CollectiveIssueWithPersonal { get; set; }
		public bool CollapseDuplicateIssuanceSheet { get; set; }
		#endregion

		public override bool Save()
		{
			if(DefaultAutoWriteoff != baseParameters.DefaultAutoWriteoff)
				baseParameters.DefaultAutoWriteoff = DefaultAutoWriteoff;
			if(CheckBalances != baseParameters.CheckBalances)
				baseParameters.CheckBalances = CheckBalances;
			if(ColDayAheadOfShedule != baseParameters.ColDayAheadOfShedule)
				baseParameters.ColDayAheadOfShedule = ColDayAheadOfShedule;
			if(ShiftExpluatacion != baseParameters.ShiftExpluatacion)
				baseParameters.ShiftExpluatacion = ShiftExpluatacion;
			if(ExtendPeriod != baseParameters.ExtendPeriod)
				baseParameters.ExtendPeriod = ExtendPeriod;
			if(CollectiveIssueWithPersonal != baseParameters.CollectiveIssueWithPersonal)
				baseParameters.CollectiveIssueWithPersonal = CollectiveIssueWithPersonal;
			if(CollapseDuplicateIssuanceSheet != baseParameters.CollapseDuplicateIssuanceSheet)
				baseParameters.CollapseDuplicateIssuanceSheet = CollapseDuplicateIssuanceSheet;
			return true;
		}
	}
}
