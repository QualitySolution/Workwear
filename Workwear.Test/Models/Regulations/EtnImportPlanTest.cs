using System.Linq;
using NUnit.Framework;
using Workwear.Domain.Regulations;
using Workwear.Models.Regulations;
using EtnComplectType = QS.Cloud.WorkwearDictionary.ComplectType;
using EtnGetNormResponse = QS.Cloud.WorkwearDictionary.GetNormResponse;
using EtnItemSIZ = QS.Cloud.WorkwearDictionary.ItemSIZ;
using EtnNorm = QS.Cloud.WorkwearDictionary.Norm;
using EtnNormItem = QS.Cloud.WorkwearDictionary.NormItem;
using EtnPeriodType = QS.Cloud.WorkwearDictionary.PeriodType;

namespace Workwear.Test.Models.Regulations {

	[TestFixture(TestOf = typeof(EtnImportPlan))]
	public class EtnImportPlanTest {

		private static EtnItemSIZ Siz(string name, int amount = 1, int periodCount = 1,
			EtnPeriodType periodType = EtnPeriodType.Year, bool dermal = false, string unit = null) =>
			new EtnItemSIZ {
				SizName = name,
				SizType = "Тип",
				Amount = amount,
				PeriodCount = periodCount,
				PeriodType = periodType,
				DermalPpe = dermal,
				Unit = unit ?? string.Empty
			};

