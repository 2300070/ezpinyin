using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;

namespace EzPinyin
{
	/// <summary>
	/// 应用程序所需使用的公共的杂项功能集合。
	/// </summary>
	internal static class Common
	{
		private const int BUFFER_SIZE = 0x100;
		private static readonly StringBuilder[] buffers = new StringBuilder[Environment.ProcessorCount];
		private static readonly Dictionary<char, char> traditional = new Dictionary<char, char>();

		internal static readonly int[] PrimeTable = { 3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369 };

		internal static readonly char[] SeparatorCharacters = { ' ', '	', ' ' };

		internal static readonly PinyinNode[] Utf16NodeTemplates;

		internal static readonly PinyinNode[] Utf32NodeTemplates;

		internal static readonly ResourceManager ResourceManager = new ResourceManager("EzPinyin.Resources", Assembly.GetExecutingAssembly());

		internal static readonly string[] EmptyArray = new string[0];

		static Common()
		{
			/**
			 * 初始化缓冲区
			 */
			for (int i = 0; i < buffers.Length; i++)
			{
				buffers[i] = new StringBuilder(BUFFER_SIZE);
			}


			/**
			 * 建立拼音索引，索引节点集合的第一个值为空白节点，以便适配那些没有拼音记录的字符。
			 */
			List<PinyinNode> utf16Nodes = new List<PinyinNode> { Utf16EmptyNode.Instance };
			List<PinyinNode> utf32Nodes = new List<PinyinNode> { Utf32EmptyNode.Instance };
			using (StringReader sr = new StringReader(Common.ResourceManager.GetString("pinyin")))
			{
				while (true)
				{
					string pinyin = sr.ReadLine();
					if (!string.IsNullOrEmpty(pinyin))
					{
						utf16Nodes.Add(new Utf16Node(pinyin));
						utf32Nodes.Add(new Utf32Node(pinyin));
					}
					else
					{
						break;
					}
				}
			}

			Common.Utf16NodeTemplates = utf16Nodes.ToArray();
			Common.Utf32NodeTemplates = utf32Nodes.ToArray();

			/**
			 * 加载繁体字字典
			 */
			Common.LoadTradionalDictionary();
		}

		internal static PinyinNode[] LoadDictionary(string resourceName, PinyinNode[] templates)
		{
			/**
			 * 从指定的资源字典加载拼音集合作为字典。
			 */
			byte[] buffer = (byte[])Common.ResourceManager.GetObject(resourceName);

			int length = buffer.Length;
			PinyinNode[] result = new PinyinNode[length >> 1];
			for (int i = 0; i < length; i += 2)
			{
				result[i >> 1] = templates[(buffer[i] << 8) | buffer[i + 1]];
			}

			return result;
		}

		internal static void LoadLexicon(PinyinNode[] nodes, int min)
		{
			/**
			 * 为指定的拼音节点集合加载词典内容。
			 */

			/**
			 * 先加载主要的词典内容。
			 */
			Common.LoadLexicon(nodes, "lex_basic_2", min, 2);
			Common.LoadLexicon(nodes, "lex_basic_3", min, 3);
			Common.LoadLexicon(nodes, "lex_basic_4", min, 4);
			Common.LoadExtraLexicon(nodes, "lex_basic_x", min);

			/**
			 * 接着加载交叉词典内容。
			 */
			Common.LoadIntersectionLexicon(nodes, "lex_it_2", min, 2);
			Common.LoadIntersectionLexicon(nodes, "lex_it_3", min, 3);
			Common.LoadIntersectionLexicon(nodes, "lex_it_4", min, 4);
			Common.LoadExtraIntersectionLexicon(nodes, "lex_it_x", min);
		}

