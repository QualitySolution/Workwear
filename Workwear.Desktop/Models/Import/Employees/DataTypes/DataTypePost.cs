using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;

namespace Workwear.Models.Import.Employees.DataTypes {
	public class DataTypePost : DataTypeEmployeeBase {
		private readonly DataParserEmployee dataParserEmployee;
		private readonly IImportModel model;

		public DataTypePost(DataParserEmployee dataParserEmployee, IImportModel model)
		{
			this.dataParserEmployee = dataParserEmployee ?? throw new ArgumentNullException(nameof(dataParserEmployee));
			this.model = model ?? throw new ArgumentNullException(nameof(model));
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
			if(String.IsNullOrWhiteSpace(value) || IsSamePost(row.EditingEmployee.Post, value, row.EditingEmployee.Subdivision, row.EditingEmployee.Department))
				return new ChangeState(ChangeType.NotChanged);

			var post = dataParserEmployee.UsedPosts.FirstOrDefault(x => IsSamePost(x, value, row.EditingEmployee.Subdivision, row.EditingEmployee.Department));
			if(post == null) {
				post = new Post { 
					Name = value,
					Subdivision = row.EditingEmployee.Subdivision,
					Department = row.EditingEmployee.Department,
					Comments = "Создана при импорте сотрудников из файла " + model.FileName,
				};
				dataParserEmployee.UsedPosts.Add(post);
			}
			var oldPost = row.EditingEmployee.Post;
			row.EditingEmployee.Post = post;
			if(post.Id == 0)
				return new ChangeState(ChangeType.NewEntity, oldValue: FullTitle(oldPost), willCreatedValues: new[] { FullTitle(post) });
			return new ChangeState(ChangeType.ChangeValue, oldValue: FullTitle(oldPost), newValue: FullTitle(post));
		}
		
		private string FullTitle(Post post) {
			if(post == null)
				return null;
			var title = post.Name;
			if(post.Department != null)
				title += "\nв отделе: " + post.Department.Name;
			if (post.Subdivision != null)
				title += "\nв подразделении: " + post.Subdivision.Name;
			return title;
		}
		
		private bool IsSamePost(Post post, string postName, Subdivision postSubdivision, Department postDepartment) {
			if(post == null)
				return false;

			return String.Equals(post.Name, postName, StringComparison.CurrentCultureIgnoreCase)
			       && (post.Subdivision == null && postSubdivision == null ||
			           DomainHelper.EqualDomainObjects(post.Subdivision, postSubdivision))
			       && (post.Department == null && postDepartment == null ||
			           DomainHelper.EqualDomainObjects(post.Department, postDepartment));
		}
		#endregion
	}
}
