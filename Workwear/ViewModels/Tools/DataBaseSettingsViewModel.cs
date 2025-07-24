using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Tools;
using Workwear.Tools.Features;

namespace Workwear.ViewModels.Tools
{
	public class DataBaseSettingsViewModel : UowDialogViewModelBase, IDialogDocumentation
	{
		private readonly BaseParameters baseParameters;
		private readonly ICurrentPermissionService permissionService;

		#region Ограниения версии
		public bool CollectiveIssueWithPersonalVisible { get; }
		public bool EditLockDateVisible { get; }
		public bool CanEdit { get; }
		public bool PeriodOfOperationsVisible { get; }
		#endregion
		
		public DataBaseSettingsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			BaseParameters baseParameters,
			ICurrentPermissionService permissionService,
			FeaturesService featuresService) : base(unitOfWorkFactory, navigation)
		{
			Title = "Настройки учёта";
			CanEdit = permissionService.ValidatePresetPermission("can_accounting_settings");
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
			EditLockDate = baseParameters.EditLockDate;
			EditLockDateVisible = featuresService.Available(WorkwearFeature.EditLockDate);
			DefaultAutoWriteoff = baseParameters.DefaultAutoWriteoff;
			CheckBalances = baseParameters.CheckBalances;
			ColDayAheadOfShedule = baseParameters.ColDayAheadOfShedule;
			ShiftExpluatacion = baseParameters.ShiftExpluatacion;
			ExtendPeriod = baseParameters.ExtendPeriod;
			CollectiveIssueWithPersonal = baseParameters.CollectiveIssueWithPersonal;
			CollapseDuplicateIssuanceSheet = baseParameters.CollapseDuplicateIssuanceSheet;
			CollectiveIssueWithPersonalVisible = featuresService.Available(WorkwearFeature.CollectiveExpense);
			UsedCurrency = baseParameters.UsedCurrency;
			IsDocNumberInIssueSign = baseParameters.IsDocNumberInIssueSign;
			IsDocNumberInReturnSign = baseParameters.IsDocNumberInReturnSign;
			StartDate = baseParameters.StartDate;
			EndDate = baseParameters.EndDate;
			PeriodOfOperationsVisible = featuresService.Available(WorkwearFeature.PeriodOfOperations);
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("settings.html#accounting-settings");
		public string ButtonTooltip => DocHelper.GetDialogDocTooltip(Title);
		#endregion

		public override bool HasChanges => EditLockDate != baseParameters.EditLockDate
										   || DefaultAutoWriteoff != baseParameters.DefaultAutoWriteoff
		                                   || CheckBalances != baseParameters.CheckBalances
		                                   || ColDayAheadOfShedule != baseParameters.ColDayAheadOfShedule
		                                   || ShiftExpluatacion != baseParameters.ShiftExpluatacion
		                                   || CollectiveIssueWithPersonal != baseParameters.CollectiveIssueWithPersonal
		                                   || CollapseDuplicateIssuanceSheet != baseParameters.CollapseDuplicateIssuanceSheet
		                                   || ExtendPeriod != baseParameters.ExtendPeriod
		                                   || UsedCurrency != baseParameters.UsedCurrency
		                                   || IsDocNumberInIssueSign!=baseParameters.IsDocNumberInIssueSign
		                                   || IsDocNumberInReturnSign != baseParameters.IsDocNumberInReturnSign
										   || StartDate != baseParameters.StartDate
										   || EndDate != baseParameters.EndDate;

		#region Parameters
		public DateTime? EditLockDate { get; set; }
		public bool DefaultAutoWriteoff { get; set; }
		public bool CheckBalances { get; set; }
		public int ColDayAheadOfShedule { get; set; }
		public AnswerOptions ShiftExpluatacion { get; set; }
		public AnswerOptions ExtendPeriod { get; set; }
		public string UsedCurrency { get; set; }

		public bool CollectiveIssueWithPersonal { get; set; }
		public bool CollapseDuplicateIssuanceSheet { get; set; }
		public bool IsDocNumberInIssueSign{get;set;}
		public bool IsDocNumberInReturnSign { get; set; }
		
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
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
			if(UsedCurrency != baseParameters.UsedCurrency)
				baseParameters.UsedCurrency = UsedCurrency;
			if(IsDocNumberInIssueSign!=baseParameters.IsDocNumberInIssueSign)
				baseParameters.IsDocNumberInIssueSign = IsDocNumberInIssueSign;
			if(IsDocNumberInReturnSign!=baseParameters.IsDocNumberInReturnSign)
				baseParameters.IsDocNumberInReturnSign = IsDocNumberInReturnSign;
			if(EditLockDate != baseParameters.EditLockDate)
				baseParameters.EditLockDate = EditLockDate;
			if(StartDate != baseParameters.StartDate)
				baseParameters.StartDate = StartDate;
			if(EndDate != baseParameters.EndDate)
				baseParameters.EndDate = EndDate;
			return true;
		}
	}
}