		internal static string FixPinyin(string pinyin)
		{
			/**
			 * 修正拼音字符串，包括移除声调，移除已经发现的不规范字符，使之标准化。
			 */

			pinyin = pinyin.Trim();
			char[] chars = pinyin.ToCharArray();
			int length = chars.Length;

			/**
			 * 可能存在拼音+数字音调的情况，如yi1；这种情况下需要将长度减一以去除声调。
			 */
			int tone = chars[length - 1] - '1';
			if (tone > -1 && tone < 5)
			{
				length--;
			}

			for (int i = 0; i < chars.Length; i++)
			{
				char ch = chars[i];

				//大写转小写
				if (ch >= 'A' && ch <= 'Z')
				{
					ch = chars[i] = (char)('a' + ch - 'A');
				}

				//全角转半角
				if (ch >= 'ａ' && ch <= 'ｚ')
				{
					ch = chars[i] = (char)('a' + ch - 'ａ');
				}
				if (ch >= 'Ａ' && ch <= 'Ｚ')
				{
					ch = chars[i] = (char)('a' + ch - 'Ａ');
				}

				switch (ch)
				{
					case 'a':
					case 'ā':
					case 'á':
					case 'ă':
					case 'ǎ':
					case 'à':
					case 'ɑ':
						chars[i] = 'a';
						break;
					case 'o':
					case 'ō':
					case 'ó':
					case 'ŏ':
					case 'ǒ':
					case 'ò':
						chars[i] = 'o';
						break;
					case 'e':
					case 'ē':
					case 'ĕ':
					case 'ě':
					case 'è':
						chars[i] = 'e';
						break;
					case 'i':
					case 'ī':
					case 'í':
					case 'ĭ':
					case 'ǐ':
					case 'ì':
						chars[i] = 'i';
						break;
					case 'u':
					case 'ū':
					case 'ú':
					case 'ŭ':
					case 'ǔ':
					case 'ù':
						chars[i] = 'u';
						break;
					case 'v':
					case 'ü':
					case 'ǖ':
					case 'ǘ':
					case 'ǚ':
					case 'ǜ':
						if (i + 1 < length)
						{
							switch (chars[i + 1])
							{
								case 'n':
								case 'e':
								case 'ē':
								case 'é':
								case 'ĕ':
								case 'ě':
								case 'è':
									chars[i] = 'u';
									break;
								default:
									chars[i] = 'v';
									break;
							}
						}
						else
						{
							chars[i] = 'v';
						}
						break;
					case 'ɡ':
						chars[i] = 'g';
						break;

				}
			}

			return new string(chars, 0, length);
		}

		internal static unsafe PinyinNode MapAnyNode(char* cursor)
		{
			/**
			 * 根据指定的游标，加载对应UTF16或者UTF32字符的拼音节点信息。
			 */

			char ch = *cursor;
			if (ch > 0x4DFF && ch < 0xA000)
			{
				return Basic.Dictionary[ch - 0x4E00];
			}

			if (ch == '〇')
			{
				return Basic.Dictionary[0x5200];
			}

			if (ch > 0x33FF && ch < 0x4DC0)
			{
				/**
				 * 扩展A区汉字
				 */
				return ExtensionA.Dictionary[ch - 0x3400];
			}

			if (ch > 0xF8FF && ch < 0xFB00)
			{
				/**
				 * 兼容汉字
				 */
				return Compatibility.Dictionary[ch - 0xF900];
			}

			if (ch > 0x2E7F && ch < 0x2FE0)
			{
				/**
				 * 部首及部首扩展区
				 */
				return Radicals.Dictionary[ch - 0x2E80];
			}
			char ch2;
			PinyinNode result;

			if (ch > 0xD7FF && ch < 0xDE00 && (ch2 = *(cursor + 1)) > 0xDBFF && ch2 < 0xE000)
			{
				int code = (ch - 0xD800) * 1024 + (ch2 - 0xDC00) + 0x10000;//使用高位字符与低位字符获得UTF-32编码。

				/**
				 * 扩展B区汉字
				 */
				if (code > 0x1FFFF && code < 0x2A6E0)
				{
					return ExtensionB.Dictionary[code - 0x20000];
				}

				/**
				 * 扩展C区汉字
				 */
				if (code > 0x2A6FF && code < 0x2B739)
				{
					return ExtensionC.Dictionary[code - 0x2A700];
				}

				/**
				 * 扩展D区汉字
				 */
				if (code > 0x2B73F && code < 0x2B81E)
				{
					return ExtensionD.Dictionary[code - 0x2B740];
				}

				/**
				 * 扩展E区汉字
				 */
				if (code > 0x2B81F && code < 0x2CEA2)
				{
					return ExtensionE.Dictionary[code - 0x2B820];
				}

				/**
				 * 扩展F区汉字
				 */
				if (code > 0x2CEAF && code < 0x2EBE1)
				{
					return ExtensionF.Dictionary[code - 0x2CEB0];
				}

				/**
				 * 扩展G区汉字
				 */
				if (code > 0x2FFFF && code < 0x3134B)
				{
					return ExtensionG.Dictionary[code - 0x30000];
				}

				/**
				 * 兼容汉字扩展
				 */
				if (code > 0x2F7FF && code < 0x2FA20)
				{
					return CompatibilitySupplement.Dictionary[code - 0x2F800];
				}

				if (Miscellaneous.Dictionary32.TryGetValue(code, out result))
				{
					return result;
				}
				return Utf32EmptyNode.Instance;
			}
			if (Miscellaneous.Dictionary32.TryGetValue(ch, out result))
			{
				return result;
			}
			return Utf16EmptyNode.Instance;
		}

