using System.Collections.Generic;
using NPOI.SS.Formula.Functions;
using NUnit.Framework;
using workwear.Domain.Company;
using workwear.Models.Import.Employees;
using workwear.Models.Import.Employees.DataTypes;
using Substitute = NSubstitute.Substitute;

namespace WorkwearTest.Models.Import.Employees {
	[TestFixture(TestOf = typeof(DataTypeSubdivision))]
	public class DataTypeSubdivisionTest {

		private static object[] EqualsSubdivisionTestCases = {
			new TestCaseData( //Нормальное срабатывание
				new[] {
					"Производственная дирекция", "Цех по эксплуатации водопроводных сетей и водопроводных насосных станций",
					"Участок водозаборных сооружений", "Водозабор \"Восточный №1\""
				},
				new Subdivision() {
					Name = "Водозабор \"Восточный №1\"",
					ParentSubdivision = new Subdivision() {
						Name = "Участок водозаборных сооружений",
						ParentSubdivision = new Subdivision() {
							Name = "Цех по эксплуатации водопроводных сетей и водопроводных насосных станций",
							ParentSubdivision = new Subdivision() {
								Name = "Производственная дирекция"
							}
						}
					}
				}
			).Returns(true),
			new TestCaseData( //Верхний уровень потерян
				new[] {
					"Производственная дирекция", "Цех по эксплуатации водопроводных сетей и водопроводных насосных станций",
					"Участок водозаборных сооружений", "Водозабор \"Восточный №1\""
				},
				new Subdivision() {
					Name = "Водозабор \"Восточный №1\"",
					ParentSubdivision = new Subdivision() {
						Name = "Участок водозаборных сооружений",
						ParentSubdivision = new Subdivision() {
							Name = "Цех по эксплуатации водопроводных сетей и водопроводных насосных станций",
						}
					}
				}
			).Returns(false),
			new TestCaseData( //Одинаковые подразделения в разных ветках
				new[] {
					"Производственная дирекция", "Цех по эксплуатации водопроводных сетей и водопроводных насосных станций",
					"Производственный отдел", "Водозабор \"Восточный №1\""
				},
				new Subdivision() {
					Name = "Водозабор \"Восточный №1\"",
					ParentSubdivision = new Subdivision() {
						Name = "Участок водозаборных сооружений",
						ParentSubdivision = new Subdivision() {
							Name = "Цех по эксплуатации водопроводных сетей и водопроводных насосных станций",
							ParentSubdivision = new Subdivision() {
								Name = "Производственная дирекция"
							}
						}
					}
				}
			).Returns(false),
			new TestCaseData( //Совпадает с верхним уровнем
				new[] {
					"Производственная дирекция"
				},
				new Subdivision() {
					Name = "Водозабор \"Восточный №1\"",
					ParentSubdivision = new Subdivision() {
						Name = "Участок водозаборных сооружений",
						ParentSubdivision = new Subdivision() {
							Name = "Цех по эксплуатации водопроводных сетей и водопроводных насосных станций",
							ParentSubdivision = new Subdivision() {
								Name = "Производственная дирекция"
							}
						}
					}
				}
			).Returns(false),
			new TestCaseData( //Совпадает с нижним уровнем
				new[] {
					"Водозабор \"Восточный №1\""
				},
				new Subdivision() {
					Name = "Водозабор \"Восточный №1\"",
					ParentSubdivision = new Subdivision() {
						Name = "Участок водозаборных сооружений",
						ParentSubdivision = new Subdivision() {
							Name = "Цех по эксплуатации водопроводных сетей и водопроводных насосных станций",
							ParentSubdivision = new Subdivision() {
								Name = "Производственная дирекция"
							}
						}
					}
				}
			).Returns(false),
		};
		
		[TestCaseSource(nameof(EqualsSubdivisionTestCases))]
		public bool EqualsSubdivisionCases(string[] names, Subdivision subdivision) {
			return DataTypeSubdivision.EqualsSubdivision(names, subdivision);
		}

		[Test(Description = "Простое создание по уровням")]
		public void GetOrCreateSubdivision_LevelSimpleCase() {
			var exist = new List<Subdivision>();
			var names = new[] {
				"Производственная дирекция", "Цех по эксплуатации водопроводных сетей и водопроводных насосных станций",
				"Участок водозаборных сооружений", "Водозабор \"Восточный №1\""
			};

			var result = DataTypeSubdivision.GetOrCreateSubdivision(names, exist, null);
			Assert.That(result.Name, Is.EqualTo("Водозабор \"Восточный №1\""));
			Assert.That(result.ParentSubdivision?.Name, Is.EqualTo("Участок водозаборных сооружений"));
			Assert.That(result.ParentSubdivision?.ParentSubdivision?.Name, Is.EqualTo("Цех по эксплуатации водопроводных сетей и водопроводных насосных станций"));
			Assert.That(result.ParentSubdivision?.ParentSubdivision?.ParentSubdivision?.Name, Is.EqualTo("Производственная дирекция"));
			Assert.That(result.ParentSubdivision?.ParentSubdivision?.ParentSubdivision?.ParentSubdivision, Is.Null);
		}
		
	}
}
