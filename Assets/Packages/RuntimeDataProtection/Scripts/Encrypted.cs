using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Glu.RuntimeDataProtection.Internal;

namespace Glu.RuntimeDataProtection
{
	public static class Encryption
	{
		public static uint Encrypt(byte[] arr, int offset, int length, uint key)
		{
			if (arr == null)
			{
				throw new ArgumentNullException("arr");
			}
			if (offset < 0 || offset > arr.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			int num = offset + length;
			if (length < 0 || num > arr.Length)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			uint num2 = key;
			for (int i = offset; i < num; i++)
			{
				byte b = (byte)(num2 & 0xFF);
				arr[i] ^= b;
				num2 = GenerateNextUInt(num2);
			}
			return num2;
		}

		private static uint GenerateNextUInt(uint value)
		{
			ulong unusedLocal1;
			ulong unusedLocal2;
			return (uint)((22695477L * (long)value + 1) & 0xFFFFFFFFu);
		}
	}
}

namespace Glu.RuntimeDataProtection
{
	[Serializable]
	public class Encrypted<T> : ICloneable, IXmlSerializable, ISerializable
	{
		private EncryptedData data;

		public Encrypted()
			: this(default(T))
		{
		}

		public Encrypted(T value)
		{
			data = new EncryptedData();
			Set(value);
		}

		public Encrypted(SerializationInfo info, StreamingContext context)
			: this((T)info.GetValue("data", typeof(T)))
		{
		}

		private Encrypted(Encrypted<T> other)
		{
			data = other.data.Clone();
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public void Set(T value)
		{
			if (object.ReferenceEquals(value, null))
			{
				data.Reset();
				return;
			}
			MemoryStream memoryStream = StaticData.MemoryStream;
			memoryStream.Seek(0L, SeekOrigin.Begin);
			memoryStream.SetLength(0L);
			StaticData.DefaultSerializer.Serialize(memoryStream, value);
			byte[] buffer = memoryStream.GetBuffer();
			int length = (int)memoryStream.Length;
			data.Set(buffer, 0, length);
			Array.Clear(buffer, 0, length);
		}

		public T Get()
		{
			int dataLength = data.DataLength;
			if (dataLength == 0)
			{
				return default(T);
			}
			MemoryStream memoryStream = StaticData.MemoryStream;
			memoryStream.Seek(0L, SeekOrigin.Begin);
			memoryStream.SetLength(dataLength);
			byte[] buffer = memoryStream.GetBuffer();
			data.Get(buffer, 0, dataLength);
			T result = (T)StaticData.DefaultSerializer.Deserialize(memoryStream, typeof(T));
			Array.Clear(buffer, 0, dataLength);
			return result;
		}

		public Encrypted<T> Clone()
		{
			return new Encrypted<T>(this);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("data", Get());
		}

		public void ReadXml(XmlReader reader)
		{
			bool isEmptyElement = reader.IsEmptyElement;
			reader.ReadStartElement();
			if (isEmptyElement)
			{
				Set(default(T));
				return;
			}
			object obj = reader.ReadContentAs(typeof(T), null);
			Set((T)obj);
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteValue(Get());
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public override string ToString()
		{
			T val = Get();
			if (object.ReferenceEquals(val, null))
			{
				return string.Empty;
			}
			return val.ToString();
		}

		public static Encrypted<T> Parse(string s)
		{
			if (object.ReferenceEquals(s, null))
			{
				throw new ArgumentNullException(s);
			}
			T value = (T)Convert.ChangeType(s, typeof(T));
			return new Encrypted<T>(value);
		}

		public static implicit operator Encrypted<T>(T value)
		{
			return new Encrypted<T>(value);
		}

		public static implicit operator T(Encrypted<T> encryptedValue)
		{
			return encryptedValue != null ? encryptedValue.Get() : default(T);
		}
	}
}
