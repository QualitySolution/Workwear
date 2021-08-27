using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NHibernate.Dialect.Function;
using NHibernate.Engine;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Utilities.Text;
using workwear.Domain.Company;
using workwear.Repository.Company;
using workwear.ViewModels.Import;
using Workwear.Domain.Company;

namespace workwear.Models.Import
{
	public class DataParserEmployee : DataParserBase<DataTypeEmployee>
	{
		private readonly SubdivisionRepository subdivisionRepository;
		private readonly PostRepository postRepository;
		private readonly IUserService userService;

		public HashSet<string> MaleNames = new HashSet<string> { "АБРАМ", "АВАЗ", "АВВАКУМ", "АВГУСТ", "АВДЕЙ", "АВРААМ", "АВТАНДИЛ", "АГАП", "АГАФОН", "АГГЕЙ", "АДАМ", "АДИС", "АДОЛЬФ", "АДРИАН", "АЗАМАТ", "АЗАРИЙ", "АЗАТ", "АЙДАР", "АЙНУР", "АЙРАТ", "АКАКИЙ", "АКИМ", "АЛАН", "АЛЕКСАНДР", "АЛЕКСЕЙ", "АЛИ", "АЛИХАН", "АЛМАЗ", "АЛЬБЕРТ", "АЛЬФРЕД", "АМАДЕЙ", "АМАДЕУС", "АМАЯК", "АМИН", "АМВРОСИЙ", "АНАТОЛИЙ", "АНВАР", "АНГЕЛ", "АНДРЕЙ", "АНДРЭ", "АНИКИТА", "АНТОН", "АНУФРИЙ", "АНФИМ", "АПОЛЛИНАРИЙ", "АРАМ", "АРИСТАРХ", "АРКАДИЙ", "АРМАН", "АРМЕН", "АРНО", "АРНОЛЬД", "АРОН", "АРСЕН", "АРСЕНИЙ", "АРСЛАН", "АРТЕМ", "АРТЕМИЙ", "АРТУР", "АРХИП", "АСКОЛЬД", "АФАНАСИЙ", "АХМЕТ", "АШОТ", "БАХРАМ", "БЕЖЕН", "БЕНЕДИКТ", "БЕРЕК", "БЕРНАР", "БОГДАН", "БОГОЛЮБ", "БОНИФАЦИЙ", "БОРЕСЛАВ", "БОРИС", "БОРИСЛАВ", "БОЯН", "БРОНИСЛАВ", "БРУНО", "БУЛАТ", "ВАДИМ", "ВАЛЕНТИН", "ВАЛЕРИЙ", "ВАЛЬДЕМАР", "ВАЛЬТЕР", "ВАРДАН", "ВАРЛААМ", "ВАРФОЛОМЕЙ", "ВАСИЛИЙ", "ВАТСЛАВ", "ВЕЛИЗАР", "ВЕЛОР", "ВЕНЕДИКТ", "ВЕНИАМИН", "ВИКЕНТИЙ", "ВИКТОР", "ВИЛЕН", "ВИЛЛИ", "ВИЛЬГЕЛЬМ", "ВИССАРИОН", "ВИТАЛИЙ", "ВИТАУТАС", "ВИТОЛЬД", "ВЛАДИМИР", "ВЛАДИСЛАВ", "ВЛАДЛЕН", "ВЛАС", "ВОЛОДАР", "ВСЕВОЛОД", "ВЯЧЕСЛАВ", "ГАВРИИЛ", "ГАЛАКТИОН", "ГАМЛЕТ", "ГАРРИ", "ГАЯС", "ГЕВОР", "ГЕВОРГ", "ГЕННАДИЙ", "ГЕНРИ", "ГЕНРИХ", "ГЕОРГИЙ", "ГЕРАЛЬД", "ГЕРАСИМ", "ГЕРМАН", "ГЛЕБ", "ГОАР", "ГОРДЕЙ", "ГОРДОН", "ГОРИСЛАВ", "ГРАДИМИР", "ГРИГОРИЙ", "ГУРИЙ", "ГУСТАВ", "ДАВИД", "ДАВЛАТ", "ДАМИР", "ДАНИИЛ", "ДАНИСЛАВ", "ДАНЬЯР", "ДЕМИД", "ДЕМЬЯН", "ДЕНИС", "ДЖАМАЛ", "ДЖЕЙМС", "ДЖЕРЕМИ", "ДЖОЗЕФ", "ДЖОРДАН", "ДЖОРЖ", "ДИК", "ДИНАР", "ДИНАСИЙ", "ДМИТРИЙ", "ДОБРЫНЯ", "ДОНАЛЬД", "ДОНАТ", "ДОНАТОС", "ДОРОФЕЙ", "ЕВГЕНИЙ", "ЕВГРАФ", "ЕВДОКИМ", "ЕВСЕЙ", "ЕВСТАФИЙ", "ЕГОР", "ЕЛИЗАР", "ЕЛИСЕЙ", "ЕМЕЛЬЯН", "ЕРЕМЕЙ", "ЕРМОЛАЙ", "ЕРОФЕЙ", "ЕФИМ", "ЕФРЕМ", "ЖАН", "ЖДАН", "ЖЕРАР", "ЗАКИР", "ЗАМИР", "ЗАУР", "ЗАХАР", "ЗЕНОН", "ЗИГМУНД", "ЗИНОВИЙ", "ЗУРАБ", "ИБРАГИМ", "ИВАН", "ИГНАТ", "ИГНАТИЙ", "ИГОРЬ", "ИЕРОНИМ", "ИЗМАИЛ", "ИЗРАИЛЬ", "ИЛИАН", "ИЛЛАРИОН", "ИЛЬХАМ", "ИЛЬШАТ", "ИЛЬЯ", "ИЛЬЯС", "ИНОКЕНТИЙ", "ИОАНН", "ИОАКИМ", "ИОН", "ИОСИФ", "ИППОЛИТ", "ИРАКЛИЙ", "ИСА", "ИСААК", "ИСИДОР", "ИСКАНДЕР", "ИСЛАМ", "ИСМАИЛ", "КАЗБЕК", "КАЗИМИР", "КАМИЛЬ", "КАРЕН", "КАРИМ", "КАРЛ", "КИМ", "КИР", "КИРИЛЛ", "КЛАВДИЙ", "КЛАУС", "КЛИМ", "КЛИМЕНТ", "КЛОД", "КОНДРАТ", "КОНСТАНТИН", "КОРНЕЙ", "КОРНИЛИЙ", "КУЗЬМА", "ЛАВР", "ЛАВРЕНТИЙ", "ЛАЗАРЬ", "ЛЕВ", "ЛЕВАН", "ЛЕВОН", "ЛЕНАР", "ЛЕОН", "ЛЕОНАРД", "ЛЕОНИД", "ЛЕОНТИЙ", "ЛЕОПОЛЬД", "ЛУКА", "ЛУКЬЯН", "ЛЮБИМ", "ЛЮБОМИР", "ЛЮДВИГ", "ЛЮСЬЕН", "ЛЮЦИЙ", "МАВЛЮДА", "МАДЛЕН", "МАЙ", "МАЙКЛ", "МАКАР", "МАКАРИЙ", "МАКСИМ", "МАКСИМИЛЬЯН", "МАКСУД", "МАНСУР", "МАНУИЛ", "МАР", "МАРАТ", "МАРИАН", "МАРК", "МАРСЕЛЬ", "МАРТИН", "МАТВЕЙ", "МАХМУД", "МЕРАБ", "МЕФОДИЙ", "МЕЧЕСЛАВ", "МИКУЛА", "МИЛАН", "МИРОН", "МИРОСЛАВ", "МИТРОФАН", "МИХАИЛ", "МИШЛОВ", "МОДЕСТ", "МОИСЕЙ", "МСТИСЛАВ", "МУРАТ", "МУСЛИМ", "МУХАММЕД", "НАЗАР", "НАЗАРИЙ", "НАИЛЬ", "НАТАН", "НАУМ", "НЕСТОР", "НИКАНОР", "НИКИТА", "НИКИФОР", "НИКОДИМ", "НИКОЛА", "НИКОЛАЙ", "НИКОН", "НИЛЬС", "НИСОН", "НИФОНТ", "НОРМАНН", "ОВИДИЙ", "ОЛАН", "ОЛЕГ", "ОЛЕСЬ", "ОНИСИМ", "ОРЕСТ", "ОРЛАНДО", "ОСИП", "ОСКАР", "ОСТАП", "ПАВЕЛ", "ПАНКРАТ", "ПАРАМОН", "ПЕТР", "ПЛАТОН", "ПОРФИРИЙ", "ПОТАП", "ПРОКОФИЙ", "ПРОХОР", "РАВИЛЬ", "РАДИЙ", "РАДИК", "РАДОМИР", "РАДОСЛАВ", "РАЗИЛЬ", "РАЙАН", "РАЙМОНД", "РАИС", "РАМАЗАН", "РАМИЗ", "РАМИЛЬ", "РАМОН", "РАНЕЛЬ", "РАСИМ", "РАСУЛ", "РАТИБОР", "РАТМИР", "РАФАИЛ", "РАФАЭЛЬ", "РАФИК", "РАШИД", "РЕМ", "РИНАТ", "РИФАТ", "РИХАРД", "РИЧАРД", "РОБЕРТ", "РОДИОН", "РОЛАН", "РОМАН", "РОСТИСЛАВ", "РУБЕН", "РУДОЛЬФ", "РУСЛАН", "РУСТАМ", "РУФИН", "РУШАН", "РЭЙ", "САБИР", "САВВА", "САВЕЛИЙ", "САМВЕЛ", "САМСОН", "САМУИЛ", "СВЯТОСЛАВ", "СЕВАСТЬЯН", "СЕВЕРИН", "СЕМЕН", "СЕРАФИМ", "СЕРГЕЙ", "СИДОР", "СОКРАТ", "СОЛОМОН", "СПАРТАК", "СПИРИДОН", "СТАКРАТ", "СТАНИСЛАВ", "СТЕПАН", "СТЕФАН", "СТИВЕН", "СТОЯН", "СУЛТАН", "ТАГИР", "ТАИС", "ТАЙЛЕР", "ТАЛИК", "ТАМАЗ", "ТАМЕРЛАН", "ТАРАС", "ТЕЛЬМАН", "ТЕОДОР", "ТЕРЕНТИЙ", "ТИБОР", "ТИГРАМ", "ТИГРАН", "ТИГРИЙ", "ТИМОФЕЙ", "ТИМУР", "ТИТ", "ТИХОН", "ТОМАС", "ТРИФОН", "ТРОФИМ", "УЛЬМАНАС", "УМАР", "УСТИН", "ФАДЕЙ", "ФАЗИЛЬ", "ФАНИС", "ФАРИД", "ФАРХАД", "ФЕДОР", "ФЕДОТ", "ФЕЛИКС", "ФЕОДОСИЙ", "ФЕРДИНАНД", "ФИДЕЛЬ", "ФИЛИМОН", "ФИЛИПП", "ФЛОРЕНТИЙ", "ФОМА", "ФРАНЦ", "ФРЕД", "ФРИДРИХ", "ФУАД", "ХАБИБ", "ХАКИМ", "ХАРИТОН", "ХРИСТИАН", "ХРИСТОС", "ХРИСТОФОР", "ЦЕЗАРЬ", "ЧАРЛЬЗ", "ЧЕСЛАВ", "ЧИНГИЗ", "ШАМИЛЬ", "ШАРЛЬ", "ЭДВАРД", "ЭДГАР", "ЭДМУНД", "ЭДУАРД", "ЭЛЬДАР", "ЭМИЛЬ", "ЭМИН", "ЭММАНУИЛ", "ЭРАСТ", "ЭРИК", "ЭРНЕСТ", "ЮЛИАН", "ЮЛИЙ", "ЮНУС", "ЮРИЙ", "ЮХИМ", "ЯКОВ", "ЯН", "ЯНУАРИЙ", "ЯРОСЛАВ", "ЯСОН" };

