using System;
using System.Collections.Generic;
using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示用来加载词典节点的类。
	/// </summary>
	internal static class LexiconLoader
	{
		//基本词典，即同时包含词汇与拼音的词典
		private static readonly byte[] basic2;
		private static readonly byte[] basic3;
		private static readonly byte[] basic4;
		//词汇长度不规则的词典
		private static readonly byte[] basicX;

		//补充词典，仅包含词汇，拼音为该字符最常见读音。
		private static readonly byte[] auxilliary2;
		private static readonly byte[] auxilliary3;
		private static readonly byte[] auxilliary4;
		//词汇长度不规则的词典
		private static readonly byte[] auxilliaryX;

		static LexiconLoader()
		{
			try
			{
				/**
				 * 主要的词典内容。
				 * 此词典的内容是包含多音字且读音与对应多音字不一样的词汇。
				 */
				basic2 = (byte[])Common.ResourceManager.GetObject("lex_basic_2");
				basic3 = (byte[])Common.ResourceManager.GetObject("lex_basic_3");
				basic4 = (byte[])Common.ResourceManager.GetObject("lex_basic_4");
				basicX = (byte[])Common.ResourceManager.GetObject("lex_basic_x");

				/**
				 * 接着加载补充词典内容。
				 * 所谓补充词典是指虽然不包含多音字读音，但是有必要记录以防止出现解析错误的词汇。
				 */
				auxilliary2 = (byte[])Common.ResourceManager.GetObject("lex_auxilliary_2");
				auxilliary3 = (byte[])Common.ResourceManager.GetObject("lex_auxilliary_3");
				auxilliary4 = (byte[])Common.ResourceManager.GetObject("lex_auxilliary_4");
				auxilliaryX = (byte[])Common.ResourceManager.GetObject("lex_auxilliary_x");
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}
		}

		internal static void LoadLexicon(string character, PinyinNode[] dictionary, int index)
		{
			/**
			 * 加载指定字符所对应的词典词条。
			 */

			//加载标准的词条
			LexiconLoader.LoadLexicon(basic2, character, dictionary, index, 2);
			LexiconLoader.LoadLexicon(basic3, character, dictionary, index, 3);
			LexiconLoader.LoadLexicon(basic4, character, dictionary, index, 4);
			LexiconLoader.LoadExtraLexicon(basicX, character, dictionary, index);

			//加载辅助词条
			LexiconLoader.LoadLexicon(auxilliary2, character, dictionary, index, 2, true);
			LexiconLoader.LoadLexicon(auxilliary3, character, dictionary, index, 3, true);
			LexiconLoader.LoadLexicon(auxilliary4, character, dictionary, index, 4, true);
			LexiconLoader.LoadExtraLexicon(auxilliaryX, character, dictionary, index, true);
		}

		private static unsafe void LoadLexicon(byte[] buffer, string character, PinyinNode[] dictionary, int index, int size, bool auxilliary = false)
		{
			/**
			 * 从指定的词典数据中以规则的方式解析指定字符开始的词条。
			 */

			fixed (byte* ptr = buffer)
			{
				byte* cursor = ptr;
				byte* end = ptr + buffer.Length;

				/**
				 * 首先定位游标到字符对应的第一个词条位置。
				 */
				if (!LexiconLoader.TryLocate(ref cursor, end, character, size * (auxilliary ? 2 : 4)))
				{
					return;
				}

				/**
				 * 轮流读取词汇与对应拼音进行注册，直到读取的词汇不满足条件为止。
				 */
				string word;
				do
				{
					word = LexiconLoader.ReadCharacter(ref cursor, size);

					//读取词汇
					if (word.StartsWith(character))
					{

						/**
						 * 如果是辅助词条，由于没有拼音信息，所以需要直接从资源文件读取。如果是标准词条，则直接读取即可。
						 */
						string[] pinyin = auxilliary ? LexiconLoader.ReadPinyinArrayDirectly(word) : LexiconLoader.ReadPinyinArray(ref cursor, size);

						if (pinyin == null)
						{
#if DEBUG
							throw new Exception($"读取'{word}'的拼音失败：出现未知的异常。");
#else
							continue;
#endif
						}

						LexiconLoader.AddLexicon(word, pinyin, dictionary, index);
					}
					else
					{
						return;
					}

				} while (cursor < end);
			}

		}

		private static unsafe void LoadExtraLexicon(byte[] buffer, string character, PinyinNode[] dictionary, int index, bool auxilliary = false)
		{
			/**
			 * 从指定的词典数据中以不规则的方式解析指定字符开始的词条。
			 */

			fixed (byte* ptr = buffer)
			{
				byte* cursor = ptr;
				byte* end = ptr + buffer.Length;


				/**
				 * 轮流读取词汇与对应拼音进行注册，直到读取的词汇不满足条件为止。
				 */
				string word;
				do
				{

					//读取一个字节以获取词汇长度
					int size = *(cursor++);

					//读取词汇
					word = LexiconLoader.ReadCharacter(ref cursor, size);
					if (word.StartsWith(character))
					{

						/**
						 * 如果是辅助词条，由于没有拼音信息，所以需要直接从资源文件读取。如果是标准词条，则直接读取即可。
						 */
						string[] pinyin = auxilliary ? LexiconLoader.ReadPinyinArrayDirectly(word) : LexiconLoader.ReadPinyinArray(ref cursor, size);

						if (pinyin == null)
						{
#if DEBUG
							throw new Exception($"读取'{word}'的拼音失败：出现未知的异常。");
#else
							continue;
#endif
						}

						LexiconLoader.AddLexicon(word, pinyin, dictionary, index);
					}
					else if (string.CompareOrdinal(word, character) > 0)
					{
						return;
					}
					else
					{
						return;
					}

				} while (cursor < end);
			}

		}

		private static void AddLexicon(string word, string[] pinyin, PinyinNode[] dictionary, int index)
		{
			LexiconNode node = dictionary[index] as LexiconNode;
			if (node == null)
			{
				/**
				 * 如果当前的节点不是词典节点，则创建一个新的词典节点。
				 */
				dictionary[index] = node = new LexiconNode(dictionary[index]);
			}

			node.Add(word, pinyin, PinyinPriority.Normal);

			string[] variants = LexiconLoader.GetVariantWords(word);
			foreach (string variant in variants)
			{
				node.Add(variant, pinyin, PinyinPriority.Low);
			}
		}

		private static string[] GetVariantWords(string word)
		{
			List<string> words = new List<string>(1);
			StringBuilder buffer = new StringBuilder();

			LexiconLoader.AnalyseVariantWords(word, buffer, 0, words);

			return words.ToArray();
		}

		private static void AnalyseVariantWords(string word, StringBuilder buffer, int index, List<string> words)
		{
			if (index >= word.Length)
			{
				string newWord = buffer.ToString();
				if (newWord != word)
				{
					words.Add(newWord);
				}
				return;
			}
			char ch = word[index];
			if (Common.TryGetVariant(ch, out VariantInfo variant))
			{
				LexiconLoader.AnalyseVariantWords(word, new StringBuilder(buffer.ToString()).Append(variant.Character), index + 1, words);
			}
			LexiconLoader.AnalyseVariantWords(word, buffer.Append(ch), index + 1, words);
		}

		private static unsafe bool TryLocate(ref byte* cursor, byte* end, string character, int size)
		{
			BEGIN:

			if (cursor > end)
			{
				return false;
			}

			/**
			 * 检查头部。
			 */
			string ch = LexiconLoader.ReadCharacter(cursor, character.Length);
			if (ch == character)
			{
				return true;
			}

			if (string.CompareOrdinal(ch, character) > 0)
			{
				return false;
			}

			/**
			 * 如果无法通过头部定位，则尝试通过尾部定位。
			 */
			byte* ptr;
			long length = (end - cursor) / size;
			if (length > 1)
			{
				ptr = end - size;
				ch = LexiconLoader.ReadCharacter(ptr, character.Length);
				if (ch == character)
				{
					/**
					 * 如果当前读取的字符命中，则前向查找直到找到第一个匹配的节点。
					 */
					cursor = LexiconLoader.LocateFirstNode(ptr, cursor, character, size);
					return true;
				}

				if (string.CompareOrdinal(ch, character) < 0)
				{

					/**
					 * 如果当前读取的字符小于需要查找的字符，由于整个区间是排序的，所以无论如何都不太可能命中。
					 */
					return false;
				}

				/**
				 * 使用二分法查找
				 */
				ptr = cursor + (length / 2) * size;
				ch = LexiconLoader.ReadCharacter(ptr, character.Length);

				/**
				 * 如果中间的位置刚好命中，则前向查找直到找到第一个匹配的节点。
				 */
				if (ch == character)
				{
					cursor = LexiconLoader.LocateFirstNode(ptr, cursor, character, size);
					return true;
				}

				//如果未命中，但当前字符小于待搜索字符，说明目标可能在右侧，则下一轮继续查找右侧。
				if (string.CompareOrdinal(ch, character) < 0)
				{
					cursor = ptr + size;
					end -= size;
					goto BEGIN;
				}

				//否则查找左侧。
				cursor += size;
				end = ptr;
				goto BEGIN;

			}

			return false;
		}

		private static unsafe byte* LocateFirstNode(byte* cursor, byte* start, string character, int size)
		{
			/**
			 * 如果当前读取的字符命中，则要前向查找直到第一个字符命中为止。
			 */
			while (cursor > start)
			{
				if (LexiconLoader.ReadCharacter(cursor - size, character.Length) != character)
				{
					return cursor;
				}

				cursor -= size;
			}

			throw new Exception("出现了未知的异常，不应该运行到此处。");
		}

		private static unsafe string[] ReadPinyinArrayDirectly(string word)
		{
			/**
			 * 直接从资源包加载拼音信息，而不从相应的API读取，避免触发各Unicode平面相关类型的初始化，导致不必要的内存占用。
			 */
			List<string> list = new List<string>();
			fixed (char* p = word)
			{
				char* cursor = p;
				char* end = p + word.Length;
				do
				{
					string item = LexiconLoader.ReadPinyinDirectly(ref cursor, end);
					if (item == null)
					{
						return null;
					}
					list.Add(item);
				} while (cursor < end);
			}
			return list.ToArray();
		}

		private static unsafe string ReadPinyinDirectly(ref char* cursor, char* end)
		{
			PinyinNode node;
			char ch = *cursor;
			if (ch > 0x4DFF && ch < 0xA000)
			{
				cursor += 1;
				return LexiconLoader.ReadPinyinDirectlyFrom("dict_basic", ch - 0x4E00);
			}

			if (ch == '〇')
			{
				cursor += 1;
				return LexiconLoader.ReadPinyinDirectlyFrom("dict_basic", 0x5200);
			}

			if (ch > 0x33FF && ch < 0x4DC0)
			{
				cursor += 1;
				return LexiconLoader.ReadPinyinDirectlyFrom("dict_ext_a", ch - 0x3400);
			}

			if (ch > 0xF8FF && ch < 0xFB00)
			{
				cursor += 1;
				return LexiconLoader.ReadPinyinDirectlyFrom("dict_cmp", ch - 0xF900);
			}

			if (ch > 0x2E7F && ch < 0x2FE0)
			{
				cursor += 1;
				return LexiconLoader.ReadPinyinDirectlyFrom("dict_rad", ch - 0x2E80);
			}
			char ch2;
			if (ch > 0xD7FF && ch < 0xDE00 && cursor + 1 < end && (ch2 = *(cursor + 1)) > 0xDBFF && ch2 < 0xE000)
			{
				cursor += 2;
				int code = (ch - 0xD800) * 1024 + (ch2 - 0xDC00) + 0x10000;//使用高位字符与低位字符获得UTF-32编码。

				/**
				 * 扩展B区汉字
				 */
				if (code > 0x1FFFF && code < 0x2A6E0)
				{
					return LexiconLoader.ReadPinyinDirectlyFrom("dict_ext_b", code - 0x20000);
				}

				/**
				 * 扩展C区汉字
				 */
				if (code > 0x2A6FF && code < 0x2B739)
				{
					return LexiconLoader.ReadPinyinDirectlyFrom("dict_ext_c", code - 0x2A700);
				}

				/**
				 * 扩展D区汉字
				 */
				if (code > 0x2B73F && code < 0x2B81E)
				{
					return LexiconLoader.ReadPinyinDirectlyFrom("dict_ext_d", code - 0x2B740);
				}

				/**
				 * 扩展E区汉字
				 */
				if (code > 0x2B81F && code < 0x2CEA2)
				{
					return LexiconLoader.ReadPinyinDirectlyFrom("dict_ext_e", code - 0x2B820);
				}

				/**
				 * 扩展F区汉字
				 */
				if (code > 0x2CEAF && code < 0x2EBE1)
				{
					return LexiconLoader.ReadPinyinDirectlyFrom("dict_ext_f", code - 0x2CEB0);
				}

				/**
				 * 扩展G区汉字
				 */
				if (code > 0x2FFFF && code < 0x3134B)
				{
					return LexiconLoader.ReadPinyinDirectlyFrom("dict_ext_g", code - 0x30000);
				}

				/**
				 * 兼容汉字扩展
				 */
				if (code > 0x2F7FF && code < 0x2FA20)
				{
					return LexiconLoader.ReadPinyinDirectlyFrom("dict_cmp_sup", code - 0x2F800);
				}

				if (Unknown.Utf32Nodes.TryGetValue(code, out node))
				{
					return node.Pinyin;
				}

				return null;
			}
			cursor += 1;

			if (Unknown.Utf16Nodes.TryGetValue(ch, out node))
			{
				return node.Pinyin;
			}
			return null;
		}

		private static string ReadPinyinDirectlyFrom(string name, int index)
		{
			/**
			 * 从指定的资源中直接读取指定索引位置处的拼音。
			 */
			byte[] buffer = (byte[])Common.ResourceManager.GetObject(name);
			index <<= 1;
			index = (buffer[index] << 8) | buffer[index + 1];
			if ((index & Common.LEXICON_FLAG) == Common.LEXICON_FLAG)
			{
				index &= Common.LEXICON_FLAG - 1;
			}
			return Common.Utf16Templates[index].Pinyin;
		}

		private static unsafe string ReadCharacter(byte* cursor, int size) => LexiconLoader.ReadCharacter(ref cursor, size);

		private static unsafe string ReadCharacter(ref byte* cursor, int size)
		{
			char[] chars = new char[size];
			for (int i = 0; i < size; i++)
			{
				chars[i] = (char)((*cursor << 8) | *(cursor + 1));
				cursor += 2;
			}
			return new string(chars);
		}
		private static unsafe string[] ReadPinyinArray(ref byte* cursor, int size)
		{
			string[] pinyin = new string[size];

			for (int i = 0; i < size; i++)
			{
				pinyin[i] = Common.Utf16Templates[(*cursor << 8) | *(cursor + 1)].Pinyin;
				cursor += 2;
			}

			return pinyin;
		}
	}
}