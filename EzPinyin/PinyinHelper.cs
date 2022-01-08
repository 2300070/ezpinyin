using System;
using System.IO;
using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// EzPinyin类库的功能入口，用来快速解析拼音、首字母及其他功能。
	/// </summary>
	public static unsafe class PinyinHelper
	{
		/// <summary>
		/// 将指定的字符串转换为对应的拼音字符串。
		/// </summary>
		/// <param name="text">需要处理的字符串</param>
		/// <param name="separator">额外指定一个用于分隔的字符串。</param>
		/// <returns><paramref name="text"/>对应的拼音字符串，如果<paramref name="text"/>为null，则返回null。</returns>
		public static string GetPinyin(string text, string separator = " ")
		{
			if (text == null)
			{
				return null;
			}


			int length = text.Length;

			fixed (char* p = text)
			{
				if (length > 1)
				{
					StringBuilder buffer = Common.AcquireBuffer();
					char* cursor = p;
					char* final = p + length - 1;

					/**
					 * 循环读取每个字符对应的节点，写入处理结果，并且自动移动游标到下一个字符的位置。
					 * 在没有读到最后一个字符之前时，可能得到一个UTF-16或者UTF-32的字符，所以调用<see cref="Common.MapAnyNode(char*)"/>方法。
					 */
					do
					{

						Common.MapAnyNode(cursor).WritePinyin(ref cursor, final, buffer, separator);

					} while (cursor < final);

					/**
					 * 如果游标与最终的指针相等，说明还余下一个字符没有处理，需要补上最后一个字符。
					 */
					if (cursor == final)
					{
						/**
						 * 由于是最后一个字符，肯定不存在UTF-32字符的可能性，所以直接调用<see cref="Common.MapUtf16Node(char*)"/>方法即可。
						 */
						buffer.Append(Common.MapUtf16Node(cursor).GetPinyin(cursor));
					}

					return Common.ReturnBuffer(buffer);

				}

				if (length == 1)
				{
					return Common.MapUtf16Node(p).GetPinyin(p);
				}

				return string.Empty;
			}


		}


		/// <summary>
		/// 将指定的字符串转换为对应的拼音数组。
		/// </summary>
		/// <param name="text">需要处理的字符串</param>
		/// <returns>包含输入字符串的拼音信息的结果数组，若<paramref name="text"/>为null，则返回null。</returns>
		public static string[] GetArray(string text)
		{
			if (text == null)
			{
				return null;
			}


			int length = text.Length;

			fixed (char* p = text)
			{
				if (length > 1)
				{
					char* cursor = p;
					char* final = p + length - 1;
					string[] buffer = new string[length];
					int index = 0;

					/**
					 * 循环读取每个字符对应的节点，写入处理结果，并且自动移动游标到下一个字符的位置。
					 * 在没有读到最后一个字符之前时，可能得到一个UTF-16或者UTF-32的字符，所以调用<see cref="Common.MapAnyNode(char*)"/>方法。
					 */
					do
					{
						Common.MapAnyNode(cursor).WritePinyin(ref cursor, final, buffer, ref index);

					} while (cursor < final);

					/**
					 * 如果游标与最终的指针相等，说明还余下一个字符没有处理，需要补上最后一个字符。
					 */
					if (cursor == final)
					{
						/**
						 * 由于是最后一个字符，肯定不存在UTF-32字符的可能性，所以直接调用<see cref="Common.MapUtf16Node(char*)"/>方法即可。
						 */
						buffer[index++] = Common.MapUtf16Node(cursor).GetPinyin(cursor);
					}

					/**
					 * 如果index与length完全相等，说明没有UTF-32字符，可以直接返回，否则需要进行相应裁剪。
					 */
					if (index == length)
					{
						return buffer;
					}
					string[] array = new string[index];
					Array.Copy(buffer, array, index);
					return array;

				}

				if (length == 1)
				{
					return new[] { Common.MapUtf16Node(p).GetPinyin(p) };
				}

				return Common.EmptyArray;
			}

		}

		/// <summary>
		/// 将指定的字符串转换为对应的拼音首字母字符串。
		/// </summary>
		/// <param name="text">需要处理的字符串</param>
		/// <param name="separator">额外指定一个用于分隔的字符串。</param>
		/// <returns><paramref name="text"/>对应的拼音首字母字符串，如果<paramref name="text"/>为null，则返回null。</returns>
		/// <remarks>
		/// 这个方法的处理结果不完全等同于对应拼音的声母，虽然很多时候都是一样的。
		/// </remarks>
		public static string GetInitial(string text, string separator = null)
		{
			if (text == null)
			{
				return null;
			}


			int length = text.Length;

			fixed (char* p = text)
			{
				if (length > 1)
				{
					StringBuilder buffer = Common.AcquireBuffer();
					char* cursor = p;
					char* final = p + length - 1;

					/**
					 * 循环读取每个字符对应的节点，写入处理结果，并且自动移动游标到下一个字符的位置。
					 * 在没有读到最后一个字符之前时，可能得到一个UTF-16或者UTF-32的字符，所以调用<see cref="Common.MapAnyNode(char*)"/>方法。
					 */
					do
					{
						Common.MapAnyNode(cursor).WriteInitial(ref cursor, final, buffer, separator);

					} while (cursor < final);

					/**
					 * 如果游标与最终的指针相等，说明还余下一个字符没有处理，需要补上最后一个字符。
					 */
					if (cursor == final)
					{
						/**
						 * 由于是最后一个字符，肯定不存在UTF-32字符的可能性，所以直接调用<see cref="Common.MapUtf16Node(char*)"/>方法即可。
						 */
						buffer.Append(Common.MapUtf16Node(cursor).GetInitial(cursor));
					}

					return Common.ReturnBuffer(buffer);

				}

				if (length == 1)
				{
					return Common.MapUtf16Node(p).GetInitial(p);
				}

				return string.Empty;
			}

		}

		/// <summary>
		/// 从指定的文件加载自定义的拼音配置信息。
		/// </summary>
		/// <param name="file">配置文件的路径。</param>
		public static void LoadFrom(string file)
		{
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
				Common.LoadFrom(sr);
			}
		}

		/// <summary>
		/// 加载指定的拼音配置内容。
		/// </summary>
		/// <param name="configuration">配置内容。</param>
		public static void Load(string configuration)
		{
			if (string.IsNullOrEmpty(configuration))
			{
				throw new ArgumentNullException(nameof(configuration));
			}
			

			using (StringReader sr = new StringReader(configuration))
			{
				Common.LoadFrom(sr);
			}
		}

		/// <summary>
		/// 定义指定字符的拼音。
		/// </summary>
		/// <param name="character">字符信息。</param>
		/// <param name="pinyin">拼音信息。</param>
		/// <remarks>
		/// 如果字典中已经存在输入字符的拼音信息，则该信息将被覆盖；对于重复定义的情形，以最后一次有效操作为准。
		/// </remarks>
		public static void Define(string character, string pinyin)
		{
			if (string.IsNullOrEmpty(character))
			{
				throw new ArgumentNullException(nameof(character));
			}
			if (string.IsNullOrEmpty(pinyin))
			{
				throw new ArgumentNullException(nameof(pinyin));
			}

			if (character.Length > 1)
			{
				if (character.Length > 2 || !char.IsHighSurrogate(character[0]) || !char.IsLowSurrogate(character[1]))
				{
					throw new ArgumentException(nameof(character));
				}
			}

			if (!Common.OverrideDictionary(character, pinyin))
			{
				throw new Exception($"重写‘{character}’的拼音失败，当前支持的汉字为Unicode基本区及补充区、扩展区A-G，请检查字符是否属于此范围内。");
			}
		}
		

		/// <summary>
		/// 重写指定词汇的拼音。
		/// </summary>
		/// <param name="character">词汇信息。</param>
		/// <param name="pinyin">拼音信息。</param>
		/// <remarks>
		/// 此方法不具备线程安全性。如果词典中已经存在输入词汇的拼音信息，则该信息将被覆盖；对于重复定义的情形，以最后一次有效操作为准。
		/// </remarks>
		public static void Define(string word, string[] pinyin)
		{
			if (string.IsNullOrEmpty(word))
			{
				throw new ArgumentNullException(nameof(word));
			}
			if (pinyin == null || pinyin.Length < 2)
			{
				throw new ArgumentNullException(nameof(pinyin));
			}

			if (!Common.OverrideLexicon(word, pinyin))
			{
				throw new Exception($"重写‘{word}’的拼音失败，当前支持的汉字为Unicode基本区及补充区、扩展区A-G，请检查首字符是否属于此范围内。");
			}
		}
	}
}