		internal static unsafe PinyinNode MapUtf16Node(char* cursor)
		{
			/**
			 * 根据指定的游标，加载对应UTF16字符的拼音节点信息。
			 */

			char ch = *cursor;
			if (ch > 0x4DFF && ch < 0xA000)
			{
				return Basic.Dictionary[ch - 0x4E00];
			}

			if (ch == '〇')
			{
				return Basic.Dictionary[0x5200];
			}

			if (ch > 0x33FF && ch < 0x4DC0)
			{
				/**
				 * 扩展A区汉字
				 */
				return ExtensionA.Dictionary[ch - 0x3400];
			}

			if (ch > 0xF8FF && ch < 0xFB00)
			{
				/**
				 * 兼容汉字
				 */
				return Compatibility.Dictionary[ch - 0xF900];
			}

			if (ch > 0x2E7F && ch < 0x2FE0)
			{
				/**
				 * 部首及部首扩展区
				 */
				return Radicals.Dictionary[ch - 0x2E80];
			}
			return Utf16EmptyNode.Instance;
		}

		internal static StringBuilder AcquireBuffer()
		{
			/**
			 * 从共享的数据池中提取一个空闲的可变字符串。
			 */

			StringBuilder buffer = Interlocked.CompareExchange(ref buffers[0], null, buffers[0]);
			if (buffer != null)
			{
				return buffer;
			}
			for (int i = 1; i < buffers.Length; i++)
			{
				if ((buffer = Interlocked.CompareExchange(ref buffers[i], null, buffers[i])) != null)
				{
					return buffer;
				}
			}

			return new StringBuilder(BUFFER_SIZE);
		}

		internal static string ReturnBuffer(StringBuilder buffer)
		{
			/**
			 * 将不再使用的可变字符串返回共享池，并返回该字符串的内容。
			 */

			string result = buffer.ToString();
			buffer.Length = 0;
			if (buffer.Capacity > 0x1000)
			{
				buffer.Capacity = BUFFER_SIZE;
			}

			if (Interlocked.CompareExchange(ref buffers[0], buffer, null) == null)
			{
				return result;
			}

			for (int i = 1; i < buffers.Length; i++)
			{
				if (Interlocked.CompareExchange(ref buffers[i], buffer, null) == null)
				{
					return result;
				}
			}

			return result;
		}

