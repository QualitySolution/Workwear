using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using workwear.Domain.Company;
using workwear.Repository.Company;
using Workwear.Domain.Company;

namespace workwear.Models.Import
{
	public class DataParserEmployee : DataParserBase<DataTypeEmployee>
	{
		private readonly SubdivisionRepository subdivisionRepository;
		private readonly PostRepository postRepository;



		public HashSet<string> MaleNames = new HashSet<string> { "АБРАМ", "АВАЗ", "АВВАКУМ", "АВГУСТ", "АВДЕЙ", "АВРААМ", "АВТАНДИЛ", "АГАП", "АГАФОН", "АГГЕЙ", "АДАМ", "АДИС", "АДОЛЬФ", "АДРИАН", "АЗАМАТ", "АЗАРИЙ", "АЗАТ", "АЙДАР", "АЙНУР", "АЙРАТ", "АКАКИЙ", "АКИМ", "АЛАН", "АЛЕКСАНДР", "АЛЕКСЕЙ", "АЛИ", "АЛИХАН", "АЛМАЗ", "АЛЬБЕРТ", "АЛЬФРЕД", "АМАДЕЙ", "АМАДЕУС", "АМАЯК", "АМИН", "АМВРОСИЙ", "АНАТОЛИЙ", "АНВАР", "АНГЕЛ", "АНДРЕЙ", "АНДРЭ", "АНИКИТА", "АНТОН", "АНУФРИЙ", "АНФИМ", "АПОЛЛИНАРИЙ", "АРАМ", "АРИСТАРХ", "АРКАДИЙ", "АРМАН", "АРМЕН", "АРНО", "АРНОЛЬД", "АРОН", "АРСЕН", "АРСЕНИЙ", "АРСЛАН", "АРТЕМ", "АРТЕМИЙ", "АРТУР", "АРХИП", "АСКОЛЬД", "АФАНАСИЙ", "АХМЕТ", "АШОТ", "БАХРАМ", "БЕЖЕН", "БЕНЕДИКТ", "БЕРЕК", "БЕРНАР", "БОГДАН", "БОГОЛЮБ", "БОНИФАЦИЙ", "БОРЕСЛАВ", "БОРИС", "БОРИСЛАВ", "БОЯН", "БРОНИСЛАВ", "БРУНО", "БУЛАТ", "ВАДИМ", "ВАЛЕНТИН", "ВАЛЕРИЙ", "ВАЛЬДЕМАР", "ВАЛЬТЕР", "ВАРДАН", "ВАРЛААМ", "ВАРФОЛОМЕЙ", "ВАСИЛИЙ", "ВАТСЛАВ", "ВЕЛИЗАР", "ВЕЛОР", "ВЕНЕДИКТ", "ВЕНИАМИН", "ВИКЕНТИЙ", "ВИКТОР", "ВИЛЕН", "ВИЛЛИ", "ВИЛЬГЕЛЬМ", "ВИССАРИОН", "ВИТАЛИЙ", "ВИТАУТАС", "ВИТОЛЬД", "ВЛАДИМИР", "ВЛАДИСЛАВ", "ВЛАДЛЕН", "ВЛАС", "ВОЛОДАР", "ВСЕВОЛОД", "ВЯЧЕСЛАВ", "ГАВРИИЛ", "ГАЛАКТИОН", "ГАМЛЕТ", "ГАРРИ", "ГАЯС", "ГЕВОР", "ГЕВОРГ", "ГЕННАДИЙ", "ГЕНРИ", "ГЕНРИХ", "ГЕОРГИЙ", "ГЕРАЛЬД", "ГЕРАСИМ", "ГЕРМАН", "ГЛЕБ", "ГОАР", "ГОРДЕЙ", "ГОРДОН", "ГОРИСЛАВ", "ГРАДИМИР", "ГРИГОРИЙ", "ГУРИЙ", "ГУСТАВ", "ДАВИД", "ДАВЛАТ", "ДАМИР", "ДАНИИЛ", "ДАНИСЛАВ", "ДАНЬЯР", "ДЕМИД", "ДЕМЬЯН", "ДЕНИС", "ДЖАМАЛ", "ДЖЕЙМС", "ДЖЕРЕМИ", "ДЖОЗЕФ", "ДЖОРДАН", "ДЖОРЖ", "ДИК", "ДИНАР", "ДИНАСИЙ", "ДМИТРИЙ", "ДОБРЫНЯ", "ДОНАЛЬД", "ДОНАТ", "ДОНАТОС", "ДОРОФЕЙ", "ЕВГЕНИЙ", "ЕВГРАФ", "ЕВДОКИМ", "ЕВСЕЙ", "ЕВСТАФИЙ", "ЕГОР", "ЕЛИЗАР", "ЕЛИСЕЙ", "ЕМЕЛЬЯН", "ЕРЕМЕЙ", "ЕРМОЛАЙ", "ЕРОФЕЙ", "ЕФИМ", "ЕФРЕМ", "ЖАН", "ЖДАН", "ЖЕРАР", "ЗАКИР", "ЗАМИР", "ЗАУР", "ЗАХАР", "ЗЕНОН", "ЗИГМУНД", "ЗИНОВИЙ", "ЗУРАБ", "ИБРАГИМ", "ИВАН", "ИГНАТ", "ИГНАТИЙ", "ИГОРЬ", "ИЕРОНИМ", "ИЗМАИЛ", "ИЗРАИЛЬ", "ИЛИАН", "ИЛЛАРИОН", "ИЛЬХАМ", "ИЛЬШАТ", "ИЛЬЯ", "ИЛЬЯС", "ИНОКЕНТИЙ", "ИОАНН", "ИОАКИМ", "ИОН", "ИОСИФ", "ИППОЛИТ", "ИРАКЛИЙ", "ИСА", "ИСААК", "ИСИДОР", "ИСКАНДЕР", "ИСЛАМ", "ИСМАИЛ", "КАЗБЕК", "КАЗИМИР", "КАМИЛЬ", "КАРЕН", "КАРИМ", "КАРЛ", "КИМ", "КИР", "КИРИЛЛ", "КЛАВДИЙ", "КЛАУС", "КЛИМ", "КЛИМЕНТ", "КЛОД", "КОНДРАТ", "КОНСТАНТИН", "КОРНЕЙ", "КОРНИЛИЙ", "КУЗЬМА", "ЛАВР", "ЛАВРЕНТИЙ", "ЛАЗАРЬ", "ЛЕВ", "ЛЕВАН", "ЛЕВОН", "ЛЕНАР", "ЛЕОН", "ЛЕОНАРД", "ЛЕОНИД", "ЛЕОНТИЙ", "ЛЕОПОЛЬД", "ЛУКА", "ЛУКЬЯН", "ЛЮБИМ", "ЛЮБОМИР", "ЛЮДВИГ", "ЛЮСЬЕН", "ЛЮЦИЙ", "МАВЛЮДА", "МАДЛЕН", "МАЙ", "МАЙКЛ", "МАКАР", "МАКАРИЙ", "МАКСИМ", "МАКСИМИЛЬЯН", "МАКСУД", "МАНСУР", "МАНУИЛ", "МАР", "МАРАТ", "МАРИАН", "МАРК", "МАРСЕЛЬ", "МАРТИН", "МАТВЕЙ", "МАХМУД", "МЕРАБ", "МЕФОДИЙ", "МЕЧЕСЛАВ", "МИКУЛА", "МИЛАН", "МИРОН", "МИРОСЛАВ", "МИТРОФАН", "МИХАИЛ", "МИШЛОВ", "МОДЕСТ", "МОИСЕЙ", "МСТИСЛАВ", "МУРАТ", "МУСЛИМ", "МУХАММЕД", "НАЗАР", "НАЗАРИЙ", "НАИЛЬ", "НАТАН", "НАУМ", "НЕСТОР", "НИКАНОР", "НИКИТА", "НИКИФОР", "НИКОДИМ", "НИКОЛА", "НИКОЛАЙ", "НИКОН", "НИЛЬС", "НИСОН", "НИФОНТ", "НОРМАНН", "ОВИДИЙ", "ОЛАН", "ОЛЕГ", "ОЛЕСЬ", "ОНИСИМ", "ОРЕСТ", "ОРЛАНДО", "ОСИП", "ОСКАР", "ОСТАП", "ПАВЕЛ", "ПАНКРАТ", "ПАРАМОН", "ПЕТР", "ПЛАТОН", "ПОРФИРИЙ", "ПОТАП", "ПРОКОФИЙ", "ПРОХОР", "РАВИЛЬ", "РАДИЙ", "РАДИК", "РАДОМИР", "РАДОСЛАВ", "РАЗИЛЬ", "РАЙАН", "РАЙМОНД", "РАИС", "РАМАЗАН", "РАМИЗ", "РАМИЛЬ", "РАМОН", "РАНЕЛЬ", "РАСИМ", "РАСУЛ", "РАТИБОР", "РАТМИР", "РАФАИЛ", "РАФАЭЛЬ", "РАФИК", "РАШИД", "РЕМ", "РИНАТ", "РИФАТ", "РИХАРД", "РИЧАРД", "РОБЕРТ", "РОДИОН", "РОЛАН", "РОМАН", "РОСТИСЛАВ", "РУБЕН", "РУДОЛЬФ", "РУСЛАН", "РУСТАМ", "РУФИН", "РУШАН", "РЭЙ", "САБИР", "САВВА", "САВЕЛИЙ", "САМВЕЛ", "САМСОН", "САМУИЛ", "СВЯТОСЛАВ", "СЕВАСТЬЯН", "СЕВЕРИН", "СЕМЕН", "СЕРАФИМ", "СЕРГЕЙ", "СИДОР", "СОКРАТ", "СОЛОМОН", "СПАРТАК", "СПИРИДОН", "СТАКРАТ", "СТАНИСЛАВ", "СТЕПАН", "СТЕФАН", "СТИВЕН", "СТОЯН", "СУЛТАН", "ТАГИР", "ТАИС", "ТАЙЛЕР", "ТАЛИК", "ТАМАЗ", "ТАМЕРЛАН", "ТАРАС", "ТЕЛЬМАН", "ТЕОДОР", "ТЕРЕНТИЙ", "ТИБОР", "ТИГРАМ", "ТИГРАН", "ТИГРИЙ", "ТИМОФЕЙ", "ТИМУР", "ТИТ", "ТИХОН", "ТОМАС", "ТРИФОН", "ТРОФИМ", "УЛЬМАНАС", "УМАР", "УСТИН", "ФАДЕЙ", "ФАЗИЛЬ", "ФАНИС", "ФАРИД", "ФАРХАД", "ФЕДОР", "ФЕДОТ", "ФЕЛИКС", "ФЕОДОСИЙ", "ФЕРДИНАНД", "ФИДЕЛЬ", "ФИЛИМОН", "ФИЛИПП", "ФЛОРЕНТИЙ", "ФОМА", "ФРАНЦ", "ФРЕД", "ФРИДРИХ", "ФУАД", "ХАБИБ", "ХАКИМ", "ХАРИТОН", "ХРИСТИАН", "ХРИСТОС", "ХРИСТОФОР", "ЦЕЗАРЬ", "ЧАРЛЬЗ", "ЧЕСЛАВ", "ЧИНГИЗ", "ШАМИЛЬ", "ШАРЛЬ", "ЭДВАРД", "ЭДГАР", "ЭДМУНД", "ЭДУАРД", "ЭЛЬДАР", "ЭМИЛЬ", "ЭМИН", "ЭММАНУИЛ", "ЭРАСТ", "ЭРИК", "ЭРНЕСТ", "ЮЛИАН", "ЮЛИЙ", "ЮНУС", "ЮРИЙ", "ЮХИМ", "ЯКОВ", "ЯН", "ЯНУАРИЙ", "ЯРОСЛАВ", "ЯСОН" };

