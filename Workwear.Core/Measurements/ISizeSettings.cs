namespace Workwear.Measurements
{
	///Настроки размеров вынесены в отдельный класс интерфейс только потому что
	///на сервере они получают информацию по другому.
	public interface ISizeSettings
	{
		bool EmployeeSizeRanges { get; }
	}
}