		public HashSet<string> FemaleNames = new HashSet<string> { "АВГУСТА", "АВДОТЬЯ", "АВРОРА", "АГАТА", "АГАПИЯ", "АГАФЬЯ", "АГЛАЯ", "АГНЕССА", "АГНИЯ", "АГРИППИНА", "АГУНДА", "АДА", "АДЕЛИНА", "АДЕЛАИДА", "АДЕЛЬ", "АДИЛЯ", "АДРИАНА", "АЗА", "АЗАЛИЯ", "АЗИЗА", "АЙГУЛЬ", "АЙЛИН", "АЙНАГУЛЬ", "АИДА", "АЙЖАН", "АКСИНЬЯ", "АКУЛИНА", "АЛАНА", "АЛЕВТИНА", "АЛЕКСАНДРА", "АЛЕНА", "АЛИКО", "АЛИНА", "АЛИСА", "АЛИЯ", "АЛЛА", "АЛСУ", "АЛЬБА", "АЛЬБЕРТА", "АЛЬБИНА", "АЛЬВИНА", "АЛЬФИЯ", "АЛЬФРЕДА", "АЛЯ", "АМАЛЬ", "АМЕЛИЯ", "АМИНА", "АМИРА", "АНАИТ", "АНАСТАСИЯ", "АНГЕЛИНА", "АНЕЛЯ", "АНЖЕЛА", "АНЖЕЛИКА", "АНИСЬЯ", "АНИТА", "АННА", "АНТОНИНА", "АНФИСА", "АПОЛЛИНАРИЯ", "АРАБЕЛЛА", "АРИАДНА", "АРИАНА", "АРИНА", "АРХЕЛИЯ", "АСЕЛЬ", "АСИЯ", "АССОЛЬ", "АСТРА", "АСТРИД", "АСЯ", "АУРЕЛИЯ", "АФАНАСИЯ", "АЭЛИТА", "БЕАТРИСА", "БЕЛИНДА", "БЕЛЛА", "БЕРТА", "БИРУТА", "БОГДАНА", "БОЖЕНА", "БОРИСЛАВА", "БРОНИСЛАВА", "ВАЛЕНТИНА", "ВАЛЕРИЯ", "ВАНДА", "ВАНЕССА", "ВАРВАРА", "ВАСИЛИНА", "ВАСИЛИСА", "ВЕНЕРА", "ВЕРА", "ВЕРОНИКА", "ВЕСЕЛИНА", "ВЕСНА", "ВЕСТА", "ВЕТА", "ВИДА", "ВИКТОРИНА", "ВИКТОРИЯ", "ВИЛЕНА", "ВИЛОРА", "ВИОЛЕТТА", "ВИРГИНИЯ", "ВИРИНЕЯ", "ВИТА", "ВИТАЛИНА", "ВЛАДА", "ВЛАДИСЛАВА", "ВЛАДЛЕНА", "ГАБРИЭЛЛА", "ГАЛИНА", "ГАЛИЯ", "ГАЯНЭ", "ГЕЛЕНА", "ГАЯНЭ", "ГЕЛЕНА", "ГЕЛЛА", "ГЕНРИЕТТА", "ГЕОРГИНА", "ГЕРА", "ГЕРТРУДА", "ГЛАФИРА", "ГЛАША", "ГЛОРИЯ", "ГРАЖИНА", "ГРЕТА", "ГУЗЕЛЬ", "ГУЛИЯ", "ГУЛЬМИРА", "ГУЛЬНАЗ", "ГУЛЬНАРА", "ГУЛЬШАТ", "ДАЙНА", "ДАЛИЯ", "ДАМИРА", "ДАНА", "ДАНИЭЛА", "ДАНУТА", "ДАРА", "ДАРИНА", "ДАРЬЯ", "ДАЯНА", "ДЕБОРА", "ДЖАМИЛЯ", "ДЖЕММА", "ДЖЕННИФЕР", "ДЖЕССИКА", "ДЖУЛИЯ", "ДЖУЛЬЕТТА", "ДИАНА", "ДИЛАРА", "ДИЛЬНАЗ", "ДИЛЬНАРА", "ДИЛЯ", "ДИНА", "ДИНАРА", "ДИОДОРА", "ДИОНИСИЯ", "ДОЛОРЕС", "ДОЛЯ", "ДОМИНИКА", "ДОРА", "ЕВА", "ЕВАНГЕЛИНА", "ЕВГЕНИЯ", "ЕВДОКИЯ", "ЕКАТЕРИНА", "ЕЛЕНА", "ЕЛИЗАВЕТА", "ЕСЕНИЯ", "ЕФИМИЯ", "ЖАННА", "ЖАСМИН", "ЖОЗЕФИНА", "ЗАБАВА", "ЗАИРА", "ЗАМИРА", "ЗАРА", "ЗАРЕМА", "ЗАРИНА", "ЗАХАРИЯ", "ЗЕМФИРА", "ЗИНАИДА", "ЗИТА", "ЗЛАТА", "ЗОРЯНА", "ЗОЯ", "ЗУЛЬФИЯ", "ЗУХРА", "ИВАННА", "ИВЕТТА", "ИВОНА", "ИДА", "ИЗАБЕЛЛА", "ИЗОЛЬДА", "ИЛАРИЯ", "ИЛИАНА", "ИЛОНА", "ИНАРА", "ИНГА", "ИНГЕБОРГА", "ИНДИРА", "ИНЕССА", "ИННА", "ИОАННА", "ИОЛАНТА", "ИРАИДА", "ИРИНА", "ИРМА", "ИСКРА", "ИЯ", "КАЛЕРИЯ", "КАМИЛЛА", "КАПИТОЛИНА", "КАРИМА", "КАРИНА", "КАРОЛИНА", "КАТАРИНА", "КИРА", "КЛАВДИЯ", "КЛАРА", "КЛАРИССА", "КЛИМЕНТИНА", "КОНСТАНЦИЯ", "КОРА", "КОРНЕЛИЯ", "КРИСТИНА", "КСЕНИЯ", "ЛАДА", "ЛАЙМА", "ЛАНА", "ЛАРА", "ЛАРИСА", "ЛАУРА", "ЛЕЙЛА", "ЛЕЙСАН", "ЛЕОКАДИЯ", "ЛЕОНИДА", "ЛЕРА", "ЛЕСЯ", "ЛИАНА", "ЛИДИЯ", "ЛИЗА", "ЛИКА", "ЛИЛИАНА", "ЛИЛИЯ", "ЛИНА", "ЛИНДА", "ЛИОРА", "ЛИРА", "ЛИЯ", "ЛОЛА", "ЛОЛИТА", "ЛОРА", "ЛУИЗА", "ЛУКЕРЬЯ", "ЛЮБОВЬ", "ЛЮДМИЛА", "ЛЯЛЯ", "ЛЮЦИЯ", "МАГДА", "МАГДАЛИНА", "МАДИНА", "МАЙЯ", "МАЛИКА", "МАЛЬВИНА", "МАРА", "МАРГАРИТА", "МАРИАННА", "МАРИКА", "МАРИНА", "МАРИЯ", "МАРСЕЛИНА", "МАРТА", "МАРУСЯ", "МАРФА", "МАРЬЯМ", "МАТИЛЬДА", "МЕЛАНИЯ", "МЕЛИССА", "МИКА", "МИЛА", "МИЛАДА", "МИЛАНА", "МИЛЕНА", "МИЛИЦА", "МИЛОЛИКА", "МИЛОСЛАВА", "МИРА", "МИРОСЛАВА", "МИРРА", "МОНИКА", "МУЗА", "МЭРИ", "НАДЕЖДА", "НАЗИРА", "НАИЛЯ", "НАИМА", "НАНА", "НАОМИ", "НАТАЛЬЯ", "НАТЕЛЛА", "НЕЛЛИ", "НЕОНИЛА", "НИКА", "НИКОЛЬ", "НИНА", "НИНЕЛЬ", "НОННА", "НОРА", "НУРИЯ", "ОДЕТТА", "ОКСАНА", "ОКТЯБРИНА", "ОЛЕСЯ", "ОЛИВИЯ", "ОЛЬГА", "ОФЕЛИЯ", "ПАВЛА", "ПАВЛИНА", "ПАМЕЛА", "ПАТРИЦИЯ", "ПЕЛАГЕЯ", "ПЕРИЗАТ", "ПОЛИНА", "ПРАСКОВЬЯ", "РАДА", "РАДМИЛА", "РАИСА", "РЕВЕККА", "РЕГИНА", "РЕМА", "РЕНАТА", "РИММА", "РИНА", "РИТА", "РОГНЕДА", "РОБЕРТА", "РОЗА", "РОКСАНА", "РОСТИСЛАВА", "РУЗАЛИЯ", "РУЗАННА", "РУЗИЛЯ", "РУМИЯ", "РУСАЛИНА", "РУСЛАНА", "РУФИНА", "САБИНА", "САБРИНА", "САЖИДА", "САИДА", "САЛОМЕЯ", "САМИРА", "САНДРА", "САНИЯ", "САНТА", "САРА", "САТИ", "СВЕТЛАНА", "СВЯТОСЛАВА", "СЕВАРА", "СЕВЕРИНА", "СЕЛЕНА", "СЕРАФИМА", "СИЛЬВА", "СИМА", "СИМОНА", "СЛАВА", "СНЕЖАНА", "СОНЯ", "СОФИЯ", "СТАНИСЛАВА", "СТЕЛЛА", "СТЕФАНИЯ", "СУСАННА", "ТАИРА", "ТАИСИЯ", "ТАЛА", "ТАМАРА", "ТАМИЛА", "ТАРА", "ТАТЬЯНА", "ТЕРЕЗА", "ТИНА", "ТОРА", "УЛЬЯНА", "УРСУЛА", "УСТИНА", "УСТИНЬЯ", "ФАИЗА", "ФАИНА", "ФАНИЯ", "ФАНЯ", "ФАРИДА", "ФАТИМА", "ФАЯ", "ФЕКЛА", "ФЕЛИЦИЯ", "ФЕРУЗА", "ФИЗУРА", "ФЛОРА", "ФРАНСУАЗА", "ФРИДА", "ХАРИТА", "ХИЛАРИ", "ХИЛЬДА", "ХЛОЯ", "ХРИСТИНА", "ЦВЕТАНА", "ЧЕЛСИ", "ЧЕСЛАВА", "ЧУЛПАН", "ШАКИРА", "ШАРЛОТТА", "ШЕЙЛА", "ШЕЛЛИ", "ШЕРИЛ", "ЭВЕЛИНА", "ЭВИТА", "ЭДДА", "ЭДИТА", "ЭЛЕОНОРА", "ЭЛИАНА", "ЭЛИЗА", "ЭЛИНА", "ЭЛЛА", "ЭЛЛАДА", "ЭЛОИЗА", "ЭЛЬВИНА", "ЭЛЬВИРА", "ЭЛЬГА", "ЭЛЬЗА", "ЭЛЬМИРА", "ЭЛЬНАРА", "ЭЛЯ", "ЭМИЛИЯ", "ЭММА", "ЭМИЛИ", "ЭРИКА", "ЭРНЕСТИНА", "ЭСМЕРАЛЬДА", "ЭТЕЛЬ", "ЭТЕРИ", "ЮЗЕФА", "ЮЛИЯ", "ЮНА", "ЮНИЯ", "ЮНОНА", "ЯДВИГА", "ЯНА", "ЯНИНА", "ЯРИНА", "ЯРОСЛАВА", "ЯСМИНА" };