		public HashSet<string> FemaleNames = new HashSet<string> { "АВГУСТА", "АВДОТЬЯ", "АВРОРА", "АГАТА", "АГАПИЯ", "АГАФЬЯ", "АГЛАЯ", "АГНЕССА", "АГНИЯ", "АГРИППИНА", "АГУНДА", "АДА", "АДЕЛИНА", "АДЕЛАИДА", "АДЕЛЬ", "АДИЛЯ", "АДРИАНА", "АЗА", "АЗАЛИЯ", "АЗИЗА", "АЙГУЛЬ", "АЙЛИН", "АЙНАГУЛЬ", "АИДА", "АЙЖАН", "АКСИНЬЯ", "АКУЛИНА", "АЛАНА", "АЛЕВТИНА", "АЛЕКСАНДРА", "АЛЕНА", "АЛИКО", "АЛИНА", "АЛИСА", "АЛИЯ", "АЛЛА", "АЛСУ", "АЛЬБА", "АЛЬБЕРТА", "АЛЬБИНА", "АЛЬВИНА", "АЛЬФИЯ", "АЛЬФРЕДА", "АЛЯ", "АМАЛЬ", "АМЕЛИЯ", "АМИНА", "АМИРА", "АНАИТ", "АНАСТАСИЯ", "АНГЕЛИНА", "АНЕЛЯ", "АНЖЕЛА", "АНЖЕЛИКА", "АНИСЬЯ", "АНИТА", "АННА", "АНТОНИНА", "АНФИСА", "АПОЛЛИНАРИЯ", "АРАБЕЛЛА", "АРИАДНА", "АРИАНА", "АРИНА", "АРХЕЛИЯ", "АСЕЛЬ", "АСИЯ", "АССОЛЬ", "АСТРА", "АСТРИД", "АСЯ", "АУРЕЛИЯ", "АФАНАСИЯ", "АЭЛИТА", "БЕАТРИСА", "БЕЛИНДА", "БЕЛЛА", "БЕРТА", "БИРУТА", "БОГДАНА", "БОЖЕНА", "БОРИСЛАВА", "БРОНИСЛАВА", "ВАЛЕНТИНА", "ВАЛЕРИЯ", "ВАНДА", "ВАНЕССА", "ВАРВАРА", "ВАСИЛИНА", "ВАСИЛИСА", "ВЕНЕРА", "ВЕРА", "ВЕРОНИКА", "ВЕСЕЛИНА", "ВЕСНА", "ВЕСТА", "ВЕТА", "ВИДА", "ВИКТОРИНА", "ВИКТОРИЯ", "ВИЛЕНА", "ВИЛОРА", "ВИОЛЕТТА", "ВИРГИНИЯ", "ВИРИНЕЯ", "ВИТА", "ВИТАЛИНА", "ВЛАДА", "ВЛАДИСЛАВА", "ВЛАДЛЕНА", "ГАБРИЭЛЛА", "ГАЛИНА", "ГАЛИЯ", "ГАЯНЭ", "ГЕЛЕНА", "ГАЯНЭ", "ГЕЛЕНА", "ГЕЛЛА", "ГЕНРИЕТТА", "ГЕОРГИНА", "ГЕРА", "ГЕРТРУДА", "ГЛАФИРА", "ГЛАША", "ГЛОРИЯ", "ГРАЖИНА", "ГРЕТА", "ГУЗЕЛЬ", "ГУЛИЯ", "ГУЛЬМИРА", "ГУЛЬНАЗ", "ГУЛЬНАРА", "ГУЛЬШАТ", "ДАЙНА", "ДАЛИЯ", "ДАМИРА", "ДАНА", "ДАНИЭЛА", "ДАНУТА", "ДАРА", "ДАРИНА", "ДАРЬЯ", "ДАЯНА", "ДЕБОРА", "ДЖАМИЛЯ", "ДЖЕММА", "ДЖЕННИФЕР", "ДЖЕССИКА", "ДЖУЛИЯ", "ДЖУЛЬЕТТА", "ДИАНА", "ДИЛАРА", "ДИЛЬНАЗ", "ДИЛЬНАРА", "ДИЛЯ", "ДИНА", "ДИНАРА", "ДИОДОРА", "ДИОНИСИЯ", "ДОЛОРЕС", "ДОЛЯ", "ДОМИНИКА", "ДОРА", "ЕВА", "ЕВАНГЕЛИНА", "ЕВГЕНИЯ", "ЕВДОКИЯ", "ЕКАТЕРИНА", "ЕЛЕНА", "ЕЛИЗАВЕТА", "ЕСЕНИЯ", "ЕФИМИЯ", "ЖАННА", "ЖАСМИН", "ЖОЗЕФИНА", "ЗАБАВА", "ЗАИРА", "ЗАМИРА", "ЗАРА", "ЗАРЕМА", "ЗАРИНА", "ЗАХАРИЯ", "ЗЕМФИРА", "ЗИНАИДА", "ЗИТА", "ЗЛАТА", "ЗОРЯНА", "ЗОЯ", "ЗУЛЬФИЯ", "ЗУХРА", "ИВАННА", "ИВЕТТА", "ИВОНА", "ИДА", "ИЗАБЕЛЛА", "ИЗОЛЬДА", "ИЛАРИЯ", "ИЛИАНА", "ИЛОНА", "ИНАРА", "ИНГА", "ИНГЕБОРГА", "ИНДИРА", "ИНЕССА", "ИННА", "ИОАННА", "ИОЛАНТА", "ИРАИДА", "ИРИНА", "ИРМА", "ИСКРА", "ИЯ", "КАЛЕРИЯ", "КАМИЛЛА", "КАПИТОЛИНА", "КАРИМА", "КАРИНА", "КАРОЛИНА", "КАТАРИНА", "КИРА", "КЛАВДИЯ", "КЛАРА", "КЛАРИССА", "КЛИМЕНТИНА", "КОНСТАНЦИЯ", "КОРА", "КОРНЕЛИЯ", "КРИСТИНА", "КСЕНИЯ", "ЛАДА", "ЛАЙМА", "ЛАНА", "ЛАРА", "ЛАРИСА", "ЛАУРА", "ЛЕЙЛА", "ЛЕЙСАН", "ЛЕОКАДИЯ", "ЛЕОНИДА", "ЛЕРА", "ЛЕСЯ", "ЛИАНА", "ЛИДИЯ", "ЛИЗА", "ЛИКА", "ЛИЛИАНА", "ЛИЛИЯ", "ЛИНА", "ЛИНДА", "ЛИОРА", "ЛИРА", "ЛИЯ", "ЛОЛА", "ЛОЛИТА", "ЛОРА", "ЛУИЗА", "ЛУКЕРЬЯ", "ЛЮБОВЬ", "ЛЮДМИЛА", "ЛЯЛЯ", "ЛЮЦИЯ", "МАГДА", "МАГДАЛИНА", "МАДИНА", "МАЙЯ", "МАЛИКА", "МАЛЬВИНА", "МАРА", "МАРГАРИТА", "МАРИАННА", "МАРИКА", "МАРИНА", "МАРИЯ", "МАРСЕЛИНА", "МАРТА", "МАРУСЯ", "МАРФА", "МАРЬЯМ", "МАТИЛЬДА", "МЕЛАНИЯ", "МЕЛИССА", "МИКА", "МИЛА", "МИЛАДА", "МИЛАНА", "МИЛЕНА", "МИЛИЦА", "МИЛОЛИКА", "МИЛОСЛАВА", "МИРА", "МИРОСЛАВА", "МИРРА", "МОНИКА", "МУЗА", "МЭРИ", "НАДЕЖДА", "НАЗИРА", "НАИЛЯ", "НАИМА", "НАНА", "НАОМИ", "НАТАЛЬЯ", "НАТЕЛЛА", "НЕЛЛИ", "НЕОНИЛА", "НИКА", "НИКОЛЬ", "НИНА", "НИНЕЛЬ", "НОННА", "НОРА", "НУРИЯ", "ОДЕТТА", "ОКСАНА", "ОКТЯБРИНА", "ОЛЕСЯ", "ОЛИВИЯ", "ОЛЬГА", "ОФЕЛИЯ", "ПАВЛА", "ПАВЛИНА", "ПАМЕЛА", "ПАТРИЦИЯ", "ПЕЛАГЕЯ", "ПЕРИЗАТ", "ПОЛИНА", "ПРАСКОВЬЯ", "РАДА", "РАДМИЛА", "РАИСА", "РЕВЕККА", "РЕГИНА", "РЕМА", "РЕНАТА", "РИММА", "РИНА", "РИТА", "РОГНЕДА", "РОБЕРТА", "РОЗА", "РОКСАНА", "РОСТИСЛАВА", "РУЗАЛИЯ", "РУЗАННА", "РУЗИЛЯ", "РУМИЯ", "РУСАЛИНА", "РУСЛАНА", "РУФИНА", "САБИНА", "САБРИНА", "САЖИДА", "САИДА", "САЛОМЕЯ", "САМИРА", "САНДРА", "САНИЯ", "САНТА", "САРА", "САТИ", "СВЕТЛАНА", "СВЯТОСЛАВА", "СЕВАРА", "СЕВЕРИНА", "СЕЛЕНА", "СЕРАФИМА", "СИЛЬВА", "СИМА", "СИМОНА", "СЛАВА", "СНЕЖАНА", "СОНЯ", "СОФИЯ", "СТАНИСЛАВА", "СТЕЛЛА", "СТЕФАНИЯ", "СУСАННА", "ТАИРА", "ТАИСИЯ", "ТАЛА", "ТАМАРА", "ТАМИЛА", "ТАРА", "ТАТЬЯНА", "ТЕРЕЗА", "ТИНА", "ТОРА", "УЛЬЯНА", "УРСУЛА", "УСТИНА", "УСТИНЬЯ", "ФАИЗА", "ФАИНА", "ФАНИЯ", "ФАНЯ", "ФАРИДА", "ФАТИМА", "ФАЯ", "ФЕКЛА", "ФЕЛИЦИЯ", "ФЕРУЗА", "ФИЗУРА", "ФЛОРА", "ФРАНСУАЗА", "ФРИДА", "ХАРИТА", "ХИЛАРИ", "ХИЛЬДА", "ХЛОЯ", "ХРИСТИНА", "ЦВЕТАНА", "ЧЕЛСИ", "ЧЕСЛАВА", "ЧУЛПАН", "ШАКИРА", "ШАРЛОТТА", "ШЕЙЛА", "ШЕЛЛИ", "ШЕРИЛ", "ЭВЕЛИНА", "ЭВИТА", "ЭДДА", "ЭДИТА", "ЭЛЕОНОРА", "ЭЛИАНА", "ЭЛИЗА", "ЭЛИНА", "ЭЛЛА", "ЭЛЛАДА", "ЭЛОИЗА", "ЭЛЬВИНА", "ЭЛЬВИРА", "ЭЛЬГА", "ЭЛЬЗА", "ЭЛЬМИРА", "ЭЛЬНАРА", "ЭЛЯ", "ЭМИЛИЯ", "ЭММА", "ЭМИЛИ", "ЭРИКА", "ЭРНЕСТИНА", "ЭСМЕРАЛЬДА", "ЭТЕЛЬ", "ЭТЕРИ", "ЮЗЕФА", "ЮЛИЯ", "ЮНА", "ЮНИЯ", "ЮНОНА", "ЯДВИГА", "ЯНА", "ЯНИНА", "ЯРИНА", "ЯРОСЛАВА", "ЯСМИНА" };

