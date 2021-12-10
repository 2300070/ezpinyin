using System;
using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// EzPinyin类库的功能入口，用来快速获得拼音或者启用词典解析拼音。
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
					StringBuilder buffer = App.AllocateBuffer();
					char* cursor = p;
					char* final = p + length - 1;

					/**
					 * 循环读取每个字符对应的节点，写入处理结果，并且自动移动游标到下一个字符的位置。
					 * 在没有读到最后一个字符之前时，可能得到一个UTF-16或者UTF-32的字符，所以调用<see cref="App.MapAnyNode(char*)"/>方法。
					 */
					do
					{

						App.MapAnyNode(cursor).WritePinyin(ref cursor, final, buffer, separator);

					} while (cursor < final);

					/**
					 * 如果游标与最终的指针相等，说明还余下一个字符没有处理，需要补上最后一个字符。
					 */
					if (cursor == final)
					{
						/**
						 * 由于是最后一个字符，肯定不存在UTF-32字符的可能性，所以直接调用<see cref="App.MapUtf16Node(char*)"/>方法即可。
						 */
						buffer.Append(App.MapUtf16Node(cursor).GetPinyin(cursor));
					}

					return App.FreeBuffer(buffer);

				}

				if (length == 1)
				{
					return App.MapUtf16Node(p).GetPinyin(p);
				}

				return string.Empty;
			}


		}


		/// <summary>
		/// 将指定的字符串转换为对应的拼音数组。
		/// </summary>
		/// <param name="text">需要处理的字符串</param>
		/// <returns>包含输入字符串的拼音信息的结果数组，若<paramref name="text"/>为null，则返回null。</returns>
		public static string[] GetPinyinArray(string text)
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
					 * 在没有读到最后一个字符之前时，可能得到一个UTF-16或者UTF-32的字符，所以调用<see cref="App.MapAnyNode(char*)"/>方法。
					 */
					do
					{
						App.MapAnyNode(cursor).WritePinyin(ref cursor, final, buffer, ref index);

					} while (cursor < final);

					/**
					 * 如果游标与最终的指针相等，说明还余下一个字符没有处理，需要补上最后一个字符。
					 */
					if (cursor == final)
					{
						/**
						 * 由于是最后一个字符，肯定不存在UTF-32字符的可能性，所以直接调用<see cref="App.MapUtf16Node(char*)"/>方法即可。
						 */
						buffer[index++] = App.MapUtf16Node(cursor).GetPinyin(cursor);
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
					return new[] { App.MapUtf16Node(p).GetPinyin(p) };
				}

				return App.EmptyArray;
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
		public static string GetFirstLetters(string text, string separator = null)
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
					StringBuilder buffer = App.AllocateBuffer();
					char* cursor = p;
					char* final = p + length - 1;

					/**
					 * 循环读取每个字符对应的节点，写入处理结果，并且自动移动游标到下一个字符的位置。
					 * 在没有读到最后一个字符之前时，可能得到一个UTF-16或者UTF-32的字符，所以调用<see cref="App.MapAnyNode(char*)"/>方法。
					 */
					do
					{
						App.MapAnyNode(cursor).WriteFirstLetter(ref cursor, final, buffer, separator);

					} while (cursor < final);

					/**
					 * 如果游标与最终的指针相等，说明还余下一个字符没有处理，需要补上最后一个字符。
					 */
					if (cursor == final)
					{
						/**
						 * 由于是最后一个字符，肯定不存在UTF-32字符的可能性，所以直接调用<see cref="App.MapUtf16Node(char*)"/>方法即可。
						 */
						buffer.Append(App.MapUtf16Node(cursor).GetFirstLetter(cursor));
					}

					return App.FreeBuffer(buffer);

				}

				if (length == 1)
				{
					return App.MapUtf16Node(p).GetFirstLetter(p);
				}

				return string.Empty;
			}

		}

	}
}