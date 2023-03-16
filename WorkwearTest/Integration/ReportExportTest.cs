using System;
using System.IO;
using fyiReporting.RDL;
using NUnit.Framework;
using QS.Utilities.Text;
using RdlEngine;

namespace WorkwearTest.Integration
{
	[TestFixture()]
	public class ReportExportTest
	{
		[Test(Description = "Проверяем что можем сделать экспорт Excel2007. Реальный кейс: отсутствие ICSharpCode.SharpZipLib")]
		public void Reports_ExportExcel2007_SharpZipLibExistInOutputDirTest()
		{
			var path = Directory.GetCurrentDirectory();
			var filePath = Path.Combine(path.ReplaceLastOccurrence("WorkwearTest", "Workwear"), "ICSharpCode.SharpZipLib.dll");
			FileAssert.Exists(filePath);
		}

		[Test(Description = "Проверяем что можем сделать экспорт Excel2007. Проверяем работает ли NPOI.")]
		public void Reports_ExportExcel2007Test()
		{
			var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Integration", "Reports", "NoDBTestReport.rdl");
			using(MemoryStream stream = ReportExporter.ExportToMemoryStream(new Uri(reportPath), OutputPresentationType.Excel2007)) {
				var file = stream.GetBuffer();
				Assert.That(file.Length, Is.GreaterThan(1), "Размер файла должен быть больше 1 байта.");
			}
		}
	}
}
