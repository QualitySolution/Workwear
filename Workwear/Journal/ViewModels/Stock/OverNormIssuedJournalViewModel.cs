using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dapper;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.Search;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Tools;

namespace Workwear.Journal.ViewModels.Stock {
	public class OverNormIssuedJournalViewModel : JournalViewModelBase, IDialogDocumentation {
		private readonly IConnectionFactory connectionFactory;

		public OverNormIssuedJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IConnectionFactory connectionFactory
		) : base(unitOfWorkFactory, interactiveService, navigation) {
			this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
			DataLoader = new AnyDataLoader<OverNormIssuedJournalNode>(GetNodes);
			TableSelectionMode = JournalSelectionMode.Multiple;
			Title = "Выдано сверх нормы";
			UpdateOnChanges(typeof(OverNormOperation));
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock-documents.html#over-norm");
		public string ButtonTooltip => DocHelper.GetDialogDocTooltip(Title);
		#endregion

		private EmployeeCard employee;
		public EmployeeCard Employee {
			get => employee;
			set {
				if(SetField(ref employee, value))
					DataLoader?.LoadData(false);
			}
		}

		private IList<OverNormIssuedJournalNode> GetNodes(CancellationToken cancellation) {
			using(var connection = connectionFactory.OpenConnection()) {
				var conditions = new List<string> {
					"warehouse_operation.warehouse_expense_id IS NOT NULL",
					"operation_over_norm.type <> 'Simple'",
					"operation_over_norm.return_from_operation IS NULL",
					@"NOT EXISTS (
						SELECT 1
						FROM operation_over_norm returned_operation
						WHERE returned_operation.return_from_operation = operation_over_norm.id
					)"
				};

				if(Employee != null)
					conditions.Add("operation_over_norm.employee_id = @employee_id");

				var search = new SqlSearchCriterion(Search)
					.WithLikeMode(LikeMatchMode.StringAnywhere)
					.By("employees.last_name")
					.By("employees.first_name")
					.By("employees.patronymic")
					.By("employees.personnel_number")
					.By("nomenclature.name")
					.By("barcode.title")
					.Finish();
				if(!String.IsNullOrEmpty(search))
					conditions.Add(search);

				var sql = $@"
SELECT
	operation_over_norm.id AS Id,
	operation_over_norm.type AS Type,
	CONCAT_WS(' ', employees.last_name, employees.first_name, employees.patronymic) AS EmployeeName,
	employees.personnel_number AS PersonnelNumber,
	nomenclature.name AS NomenclatureName,
	sizealias.name AS WearSize,
	heightalias.name AS Height,
	warehouse_operation.amount AS Amount,
	warehouse_operation.wear_percent AS WearPercent,
	warehouse_operation.operation_time AS Date,
	GROUP_CONCAT(DISTINCT barcode.title SEPARATOR '\n') AS Barcodes
FROM operation_over_norm
	JOIN operation_warehouse warehouse_operation ON warehouse_operation.id = operation_over_norm.operation_warehouse_id
	JOIN employees ON employees.id = operation_over_norm.employee_id
	JOIN nomenclature ON nomenclature.id = operation_over_norm.nomenclature_id
	LEFT JOIN sizes sizealias ON sizealias.id = operation_over_norm.size_id
	LEFT JOIN sizes heightalias ON heightalias.id = operation_over_norm.height_id
	LEFT JOIN operation_barcodes barcode_operation ON barcode_operation.over_norm_operation_id = operation_over_norm.id
	LEFT JOIN barcodes barcode ON barcode.id = barcode_operation.barcode_id
WHERE {String.Join(" AND ", conditions)}
GROUP BY operation_over_norm.id
ORDER BY warehouse_operation.operation_time DESC, operation_over_norm.id DESC";

				return connection.Query<OverNormIssuedJournalNode>(sql, new {
					employee_id = Employee?.Id
				}).ToList();
			}
		}
	}

	public class OverNormIssuedJournalNode {
		public int Id { get; set; }
		public string Type { get; set; }
		public string EmployeeName { get; set; }
		public string PersonnelNumber { get; set; }
		public string NomenclatureName { get; set; }
		public string WearSize { get; set; }
		public string Height { get; set; }
		public int Amount { get; set; }
		public decimal WearPercent { get; set; }
		public DateTime Date { get; set; }
		public string Barcodes { get; set; }
		public string TypeText => ((OverNormType)Enum.Parse(typeof(OverNormType), Type)).GetEnumTitle();
		public string DateText => Date.ToShortDateString();
		public string WearPercentText => WearPercent.ToString("P0");
	}
}