		public DataParserEmployee(SubdivisionRepository subdivisionRepository = null, PostRepository postRepository = null)
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
			AddColumnName(DataTypeEmployee.Subdivision,
				"Подразделение"
				);
			AddColumnName(DataTypeEmployee.Post,
				"Должность"
				);

			this.subdivisionRepository = subdivisionRepository;
			this.postRepository = postRepository;
		}

		private void AddColumnName(DataTypeEmployee type, params string[] names)
		{
			foreach(var name in names)
				ColumnNames.Add(name.ToLower(), type);
		}

		#region Обработка данных
		public bool IsDiff(EmployeeCard employee, DataTypeEmployee dataType, string value)
		{
			if(String.IsNullOrWhiteSpace(value))
				return false;

			switch(dataType) {
				case DataTypeEmployee.CardKey:
					return !String.Equals(employee.CardKey, value, StringComparison.InvariantCultureIgnoreCase);
				case DataTypeEmployee.PersonnelNumber:
					return !String.Equals(employee.PersonnelNumber, value, StringComparison.InvariantCultureIgnoreCase);
				case DataTypeEmployee.LastName:
					return !String.Equals(employee.LastName, value, StringComparison.CurrentCultureIgnoreCase);
				case DataTypeEmployee.FirstName:
					return !String.Equals(employee.FirstName, value, StringComparison.CurrentCultureIgnoreCase);
				case DataTypeEmployee.Patronymic:
					return !String.Equals(employee.Patronymic, value, StringComparison.CurrentCultureIgnoreCase);
				case DataTypeEmployee.Sex:
					//Первая М английская, вторая русская.
					if(value.StartsWith("M", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("М", StringComparison.CurrentCultureIgnoreCase))
						return employee.Sex != Sex.M;
					if(value.StartsWith("F", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("Ж", StringComparison.CurrentCultureIgnoreCase))
						return employee.Sex != Sex.F;
					return false;
				case DataTypeEmployee.Fio:
					value.SplitFullName(out string lastName, out string firstName, out string patronymic);
					bool lastDiff = !String.IsNullOrEmpty(lastName) && !String.Equals(employee.LastName, lastName, StringComparison.CurrentCultureIgnoreCase);
					bool firstDiff = !String.IsNullOrEmpty(firstName) && !String.Equals(employee.FirstName, firstName, StringComparison.CurrentCultureIgnoreCase);
					bool patronymicDiff = !String.IsNullOrEmpty(patronymic) && !String.Equals(employee.Patronymic, patronymic, StringComparison.CurrentCultureIgnoreCase);
					return lastDiff || firstDiff || patronymicDiff;
				case DataTypeEmployee.HireDate:
					return !String.IsNullOrWhiteSpace(value) && DateTime.TryParse(value, out DateTime date) && employee.HireDate != date;
				case DataTypeEmployee.Subdivision:
					return !String.IsNullOrWhiteSpace(value) && !String.Equals(employee.Subdivision?.Name, value, StringComparison.CurrentCultureIgnoreCase);
				case DataTypeEmployee.Post:
					return !String.IsNullOrWhiteSpace(value) && !String.Equals(employee.Post?.Name, value, StringComparison.CurrentCultureIgnoreCase);
				default:
					throw new NotSupportedException($"Тип данных {dataType} не подерживатся.");
			}
		}
		#endregion
		#region Сохранение данных
		private List<Subdivision> createdSubdivisions = new List<Subdivision>();
		private List<Post> createdPosts = new List<Post>();
		public EmployeeCard PrepareToSave(IUnitOfWork uow, SheetRowEmployee row)
		{
			var employee = row.Employees.FirstOrDefault() ?? new EmployeeCard();
			//Здесь колонки сортируются чтобы процесс обработки данных был в порядке следования описания типов в Enum
			//Это надо для того чтобы наличие 2 полей с похожими данными заполнялись правильно. Например чтобы отдельное поле с фамилией могло перезаписать значение фамилии поученой из общего поля ФИО.
			foreach(var column in row.ChangedColumns.OrderBy(x => x.DataType)) {
				SetValue(uow, employee, column.DataType, row.CellValue(column.Index));
			}
			return employee;
		}

		private void SetValue(IUnitOfWork uow, EmployeeCard employee, DataTypeEmployee dataType, string value)
		{
			if(String.IsNullOrWhiteSpace(value))
				return;

			switch(dataType) {
				case DataTypeEmployee.CardKey:
					employee.CardKey = value;
					break;
				case DataTypeEmployee.PersonnelNumber:
					employee.PersonnelNumber = value;
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
					if(DateTime.TryParse(value, out DateTime date))
					 	employee.HireDate = date;
					break;

				case DataTypeEmployee.Subdivision:
					Subdivision subdivision = createdSubdivisions.FirstOrDefault(x =>
						String.Equals(x.Name, value, StringComparison.CurrentCultureIgnoreCase));
					if(subdivision == null)
						subdivision = subdivisionRepository.GetSubdivisionByName(uow, value);
					if(subdivision == null) {
						subdivision = new Subdivision { Name = value };
						uow.Save(subdivision);
						createdSubdivisions.Add(subdivision);
					}
					employee.Subdivision = subdivision;
					break;
				case DataTypeEmployee.Post:
					Post post = createdPosts.FirstOrDefault(x =>
						String.Equals(x.Name, value, StringComparison.CurrentCultureIgnoreCase)
						&& DomainHelper.EqualDomainObjects(x.Subdivision, employee.Subdivision));
					if(post == null)
						post = postRepository.GetPostByName(uow, value, employee.Subdivision);
					if(post == null) {
						post = new Post { Name = value, Subdivision = employee.Subdivision};
						uow.Save(post);
						createdPosts.Add(post);
					}
					employee.Post = post;
					break;
				default:
					throw new NotSupportedException($"Тип данных {dataType} не подерживатся.");
			}
		}
		#endregion
	}
}