		private static unsafe string LoadPinyinTemparory(ref char* cursor, char* last)
		{
			/**
			 * 临时读取拼音。
			 */
			PinyinNode node;
			char ch = *cursor;
			if (ch > 0x4DFF && ch < 0xA000)
			{
				cursor += 1;
				return Common.LoadPinyinTemparory("dict_basic", ch - 0x4E00);
			}

			if (ch == '〇')
			{
				cursor += 1;
				return Common.LoadPinyinTemparory("dict_basic", 0x5200);
			}

			if (ch > 0x33FF && ch < 0x4DC0)
			{
				cursor += 1;
				return Common.LoadPinyinTemparory("dict_ext_a", ch - 0x3400);
			}

			if (ch > 0xF8FF && ch < 0xFB00)
			{
				cursor += 1;
				return Common.LoadPinyinTemparory("dict_cmp", ch - 0xF900);
			}

			if (ch > 0x2E7F && ch < 0x2FE0)
			{
				cursor += 1;
				return Common.LoadPinyinTemparory("dict_rad", ch - 0x2E80);
			}
			char ch2;
			if (ch > 0xD7FF && ch < 0xDE00 && cursor + 1 < last && (ch2 = *(cursor + 1)) > 0xDBFF && ch2 < 0xE000)
			{
				cursor += 2;
				int code = (ch - 0xD800) * 1024 + (ch2 - 0xDC00) + 0x10000;//使用高位字符与低位字符获得UTF-32编码。

				/**
				 * 扩展B区汉字
				 */
				if (code > 0x1FFFF && code < 0x2A6E0)
				{
					return Common.LoadPinyinTemparory("dict_ext_b", code - 0x20000);
				}

				/**
				 * 扩展C区汉字
				 */
				if (code > 0x2A6FF && code < 0x2B739)
				{
					return Common.LoadPinyinTemparory("dict_ext_c", code - 0x2A700);
				}

				/**
				 * 扩展D区汉字
				 */
				if (code > 0x2B73F && code < 0x2B81E)
				{
					return Common.LoadPinyinTemparory("dict_ext_d", code - 0x2B740);
				}

				/**
				 * 扩展E区汉字
				 */
				if (code > 0x2B81F && code < 0x2CEA2)
				{
					return Common.LoadPinyinTemparory("dict_ext_e", code - 0x2B820);
				}

				/**
				 * 扩展F区汉字
				 */
				if (code > 0x2CEAF && code < 0x2EBE1)
				{
					return Common.LoadPinyinTemparory("dict_ext_f", code - 0x2CEB0);
				}

				/**
				 * 扩展G区汉字
				 */
				if (code > 0x2FFFF && code < 0x3134B)
				{
					return Common.LoadPinyinTemparory("dict_ext_g", code - 0x30000);
				}

				/**
				 * 兼容汉字扩展
				 */
				if (code > 0x2F7FF && code < 0x2FA20)
				{
					return Common.LoadPinyinTemparory("dict_cmp_sup", code - 0x2F800);
				}

				if (Miscellaneous.Dictionary32.TryGetValue(code, out node))
				{
					return node.Pinyin;
				}

				return null;
			}
			cursor += 1;

			if (Miscellaneous.Dictionary32.TryGetValue(ch, out node))
			{
				return node.Pinyin;
			}
			return null;
		}

		private static string LoadPinyinTemparory(string name, int index)
		{
			/**
			 * 从指定的资源名称中临时读取一个拼音。
			 */
			byte[] buffer = (byte[])Common.ResourceManager.GetObject(name);
			index = index << 1;
			return Common.Utf16NodeTemplates[(buffer[index] << 8) | buffer[index + 1]].Pinyin;
		}
		private static bool TryParseTradional(string simplified, out string result)
		{
			/**
			 * 将指定的字符串中的简体字转换为繁体字。
			 */
			char[] chars = simplified.ToCharArray();
			bool succ = false;

			for (int i = chars.Length - 1; i > -1; i--)
			{
				if (Common.traditional.TryGetValue(chars[i], out char ch))
				{
					succ = true;
					chars[i] = ch;
				}
			}

			result = succ ? new string(chars) : null;
			return succ;
		}

