﻿using System.Collections.Generic;
using Workwear.Domain.Company;

namespace Workwear.Models.Company
{
	public class PersonNames
	{
		//Имена в списке специально без буквы Ё чтобы не дублировать 2 варианта написания.
		//При проверке все имена приводятся к написанию через E
		public HashSet<string> MaleNames = new HashSet<string>
		{
			"АБДУЛАШИМ",
			"АБРАМ",
			"АВАЗ",
			"АВВАКУМ",
			"АВГУСТ",
			"АВДЕЙ",
			"АВРААМ",
			"АВТАНДИЛ",
			"АГАП",
			"АГАФОН",
			"АГГЕЙ",
			"АДАМ",
			"АДИС",
			"АДОЛЬФ",
			"АДРИАН",
			"АЗАМАТ",
			"АЗАРИЙ",
			"АЗАТ",
			"АЙДАР",
			"АЙНУР",
			"АЙРАТ",
			"АКАКИЙ",
			"АКИМ",
			"АЛАН",
			"АЛЕКСАНДР",
			"АЛЕКСЕЙ",
			"АЛИ",
			"АЛИХАН",
			"АЛМАЗ",
			"АЛЬБЕРТ",
			"АЛЬФРЕД",
			"АМАДЕЙ",
			"АМАДЕУС",
			"АМАЯК",
			"АМИН",
			"АМВРОСИЙ",
			"АНАТОЛИЙ",
			"АНВАР",
			"АНГЕЛ",
			"АНДРЕЙ",
			"АНДРЭ",
			"АНИКИТА",
			"АНТОН",
			"АНУФРИЙ",
			"АНФИМ",
			"АПОЛЛИНАРИЙ",
			"АРАМ",
			"АРИСТАРХ",
			"АРКАДИЙ",
			"АРМАН",
			"АРМЕН",
			"АРНО",
			"АРНОЛЬД",
			"АРОН",
			"АРСЕН",
			"АРСЕНИЙ",
			"АРСЛАН",
			"АРТЕМ",
			"АРТЕМИЙ",
			"АРТУР",
			"АРХИП",
			"АСКОЛЬД",
			"АСЛАН",
			"АФАНАСИЙ",
			"АХМЕТ",
			"АШОТ",
			"БАХРАМ",
			"БЕЖЕН",
			"БЕНЕДИКТ",
			"БЕРЕК",
			"БЕРНАР",
			"БОГДАН",
			"БОГОЛЮБ",
			"БОНИФАЦИЙ",
			"БОРЕСЛАВ",
			"БОРИС",
			"БОРИСЛАВ",
			"БОЯН",
			"БРОНИСЛАВ",
			"БРУНО",
			"БУЛАТ",
			"ВАДИМ",
			"ВАЛЕНТИН",
			"ВАЛЕРИЙ",
			"ВАЛЬДЕМАР",
			"ВАЛЬТЕР",
			"ВАРДАН",
			"ВАРЛААМ",
			"ВАРФОЛОМЕЙ",
			"ВАСИЛИЙ",
			"ВАТСЛАВ",
			"ВЕЛИЗАР",
			"ВЕЛОР",
			"ВЕНЕДИКТ",
			"ВЕНИАМИН",
			"ВИКЕНТИЙ",
			"ВИКТОР",
			"ВИЛЕН",
			"ВИЛЛИ",
			"ВИЛЬГЕЛЬМ",
			"ВИССАРИОН",
			"ВИТАЛИЙ",
			"ВИТАУТАС",
			"ВИТОЛЬД",
			"ВЛАДИМИР",
			"ВЛАДИСЛАВ",
			"ВЛАДЛЕН",
			"ВЛАС",
			"ВОЛОДАР",
			"ВСЕВОЛОД",
			"ВЯЧЕСЛАВ",
			"ГАВРИИЛ",
			"ГАЛАКТИОН",
			"ГАМЛЕТ",
			"ГАРРИ",
			"ГАЯС",
			"ГЕВОР",
			"ГЕВОРГ",
			"ГЕННАДИЙ",
			"ГЕНРИ",
			"ГЕНРИХ",
			"ГЕОРГИЙ",
			"ГЕРАЛЬД",
			"ГЕРАСИМ",
			"ГЕРМАН",
			"ГЛЕБ",
			"ГОАР",
			"ГОРДЕЙ",
			"ГОРДОН",
			"ГОРИСЛАВ",
			"ГРАДИМИР",
			"ГРИГОРИЙ",
			"ГУРИЙ",
			"ГУСТАВ",
			"ДАВИД",
			"ДАВЛАТ",
			"ДАМИР",
			"ДАНИИЛ",
			"ДАНИЛ",
			"ДАНИСЛАВ",
			"ДАНЬЯР",
			"ДЕМИД",
			"ДЕМЬЯН",
			"ДЕНИС",
			"ДЖАМАЛ",
			"ДЖАНПУЛАТ",
			"ДЖЕЙМС",
			"ДЖЕРЕМИ",
			"ДЖОЗЕФ",
			"ДЖОРДАН",
			"ДЖОРЖ",
			"ДИК",
			"ДИНАР",
			"ДИНАСИЙ",
			"ДМИТРИЙ",
			"ДОБРЫНЯ",
			"ДОНАЛЬД",
			"ДОНАТ",
			"ДОНАТОС",
			"ДОРОФЕЙ",
			"ЕВГЕНИЙ",
			"ЕВГРАФ",
			"ЕВДОКИМ",
			"ЕВСЕЙ",
			"ЕВСТАФИЙ",
			"ЕГОР",
			"ЕЛИЗАР",
			"ЕЛИСЕЙ",
			"ЕМЕЛЬЯН",
			"ЕРЕМЕЙ",
			"ЕРМОЛАЙ",
			"ЕРОФЕЙ",
			"ЕФИМ",
			"ЕФРЕМ",
			"ЖАН",
			"ЖДАН",
			"ЖЕРАР",
			"ЗАКИР",
			"ЗАМИР",
			"ЗАУР",
			"ЗАХАР",
			"ЗЕНОН",
			"ЗИГМУНД",
			"ЗИНОВИЙ",
			"ЗУРАБ",
			"ИБРАГИМ",
			"ИВАН",
			"ИГНАТ",
			"ИГНАТИЙ",
			"ИГОРЬ",
			"ИЕРОНИМ",
			"ИЗМАИЛ",
			"ИЗРАИЛЬ",
			"ИЛИАН",
			"ИЛЛАРИОН",
			"ИЛЬХАМ",
			"ИЛЬШАТ",
			"ИЛЬЯ",
			"ИЛЬЯС",
			"ИНОКЕНТИЙ",
			"ИОАНН",
			"ИОАКИМ",
			"ИОН",
			"ИОСИФ",
			"ИППОЛИТ",
			"ИРАКЛИЙ",
			"ИРШАТ",
			"ИСА",
			"ИСААК",
			"ИСИДОР",
			"ИСКАНДЕР",
			"ИСЛАМ",
			"ИСМАИЛ",
			"КАЗБЕК",
			"КАЗИМИР",
			"КАМИЛЬ",
			"КАРЕН",
			"КАРИМ",
			"КАРЛ",
			"КИМ",
			"КИР",
			"КИРИЛЛ",
			"КЛАВДИЙ",
			"КЛАУС",
			"КЛИМ",
			"КЛИМЕНТ",
			"КЛОД",
			"КОНДРАТ",
			"КОНСТАНТИН",
			"КОРНЕЙ",
			"КОРНИЛИЙ",
			"КУЗЬМА",
			"ЛАВР",
			"ЛАВРЕНТИЙ",
			"ЛАЗАРЬ",
			"ЛЕВ",
			"ЛЕВАН",
			"ЛЕВОН",
			"ЛЕНАР",
			"ЛЕОН",
			"ЛЕОНАРД",
			"ЛЕОНИД",
			"ЛЕОНТИЙ",
			"ЛЕОПОЛЬД",
			"ЛЕЧА",
			"ЛУКА",
			"ЛУКЬЯН",
			"ЛЮБИМ",
			"ЛЮБОМИР",
			"ЛЮДВИГ",
			"ЛЮСЬЕН",
			"ЛЮЦИЙ",
			"МАВЛЮДА",
			"МАДЛЕН",
			"МАЙ",
			"МАЙКЛ",
			"МАКАР",
			"МАКАРИЙ",
			"МАКСИМ",
			"МАКСИМИЛЬЯН",
			"МАКСУД",
			"МАНСУР",
			"МАНУИЛ",
			"МАР",
			"МАРАТ",
			"МАРИАН",
			"МАРК",
			"МАРСЕЛЬ",
			"МАРТИН",
			"МАТВЕЙ",
			"МАХМУД",
			"МЕРАБ",
			"МЕФОДИЙ",
			"МЕЧЕСЛАВ",
			"МИКУЛА",
			"МИЛАН",
			"МИРОН",
			"МИРОСЛАВ",
			"МИТРОФАН",
			"МИХАИЛ",
			"МИШЛОВ",
			"МОДЕСТ",
			"МОИСЕЙ",
			"МСТИСЛАВ",
			"МУРАТ",
			"МУСЛИМ",
			"МУХАММЕД",
			"НАДИР",
			"НАЗАР",
			"НАЗАРИЙ",
			"НАИЛЬ",
			"НАТАН",
			"НАУМ",
			"НЕСТОР",
			"НИКАНОР",
			"НИКИТА",
			"НИКИФОР",
			"НИКОДИМ",
			"НИКОЛА",
			"НИКОЛАЙ",
			"НИКОН",
			"НИЛЬС",
			"НИСОН",
			"НИФОНТ",
			"НОРМАНН",
			"ОВИДИЙ",
			"ОЛАН",
			"ОЛЕГ",
			"ОЛЕСЬ",
			"ОНИСИМ",
			"ОРЕСТ",
			"ОРЛАНДО",
			"ОСИП",
			"ОСКАР",
			"ОСТАП",
			"ПАВЕЛ",
			"ПАНКРАТ",
			"ПАРАМОН",
			"ПЕТР",
			"ПЛАТОН",
			"ПОРФИРИЙ",
			"ПОТАП",
			"ПРОКОФИЙ",
			"ПРОХОР",
			"РАВИЛЬ",
			"РАДИЙ",
			"РАДИК",
			"РАДОМИР",
			"РАДОСЛАВ",
			"РАЗИЛЬ",
			"РАЙАН",
			"РАЙМОНД",
			"РАИС",
			"РАМАЗАН",
			"РАМИЗ",
			"РАМИЛЬ",
			"РАМОН",
			"РАНЕЛЬ",
			"РАСИМ",
			"РАСУЛ",
			"РАТИБОР",
			"РАТМИР",
			"РАФАИЛ",
			"РАФАЭЛЬ",
			"РАФИК",
			"РАШИД",
			"РЕМ",
			"РИНАТ",
			"РИФАТ",
			"РИШАТ",
			"РИХАРД",
			"РИЧАРД",
			"РОБЕРТ",
			"РОДИОН",
			"РОЛАН",
			"РОМАН",
			"РОСТИСЛАВ",
			"РУБЕН",
			"РУБИК",
			"РУДОЛЬФ",
			"РУСЛАН",
			"РУСТАМ",
			"РУФИН",
			"РУШАН",
			"РЭЙ",
			"РЯШИД",
			"САБИР",
			"САВВА",
			"САВЕЛИЙ",
			"САЙПУЛЛА",
			"САКИТ",
			"САМВЕЛ",
			"САМСОН",
			"САМУИЛ",
			"СВЯТОСЛАВ",
			"СЕВАСТЬЯН",
			"СЕВЕРИН",
			"СЕМЕН",
			"СЕРАФИМ",
			"СЕРГЕЙ",
			"СИДОР",
			"СОКРАТ",
			"СОЛОМОН",
			"СПАРТАК",
			"СПИРИДОН",
			"СТАКРАТ",
			"СТАНИСЛАВ",
			"СТЕПАН",
			"СТЕФАН",
			"СТИВЕН",
			"СТОЯН",
			"СУЛТАН",
			"ТАГИР",
			"ТАИС",
			"ТАЙЛЕР",
			"ТАЛИК",
			"ТАМАЗ",
			"ТАМЕРЛАН",
			"ТАРАС",
			"ТЕЛЬМАН",
			"ТЕОДОР",
			"ТЕРЕНТИЙ",
			"ТИБОР",
			"ТИГРАМ",
			"ТИГРАН",
			"ТИГРИЙ",
			"ТИМОФЕЙ",
			"ТИМУР",
			"ТИТ",
			"ТИХОН",
			"ТОМАС",
			"ТРИФОН",
			"ТРОФИМ",
			"УЛЬМАНАС",
			"УМАР",
			"УСТИН",
			"ФАДЕЙ",
			"ФАЗИЛЬ",
			"ФАНИС",
			"ФАРИД",
			"ФАРХАД",
			"ФЕДОР",
			"ФЕДОТ",
			"ФЕЛИКС",
			"ФЕОДОСИЙ",
			"ФЕРДИНАНД",
			"ФИДЕЛЬ",
			"ФИЛИМОН",
			"ФИЛИПП",
			"ФЛОРЕНТИЙ",
			"ФОМА",
			"ФРАНЦ",
			"ФРЕД",
			"ФРИДРИХ",
			"ФУАД",
			"ХАБИБ",
			"ХАКИМ",
			"ХАРИТОН",
			"ХРИСТИАН",
			"ХРИСТОС",
			"ХРИСТОФОР",
			"ХУСАИН",
			"ЦЕЗАРЬ",
			"ЧАРЛЬЗ",
			"ЧЕСЛАВ",
			"ЧИНГИЗ",
			"ШАМИЛЬ",
			"ШАРЛЬ",
			"ШАХИН",
			"ЭДВАРД",
			"ЭДГАР",
			"ЭДМУНД",
			"ЭДУАРД",
			"ЭЙВАЗ",
			"ЭЛЬДАР",
			"ЭМИЛЬ",
			"ЭМИН",
			"ЭММАНУИЛ",
			"ЭРАСТ",
			"ЭРИК",
			"ЭРНЕСТ",
			"ЮЛИАН",
			"ЮЛИЙ",
			"ЮНУС",
			"ЮРИЙ",
			"ЮХИМ",
			"ЯКОВ",
			"ЯН",
			"ЯНУАРИЙ",
			"ЯРОСЛАВ",
			"ЯСОН"
		};

