﻿using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Sizes;
using workwear.Domain.Stock;
using Workwear.Measurements;
using workwear.Tools.Features;

namespace workwear.ViewModels.Stock
{
	public class ItemTypeViewModel : EntityDialogViewModelBase<ItemsType>
	{
		private readonly FeaturesService featuresService;
		public SizeService SizeService { get; }

		public ItemTypeViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation, 
			FeaturesService featuresService, 
			SizeService sizeService,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			SizeService = sizeService;
		}

		#region Visible
		public bool VisibleIssueType => featuresService.Available(WorkwearFeature.CollectiveExpense);
		#endregion
	}
}