		internal static void LoadCustomFile(string file)
		{
			/**
			 * 从指定的文件加载自定义的拼音定义，并且更新到对应的字典中。
			 */

			using (StreamReader sr = new StreamReader(file, Encoding.UTF8, true))
			{
				int row = 0;

				/**
				 * 循环读取每一行并且进行解析
				 */
				while (!sr.EndOfStream)
				{
					/**
					 * 检查该行是否是空白
					 */
					string line = sr.ReadLine();
					if (string.IsNullOrEmpty(line))
					{
						continue;
					}

					row++;

					/**
					 * 检查该行是否有注释
					 */
					string content = line.Trim();
					int index = content.IndexOf('#');
					if (index == 0)
					{
						continue;
					}
					content = content.Substring(0, index).Trim();

					/**
					 * 检查该行是否是单字
					 */
					index = content.IndexOfAny(SeparatorCharacters);
					if (index < 1)
					{
						if (Check.IsIdeEnvironment)
						{
							throw new Exception($"检测到第{row}行自定义字典错误：'{line}'的格式不正确。");
						}

						continue;
					}

					string caption = content.Substring(0, index);
					string pinyin = content.Substring(index + 1).Trim(SeparatorCharacters);
					/**
					 * 如果区块的长度为2，说明是定义字典。
					 */
					if (caption.Length == 1 || caption.Length == 2 && char.IsHighSurrogate(caption[0]) && char.IsLowSurrogate(caption[1]))
					{
						if (!Common.OverrideDictionary(caption, Common.FixPinyin(pinyin)))
						{
							if (Check.IsIdeEnvironment)
							{
								throw new FormatException($"无法将第{row}行自定义项添加到字典：'{line}'。");
							}
						}
					}
					else
					{
						if (!Common.OverrideLexicon(caption, Common.FixPinyin(pinyin).Split(SeparatorCharacters, StringSplitOptions.RemoveEmptyEntries)))
						{
							if (Check.IsIdeEnvironment)
							{
								throw new FormatException($"无法将第{row}行自定义项添加到词典：'{line}'。");
							}
						}
					}
				}
			}
		}

		private static bool OverrideDictionary(string character, string pinyin)
		{
			PinyinNode node;

			int index;
			int code;
			Predicate<PinyinNode> searcher = n => n.Pinyin == pinyin;

			if (character.Length == 1)
			{
				index = Array.FindIndex<PinyinNode>(Common.Utf16NodeTemplates, searcher);
				if (index > 0)
				{
					node = Common.Utf16NodeTemplates[index];
				}
				else
				{
					node = new Utf16Node(pinyin);
				}

				code = character[0];
				/**
				 * 基本区
				 */
				if (code > 0x4DFF && code < 0xA000)
				{
					Basic.Dictionary[code - 0x4E00] = node;
					return true;
				}
				if (code == '〇')
				{
					Basic.Dictionary[0x5200] = node;
					return true;
				}
				if (code > 0x33FF && code < 0x4DC0)
				{
					/**
					 * 扩展A区汉字
					 */
					ExtensionA.Dictionary[code - 0x3400] = node;
					return true;
				}

				Miscellaneous.Dictionary16[code] = node;
				return true;
			}

			if (character.Length == 2)
			{
				index = Array.FindIndex<PinyinNode>(Common.Utf32NodeTemplates, searcher);
				if (index > 0)
				{
					node = Common.Utf32NodeTemplates[index];
				}
				else
				{
					node = new Utf32Node(pinyin);
				}

				if (char.IsHighSurrogate(character[0]) && char.IsLowSurrogate(character[1]))
				{
					code = char.ConvertToUtf32(character[0], character[1]);//使用高位字符与低位字符获得UTF-32编码。

					/**
					 * 扩展B区汉字
					 */
					if (code > 0x1FFFF && code < 0x2A6E0)
					{
						ExtensionB.Dictionary[code - 0x20000] = node;
						return true;
					}

					/**
					 * 扩展C区汉字
					 */
					if (code > 0x2A6FF && code < 0x2B739)
					{
						ExtensionC.Dictionary[code - 0x2A700] = node;
						return true;
					}

					/**
					 * 扩展D区汉字
					 */
					if (code > 0x2B73F && code < 0x2B81E)
					{
						ExtensionD.Dictionary[code - 0x2B740] = node;
						return true;
					}

					/**
					 * 扩展E区汉字
					 */
					if (code > 0x2B81F && code < 0x2CEA2)
					{
						ExtensionE.Dictionary[code - 0x2B820] = node;
						return true;
					}

					/**
					 * 扩展F区汉字
					 */
					if (code > 0x2CEAF && code < 0x2EBE1)
					{
						ExtensionF.Dictionary[code - 0x2CEB0] = node;
						return true;
					}

					/**
					 * 扩展G区汉字
					 */
					if (code > 0x2FFFF && code < 0x3134B)
					{
						ExtensionG.Dictionary[code - 0x30000] = node;
						return true;
					}

					Miscellaneous.Dictionary32[code] = node;
					return true;
				}
			}

			return false;
		}

