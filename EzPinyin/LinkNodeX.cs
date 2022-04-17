using System;
using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示适用于一个词汇长度不固定的链表节点。
	/// </summary>
	internal sealed class LinkNodeX : LinkNode
	{
		private readonly string[] pinyin;

		internal LinkNodeX(string word, string[] pinyin, PinyinPriority priority)
			: base(word, priority)
		{
			this.pinyin = pinyin;
		}

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">额外指定的分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, StringBuilder buffer, string separator)
		{
			/**
			 * 先比较前三个字符能否匹配，如果能够匹配，再比较其余的。
			 */
			string word = this.Word;
			int length = this.Length;
			/**
			 * 由于word要么包含一个UTF32字符，要么长度大于4，所以无论什么情形下长度至少不小于3个字符，所以无需考虑cursor+2溢出的问题。
			 */
			if (cursor + length - 1 <= end && *(cursor + 1) == word[1] && *(cursor + 2) == word[2])
			{
				for (int i = length - 1; i > 2; i--)
				{
					if (*(cursor + i) != word[i])
					{
						this.Next.WritePinyin(ref cursor, end, buffer, separator);
						return;
					}
				}

				cursor += length;

				string[] pinyin = this.pinyin;

				buffer.Append(pinyin[0]).Append(separator).Append(pinyin[1]);

				length = pinyin.Length;
				for (int i = 2; i < length; i++)
				{
					buffer.Append(separator).Append(pinyin[i]);
				}
				return;
			}
			this.Next.WritePinyin(ref cursor, end, buffer, separator);
		}

		/// <summary>
		/// 将拼音首字母写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">额外指定的分隔符。</param>
		public override unsafe void WriteInitial(ref char* cursor, char* end, StringBuilder buffer, string separator)
		{
			/**
			 * 先比较前两个字符能否匹配，如果能够匹配，再比较其余的。
			 */
			string word = this.Word;
			int length = this.Length;
			if (cursor + length - 1 <= end && *(cursor + 1) == word[1] && *(cursor + 2) == word[2])
			{
				for (int i = 3; i < length; i++)
				{
					if (*(cursor + i) != word[i])
					{
						this.Next.WriteInitial(ref cursor, end, buffer, separator);
						return;
					}
				}

				cursor += length;

				string[] pinyin = this.pinyin;

				if (separator != null && buffer.Length > 0)
				{
					buffer.Append(separator);
				}

				buffer.Append(pinyin[0][0]).Append(separator).Append(pinyin[1][0]);

				length = pinyin.Length;
				for (int i = 2; i < length; i++)
				{
					buffer.Append(separator).Append(pinyin[i][0]);
				}

				return;
			}
			this.Next.WriteInitial(ref cursor, end, buffer, separator);
		}

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标与索引到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="index">指示操作结果在缓存区中存储位置的索引值。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, string[] buffer, ref int index)
		{
			string word = this.Word;
			int length = this.Length;
			if (cursor + length - 1 <= end && *(cursor + 1) == word[1] && *(cursor + 2) == word[2])
			{
				for (int i = 3; i < length; i++)
				{
					if (*(cursor + i) != word[i])
					{
						this.Next.WritePinyin(ref cursor, end, buffer, ref index);
						return;
					}
				}

				string[] pinyin = this.pinyin;
				Array.Copy(pinyin, 0, buffer, index, pinyin.Length);
				index += pinyin.Length;
				cursor += length;
				return;
			}
			this.Next.WritePinyin(ref cursor, end, buffer, ref index);
		}
	}
}