		public DataParserEmployee(SubdivisionRepository subdivisionRepository = null, PostRepository postRepository = null, IUserService userService = null)
		{
			AddColumnName(DataTypeEmployee.Fio,
				"ФИО",
				"Ф.И.О.",
				"Фамилия Имя Отчество",
				"Наименование"//Встречается при выгрузке из 1C
				);
			AddColumnName(DataTypeEmployee.CardKey,
				"CARD_KEY",
				"card",
				"uid"
				);
			AddColumnName(DataTypeEmployee.FirstName,
				"FIRST_NAME",
				"имя",
				"FIRST NAME"
				);
			AddColumnName(DataTypeEmployee.LastName,
				"LAST_NAME",
				"фамилия",
				"LAST NAME"
				);
			AddColumnName(DataTypeEmployee.Patronymic,
				"SECOND_NAME",
				"SECOND NAME",
				"Patronymic",
				"Отчество"
				);
			AddColumnName(DataTypeEmployee.Sex,
				"Sex",
				"Gender",
				"Пол"
				);
			AddColumnName(DataTypeEmployee.PersonnelNumber,
				"TN",
				"Табельный"
				);
			AddColumnName(DataTypeEmployee.HireDate,
				"Дата приема",
				"Дата приёма"
				);
			AddColumnName(DataTypeEmployee.DismissDate,
				"Дата увольнения"
			);
			AddColumnName(DataTypeEmployee.Subdivision,
				"Подразделение"
				);
			AddColumnName(DataTypeEmployee.Post,
				"Должность"
				);

			this.subdivisionRepository = subdivisionRepository;
			this.postRepository = postRepository;
			this.userService = userService;
		}

