using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Glu.Localization.PortableObject
{
	public sealed class Parser
	{
		private Lexer lexer;

		private int currentToken;

		private StringBuilder stringBuilder;

		public static IEnumerable<Entry> Parse(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			Parser parser = new Parser(stream);
			for (Entry e = parser.ParseEntry(); e != null; e = parser.ParseEntry())
			{
				if (!e.IsHeader())
				{
					yield return e;
				}
			}
		}

		public Parser(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			lexer = new Lexer(stream);
			stringBuilder = new StringBuilder();
			currentToken = lexer.ReadToken();
		}

		private Entry ParseEntry()
		{
			Entry entry;
			for (;;)
			{
				while (currentToken == 10)
				{
					ReadToken();
				}
				if (currentToken == -1)
				{
					return null;
				}
				entry = new Entry();
				while (currentToken == 262)
				{
					if (IsObsoleteComment(lexer.String))
					{
						ReadObsoleteEntry(entry);
						continue;
					}
					MunchComment(entry);
					ReadEolfToken();
				}
				if (IsCurrentTokenEolf)
				{
					continue;
				}
				if (currentToken == 259)
				{
					entry.Context = ReadIdentifier(259);
				}
				entry.Id = ReadIdentifier(260);
				entry.String = ReadIdentifier(261);
				return entry;
			}
		}

		private string ConcatComment(string a, string b)
		{
			if (a == null)
			{
				return b;
			}
			return string.Format("{0}\n{1}", a, b);
		}

		private void MunchComment(Entry entry)
		{
			string text = lexer.String;
			if (text.Length > 0)
			{
				string b = CommentTrimStart(text);
				switch (text[0])
				{
				case ' ':
					entry.TranslatorComment = ConcatComment(entry.TranslatorComment, b);
					break;
				case '.':
					entry.ExtractedComment = ConcatComment(entry.ExtractedComment, b);
					break;
				case ':':
					entry.References = ConcatComment(entry.References, b);
					break;
				default:
					break;
				}
			}
			else
			{
				entry.TranslatorComment = ConcatComment(entry.TranslatorComment, text);
			}
			ReadToken();
		}

		private void ReadObsoleteEntry(Entry entry)
		{
			while (currentToken == 262 && IsObsoleteComment(lexer.String))
			{
				MunchComment(entry);
				ReadEolfToken();
			}
		}

		private string ReadIdentifier(int expectedToken)
		{
			AssertToken(expectedToken);
			ReadToken();
			return ReadMultilineString();
		}

		private void AssertToken(int expectedToken)
		{
			if (expectedToken != currentToken)
			{
				string tokenDescription = GetTokenDescription(expectedToken, null);
				string tokenDescription2 = GetTokenDescription(currentToken, lexer);
				StringBuilder emptyStringBuilder = GetEmptyStringBuilder();
				emptyStringBuilder.AppendFormat("Expected {0}, got {1}", tokenDescription, tokenDescription2);
				emptyStringBuilder.AppendFormat(" ({0})", lexer.Line);
				throw new InvalidDataException(emptyStringBuilder.ToString());
			}
		}

		private static string GetTokenDescription(int t, Lexer lexer)
		{
			switch (t)
			{
			case -1:
				return "EOF";
			case 10:
				return "EOL";
			case -2:
				return lexer != null ? lexer.String : "invalid";
			case 258:
				return lexer != null ? string.Format("string {0}", lexer.String) : "string";
			case 257:
				return lexer != null ? string.Format("number {0}", lexer.Number) : "number";
			case 259:
				return "msgctxt";
			case 260:
				return "msgid";
			case 261:
				return "msgstr";
			case 256:
				return lexer != null ? string.Format("identifier {0}", lexer.String) : "identifier";
			default:
				break;
			}
			return string.Format("unknown token {0}", t);
		}

		private static bool IsObsoleteComment(string s)
		{
			return s.Length > 0 && s[0] == '~';
		}

		private static string CommentTrimStart(string s)
		{
			int length = s.Length;
			if (length == 0)
			{
				return s;
			}
			int num = 0;
			if (s[0] != ' ')
			{
				num++;
			}
			if (length > num && s[num] == ' ')
			{
				num++;
			}
			return s.Substring(num);
		}

		private string ReadMultilineString()
		{
			AssertToken(258);
			StringBuilder emptyStringBuilder = GetEmptyStringBuilder();
			while (currentToken == 258)
			{
				do
				{
					string value = lexer.String;
					stringBuilder.Append(value);
					ReadToken();
				}
				while (currentToken == 258);
				if (currentToken == -1)
				{
					break;
				}
				AssertToken(10);
				ReadToken();
			}
			return emptyStringBuilder.ToString();
		}

		private bool IsCurrentTokenEolf
		{
			get
			{
				if (currentToken == 10 || currentToken == -1)
				{
					return true;
				}
				return false;
			}
		}

		private int ReadEolfToken()
		{
			if (currentToken == 10)
			{
				return ReadToken();
			}
			return -1;
		}

		private StringBuilder GetEmptyStringBuilder()
		{
			stringBuilder.Remove(0, stringBuilder.Length);
			return stringBuilder;
		}

		private int ReadToken()
		{
			currentToken = lexer.ReadToken();
			return currentToken;
		}
	}
}
