using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Workwear.Domain.Company;
using Workwear.Models.Import.Norms;
using Workwear.Repository.Regulations;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Import;

namespace Workwear.Test.Models.Import.Norms {
	[TestFixture(TestOf = typeof(DataParserNorm))]
	public class DataParserNormTest {

		[Test(Description =
			"Проверяем что при отсутствии колонок с подразделением и отделом мы подбираем все должности с подходящем названием из любых подразделение или отделов.")]
		public void SetOrMakePost_WithoutDepartmentAndSubdivision() {
			var normRepository = Substitute.For<NormRepository>();
			var protectionToolsRepository = Substitute.For<ProtectionToolsRepository>();
			var sizeService = Substitute.For<SizeService>();

			var subdivision1 = new Subdivision {
				Name = "Цех 1",
			};
			
			var subdivision2 = new Subdivision {
				Name = "Цех 2",
			};
			
			var department1 = new Department {
				Name = "Отдел 1",
				Subdivision = subdivision1
			};

			var posts = new List<Post> {
				new Post {
					Name = "Директор",
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision1
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision2
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision1,
					Department = department1
				},
			};
			
			var settings = Substitute.For<SettingsNormsViewModel>(new ParametersServiceForTest());
			var postCombination = new SubdivisionPostCombination(settings, null,"Мастер", null, null);
			
			var parser = new DataParserNorm(normRepository, protectionToolsRepository, sizeService);
			
			parser.SetOrMakePost(postCombination, posts, new List<Subdivision>(), new List<Department>(), true, true, String.Empty);
			Assert.That(postCombination.Posts.Count, Is.EqualTo(3));
			Assert.That(postCombination.Posts.Any(x => x.Name == "Директор"), Is.False);
		}

		[Test(Description =
			"Противоположность тесту выше! Проверяем что с колонкой подразделение мы подбираем все должности с подходящем названием и подразделением без учета отделов.")]
		public void SetOrMakePost_WithSubdivisionWithoutDepartment() {
			var normRepository = Substitute.For<NormRepository>();
			var protectionToolsRepository = Substitute.For<ProtectionToolsRepository>();
			var sizeService = Substitute.For<SizeService>();

			var subdivision1 = new Subdivision {
				Name = "Цех 1",
			};
			
			var subdivision2 = new Subdivision {
				Name = "Цех 2",
			};
			
			var department1 = new Department {
				Name = "Отдел 1",
				Subdivision = subdivision1
			};

			var posts = new List<Post> {
				new Post {
					Name = "Директор",
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision1
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision2
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision1,
					Department = department1
				},
			};
			
			var settings = Substitute.For<SettingsNormsViewModel>(new ParametersServiceForTest());
			var postCombination = new SubdivisionPostCombination(settings, null,"Мастер", "Цех 1", null);
			
			var parser = new DataParserNorm(normRepository, protectionToolsRepository, sizeService);
			
			parser.SetOrMakePost(postCombination, posts, new List<Subdivision>(), new List<Department>(), false, true, String.Empty);
			Assert.That(postCombination.Posts.Count, Is.EqualTo(2));
			Assert.That(postCombination.Posts.Any(x => x.Name == "Директор"), Is.False);
			Assert.That(postCombination.Posts.Any(x => x.Name == "Мастер" && x.Subdivision == subdivision2), Is.False);
		}
		
		[Test(Description =
			"Как выше но с отделом! Проверяем что с колонкой подразделение мы подбираем все должности с подходящем названием и подразделением без учета отделов.")]
		public void SetOrMakePost_WithSubdivisionAndDepartment() {
			var normRepository = Substitute.For<NormRepository>();
			var protectionToolsRepository = Substitute.For<ProtectionToolsRepository>();
			var sizeService = Substitute.For<SizeService>();

			var subdivision1 = new Subdivision {
				Name = "Цех 1",
			};
			
			var subdivision2 = new Subdivision {
				Name = "Цех 2",
			};
			
			var department1 = new Department {
				Name = "Отдел 1",
				Subdivision = subdivision1
			};

			var posts = new List<Post> {
				new Post {
					Name = "Директор",
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision1
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision2
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision1,
					Department = department1
				},
			};
			
			var settings = Substitute.For<SettingsNormsViewModel>(new ParametersServiceForTest());
			var postCombination = new SubdivisionPostCombination(settings, null,"Мастер", "Цех 1", "Отдел 1");
			
			var parser = new DataParserNorm(normRepository, protectionToolsRepository, sizeService);
			
			parser.SetOrMakePost(postCombination, posts, new List<Subdivision>(), new List<Department>(), false, false, String.Empty);
			Assert.That(postCombination.Posts.Count, Is.EqualTo(1));
			Assert.That(postCombination.Posts.Any(x => x.Name == "Мастер" && x.Subdivision == subdivision1 && x.Department == department1), Is.True);
		}
		
		[Test(Description =
			"Проверяем что при отсутствии колонок с подразделением и отделом мы подбираем все должности(с учетом разделителя) с подходящем названием из любых подразделение или отделов.")]
		public void SetOrMakePost_WithoutDepartmentAndSubdivisionAllPosts() {
			var normRepository = Substitute.For<NormRepository>();
			var protectionToolsRepository = Substitute.For<ProtectionToolsRepository>();
			var sizeService = Substitute.For<SizeService>();

			var subdivision1 = new Subdivision {
				Name = "Цех 1",
			};
			
			var subdivision2 = new Subdivision {
				Name = "Цех 2",
			};
			
			var department1 = new Department {
				Name = "Отдел 1",
				Subdivision = subdivision1
			};

			var posts = new List<Post> {
				new Post {
					Name = "Директор",
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision1
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision2
				},
				new Post {
					Name = "Мастер",
					Subdivision = subdivision1,
					Department = department1
				},
			};
			
			var settings = Substitute.For<SettingsNormsViewModel>(new ParametersServiceForTest());
			settings.ListSeparator.Returns(",");
			var postCombination = new SubdivisionPostCombination(settings, null,"Мастер,Директор", null, null);
			
			var parser = new DataParserNorm(normRepository, protectionToolsRepository, sizeService);
			
			parser.SetOrMakePost(postCombination, posts, new List<Subdivision>(), new List<Department>(), true, true, String.Empty);
			Assert.That(postCombination.Posts.Count, Is.EqualTo(4));
		}
		
		
	}
}
