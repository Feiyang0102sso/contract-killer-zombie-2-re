using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Glu.ABTesting
{
	public static class ABTesting
	{
		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.ABTesting");
			}
		}

		private const string kDefaultParametersFilename = "ABTestingParameters.dat";

		private static Parameters m_parameters = null;

		private static DecisionTable m_decisionTable;

		public static Parameters parameters
		{
			get
			{
				return (m_parameters != null) ? m_parameters : (m_parameters = new Parameters(DefaultParametersFilename));
			}
		}

		public static void Init(params DecisionTable[] decisionTable)
		{
			m_parameters = parameters;
			m_decisionTable = (!(decisionTable == null || decisionTable.Length <= 0) ? decisionTable[0] : null);
		}

		public static Dictionary<string, string> Resolve()
		{
			UpdateParameters();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Clear();
			if (m_decisionTable != null)
			{
				dictionary = m_decisionTable.Resolve(parameters);
			}
		else
		{
		}
			return dictionary;
		}

		private static string DefaultParametersFilename
		{
			get
			{
				return Path.Combine(Application.persistentDataPath, "ABTestingParameters.dat");
			}
		}

		private static void UpdateParameters()
		{
			parameters.Set("__DateTime", DateTime.Now);
			int num = (int)parameters["__RawId"];
			int[] array;
			int num2;
			System.Random random;
			int num3;
			int num4;
			int num5;
			int? seed;
			if (m_decisionTable.m_options != null)
			{
				seed = m_decisionTable.m_options.m_seed;
				if (seed.HasValue && m_decisionTable.m_options.m_seed.HasValue)
				{
					array = new int[101];
					for (num2 = 100; num2 > 0; num2--)
					{
						array[num2] = num2;
					}
					random = new System.Random(m_decisionTable.m_options.m_seed.Value);
					for (num3 = 100; num3 > 1; num3--)
					{
						num4 = random.Next(1, num3 + 1);
						num5 = array[num3];
						array[num3] = array[num4];
						array[num4] = num5;
						if (num3 == num)
						{
							break;
						}
					}
					num = array[num];
				}
			}
			parameters.Set("__RandomId", num);
		}
	}
}