		private static bool OverrideLexicon(string word, string[] pinyin)
		{

			int code;

			if (word[0] < 0xFFFF && (!char.IsHighSurrogate(word[0]) || !char.IsLowSurrogate(word[1])))
			{
				code = word[0];
				/**
				 * 基本区
				 */
				if (code > 0x4DFF && code < 0xA000)
				{
					Common.AddLexiconNode(Basic.Dictionary, code - 0x4E00, word, pinyin);
					return true;
				}
				if (code == '〇')
				{
					Common.AddLexiconNode(Basic.Dictionary, 0x5200, word, pinyin);
					return true;
				}
				if (code > 0x33FF && code < 0x4DC0)
				{
					/**
					 * 扩展A区汉字
					 */
					Common.AddLexiconNode(ExtensionA.Dictionary, code - 0x3400, word, pinyin);
					return true;
				}

				if (Miscellaneous.Dictionary16.TryGetValue(code, out PinyinNode node))
				{
					if (!(node is LexiconNode lexicon))
					{
						lexicon = new LexiconNode(node);
						Miscellaneous.Dictionary16[code] = lexicon;
					}

					lexicon.Add(word, pinyin);
				}
				else
				{
					LexiconNode lexicon = new LexiconNode(new Utf16Node(pinyin[0]));
					lexicon.Add(word, pinyin);
					Miscellaneous.Dictionary16[code] = lexicon;
				}
				return true;
			}

			if (char.IsHighSurrogate(word[0]) && char.IsLowSurrogate(word[1]))
			{
				code = char.ConvertToUtf32(word[0], word[1]);//使用高位字符与低位字符获得UTF-32编码。

				/**
				 * 扩展B区汉字
				 */
				if (code > 0x1FFFF && code < 0x2A6E0)
				{
					Common.AddLexiconNode(ExtensionB.Dictionary, code - 0x20000, word, pinyin);
					return true;
				}

				/**
				 * 扩展C区汉字
				 */
				if (code > 0x2A6FF && code < 0x2B739)
				{
					Common.AddLexiconNode(ExtensionC.Dictionary, code - 0x2A700, word, pinyin);
					return true;
				}

				/**
				 * 扩展D区汉字
				 */
				if (code > 0x2B73F && code < 0x2B81E)
				{
					Common.AddLexiconNode(ExtensionD.Dictionary, code - 0x2B740, word, pinyin);
					return true;
				}

				/**
				 * 扩展E区汉字
				 */
				if (code > 0x2B81F && code < 0x2CEA2)
				{
					Common.AddLexiconNode(ExtensionE.Dictionary, code - 0x2B820, word, pinyin);
					return true;
				}

				/**
				 * 扩展F区汉字
				 */
				if (code > 0x2CEAF && code < 0x2EBE1)
				{
					Common.AddLexiconNode(ExtensionF.Dictionary, code - 0x2CEB0, word, pinyin);
					return true;
				}

				/**
				 * 扩展G区汉字
				 */
				if (code > 0x2FFFF && code < 0x3134B)
				{
					Common.AddLexiconNode(ExtensionG.Dictionary, code - 0x30000, word, pinyin);
					return true;
				}

				if (Miscellaneous.Dictionary32.TryGetValue(code, out PinyinNode node))
				{
					if (!(node is LexiconNode lexicon))
					{
						lexicon = new LexiconNode(node);
						Miscellaneous.Dictionary32[code] = lexicon;
					}

					lexicon.Add(word, pinyin);
				}
				else
				{
					LexiconNode lexicon = new LexiconNode(new Utf32Node(pinyin[0]));
					lexicon.Add(word, pinyin);
					Miscellaneous.Dictionary32[code] = lexicon;
				}
				return true;
			}

			return false;
		}


