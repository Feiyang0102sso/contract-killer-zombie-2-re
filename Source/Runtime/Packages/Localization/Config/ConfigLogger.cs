using System;
using System.Runtime.Serialization;

namespace Glu.Localization.Config
{
	[Serializable]
	public class InvalidConfigException : LocalizationException
	{
		public InvalidConfigException()
		{
		}

		public InvalidConfigException(string message)
			: base(message)
		{
		}

		public InvalidConfigException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected InvalidConfigException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}

namespace Glu.Localization.Config
{
	public sealed class ConfigLogger : LoggerSingleton<ConfigLogger>
	{
		public ConfigLogger()
		{
			LoggerSingleton<ConfigLogger>.SetLoggerName("Package.Localization.Config");
		}
	}
}
