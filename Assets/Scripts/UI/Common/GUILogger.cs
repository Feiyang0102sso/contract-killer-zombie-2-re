public class GUILogger : LoggerSingleton<GUILogger>
{
	public GUILogger()
	{
		LoggerSingleton<GUILogger>.SetLoggerName("App.GUI");
	}
}