		private void AddColumnName(DataTypeEmployee type, params string[] names)
		{
			foreach(var name in names)
				ColumnNames.Add(name.ToLower(), type);
		}

		#region Обработка изменений
		public void FindChanges(IUnitOfWork uow, IEnumerable<SheetRowEmployee> list, ImportedColumn<DataTypeEmployee>[] meaningfulColumns, IProgressBarDisplayable progress, SettingsMatchEmployeesViewModel settings)
		{
			progress.Start(list.Count(), text: "Поиск изменений");
			foreach(var row in list) {
				progress.Add();
				if(row.Skiped)
					continue;

				var employee = row.Employees.FirstOrDefault();
				var rowChange = ChangeType.ChangeValue;

				if(employee == null) {
					employee = new EmployeeCard();
					employee.Comment = "Импортирован из Excel";
					employee.CreatedbyUser = userService?.GetCurrentUser(uow);
					row.Employees.Add(employee);
					rowChange = ChangeType.NewEntity;
				}

				foreach(var column in meaningfulColumns) {
					row.ChangedColumns.Add(column, MakeChange(settings, employee, row, column, rowChange));
				}
			}
			progress.Close();
		}

		public ChangeType MakeChange(SettingsMatchEmployeesViewModel settings, EmployeeCard employee, SheetRowEmployee row, ImportedColumn<DataTypeEmployee> column, ChangeType rowChange)
		{
			string value = row.CellValue(column.Index);
			var dataType = column.DataType;
			if(String.IsNullOrWhiteSpace(value))
				return ChangeType.NotChanged;

			switch(dataType) {
				case DataTypeEmployee.CardKey:
					return String.Equals(employee.CardKey, value, StringComparison.InvariantCultureIgnoreCase) ? ChangeType.NotChanged : rowChange;
				case DataTypeEmployee.PersonnelNumber:
					return String.Equals(employee.PersonnelNumber, settings.ConvertPersonnelNumber ? ConvertPersonnelNumber(value) : value, StringComparison.InvariantCultureIgnoreCase) ? ChangeType.NotChanged : rowChange;
				case DataTypeEmployee.LastName:
					return String.Equals(employee.LastName, value, StringComparison.CurrentCultureIgnoreCase) ? ChangeType.NotChanged : rowChange;
				case DataTypeEmployee.FirstName:
					return String.Equals(employee.FirstName, value, StringComparison.CurrentCultureIgnoreCase) ? ChangeType.NotChanged : rowChange;
				case DataTypeEmployee.Patronymic:
					return String.Equals(employee.Patronymic, value, StringComparison.CurrentCultureIgnoreCase) ? ChangeType.NotChanged : rowChange;
				case DataTypeEmployee.Sex:
					//Первая М английская, вторая русская.
					if(value.StartsWith("M", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("М", StringComparison.CurrentCultureIgnoreCase))
						return employee.Sex == Sex.M ? ChangeType.NotChanged : rowChange;
					if(value.StartsWith("F", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("Ж", StringComparison.CurrentCultureIgnoreCase))
						return employee.Sex == Sex.F ? ChangeType.NotChanged : rowChange;
					return ChangeType.ParseError;
				case DataTypeEmployee.Fio:
					value.SplitFullName(out string lastName, out string firstName, out string patronymic);
					bool lastDiff = !String.IsNullOrEmpty(lastName) && !String.Equals(employee.LastName, lastName, StringComparison.CurrentCultureIgnoreCase);
					bool firstDiff = !String.IsNullOrEmpty(firstName) && !String.Equals(employee.FirstName, firstName, StringComparison.CurrentCultureIgnoreCase);
					bool patronymicDiff = !String.IsNullOrEmpty(patronymic) && !String.Equals(employee.Patronymic, patronymic, StringComparison.CurrentCultureIgnoreCase);
					return (lastDiff || firstDiff || patronymicDiff) ? rowChange : ChangeType.NotChanged;
				case DataTypeEmployee.HireDate:
					return employee.HireDate != row.CellDateTimeValue(column.Index) ? rowChange : ChangeType.NotChanged;
				case DataTypeEmployee.DismissDate:
					return employee.DismissDate != row.CellDateTimeValue(column.Index) ? rowChange : ChangeType.NotChanged;
				case DataTypeEmployee.Subdivision:
					if(String.Equals(employee.Subdivision?.Name, value, StringComparison.CurrentCultureIgnoreCase))
						return ChangeType.NotChanged;

					Subdivision subdivision = UsedSubdivisions.FirstOrDefault(x =>
						String.Equals(x.Name, value, StringComparison.CurrentCultureIgnoreCase));
					if(subdivision == null) {
						subdivision = new Subdivision { Name = value };
						UsedSubdivisions.Add(subdivision);
					}
					employee.Subdivision = subdivision;
					return subdivision.Id == 0 ? ChangeType.NewEntity : rowChange;
				case DataTypeEmployee.Post:
					if(String.Equals(employee.Post?.Name, value, StringComparison.CurrentCultureIgnoreCase))
						return ChangeType.NotChanged;
					Post post = UsedPosts.FirstOrDefault(x =>
						String.Equals(x.Name, value, StringComparison.CurrentCultureIgnoreCase)
						&& DomainHelper.EqualDomainObjects(x.Subdivision, employee.Subdivision));
					if(post == null) {
						post = new Post { 
							Name = value, 
							Subdivision = employee.Subdivision,
							Comments = "Создана при импорте сотрудников из Excel"
						};
						UsedPosts.Add(post);
					}
					employee.Post = post;
					return post.Id == 0 ? ChangeType.NewEntity : rowChange;
				default:
					throw new NotSupportedException($"Тип данных {dataType} не подерживатся.");
			}
		}
		#endregion

		#region Сопоставление
		public void MatchByName(IUnitOfWork uow, IEnumerable<SheetRowEmployee> list, List<ImportedColumn<DataTypeEmployee>> columns, IProgressBarDisplayable progress)
		{
			progress.Start(2, text: "Сопоставление с существующими сотрудниками");
			var searchValues = list.Select(x => GetFIO(x, columns))
				.Where(fio => !String.IsNullOrEmpty(fio.LastName) && !String.IsNullOrEmpty(fio.FirstName))
				.Select(fio => (fio.LastName + "|" + fio.FirstName).ToUpper())
				.Distinct().ToArray();
			
			Console.WriteLine((uow.Session.SessionFactory as ISessionFactoryImplementor).Dialect);
			var exists = uow.Session.QueryOver<EmployeeCard>()
				.Where(Restrictions.In(
				Projections.SqlFunction(
							  "upper", NHibernateUtil.String,
							  (uow.Session.SessionFactory as ISessionFactoryImplementor).Dialect is SQLiteDialect //Данный диалект используется в тестах.
								  ? 
								  Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.String, "( ?1 || '|' || ?2)"),
									  NHibernateUtil.String,
									  Projections.Property<EmployeeCard>(x => x.LastName),
									  Projections.Property<EmployeeCard>(x => x.FirstName)
								  )
							: Projections.SqlFunction(new StandardSQLFunction("CONCAT_WS"),
							  	NHibernateUtil.String,
							  	Projections.Constant(""),
								Projections.Property<EmployeeCard>(x => x.LastName),
								Projections.Constant("|"),
								Projections.Property<EmployeeCard>(x => x.FirstName)
							   )),
						   searchValues))
				.List();

			progress.Add();
			foreach(var employee in exists) {
				var found = list.Where(x => СompareFio(x, employee, columns)).ToArray();
				if(!found.Any())
					continue; //Так как из базе ищем без отчества, могуть быть лишние.
				found.First().Employees.Add(employee);
			}

			progress.Add();
			//Пропускаем дубликаты имен в файле
			var groups = list.GroupBy(x => GetFIO(x, columns).GetHash());
			foreach(var group in groups) {
				if(String.IsNullOrWhiteSpace(group.Key)) {
					group.First().Skiped = true;
				}

				foreach(var item in group.Skip(1)) {
					item.Skiped = true;
				}
			}
			progress.Close();
		}