		private static void LoadTradionalDictionary()
		{
			/**
			 * 加载所有的简体字、繁体字。
			 */
			byte[] buffer = (byte[])Common.ResourceManager.GetObject("simplified");
			for (int i = 0; i < buffer.Length; i += 4)
			{
				char cht = (char)((buffer[i] << 8) | buffer[i + 1]);
				char chs = (char)((buffer[i + 2] << 8) | buffer[i + 3]);
				Common.traditional[chs] = cht;
			}
		}

		private static void LoadLexicon(PinyinNode[] dictionary, string name, int min, int length)
		{
			/**
			 * 为指定集合中的拼音节点加载固定长度的词典内容。
			 */
			using (MemoryStream stream = new MemoryStream((byte[])Common.ResourceManager.GetObject(name)))
			{
				while (stream.Position < stream.Length)
				{
					/**
					 * 读取词汇信息，并根据词汇第一个词确定索引值。
					 */

					/**
					 * 读取词汇信息。
					 */
					string word = Common.ReadWord(stream, length);
					string[] pinyin = Common.ReadPinyinArray(stream, length);

					/**
					 * 首先尝试查找简体词汇在给定字典中的索引。
					 */
					if (Common.TryMapIndex(dictionary, min, word, out int index))
					{
						Common.AddLexiconNode(dictionary, index, word, pinyin);
					}

					/**
					 * 接着尝试查找繁体词汇在给点字典中的索引。
					 */
					if (Common.TryParseTradional(word, out string traditional) && traditional != word && Common.TryMapIndex(dictionary, min, traditional, out index))
					{
						Common.AddLexiconNode(dictionary, index, traditional, pinyin);
					}
				}
			}
		}

		private static void LoadExtraLexicon(PinyinNode[] dictionary, string name, int min)
		{
			/**
			 * 为指定集合中的拼音节点加载无固定长度的词典的内容。
			 */
			using (MemoryStream stream = new MemoryStream((byte[])Common.ResourceManager.GetObject(name)))
			{
				while (stream.Position < stream.Length)
				{
					/**
					 * 首先读取长度，接着根据长度读取词汇信息，并根据词汇第一个词确定索引值。
					 */
					int length = stream.ReadByte();

					/**
					 * 读取词汇信息。
					 */
					string word = Common.ReadWord(stream, length);
					string[] pinyin = Common.ReadPinyinArray(stream, length);

					/**
					 * 首先尝试查找简体词汇在给定字典中的索引。
					 */
					if (Common.TryMapIndex(dictionary, min, word, out int index))
					{
						Common.AddLexiconNode(dictionary, index, word, pinyin);
					}

					/**
					 * 接着尝试查找繁体词汇在给点字典中的索引。
					 */
					if (Common.TryParseTradional(word, out string traditional) && traditional != word && Common.TryMapIndex(dictionary, min, traditional, out index))
					{
						Common.AddLexiconNode(dictionary, index, traditional, pinyin);
					}
				}
			}
		}

		private static void LoadIntersectionLexicon(PinyinNode[] dictionary, string name, int min, int length)
		{
			/**
			 * 为指定集合中的拼音节点加载固定长度的交叉词典内容。
			 */
			using (MemoryStream stream = new MemoryStream((byte[])Common.ResourceManager.GetObject(name)))
			{
				while (stream.Position < stream.Length)
				{
					/**
					 * 读取词汇信息，并根据词汇第一个词确定索引值。
					 */

					/**
					 * 读取词汇信息。
					 */
					string word = Common.ReadWord(stream, length);
					string[] pinyin = null;

					/**
					 * 首先尝试查找简体词汇在给定字典中的索引。
					 */
					if (Common.TryMapIndex(dictionary, min, word, out int index))
					{
						pinyin = Common.LoadPinyinArray(word, length);
						Common.AddLexiconNode(dictionary, index, word, pinyin);
					}

					/**
					 * 接着尝试查找繁体词汇在给点字典中的索引。
					 */
					if (Common.TryParseTradional(word, out string traditional) && traditional != word && Common.TryMapIndex(dictionary, min, traditional, out index))
					{
						Common.AddLexiconNode(dictionary, index, traditional, pinyin ?? Common.LoadPinyinArray(traditional, length));
					}
				}
			}
		}

