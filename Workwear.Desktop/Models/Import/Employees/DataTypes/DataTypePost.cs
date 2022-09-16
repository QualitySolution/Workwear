using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypePost : DataTypeEmployeeBase {
		private readonly DataParserEmployee dataParserEmployee;

		public DataTypePost(DataParserEmployee dataParserEmployee)
		{
			this.dataParserEmployee = dataParserEmployee ?? throw new ArgumentNullException(nameof(dataParserEmployee));
			ColumnNameKeywords.Add("должность");
			ColumnNameKeywords.Add("профессия");
			Data = DataTypeEmployee.Post;
		}

		public override void CalculateChange(SheetRowEmployee row, ExcelValueTarget target, IUnitOfWork uow) {
			var value = row.CellStringValue(target);
			row.AddColumnChange(target, GetChangeState(row, value));
		}
		
		#region Helpers
		private ChangeState GetChangeState(SheetRowEmployee row, string value) {
			if(String.Equals(row.EditingEmployee.Post?.Name, value, StringComparison.CurrentCultureIgnoreCase)) {
				return new ChangeState(ChangeType.NotChanged);
			}
			var post = dataParserEmployee.UsedPosts.FirstOrDefault(x =>
				String.Equals(x.Name, value, StringComparison.CurrentCultureIgnoreCase)
				&&(row.EditingEmployee.Subdivision == null && x.Subdivision == null || 
				   DomainHelper.EqualDomainObjects(x.Subdivision, row.EditingEmployee.Subdivision)));
			if(post == null) {
				post = new Post { 
					Name = value, 
					Subdivision = row.EditingEmployee.Subdivision,
					Comments = "Создана при импорте сотрудников из Excel"
				};
				dataParserEmployee.UsedPosts.Add(post);
			}
			row.EditingEmployee.Post = post;
			if(post.Id == 0)
				return new ChangeState(ChangeType.NewEntity, willCreatedValues: new[] { "Должность:" + post.Name });
			return new ChangeState(ChangeType.ChangeValue, oldValue: row.EditingEmployee.Post?.Name);
		}
		#endregion
	}
}