		private bool СompareFio(SheetRowEmployee x, EmployeeCard employee, List<ImportedColumn<DataTypeEmployee>> columns)
		{
			var fio = GetFIO(x, columns);
			return String.Equals(fio.LastName, employee.LastName, StringComparison.CurrentCultureIgnoreCase)
				&& String.Equals(fio.FirstName, employee.FirstName, StringComparison.CurrentCultureIgnoreCase)
				&& (fio.Patronymic == null || String.Equals(fio.Patronymic, employee.Patronymic, StringComparison.CurrentCultureIgnoreCase));
		}

		public void MatchByNumber(IUnitOfWork uow, IEnumerable<SheetRowEmployee> list, List<ImportedColumn<DataTypeEmployee>> columns, SettingsMatchEmployeesViewModel settings, IProgressBarDisplayable progress)
		{
			progress.Start(2, text: "Сопоставление с существующими сотрудниками");
			var numberColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.PersonnelNumber);
			var numbers = list.Select(x => settings.ConvertPersonnelNumber ? ConvertPersonnelNumber(x.CellValue(numberColumn.Index)) : x.CellValue(numberColumn.Index))
							.Where(x => !String.IsNullOrWhiteSpace(x))
							.Distinct().ToArray();
			var exists = uow.Session.QueryOver<EmployeeCard>()
				.Where(x => x.PersonnelNumber.IsIn(numbers))
				.List();

