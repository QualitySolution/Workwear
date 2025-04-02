using System;
using QS.DomainModel.Entity;
using QS.Project.Versioning;
using QS.Utilities.Text;

namespace Workwear.Tools {
	public static class DocHelper {
		public static string GetDocUrl(string internalLink) {
			var version = new ApplicationVersionInfo().Version;
		    var baseUrl = $"https://doc.qsolution.ru/workwear/{version.ToString(2)}/";
		    var queryParams = "?utm_source=qs&utm_medium=app_workwear&utm_campaign=open_documentation";
			
		    if (internalLink.Contains("#")) {
		        var parts = internalLink.Split('#');
		        return $"{baseUrl}{parts[0]}{queryParams}#{parts[1]}";
		    } else {
		        return $"{baseUrl}{internalLink}{queryParams}";
		    }
		}
		
		public static string GetEntityDocTooltip(Type entityType) {
			var names = entityType.GetSubjectNames();
			if(names == null)
				return "Открыть онлайн документацию";
			if(!String.IsNullOrEmpty(names.Genitive) )
				return $"Открыть онлайн документацию для {names.Genitive}";
			return $"{names.Nominative.StringToTitleCase()} в онлайн-документации";
		}
		
		public static string GetJournalDocTooltip(Type entityType) {
			var names = entityType.GetSubjectNames();
			if(names == null)
				return "Открыть онлайн документацию";
			if(!String.IsNullOrEmpty(names.GenitivePlural) )
				return $"Открыть онлайн документацию для журнала {names.GenitivePlural}";
			if (!String.IsNullOrEmpty(names.NominativePlural) )
				return $"Онлайн-документация: Журнал '{names.NominativePlural.StringToTitleCase()}'";
			return "Открыть онлайн документацию";
		}
		
		public static string GetDialogDocTooltip(string dialogName) {
			return $"Открыть онлайн документацию для диалога '{dialogName}'";
		}
	}
}
