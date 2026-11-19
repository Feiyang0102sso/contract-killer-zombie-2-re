public class StoreFrontLogger : LoggerSingleton<StoreFrontLogger>
{
	public StoreFrontLogger()
	{
		LoggerSingleton<StoreFrontLogger>.SetLoggerName("Package.StoreFront");
	}
}
