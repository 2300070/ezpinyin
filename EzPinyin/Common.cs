using System;
using System.Collections.Generic;
using System.Globalization;
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
		private static readonly Dictionary<char, VariantInfo> variants = new Dictionary<char, VariantInfo>();

		/// <summary>
		/// 一个标记，表示节点所对应的字符包含词汇信息，在使用时需要额外处理，参看<see cref="LexiconFakeNode"/>。
		/// </summary>
		internal const int LEXICON_FLAG = 0x8000;

		internal static readonly char[] CharacterSeparator = { ' ', '	', ' ' };

		internal static readonly PinyinNode[] Utf16Templates;

		internal static readonly PinyinNode[] Utf32Templates;

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
			using (StringReader sr = new StringReader(ResourceManager.GetString("pinyin")))
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

			Utf16Templates = utf16Nodes.ToArray();
			Utf32Templates = utf32Nodes.ToArray();

			/**
			 * 加载繁体字字典
			 */
			Common.LoadVariantDictionary();


		}

		internal static PinyinNode[] LoadDictionary(string resourceName, PinyinNode[] templates, int head)
		{
			/**
			 * 从指定的资源字典加载拼音集合作为字典。
			 */
			byte[] buffer = (byte[])ResourceManager.GetObject(resourceName);

			int length = buffer.Length;
			PinyinNode[] result = new PinyinNode[length >> 1];
			for (int i = 0; i < length; i += 2)
			{
				int pinyinIndex = (buffer[i] << 8) | buffer[i + 1];
				int index = i >> 1;
				if ((pinyinIndex & Common.LEXICON_FLAG) == Common.LEXICON_FLAG)
				{
					string character;
					if (head < 0xFFFF)
					{
						if (head == 0x4E00 && index == 0x5200)
						{
							/**
							 * 修正字符‘〇’。
							 */
							character = "〇";
						}
						else
						{
							character = new string((char)(head + index), 1);
						}
					}
					else
					{
						character = char.ConvertFromUtf32(head + index);
					}
					pinyinIndex &= Common.LEXICON_FLAG - 1;
					result[index] = new LexiconFakeNode(character, templates[pinyinIndex], result, index);
				}
				else
				{
					result[index] = templates[pinyinIndex];
				}
			}

			return result;
		}

		internal static void LoadUserFiles()
		{
			/**
			 * 加载用户自定义的配置文件。
			 */
			UserFileLoader.LoadAll();
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
					chars[i] = (char)('a' + ch - 'A');
				}
				//全角转半角
				else if (ch >= 'ａ' && ch <= 'ｚ')
				{
					chars[i] = (char)('a' + ch - 'ａ');
				}
				else if (ch >= 'Ａ' && ch <= 'Ｚ')
				{
					chars[i] = (char)('a' + ch - 'Ａ');
				}
				else
				{
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

					}
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

				if (Unknown.Utf32Nodes.TryGetValue(code, out result))
				{
					return result;
				}
				return Utf32EmptyNode.Instance;
			}
			if (Unknown.Utf16Nodes.TryGetValue(ch, out result))
			{
				return result;
			}

			//匹配其它无法匹配的情况
			if (ch < 0x100)
			{
				return Ascii.Dictionary[ch];
			}

			switch (char.GetUnicodeCategory(ch))
			{
				//字母
				case UnicodeCategory.UppercaseLetter:
				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.TitlecaseLetter:
				case UnicodeCategory.ModifierLetter:
				case UnicodeCategory.OtherLetter:
				//数字
				case UnicodeCategory.DecimalDigitNumber:
				case UnicodeCategory.LetterNumber:
				case UnicodeCategory.OtherNumber:
					return Utf16EmptyNode.Instance;
			}
			return UnknownNode.Instance;
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

			if (Unknown.Utf16Nodes.TryGetValue(ch, out PinyinNode result))
			{
				return result;
			}

			//匹配其它无法匹配的情况
			if (ch < 0x100)
			{
				return Ascii.Dictionary[ch];
			}

			switch (char.GetUnicodeCategory(ch))
			{
				//字母
				case UnicodeCategory.UppercaseLetter:
				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.TitlecaseLetter:
				case UnicodeCategory.ModifierLetter:
				case UnicodeCategory.OtherLetter:
				//数字
				case UnicodeCategory.DecimalDigitNumber:
				case UnicodeCategory.LetterNumber:
				case UnicodeCategory.OtherNumber:
					return Utf16EmptyNode.Instance;
			}
			return UnknownNode.Instance;
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

		internal static bool TryGetVariant(char ch, out VariantInfo variant) => variants.TryGetValue(ch, out variant);
		internal static bool TryGetVariant(string text, VariantType type, out string result)
		{
			/**
			 * 转换指定字符串中的简体字或繁体字。
			 */
			char[] chars = text.ToCharArray();
			bool succ = false;

			for (int i = chars.Length - 1; i > -1; i--)
			{
				if (variants.TryGetValue(chars[i], out VariantInfo info) && info.VariantType == type)
				{
					succ = true;
					chars[i] = info.Character;
				}
			}

			result = succ ? new string(chars) : null;
			return succ;
		}

		internal static void LoadFrom(string file, PinyinPriority priority)
		{
			/**
			 * 从指定的文件加载自定义的拼音定义，并且更新到对应的字典中。
			 */

			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException(nameof(file));
			}

			if (!File.Exists(file))
			{
				throw new FileNotFoundException(file);
			}

			using (StreamReader sr = new StreamReader(file, Encoding.UTF8, true))
			{
				Common.LoadFrom(sr, priority);
			}
		}

		internal static void LoadFrom(TextReader reader, PinyinPriority priority)
		{
			int row = 0;

			/**
			 * 循环读取每一行并且进行解析
			 */
			while (reader.Peek() > -1)
			{
				string line = reader.ReadLine();
				row++;

				/**
				 * 检查该行是否是空白
				 */
				if (string.IsNullOrEmpty(line))
				{
					continue;
				}

				/**
				 * 检查该行是否有注释
				 */
				string content = line.Trim();
				int index = content.IndexOf('#');
				if (index == 0)
				{
					continue;
				}

				if (index > 0)
				{
					content = content.Substring(0, index).Trim();
				}
				if (content.Length == 0)
				{
					continue;
				}

				/**
				 * 检查该行是否是单字
				 */
				index = content.IndexOfAny(CharacterSeparator);
				if (index < 1)
				{
					if (Check.IsIdeEnvironment)
					{
						throw new Exception($"检测到第{row}行的自定义配置错误：'{line}'的格式不正确。");
					}

					continue;
				}

				string caption = content.Substring(0, index);
				string pinyin = content.Substring(index + 1).Trim(CharacterSeparator);

				/**
				 * 如果区块的长度为2，说明是定义字典。
				 */
				if (caption.Length == 1 || caption.Length == 2 && char.IsHighSurrogate(caption[0]) && char.IsLowSurrogate(caption[1]))
				{
					if (!Common.OverrideDictionary(caption, Common.FixPinyin(pinyin)))
					{
						if (Check.IsIdeEnvironment)
						{
							throw new FormatException($"无法将第{row}行的自定义项添加到字典：'{line}'。");
						}
					}
				}
				else
				{
					if (!Common.OverrideLexicon(caption, Common.FixPinyin(pinyin).Split(CharacterSeparator, StringSplitOptions.RemoveEmptyEntries), priority))
					{
						if (Check.IsIdeEnvironment)
						{
							throw new FormatException($"无法将第{row}行的自定义项添加到词典：'{line}'。");
						}
					}
				}
			}
		}

		internal static bool OverrideDictionary(string character, string pinyin)
		{
			PinyinNode node;

			int index;
			int code;

			if (character.Length == 1)
			{
				index = Common.FindIndex(Utf16Templates, pinyin);
				if (index > 0)
				{
					node = Utf16Templates[index];
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

				Unknown.Utf16Nodes[code] = node;
				return true;
			}

			if (character.Length == 2)
			{
				index = Common.FindIndex(Utf32Templates, pinyin);
				if (index > 0)
				{
					node = Utf32Templates[index];
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

					Unknown.Utf32Nodes[code] = node;
					return true;
				}
			}

			return false;
		}

		internal static bool OverrideLexicon(string word, string[] pinyin, PinyinPriority priority)
		{
			if (Common.OverrideLexiconItem(word, pinyin, priority))
			{
				if (Common.TryGetVariant(word, VariantType.Traditional, out string traditional))
				{
					Common.OverrideLexiconItem(traditional, pinyin, priority);
				}
				return true;
			}
			return false;
		}



		private static bool OverrideLexiconItem(string word, string[] pinyin, PinyinPriority priority)
		{
			int code;

			if (!char.IsHighSurrogate(word[0]) || !char.IsLowSurrogate(word[1]))
			{
				code = word[0];

				/**
				 * 基本区
				 */
				if (code > 0x4DFF && code < 0xA000)
				{
					Common.AddLexiconNode(Basic.Dictionary, code - 0x4E00, word, pinyin, priority);
					return true;
				}
				if (code == '〇')
				{
					Common.AddLexiconNode(Basic.Dictionary, 0x5200, word, pinyin, priority);
					return true;
				}
				if (code > 0x33FF && code < 0x4DC0)
				{
					/**
					 * 扩展A区汉字
					 */
					Common.AddLexiconNode(ExtensionA.Dictionary, code - 0x3400, word, pinyin, priority);
					return true;
				}

				if (Unknown.Utf16Nodes.TryGetValue(code, out PinyinNode node))
				{
					if (!(node is LexiconNode lexicon))
					{
						lexicon = new LexiconNode(node);
						Unknown.Utf16Nodes[code] = lexicon;
					}

					lexicon.Add(word, pinyin, priority);
				}
				else
				{
					LexiconNode lexicon = new LexiconNode(new Utf16Node(pinyin[0]));
					lexicon.Add(word, pinyin, priority);
					Unknown.Utf16Nodes[code] = lexicon;
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
					Common.AddLexiconNode(ExtensionB.Dictionary, code - 0x20000, word, pinyin, priority);
					return true;
				}

				/**
				 * 扩展C区汉字
				 */
				if (code > 0x2A6FF && code < 0x2B739)
				{
					Common.AddLexiconNode(ExtensionC.Dictionary, code - 0x2A700, word, pinyin, priority);
					return true;
				}

				/**
				 * 扩展D区汉字
				 */
				if (code > 0x2B73F && code < 0x2B81E)
				{
					Common.AddLexiconNode(ExtensionD.Dictionary, code - 0x2B740, word, pinyin, priority);
					return true;
				}

				/**
				 * 扩展E区汉字
				 */
				if (code > 0x2B81F && code < 0x2CEA2)
				{
					Common.AddLexiconNode(ExtensionE.Dictionary, code - 0x2B820, word, pinyin, priority);
					return true;
				}

				/**
				 * 扩展F区汉字
				 */
				if (code > 0x2CEAF && code < 0x2EBE1)
				{
					Common.AddLexiconNode(ExtensionF.Dictionary, code - 0x2CEB0, word, pinyin, priority);
					return true;
				}

				/**
				 * 扩展G区汉字
				 */
				if (code > 0x2FFFF && code < 0x3134B)
				{
					Common.AddLexiconNode(ExtensionG.Dictionary, code - 0x30000, word, pinyin, priority);
					return true;
				}

				if (Unknown.Utf32Nodes.TryGetValue(code, out PinyinNode node))
				{
					if (!(node is LexiconNode lexicon))
					{
						lexicon = new LexiconNode(node);
						Unknown.Utf32Nodes[code] = lexicon;
					}

					lexicon.Add(word, pinyin, priority);
				}
				else
				{
					LexiconNode lexicon = new LexiconNode(new Utf32Node(pinyin[0]));
					lexicon.Add(word, pinyin, priority);
					Unknown.Utf32Nodes[code] = lexicon;
				}
				return true;
			}

			return false;
		}

		private static int FindIndex(PinyinNode[] templates, string pinyin)
		{
			for (int i = 1; i < templates.Length; i++)
			{
				if (templates[i].Pinyin == pinyin)
				{
					return i;
				}
			}
			return -1;
		}

		private static void LoadVariantDictionary()
		{
			/**
			 * 加载所有的简体字、繁体字的转换字典。
			 */
			byte[] buffer = (byte[])ResourceManager.GetObject("variants");
			for (int i = 0; i < buffer.Length; i += 4)
			{
				char cht = (char)((buffer[i] << 8) | buffer[i + 1]);
				char chs = (char)((buffer[i + 2] << 8) | buffer[i + 3]);
				variants[chs] = new VariantInfo(cht, VariantType.Traditional);
				variants[cht] = new VariantInfo(chs, VariantType.Simplified);
			}
		}

		private static void AddLexiconNode(PinyinNode[] dictionary, int index, string word, string[] pinyin, PinyinPriority priority)
		{
			if (dictionary[index] is LexiconFakeNode fake)
			{
				fake.LoadActualNode();
			}
			LexiconNode node = dictionary[index] as LexiconNode;
			if (node == null)
			{
				/**
				 * 如果当前的节点不是词典节点，则创建一个新的词典节点。
				 */
				dictionary[index] = node = new LexiconNode(dictionary[index]);
			}

			node.Add(word, pinyin, priority);
		}



	}
}