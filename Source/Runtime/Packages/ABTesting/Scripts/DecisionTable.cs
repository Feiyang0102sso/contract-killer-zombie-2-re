using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Glu.ABTesting
{
	[XmlType("DecisionTable")]
	public class DecisionTable
	{
		[XmlType("Variant")]
		public class Variant
		{
			[XmlType("Entry")]
			public class Entry
			{
				[XmlAttribute("Key")]
				public string m_key;

				[XmlAttribute("Value")]
				public string m_value;

				private Entry()
				{
					m_key = null;
					m_value = null;
				}

				public Entry(string key, string value)
				{
					m_key = key;
					m_value = value;
				}
			}

			[XmlAttribute("Id")]
			public string m_guid;

			[XmlArray("Data")]
			[XmlArrayItem("Entry")]
			public List<Entry> m_data;

			public Variant()
			{
				m_guid = Guid.NewGuid().ToString();
				m_data = new List<Entry>();
				m_data.Clear();
			}
		}

		[XmlType("Condition")]
		public class Condition
		{
			[XmlType("Operator")]
			public enum Operator
			{
				Unknown,
				Equal,
				Larger,
				LargerOrEqual,
				Less,
				LessOrEqual,
				NotEqual
			}

			private class Logger : LoggerSingleton<Logger>
			{
				public Logger()
				{
					LoggerSingleton<Logger>.SetLoggerName("Package.ABTesting.DecisionTable.Condition");
				}
			}

			[XmlAttribute("Key")]
			public string m_lhsName;

			[XmlAttribute("Operator")]
			public Operator m_operator;

			[XmlAttribute("Value")]
			public string m_rhs;

			private Condition()
			{
				m_lhsName = null;
				m_operator = Operator.Unknown;
				m_rhs = null;
			}

			public Condition(string key, Operator op, string value)
			{
				m_lhsName = key;
				m_operator = op;
				m_rhs = value;
			}

			public bool Check(Parameters parameters)
			{
				bool result = false;
				int result2 = 0;
				if (Compare(parameters, out result2))
				{
					switch (m_operator)
					{
					case Operator.Equal:
						result = result2 == 0;
						break;
					case Operator.Larger:
						result = result2 > 0;
						break;
					case Operator.LargerOrEqual:
						result = result2 >= 0;
						break;
					case Operator.Less:
						result = result2 < 0;
						break;
					case Operator.LessOrEqual:
						result = result2 <= 0;
						break;
					case Operator.NotEqual:
						result = result2 != 0;
						break;
					default:
						break;
					}
				}
				return result;
			}

			private bool Compare(Parameters parameters, out int result)
			{
				bool result2 = false;
				result = 0;
				if (parameters != null)
				{
					object val = null;
					if (parameters.TryGetValue(m_lhsName, out val))
					{
						try
						{
							IComparable comparable = val as IComparable;
							IComparable obj = Convert.ChangeType(m_rhs, comparable.GetType()) as IComparable;
							result = comparable.CompareTo(obj);
							result2 = true;
						}
						catch (Exception e)
						{
						}
					}
				}
				return result2;
			}
		}

		[XmlType("Transition")]
		public class Transition
		{
			[XmlAttribute("From")]
			public string m_from;

			[XmlAttribute("To")]
			public string m_to;

			private Transition()
			{
				m_from = null;
				m_to = null;
			}

			public Transition(string from, string to)
			{
				m_from = from;
				m_to = to;
			}
		}

		[XmlType("Rule")]
		public class Rule
		{
			[XmlElement("Condition")]
			public Condition m_condition;

			[XmlArray("Transitions")]
			[XmlArrayItem("Transition")]
			public List<Transition> m_transitions;

			private Rule()
			{
				m_condition = null;
				m_transitions = null;
			}

			public Rule(Condition condition, List<Transition> transitions)
			{
				m_condition = condition;
				m_transitions = transitions;
			}
		}

		[XmlType("Options")]
		public class Options
		{
			[XmlElement("RandomSeed")]
			public int? m_seed;

			public Options()
			{
				m_seed = null;
			}
		}

		private class Logger : LoggerSingleton<Logger>
		{
			public Logger()
			{
				LoggerSingleton<Logger>.SetLoggerName("Package.ABTesting.DecisionTable");
			}
		}

		public const string RESOLUTION_KEY_VARIANT_ID = "VariantId";

		[XmlAttribute("Version")]
		public string m_version;

		[XmlArray("Variants")]
		public List<Variant> m_variants;

		[XmlArray("Rules")]
		public List<Rule> m_rules;

		[XmlElement("Options")]
		public Options m_options;

		private static XmlWriterSettings m_xmlWriterSettings = null;

		private static XmlSerializer m_xmlSerializer;

		public static DecisionTable Load(byte[] bytes)
		{
			DecisionTable result = null;
			using (Stream input = new MemoryStream(bytes))
			{
				try
				{
					using (XmlTextReader xmlReader = new XmlTextReader(input) { XmlResolver = null })
					{
						result = xmlSerializer.Deserialize(xmlReader) as DecisionTable;
					}
				}
				catch (Exception e)
				{
					result = null;
				}
			}
			return result;
		}

		public static DecisionTable Load(string filename)
		{
			DecisionTable result = null;
			if (!string.IsNullOrEmpty(filename))
			{
				try
				{
					result = StorageManager.ReadXmlFromLocation(filename, typeof(DecisionTable)) as DecisionTable;
				}
				catch (Exception e)
				{
					result = null;
				}
			}
			return result;
		}

		private DecisionTable()
		{
			m_version = null;
			m_variants = new List<Variant>();
			m_variants.Clear();
			m_rules = new List<Rule>();
			m_rules.Clear();
			m_options = null;
		}

		public bool Save(string filename)
		{
			bool result = false;
			if (!string.IsNullOrEmpty(filename))
			{
				try
				{
					string directoryName = Path.GetDirectoryName(filename);
					if (!Directory.Exists(directoryName))
					{
						try
						{
							Directory.CreateDirectory(directoryName);
						}
						catch (Exception)
						{
						}
					}
					StorageManager.WriteXmlToLocation(filename, filename, this);
					result = true;
				}
				catch (Exception e)
				{
				}
			}
			return result;
		}

		public Dictionary<string, string> Resolve(Parameters parameters)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Clear();
			if (m_variants != null && m_variants.Count > 0)
			{
				Variant variant = m_variants[0];
				if (variant != null)
				{
					if (m_rules != null && m_rules.Count > 0)
					{
						int num = 0;
						foreach (Rule rule in m_rules)
						{
							num++;
							Condition condition = rule.m_condition;
							if (condition != null)
							{
								bool conditionPassed = condition.Check(parameters);
								if (conditionPassed)
								{
									List<Transition> transitions = rule.m_transitions;
									if (transitions != null && transitions.Count > 0)
									{
										string guid = variant.m_guid;
										string text = null;
										foreach (Transition item in transitions)
										{
											if (item.m_from.Equals(guid))
											{
												text = item.m_to;
												break;
											}
										}
										if (text != null)
										{
											Variant variant2 = null;
											foreach (Variant variant3 in m_variants)
											{
												if (variant3.m_guid.Equals(text))
												{
													variant2 = variant3;
													break;
												}
											}
											if (variant2 != null)
											{
												variant = variant2;
											}
											else
											{
											}
										}
										else
										{
										}
									}
									else
									{
									}
								}
							}
							else
							{
							}
						}
					}
					else
					{
					}
				}
				else
				{
				}
				if (variant != null)
				{
					dictionary.Add("VariantId", variant.m_guid);
					foreach (Variant.Entry datum in variant.m_data)
					{
						if (datum.m_key != null)
						{
							dictionary.Add(datum.m_key, datum.m_value);
						}
						else
						{
						}
					}
				}
				else
				{
				}
			}
			else
			{
			}
			return dictionary;
		}

		private static XmlWriterSettings xmlWriterSettings
		{
			get
			{
				if (m_xmlWriterSettings != null)
				{
					return m_xmlWriterSettings;
				}
				m_xmlWriterSettings = new XmlWriterSettings();
				m_xmlWriterSettings.Encoding = Encoding.UTF8;
				m_xmlWriterSettings.Indent = true;
				m_xmlWriterSettings.IndentChars = "\t";
				m_xmlWriterSettings.NewLineChars = "\r\n";
				m_xmlWriterSettings.NewLineHandling = NewLineHandling.None;
				m_xmlWriterSettings.OmitXmlDeclaration = true;
				return m_xmlWriterSettings;
			}
		}

		private static XmlSerializer xmlSerializer
		{
			get
			{
				if (m_xmlSerializer != null)
				{
					return m_xmlSerializer;
				}
				m_xmlSerializer = new XmlSerializer(typeof(DecisionTable));
				return m_xmlSerializer;
			}
		}
	}
}
