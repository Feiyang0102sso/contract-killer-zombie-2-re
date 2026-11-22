// New Unity port: XML save/load of key-value pair lists.
// The game stores List<KeyValuePair<string, object>> (A/B test parameters) and
// List<KeyValuePair<string, string>> (A/B test results, including asset bundle URLs) with XmlSerializer. KeyValuePair's
// Key/Value are read-only properties: Unity 3.5's old Mono handled them, the newer XmlSerializer writes empty elements
// and reads back empty keys.
// At StorageManager's XML read/write entry points, these two list types are swapped for serializable pair types;
// every other type is handled as usual.
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public static class NewUnityXmlPairs
{
	/// <summary>Serializable string -> object pair (XmlSerializer writes xsi:type for primitive values).</summary>
	public class StringObjectPair
	{
		public string Key;

		public object Value;
	}

	/// <summary>Serializable string -> string pair.</summary>
	public class StringStringPair
	{
		public string Key;

		public string Value;
	}

	public static void Serialize(XmlWriter writer, object data)
	{
		List<KeyValuePair<string, object>> objectPairs = data as List<KeyValuePair<string, object>>;
		if (objectPairs != null)
		{
			List<StringObjectPair> converted = new List<StringObjectPair>();
			foreach (KeyValuePair<string, object> pair in objectPairs)
			{
				StringObjectPair item = new StringObjectPair();
				item.Key = pair.Key;
				item.Value = pair.Value;
				converted.Add(item);
			}
			new XmlSerializer(typeof(List<StringObjectPair>)).Serialize(writer, converted);
			return;
		}
		List<KeyValuePair<string, string>> stringPairs = data as List<KeyValuePair<string, string>>;
		if (stringPairs != null)
		{
			List<StringStringPair> converted = new List<StringStringPair>();
			foreach (KeyValuePair<string, string> pair in stringPairs)
			{
				StringStringPair item = new StringStringPair();
				item.Key = pair.Key;
				item.Value = pair.Value;
				converted.Add(item);
			}
			new XmlSerializer(typeof(List<StringStringPair>)).Serialize(writer, converted);
			return;
		}
		new XmlSerializer(data.GetType()).Serialize(writer, data);
	}

	public static object Deserialize(XmlReader reader, Type type)
	{
		if (type == typeof(List<KeyValuePair<string, object>>))
		{
			List<StringObjectPair> items = (List<StringObjectPair>)new XmlSerializer(typeof(List<StringObjectPair>)).Deserialize(reader);
			List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>();
			foreach (StringObjectPair item in items)
			{
				result.Add(new KeyValuePair<string, object>(item.Key, item.Value));
			}
			return result;
		}
		if (type == typeof(List<KeyValuePair<string, string>>))
		{
			List<StringStringPair> items = (List<StringStringPair>)new XmlSerializer(typeof(List<StringStringPair>)).Deserialize(reader);
			List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
			foreach (StringStringPair item in items)
			{
				result.Add(new KeyValuePair<string, string>(item.Key, item.Value));
			}
			return result;
		}
		return new XmlSerializer(type).Deserialize(reader);
	}

	/// <summary>
	/// Stand-in for XmlDictionary keys: KeyValuePair's Key/Value are read-only properties, which the newer XmlSerializer
	/// writes as empty elements; they read back as defaults, the second key then "repeats" and the whole save fails to
	/// load (day fixes section 5). Stored through a type with writable fields instead.
	/// </summary>
	public class KeyPair<TFirst, TSecond>
	{
		public TFirst Key;

		public TSecond Value;
	}

	/// <summary>Replaces keySerializer.Serialize(writer, key) in XmlDictionary.WriteXml.</summary>
	public static void SerializeKey(XmlWriter writer, XmlSerializer keySerializer, object key)
	{
		Type keyType = key.GetType();
		if (!IsKeyValuePair(keyType))
		{
			keySerializer.Serialize(writer, key);
			return;
		}
		Type pairType = PairTypeFor(keyType);
		object pair = Activator.CreateInstance(pairType);
		pairType.GetField("Key").SetValue(pair, keyType.GetProperty("Key").GetValue(key, null));
		pairType.GetField("Value").SetValue(pair, keyType.GetProperty("Value").GetValue(key, null));
		new XmlSerializer(pairType, new XmlRootAttribute(RootNameFor(keyType))).Serialize(writer, pair);
	}

	/// <summary>Replaces keySerializer.Deserialize(reader) in XmlDictionary.ReadXml.</summary>
	public static object DeserializeKey(XmlReader reader, XmlSerializer keySerializer, Type keyType)
	{
		if (!IsKeyValuePair(keyType))
		{
			return keySerializer.Deserialize(reader);
		}
		Type pairType = PairTypeFor(keyType);
		object pair = new XmlSerializer(pairType, new XmlRootAttribute(RootNameFor(keyType))).Deserialize(reader);
		object first = pairType.GetField("Key").GetValue(pair);
		object second = pairType.GetField("Value").GetValue(pair);
		return Activator.CreateInstance(keyType, first, second);
	}

	private static bool IsKeyValuePair(Type type)
	{
		return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
	}

	private static Type PairTypeFor(Type keyValuePairType)
	{
		return typeof(KeyPair<,>).MakeGenericType(keyValuePairType.GetGenericArguments());
	}

	/// <summary>Root element name as XmlSerializer names a KeyValuePair, e.g. KeyValuePairOfStringString.</summary>
	private static string RootNameFor(Type keyValuePairType)
	{
		Type[] arguments = keyValuePairType.GetGenericArguments();
		return "KeyValuePairOf" + arguments[0].Name + arguments[1].Name;
	}
}
