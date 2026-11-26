using System;
using System.Collections.Generic;
using System.Reflection;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Models.Analytics;
using Workwear.Models.Operations;
using Workwear.Tools.Sizes;

namespace Workwear.Test.Models.Analytics.WarehouseForecasting {
	public abstract class WarehouseForecastingModelTestBase {
		protected static void SetCachedSizes(SizeService sizeService, params Size[] sizes) {
			var field = typeof(SizeService).GetField("sizes", BindingFlags.Instance | BindingFlags.NonPublic);
			field.SetValue(sizeService, sizes);
		}

		protected static void SetStockBalances(StockBalanceModel stockBalance, params StockBalance[] balances) {
			var field = typeof(StockBalanceModel).GetField("stockBalances", BindingFlags.Instance | BindingFlags.NonPublic);
			field.SetValue(stockBalance, new List<StockBalance>(balances));
		}

		protected class TestFutureIssue : FutureIssue {
			private readonly ProtectionTools protectionTools;
			private readonly Size size;

			public TestFutureIssue(ProtectionTools protectionTools, Size size) {
				this.protectionTools = protectionTools;
				this.size = size;
				Amount = 1;
				OperationDate = new DateTime(2026, 6, 16);
			}

			public override ProtectionTools ProtectionTools => protectionTools;
			public override Subdivision Subdivision => null;
			public override EmployeeCard Employee => null;
			public override Size Size => size;
			public override string NormAmountText => "";
			public override string NormLifeText => "";
			public override string NormConditionName => "";
			public override int? NormId => null;
			public override string IssueTypeTitle => "";
		}
	}
}