		private static unsafe void LoadExtraIntersectionLexicon(PinyinNode[] dictionary, string name, int min)
		{
			/**
			 * 为指定集合中的拼音节点加载固定长度的交叉词典内容。
			 */

			using (MemoryStream stream = new MemoryStream((byte[])Common.ResourceManager.GetObject(name)))
			{
				while (stream.Position < stream.Length)
				{
					/**
					 * 首先读取长度，接着根据长度读取词汇信息，并根据词汇第一个词确定索引值。
					 */
					int length = stream.ReadByte();

					/**
					 * 读取词汇信息。
					 */
					string word = Common.ReadWord(stream, length);
					string[] pinyin = null;

					/**
					 * 首先尝试查找简体词汇在给定字典中的索引。
					 */
					if (Common.TryMapIndex(dictionary, min, word, out int index))
					{
						pinyin = Common.LoadPinyinArray(word, length);
						Common.AddLexiconNode(dictionary, index, word, pinyin);
					}

					/**
					 * 接着尝试查找繁体词汇在给点字典中的索引。
					 */
					if (Common.TryParseTradional(word, out string traditional) && traditional != word && Common.TryMapIndex(dictionary, min, traditional, out index))
					{
						Common.AddLexiconNode(dictionary, index, traditional, pinyin ?? Common.LoadPinyinArray(traditional, length));
					}
				}
			}
		}

		private static string ReadWord(MemoryStream stream, int length)
		{
			char[] chars = new char[length];
			for (int i = 0; i < length; i++)
			{
				chars[i] = (char)((stream.ReadByte() << 8) | stream.ReadByte());
			}

			return new string(chars);
		}

		private static unsafe string[] LoadPinyinArray(string word, int length)
		{
			string[] pinyin = new string[length];

			fixed (char* p = word)
			{
				char* cursor = p;
				char* last = p + length;
				int index = 0;
				do
				{
					pinyin[index++] = Common.LoadPinyinTemparory(ref cursor, last);
				} while (index < length);
			}

			return pinyin;
		}

		private static string[] ReadPinyinArray(MemoryStream stream, int length)
		{
			string[] pinyin = new string[length];

			for (int i = 0; i < length; i++)
			{
				int byte1 = stream.ReadByte();
				int byte2 = stream.ReadByte();
				pinyin[i] = Common.Utf16NodeTemplates[(byte1 << 8) | byte2].Pinyin;
			}

			return pinyin;
		}

		private static bool TryMapIndex(PinyinNode[] dictionary, int min, string word, out int index)
		{
			if (char.IsHighSurrogate(word[0]) && char.IsLowSurrogate(word[1]))
			{
				index = char.ConvertToUtf32(word[0], word[1]) - min;
			}
			else
			{
				index = word[0] - min;
			}
			if (index < 0 || index >= dictionary.Length)
			{
				if (min == 0x4E00 && word[0] == '〇')//额外补上‘〇’的索引
				{
					index = 0x5200;
				}
				else
				{
					/**
					 * 如果索引值不在提供的集合的范围，则忽略，以免加载不需要的内容。
					 */
					index = -1;
					return false;
				}
			}

			return true;
		}

		private static void AddLexiconNode(PinyinNode[] dictionary, int index, string word, string[] pinyin)
		{
			LexiconNode node = dictionary[index] as LexiconNode;
			if (node == null)
			{
				/**
				 * 如果当前的节点不是词典节点，则创建一个新的词典节点。
				 */
				dictionary[index] = node = new LexiconNode(dictionary[index]);
			}

			node.Add(word, pinyin);
		}

	}
}