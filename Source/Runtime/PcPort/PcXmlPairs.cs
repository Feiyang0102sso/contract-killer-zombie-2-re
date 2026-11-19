// PC 桥接：键值对列表的 XML 存取。
// 游戏用 XmlSerializer 存取 List<KeyValuePair<string, object>>（A/B 测试参数）和
// List<KeyValuePair<string, string>>（A/B 测试结果，含资源包地址）。KeyValuePair 的 Key/Value 是只读属性，
// Unity 3.5 的旧 Mono 能正确处理，新版 XmlSerializer 却会写出空元素、读回空键。
// 这里在 StorageManager 的 XML 读写入口处，把这两种列表换成可正常序列化的键值对类型；其余类型照常处理。
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public static class PcXmlPairs
{
	/// <summary>可序列化的 string -> object 键值对（值为基础类型时 XmlSerializer 会写 xsi:type）。</summary>
	public class StringObjectPair
	{
		public string Key;

		public object Value;
	}

	/// <summary>可序列化的 string -> string 键值对。</summary>
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
	/// XmlDictionary 键的替身：KeyValuePair 的 Key/Value 是只读属性，新版 XmlSerializer 写成空元素，
	/// 读回来全是默认值，第二个键就“重复”，整个存档读不回来（白天修复第 5 节）。换成可写字段的类型存取。
	/// </summary>
	public class KeyPair<TFirst, TSecond>
	{
		public TFirst Key;

		public TSecond Value;
	}

	/// <summary>代替 XmlDictionary.WriteXml 里的 keySerializer.Serialize(writer, key)。</summary>
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

	/// <summary>代替 XmlDictionary.ReadXml 里的 keySerializer.Deserialize(reader)。</summary>
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

	/// <summary>根元素名沿用 XmlSerializer 给 KeyValuePair 起的名字，例如 KeyValuePairOfStringString。</summary>
	private static string RootNameFor(Type keyValuePairType)
	{
		Type[] arguments = keyValuePairType.GetGenericArguments();
		return "KeyValuePairOf" + arguments[0].Name + arguments[1].Name;
	}
}