		[Test(Description = "Комплект \"один\" выбора не даёт: все позиции становятся строками плана сразу.")]
		public void AddFrom_OneComplect_AllItemsBecomeRowsWithoutChoice() {
			var response = new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 10, NumberNorm = "1.1.1", NormName = "Событие" } },
				Items = {
					new EtnNormItem {
						NormId = 10,
						ComplectType = EtnComplectType.One,
						Items = { Siz("Каска"), Siz("Очки") }
					}
				}
			};

			var plan = new EtnImportPlan();
			plan.AddFrom(response, appendixNumber: 2, sourceKey: 10);

			Assert.That(plan.Rows.Count, Is.EqualTo(2));
			Assert.That(plan.Rows.Select(x => x.Name), Is.EquivalentTo(new[] { "Каска", "Очки" }));
			Assert.That(plan.Rows.All(x => !x.HasChoice));
			Assert.That(plan.Rows.All(x => x.IsComplete));
		}

		[Test(Description = "Комплект \"или\" даёт выбор: объединённый вариант плюс каждая позиция, по умолчанию объединённый.")]
		public void AddFrom_OrComplect_OffersCombinedAndEachItem() {
			var response = new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 10, NumberNorm = "1.1.1", NormName = "Событие" } },
				Items = {
					new EtnNormItem {
						NormId = 10,
						ComplectType = EtnComplectType.Or,
						Items = { Siz("Костюм"), Siz("Халат") }
					}
				}
			};

			var plan = new EtnImportPlan();
			plan.AddFrom(response, appendixNumber: 2, sourceKey: 10);

			Assert.That(plan.Rows.Count, Is.EqualTo(1), "Комплект \"или\" - одна строка нормы, а не по строке на вариант.");
			var row = plan.Rows.Single();
			Assert.That(row.HasChoice);
			Assert.That(row.Variants.Count, Is.EqualTo(3));
			Assert.That(row.Variants.First().IsCombined);
			Assert.That(row.Name, Is.EqualTo("Костюм или Халат"));
		}

		[Test(Description = "Смена варианта в комплекте \"или\" не добавляет строк и переносит данные выбранной позиции.")]
		public void Variant_Changed_RowIsUpdatedInPlaceNotAdded() {
			var response = new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 10, NumberNorm = "1.1.1", NormName = "Событие" } },
				Items = {
					new EtnNormItem {
						NormId = 10,
						ComplectType = EtnComplectType.Or,
						Items = { Siz("Костюм", periodCount: 1), Siz("Халат", periodCount: 2) }
					}
				}
			};

			var plan = new EtnImportPlan();
			plan.AddFrom(response, appendixNumber: 2, sourceKey: 10);
			var rowBefore = plan.Rows.Single();

			rowBefore.Variant = rowBefore.Variants.Single(x => x.Name == "Халат");

			Assert.That(plan.Rows.Count, Is.EqualTo(1));
			Assert.That(plan.Rows.Single(), Is.SameAs(rowBefore), "Строка должна обновиться на месте, а не быть заменена новой.");
			Assert.That(plan.Rows.Single().Name, Is.EqualTo("Халат"));
			Assert.That(plan.Rows.Single().PeriodCount, Is.EqualTo(2), "Срок должен приехать из выбранной позиции приказа.");
		}

		[Test(Description = "Блоки с общей ненулевой alt-группой - один выбор; вариантов столько, сколько позиций во всех блоках группы плюс объединённые.")]
		public void AddFrom_AltGroup_BlocksMergedIntoSingleChoice() {
			var response = new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 10, NumberNorm = "2.1.1", NormName = "Событие" } },
				Items = {
					new EtnNormItem {
						NormId = 10, AltGroup = 1, ComplectName = "Блок А",
						ComplectType = EtnComplectType.One,
						Items = { Siz("Средство А") }
					},
					new EtnNormItem {
						NormId = 10, AltGroup = 1, ComplectName = "Блок Б",
						ComplectType = EtnComplectType.One,
						Items = { Siz("Средство Б") }
					}
				}
			};

			var plan = new EtnImportPlan();
			plan.AddFrom(response, appendixNumber: 2, sourceKey: 10);

			Assert.That(plan.Rows.Count, Is.EqualTo(1), "Альтернативные блоки - один выбор, одна строка.");
			Assert.That(plan.Rows.Single().Variants.Count, Is.EqualTo(2));
		}

		[Test(Description = "Нулевая alt-группа не объединяет комплекты: каждый остаётся самостоятельным блоком.")]
		public void AddFrom_ZeroAltGroup_ComplectsAreNotMerged() {
			var response = new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 10, NumberNorm = "1.1.1", NormName = "Событие" } },
				Items = {
					new EtnNormItem { NormId = 10, ComplectType = EtnComplectType.One, Items = { Siz("Каска") } },
					new EtnNormItem { NormId = 10, ComplectType = EtnComplectType.One, Items = { Siz("Очки") } }
				}
			};

			var plan = new EtnImportPlan();
			plan.AddFrom(response, appendixNumber: 2, sourceKey: 10);

			Assert.That(plan.Blocks.Count, Is.EqualTo(2));
			Assert.That(plan.Rows.Count, Is.EqualTo(2));
			Assert.That(plan.Rows.All(x => !x.HasChoice));
		}

		[Test(Description = "Позиция без определённого срока в приказе приходит незаполненной и не готова к добавлению.")]
		public void AddFrom_NeedSetPeriod_RowIsIncomplete() {
			var response = new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 10, NumberNorm = "1.1.1", NormName = "Событие" } },
				Items = {
					new EtnNormItem {
						NormId = 10, ComplectType = EtnComplectType.One,
						Items = { Siz("Респиратор", periodType: EtnPeriodType.NeedSet) }
					}
				}
			};

			var plan = new EtnImportPlan();
			plan.AddFrom(response, appendixNumber: 2, sourceKey: 10);

			var row = plan.Rows.Single();
			Assert.That(row.NeedsPeriod);
			Assert.That(row.IsComplete, Is.False);
			Assert.That(plan.IncompleteRows.Count(), Is.EqualTo(1));

			row.Amount = 1;
			row.PeriodCount = 6;
			row.PeriodType = NormPeriodType.Month;
			Assert.That(row.IsComplete);
		}

		[Test(Description = "\"До износа\" не требует числа периодов - такая строка готова к добавлению без него.")]
		public void Row_Wearout_IsCompleteWithoutPeriodCount() {
			var response = new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 10, NumberNorm = "1.1.1", NormName = "Событие" } },
				Items = {
					new EtnNormItem {
						NormId = 10, ComplectType = EtnComplectType.One,
						Items = { Siz("Сапоги", periodCount: 0, periodType: EtnPeriodType.Wearout) }
					}
				}
			};

			var plan = new EtnImportPlan();
			plan.AddFrom(response, appendixNumber: 2, sourceKey: 10);

			var row = plan.Rows.Single();
			Assert.That(row.PeriodType, Is.EqualTo(NormPeriodType.Wearout));
			Assert.That(row.PeriodCount, Is.Null);
			Assert.That(row.IsComplete);
		}

		[Test(Description = "Пункт приказа берётся у каждой строки от своего узла - в одном ответе приезжают строки нескольких выбранных пунктов.")]
		public void AddFrom_SeveralNorms_EachRowKeepsItsOwnParagraph() {
			var response = new EtnGetNormResponse {
				NumberNorm = "1.1",
				Norms = {
					new EtnNorm { NormId = 10, NumberNorm = "1.1.1", NormName = "Первое событие" },
					new EtnNorm { NormId = 11, NumberNorm = "1.1.2", NormName = "Второе событие" }
				},
				Items = {
					new EtnNormItem { NormId = 10, ComplectType = EtnComplectType.One, Items = { Siz("Каска") } },
					new EtnNormItem { NormId = 11, ComplectType = EtnComplectType.One, Items = { Siz("Очки") } }
				}
			};

			var plan = new EtnImportPlan();
			plan.AddFrom(response, appendixNumber: 2, sourceKey: 5);

			var byName = plan.Rows.ToDictionary(x => x.Name);
			Assert.That(byName["Каска"].NormParagraph, Does.Contain("1.1.1"));
			Assert.That(byName["Очки"].NormParagraph, Does.Contain("1.1.2"));
			Assert.That(byName["Каска"].NormParagraph, Does.Contain("Приложение 2"));
			Assert.That(byName["Каска"].SourceName, Is.EqualTo("Первое событие"));
		}

		[Test(Description = "Снятие отметки с узла убирает из плана только его строки, правки в строках других узлов сохраняются.")]
		public void RemoveSource_RemovesOnlyItsRowsAndKeepsEditsOfOthers() {
			var plan = new EtnImportPlan();
			plan.AddFrom(new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 10, NumberNorm = "1.1.1", NormName = "Первое" } },
				Items = { new EtnNormItem { NormId = 10, ComplectType = EtnComplectType.One, Items = { Siz("Каска") } } }
			}, appendixNumber: 2, sourceKey: 10);
			plan.AddFrom(new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 11, NumberNorm = "1.1.2", NormName = "Второе" } },
				Items = { new EtnNormItem { NormId = 11, ComplectType = EtnComplectType.One, Items = { Siz("Очки") } } }
			}, appendixNumber: 2, sourceKey: 11);

			plan.Rows.First(x => x.Name == "Каска").PeriodCount = 7;
			plan.RemoveSource(11);

			Assert.That(plan.Rows.Count, Is.EqualTo(1));
			Assert.That(plan.Rows.Single().Name, Is.EqualTo("Каска"));
			Assert.That(plan.Rows.Single().PeriodCount, Is.EqualTo(7), "Правка пользователя в оставшейся строке должна сохраниться.");
			Assert.That(plan.Blocks.Count, Is.EqualTo(1));
		}

		[Test(Description = "У дерматологических СИЗ объём/масса уходят в название номенклатуры, а количество значит число упаковок.")]
		public void AddFrom_DermalPpe_VolumeGoesIntoNameAndAmountIsOne() {
			var response = new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 20, NumberNorm = "1", NormName = "Общие загрязнения" } },
				Items = {
					new EtnNormItem {
						NormId = 20, ComplectType = EtnComplectType.One,
						Items = { Siz("Мыло", amount: 200, periodCount: 1, periodType: EtnPeriodType.Month, dermal: true, unit: "г") }
					}
				}
			};

			var plan = new EtnImportPlan();
			plan.AddFrom(response, appendixNumber: 3, sourceKey: 20);

			var row = plan.Rows.Single();
			Assert.That(row.Name, Is.EqualTo("Мыло, 200 г"));
			Assert.That(row.Amount, Is.EqualTo(1));
			Assert.That(row.PeriodType, Is.EqualTo(NormPeriodType.Month));
		}

		[Test(Description = "Пометка \"доп. СИЗ по оценке профрисков\" с комплекта переносится на строки плана.")]
		public void AddFrom_AdditionalComplect_RowsAreMarkedAdditional() {
			var response = new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 10, NumberNorm = "1.1.1", NormName = "Событие" } },
				Items = {
					new EtnNormItem {
						NormId = 10, IsAdditional = true, ComplectType = EtnComplectType.One,
						Items = { Siz("Наколенники") }
					}
				}
			};

			var plan = new EtnImportPlan();
			plan.AddFrom(response, appendixNumber: 2, sourceKey: 10);

			Assert.That(plan.Rows.Single().IsAdditional);
		}

		[TestCase(EtnComplectType.One)]
		[TestCase(EtnComplectType.And)]
		public void AddFrom_SpecialPeriod_CommentBelongsOnlyToItsItem(EtnComplectType complectType) {
			var item = Siz("Куртка", periodType: EtnPeriodType.NeedSet);
			item.PeriodSpecial = "На время выполнения работ";
			var plan = new EtnImportPlan();
			plan.AddFrom(new EtnGetNormResponse {
				Items = { new EtnNormItem {
					NormId = 10, ComplectType = complectType, ComplectName = "Комплект",
					Items = { item, Siz("Капюшон") }
				} }
			}, appendixNumber: 2, sourceKey: 10);

			Assert.That(plan.Rows[0].Comment, Is.EqualTo(complectType == EtnComplectType.And
				? "Комплект. На время выполнения работ" : "На время выполнения работ"));
			Assert.That(plan.Rows[1].Comment, Is.EqualTo(complectType == EtnComplectType.And ? "Комплект" : null));
		}

		[Test]
		public void Variant_Changed_CommentsFollowSelectedItems() {
			var first = Siz("Костюм", periodType: EtnPeriodType.NeedSet);
			first.PeriodSpecial = "На время выполнения работ";
			var second = Siz("Халат", periodType: EtnPeriodType.NeedSet);
			second.PeriodSpecial = "По необходимости";
			var third = Siz("Фартук", periodType: EtnPeriodType.NeedSet);
			third.PeriodSpecial = first.PeriodSpecial;
			var plan = new EtnImportPlan();
			plan.AddFrom(new EtnGetNormResponse {
				Items = { new EtnNormItem {
					NormId = 10, ComplectType = EtnComplectType.Or,
					Items = { first, second, third }
				} }
			}, appendixNumber: 2, sourceKey: 10);

			var row = plan.Rows.Single();
			Assert.That(row.Comment, Is.EqualTo("На время выполнения работ. По необходимости"));
			row.Variant = row.Variants[2];
			Assert.That(row.Comment, Is.EqualTo(second.PeriodSpecial));
			row.Variant = row.Variants[1];
			Assert.That(row.Comment, Is.EqualTo(first.PeriodSpecial));
		}

		[Test(Description = "Название комплекта \"и\" попадает в комментарий строки - только оно связывает получившиеся строки между собой.")]
		public void AddFrom_AndComplect_BlockNameGoesToRowComment() {
			var response = new EtnGetNormResponse {
				Norms = { new EtnNorm { NormId = 10, NumberNorm = "1.1.1", NormName = "Событие" } },
				Items = {
					new EtnNormItem {
						NormId = 10, ComplectName = "Костюм с капюшоном",
						ComplectType = EtnComplectType.And,
						Items = { Siz("Куртка"), Siz("Капюшон") }
					}
				}
			};

			var plan = new EtnImportPlan();
			plan.AddFrom(response, appendixNumber: 2, sourceKey: 10);

			Assert.That(plan.Rows.Count, Is.EqualTo(2));
			Assert.That(plan.Rows.All(x => x.Comment == "Костюм с капюшоном"));
		}
	}
}
