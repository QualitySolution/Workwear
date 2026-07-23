using System;

namespace Workwear.Domain.Operations.Graph {
	/// <summary>
	/// Лёгкий DTO для построения <see cref="IssueGraph"/> без загрузки полных сущностей
	/// <see cref="EmployeeIssueOperation"/> в Identity Map NHibernate.
	/// Используется в прогнозировании склада, где нужно загрузить историю всех сотрудников.
	/// </summary>
	public class GraphIssueOperationDto : IGraphIssueOperation {
		public int Id { get; set; }
		public DateTime OperationTime { get; set; }
		public DateTime? StartOfUse { get; set; }
		public DateTime? AutoWriteoffDate { get; set; }
		public DateTime? ExpiryByNorm { get; set; }
		public int Issued { get; set; }
		public int Returned { get; set; }
		public bool OverrideBefore { get; set; }

		/// <inheritdoc />
		public IGraphIssueOperation IssuedOperation { get; set; }

		/// <summary>
		/// Id сопутствующей операции выдачи. Временное поле, используется при разрешении ссылок после загрузки.
		/// </summary>
		public int? IssuedOperationId { get; set; }

		/// <summary>Id сотрудника — для группировки в <see cref="Workwear.Models.Operations.EmployeeIssueModel"/>.</summary>
		public int EmployeeId { get; set; }

		/// <summary>Id номенклатуры нормы — для группировки по типу СИЗ.</summary>
		public int ProtectionToolsId { get; set; }
	}
}