		public HashSet<string> FemaleNames = new HashSet<string>
		{
			"АВГУСТА",
			"АВДОТЬЯ",
			"АВРОРА",
			"АГАТА",
			"АГАПИЯ",
			"АГАФЬЯ",
			"АГЛАЯ",
			"АГНЕССА",
			"АГНИЯ",
			"АГРИППИНА",
			"АГУНДА",
			"АДА",
			"АДЕЛИНА",
			"АДЕЛАИДА",
			"АДЕЛЬ",
			"АДИЛЯ",
			"АДРИАНА",
			"АЗА",
			"АЗАЛИЯ",
			"АЗИЗА",
			"АЙВАЗ",
			"АЙГУЛЬ",
			"АЙЛИН",
			"АЙНАГУЛЬ",
			"АИДА",
			"АЙЖАН",
			"АКСИНЬЯ",
			"АКУЛИНА",
			"АЛАНА",
			"АЛЕВТИНА",
			"АЛЕКСАНДРА",
			"АЛЕНА",
			"АЛИКО",
			"АЛИНА",
			"АЛИСА",
			"АЛИЯ",
			"АЛЛА",
			"АЛСУ",
			"АЛЬБА",
			"АЛЬБЕРТА",
			"АЛЬБИНА",
			"АЛЬВИНА",
			"АЛЬФИЯ",
			"АЛЬФРЕДА",
			"АЛЯ",
			"АМАЛЬ",
			"АМЕЛИЯ",
			"АМИНА",
			"АМИРА",
			"АНАИТ",
			"АНАСТАСИЯ",
			"АНГЕЛИНА",
			"АНЕЛЯ",
			"АНЖЕЛА",
			"АНЖЕЛИКА",
			"АНИСЬЯ",
			"АНИТА",
			"АННА",
			"АНТОНИНА",
			"АНФИСА",
			"АПОЛЛИНАРИЯ",
			"АРАБЕЛЛА",
			"АРЗУ",
			"АРИАДНА",
			"АРИАНА",
			"АРИНА",
			"АРХЕЛИЯ",
			"АСЕЛЬ",
			"АСИЯ",
			"АССОЛЬ",
			"АСТРА",
			"АСТРИД",
			"АСЯ",
			"АУРЕЛИЯ",
			"АФАНАСИЯ",
			"АЭЛИТА",
			"БЕАТРИСА",
			"БЕЛИНДА",
			"БЕЛЛА",
			"БЕРТА",
			"БИРУТА",
			"БОГДАНА",
			"БОЖЕНА",
			"БОРИСЛАВА",
			"БРОНИСЛАВА",
			"ВАЛЕНТИНА",
			"ВАЛЕРИЯ",
			"ВАНДА",
			"ВАНЕССА",
			"ВАРВАРА",
			"ВАСИЛИНА",
			"ВАСИЛИСА",
			"ВЕНЕРА",
			"ВЕРА",
			"ВЕРОНИКА",
			"ВЕСЕЛИНА",
			"ВЕСНА",
			"ВЕСТА",
			"ВЕТА",
			"ВИДА",
			"ВИКТОРИНА",
			"ВИКТОРИЯ",
			"ВИЛЕНА",
			"ВИЛОРА",
			"ВИОЛЕТТА",
			"ВИРГИНИЯ",
			"ВИРИНЕЯ",
			"ВИТА",
			"ВИТАЛИНА",
			"ВЛАДА",
			"ВЛАДИСЛАВА",
			"ВЛАДЛЕНА",
			"ГАБРИЭЛЛА",
			"ГАЛИНА",
			"ГАЛИЯ",
			"ГАЯНЭ",
			"ГЕЛЕНА",
			"ГАЯНЭ",
			"ГЕЛЕНА",
			"ГЕЛЛА",
			"ГЕНРИЕТТА",
			"ГЕОРГИНА",
			"ГЕРА",
			"ГЕРТРУДА",
			"ГЛАФИРА",
			"ГЛАША",
			"ГЛОРИЯ",
			"ГРАЖИНА",
			"ГРЕТА",
			"ГУЗЕЛЬ",
			"ГУЛИЯ",
			"ГУЛЬМИРА",
			"ГУЛЬНАЗ",
			"ГУЛЬНАРА",
			"ГУЛЬШАТ",
			"ГЮЛЬНАРА",
			"ДАЙНА",
			"ДАЛИЯ",
			"ДАМИРА",
			"ДАНА",
			"ДАНИЭЛА",
			"ДАНУТА",
			"ДАРА",
			"ДАРИНА",
			"ДАРЬЯ",
			"ДАЯНА",
			"ДЕБОРА",
			"ДЖАМИЛЯ",
			"ДЖЕММА",
			"ДЖЕННИФЕР",
			"ДЖЕССИКА",
			"ДЖУЛИЯ",
			"ДЖУЛЬЕТТА",
			"ДИАНА",
			"ДИЛАРА",
			"ДИЛЯРА",
			"ДИЛЬНАЗ",
			"ДИЛЬНАРА",
			"ДИЛЯ",
			"ДИНА",
			"ДИНАРА",
			"ДИОДОРА",
			"ДИОНИСИЯ",
			"ДОЛОРЕС",
			"ДОЛЯ",
			"ДОМИНИКА",
			"ДОРА",
			"ЕВА",
			"ЕВАНГЕЛИНА",
			"ЕВГЕНИЯ",
			"ЕВДОКИЯ",
			"ЕКАТЕРИНА",
			"ЕЛЕНА",
			"ЕЛИЗАВЕТА",
			"ЕСЕНИЯ",
			"ЕФИМИЯ",
			"ЖАННА",
			"ЖАСМИН",
			"ЖОЗЕФИНА",
			"ЗАБАВА",
			"ЗАИРА",
			"ЗАМИРА",
			"ЗАРА",
			"ЗАРЕМА",
			"ЗАРИНА",
			"ЗАХАРИЯ",
			"ЗЕМФИРА",
			"ЗИНАИДА",
			"ЗИТА",
			"ЗЛАТА",
			"ЗОРЯНА",
			"ЗОЯ",
			"ЗУЛЬФИЯ",
			"ЗУХРА",
			"ИВАННА",
			"ИВЕТТА",
			"ИВОНА",
			"ИДА",
			"ИЗАБЕЛЛА",
			"ИЗОЛЬДА",
			"ИЛАРИЯ",
			"ИЛИАНА",
			"ИЛОНА",
			"ИНАРА",
			"ИНГА",
			"ИНГЕБОРГА",
			"ИНДИРА",
			"ИНЕССА",
			"ИННА",
			"ИОАННА",
			"ИОЛАНТА",
			"ИРАИДА",
			"ИРИНА",
			"ИРМА",
			"ИСКРА",
			"ИЯ",
			"КАЛЕРИЯ",
			"КАМИЛЛА",
			"КАПИТОЛИНА",
			"КАРИМА",
			"КАРИНА",
			"КАРОЛИНА",
			"КАТАРИНА",
			"КИРА",
			"КЛАВДИЯ",
			"КЛАРА",
			"КЛАРИССА",
			"КЛИМЕНТИНА",
			"КОНСТАНЦИЯ",
			"КОРА",
			"КОРНЕЛИЯ",
			"КРИСТИНА",
			"КСЕНИЯ",
			"ЛАДА",
			"ЛАЙМА",
			"ЛАНА",
			"ЛАРА",
			"ЛАРИСА",
			"ЛАУРА",
			"ЛЕЙЛА",
			"ЛЕЙСАН",
			"ЛЕОКАДИЯ",
			"ЛЕОНИДА",
			"ЛЕРА",
			"ЛЕСЯ",
			"ЛИАНА",
			"ЛИДИЯ",
			"ЛИЗА",
			"ЛИКА",
			"ЛИЛИАНА",
			"ЛИЛИЯ",
			"ЛИНА",
			"ЛИНДА",
			"ЛИОРА",
			"ЛИРА",
			"ЛИЯ",
			"ЛОЛА",
			"ЛОЛИТА",
			"ЛОРА",
			"ЛУИЗА",
			"ЛУКЕРЬЯ",
			"ЛЮБОВЬ",
			"ЛЮДМИЛА",
			"ЛЯЛЯ",
			"ЛЮЦИЯ",
			"МАГДА",
			"МАГДАЛИНА",
			"МАДИНА",
			"МАЙЯ",
			"МАЛИКА",
			"МАЛЬВИНА",
			"МАРА",
			"МАРГАРИТА",
			"МАРИАННА",
			"МАРИКА",
			"МАРИНА",
			"МАРИЯ",
			"МАРСЕЛИНА",
			"МАРТА",
			"МАРУСЯ",
			"МАРФА",
			"МАРЬЯМ",
			"МАТИЛЬДА",
			"МЕЛАНИЯ",
			"МЕЛИССА",
			"МЕХРИБАН",
			"МИКА",
			"МИЛА",
			"МИЛАДА",
			"МИЛАНА",
			"МИЛЕНА",
			"МИЛИЦА",
			"МИЛОЛИКА",
			"МИЛОСЛАВА",
			"МИРА",
			"МИРОСЛАВА",
			"МИРРА",
			"МОНИКА",
			"МУЗА",
			"МЭРИ",
			"НАДЕЖДА",
			"НАЗИРА",
			"НАИЛЯ",
			"НАИМА",
			"НАИРА",
			"НАНА",
			"НАОМИ",
			"НАТАЛИЯ",
			"НАТАЛЬЯ",
			"НАТЕЛЛА",
			"НЕЛЛИ",
			"НЕОНИЛА",
			"НИКА",
			"НИКОЛЬ",
			"НИНА",
			"НИНЕЛЬ",
			"НОННА",
			"НОРА",
			"НУРИЯ",
			"ОДЕТТА",
			"ОКСАНА",
			"ОКТЯБРИНА",
			"ОЛЕСЯ",
			"ОЛИВИЯ",
			"ОЛЬГА",
			"ОФЕЛИЯ",
			"ПАВЛА",
			"ПАВЛИНА",
			"ПАМЕЛА",
			"ПАТРИЦИЯ",
			"ПЕЛАГЕЯ",
			"ПЕРИЗАТ",
			"ПОЛИНА",
			"ПРАСКОВЬЯ",
			"РАДА",
			"РАДМИЛА",
			"РАИСА",
			"РЕВЕККА",
			"РЕГИНА",
			"РЕМА",
			"РЕНАТА",
			"РИММА",
			"РИНА",
			"РИТА",
			"РОГНЕДА",
			"РОБЕРТА",
			"РОЗА",
			"РОКСАНА",
			"РОСТИСЛАВА",
			"РУЗАЛИЯ",
			"РУЗАННА",
			"РУЗИЛЯ",
			"РУМИЯ",
			"РУСАЛИНА",
			"РУСЛАНА",
			"РУФИНА",
			"САБИНА",
			"САБРИНА",
			"САЖИДА",
			"САИДА",
			"САЛОМЕЯ",
			"САМИРА",
			"САНДРА",
			"САНИЯ",
			"САНТА",
			"САРА",
			"САТИ",
			"СВЕТЛАНА",
			"СВЯТОСЛАВА",
			"СЕВАРА",
			"СЕВЕРИНА",
			"СЕЛЕНА",
			"СЕРАФИМА",
			"СИЛЬВА",
			"СИМА",
			"СИМОНА",
			"СЛАВА",
			"СНЕЖАНА",
			"СОНЯ",
			"СОФИЯ",
			"СТАНИСЛАВА",
			"СТЕЛЛА",
			"СТЕФАНИЯ",
			"СУСАННА",
			"ТАИРА",
			"ТАИСИЯ",
			"ТАЛА",
			"ТАМАРА",
			"ТАМИЛА",
			"ТАРА",
			"ТАТЬЯНА",
			"ТЕРЕЗА",
			"ТИНА",
			"ТОРА",
			"УЛЬЯНА",
			"УРСУЛА",
			"УСТИНА",
			"УСТИНЬЯ",
			"ФАИЗА",
			"ФАИНА",
			"ФАНИЯ",
			"ФАНЯ",
			"ФАРИДА",
			"ФАТИМА",
			"ФАЯ",
			"ФЕКЛА",
			"ФЕЛИЦИЯ",
			"ФЕРУЗА",
			"ФИЗУРА",
			"ФЛОРА",
			"ФРАНСУАЗА",
			"ФРИДА",
			"ХАРИТА",
			"ХИЛАРИ",
			"ХИЛЬДА",
			"ХЛОЯ",
			"ХРИСТИНА",
			"ЦВЕТАНА",
			"ЧЕЛСИ",
			"ЧЕСЛАВА",
			"ЧУЛПАН",
			"ШАКИРА",
			"ШАРЛОТТА",
			"ШЕЙЛА",
			"ШЕЛЛИ",
			"ШЕРИЛ",
			"ЭВЕЛИНА",
			"ЭВИТА",
			"ЭДДА",
			"ЭДИТА",
			"ЭЛЕОНОРА",
			"ЭЛИАНА",
			"ЭЛИЗА",
			"ЭЛИНА",
			"ЭЛЛА",
			"ЭЛЛАДА",
			"ЭЛОИЗА",
			"ЭЛЬВИНА",
			"ЭЛЬВИРА",
			"ЭЛЬГА",
			"ЭЛЬЗА",
			"ЭЛЬМИРА",
			"ЭЛЬНАРА",
			"ЭЛЯ",
			"ЭМИЛИЯ",
			"ЭММА",
			"ЭМИЛИ",
			"ЭРИКА",
			"ЭРНЕСТИНА",
			"ЭСМЕРАЛЬДА",
			"ЭТЕЛЬ",
			"ЭТЕРИ",
			"ЮЗЕФА",
			"ЮЛИЯ",
			"ЮНА",
			"ЮНИЯ",
			"ЮНОНА",
			"ЯДВИГА",
			"ЯНА",
			"ЯНИНА",
			"ЯРИНА",
			"ЯРОСЛАВА",
			"ЯСМИНА"
		};

		public Sex GetSexByName(string name) {
			name = name.ToUpper().Replace("Ё", "Е");
			if(MaleNames.Contains(name))
				return Sex.M;
			if(FemaleNames.Contains(name))
				return Sex.F;
			return Sex.None;
		}
	}
}