			progress.Add();
			foreach(var employee in exists) {
				var found = list.Where(x => (settings.ConvertPersonnelNumber ? ConvertPersonnelNumber(x.CellValue(numberColumn.Index)) : x.CellValue(numberColumn.Index)) == employee.PersonnelNumber).ToArray();
				found.First().Employees.Add(employee);
			}

			//Пропускаем дубликаты Табельных номеров в файле
			progress.Add();
			var groups = list.GroupBy(x => settings.ConvertPersonnelNumber ? ConvertPersonnelNumber(x.CellValue(numberColumn.Index)) : x.CellValue(numberColumn.Index));
			foreach(var group in groups) {
				if(String.IsNullOrWhiteSpace(group.Key)) {
					//Если табельного номера нет проверяем по FIO
					MatchByName(uow, group, columns, progress);
				}

				foreach(var item in group.Skip(1)) {
					item.Skiped = true;
				}
			}
			progress.Close();
		}
		#endregion

		#region Создание объектов

		public readonly List<Subdivision> UsedSubdivisions = new List<Subdivision>();
		public readonly List<Post> UsedPosts = new List<Post>();

		public void FillExistEntities(IUnitOfWork uow, IEnumerable<SheetRowEmployee> list, List<ImportedColumn<DataTypeEmployee>> columns, IProgressBarDisplayable progress)
		{
			progress.Start(2, text: "Загружаем должности и подразделения");
			var subdivisionColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.Subdivision);
			if(subdivisionColumn != null) {
				var subdivisionNames = list.Select(x => x.CellValue(subdivisionColumn.Index)).Distinct().ToArray();
				UsedSubdivisions.AddRange(uow.Session.QueryOver<Subdivision>()
					.Where(x => x.Name.IsIn(subdivisionNames))
					.List());
			}
			progress.Add();
			var postColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.Post);
			if(postColumn != null) {
				var postNames = list.Select(x => x.CellValue(postColumn.Index)).Distinct().ToArray();
				UsedPosts.AddRange( uow.Session.QueryOver<Post>()
					.Where(x => x.Name.IsIn(postNames))
					.List());
			}
			progress.Close();
		}

		#endregion
		#region Сохранение данных

		public IEnumerable<object> PrepareToSave(IUnitOfWork uow, SettingsMatchEmployeesViewModel settings, SheetRowEmployee row)
		{
			var employee = row.Employees.FirstOrDefault() ?? new EmployeeCard();
			//Здесь колонки сортируются чтобы процесс обработки данных был в порядке следования описания типов в Enum
			//Это надо для того чтобы наличие 2 полей с похожими данными заполнялись правильно. Например чтобы отдельное поле с фамилией могло перезаписать значение фамилии поученой из общего поля ФИО.
			foreach(var column in row.ChangedColumns.Keys.OrderBy(x => x.DataType)) {
				SetValue(settings, uow, employee, row, column);
			}
			yield return employee;
		}

		private void SetValue(SettingsMatchEmployeesViewModel settings, IUnitOfWork uow, EmployeeCard employee, SheetRowEmployee row, ImportedColumn<DataTypeEmployee> column)
		{
			string value = row.CellValue(column.Index);
			var dataType = column.DataType;
			if(String.IsNullOrWhiteSpace(value))
				return;

			switch(dataType) {
				case DataTypeEmployee.CardKey:
					employee.CardKey = value;
					break;
				case DataTypeEmployee.PersonnelNumber:
					employee.PersonnelNumber = settings.ConvertPersonnelNumber ? ConvertPersonnelNumber(value) : value;
					break;
				case DataTypeEmployee.LastName:
					employee.LastName = value;
					break;
				case DataTypeEmployee.FirstName:
					employee.FirstName = value;
					if(employee.Sex == Sex.None) {
						if(MaleNames.Contains(employee.FirstName.ToUpper()))
							employee.Sex = Sex.M;
						if(FemaleNames.Contains(employee.FirstName.ToUpper()))
							employee.Sex = Sex.F;
					}
					break;
				case DataTypeEmployee.Patronymic:
					employee.Patronymic = value;
					break;
				case DataTypeEmployee.Sex:
					//Первая М английская, вторая русская.
					if(value.StartsWith("M", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("М", StringComparison.CurrentCultureIgnoreCase))
						employee.Sex = Sex.M;
					if(value.StartsWith("F", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("Ж", StringComparison.CurrentCultureIgnoreCase))
						employee.Sex = Sex.F;
					break;
				case DataTypeEmployee.Fio:
					value.SplitFullName(out string lastName, out string firstName, out string patronymic);
					if(!String.IsNullOrEmpty(lastName) && !String.Equals(employee.LastName, value, StringComparison.CurrentCultureIgnoreCase))
						employee.LastName = lastName;
					if(!String.IsNullOrEmpty(firstName) && !String.Equals(employee.FirstName, value, StringComparison.CurrentCultureIgnoreCase))
						employee.FirstName = firstName;
					if(!String.IsNullOrEmpty(patronymic) && !String.Equals(employee.Patronymic, value, StringComparison.CurrentCultureIgnoreCase))
						employee.Patronymic = patronymic;
					if(employee.Sex == Sex.None && !String.IsNullOrWhiteSpace(employee.FirstName)) {
						if(MaleNames.Contains(employee.FirstName.ToUpper()))
							employee.Sex = Sex.M;
						if(FemaleNames.Contains(employee.FirstName.ToUpper()))
							employee.Sex = Sex.F;
					}
					break;
				case DataTypeEmployee.HireDate:
					var hireDate = row.CellDateTimeValue(column.Index);
					if(hireDate != null)
					 	employee.HireDate = hireDate;
					break;
				case DataTypeEmployee.DismissDate:
					var dismissDate = row.CellDateTimeValue(column.Index);
					if(dismissDate != null)
						employee.DismissDate = dismissDate;
					break;

				case DataTypeEmployee.Subdivision:
				case DataTypeEmployee.Post:
					//Устанавливаем в MakeChange;
					break;
				default:
					throw new NotSupportedException($"Тип данных {dataType} не подерживатся.");
			}
		}
		#endregion

		#region Helpers

		public FIO GetFIO(SheetRowEmployee row, List<ImportedColumn<DataTypeEmployee>> columns)
		{
			var fio = new FIO();
			var lastnameColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.LastName);
			var firstNameColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.FirstName);
			var patronymicColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.Patronymic);
			var fioColumn = columns.FirstOrDefault(x => x.DataType == DataTypeEmployee.Fio);
			if(fioColumn != null)
				row.CellValue(fioColumn.Index)?.SplitFullName(out fio.LastName, out fio.FirstName, out fio.Patronymic);
			if(lastnameColumn != null)
				fio.LastName = row.CellValue(lastnameColumn.Index);
			if(firstNameColumn != null)
				fio.FirstName = row.CellValue(firstNameColumn.Index);
			if(patronymicColumn != null)
				fio.Patronymic = row.CellValue(patronymicColumn.Index);
			return fio;
		}

		public string ConvertPersonnelNumber (string cellValue)
		{
			if(int.TryParse(cellValue, out int number))
				return number.ToString();
			else
				return cellValue;
		}
		#endregion
	}
}
