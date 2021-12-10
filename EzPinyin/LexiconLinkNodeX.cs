using System;
using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示适用于包含四个以上字符或者包含UTF-32字符的词汇的节点。
	/// </summary>
	internal sealed class LexiconLinkNodeX : LexiconLinkNode
	{
		private readonly string[] pinyin;

		internal LexiconLinkNodeX(string word, string[] pinyin)
			: base(word)
		{
			/**
			 * 能够运行到此处，说明word要么长度超过4，要么至少含有一个UTF-32字符，无论何种情况，说明word至少有3个字符，提取后两个字符，验证时先进行一次比较，可以减少循环的次数。
			 */

			this.pinyin = pinyin;
		}

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="final">字符串中最后一个字符的位置</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* final, StringBuilder buffer, string separator)
		{
			/**
			 * 先比较前两个字符能否匹配，如果能够匹配，再比较其余的。
			 */
			string word = this.Word;
			int length = this.Length;
			if (cursor + length - 1 <= final && *(cursor + 1) == word[1] && *(cursor + 2) == word[2])
			{
				for (int i = 3; i < length; i++)
				{
					if (*(cursor + i) != word[i])
					{
						this.Next.WritePinyin(ref cursor, final, buffer, separator);
						return;
					}
				}

				cursor += length;

				string[] pinyin = this.pinyin;
				buffer.Append(pinyin[0]).Append(separator).Append(pinyin[1]);
				length = pinyin.Length - 1;
				for (int i = 2; i < length; i++)
				{
					buffer.Append(separator).Append(pinyin[i]);
				}

				if (cursor < final)
				{
					buffer.Append(separator).Append(pinyin[length]).Append(separator);
				}
				else
				{
					buffer.Append(separator).Append(pinyin[length]);
				}
				return;
			}
			this.Next.WritePinyin(ref cursor, final, buffer, separator);
		}

		/// <summary>
		/// 将拼音首字母写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="final">字符串中最后一个字符的位置</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WriteFirstLetter(ref char* cursor, char* final, StringBuilder buffer, string separator)
		{
			/**
			 * 先比较前两个字符能否匹配，如果能够匹配，再比较其余的。
			 */
			string word = this.Word;
			int length = this.Length;
			if (cursor + length - 1 <= final && *(cursor + 1) == word[1] && *(cursor + 2) == word[2])
			{
				for (int i = 3; i < length; i++)
				{
					if (*(cursor + i) != word[i])
					{
						this.Next.WriteFirstLetter(ref cursor, final, buffer, separator);
						return;
					}
				}

				cursor += length;

				string[] pinyin = this.pinyin;
				buffer.Append(pinyin[0][0]).Append(separator).Append(pinyin[1][0]);
				length = pinyin.Length - 1;
				for (int i = 2; i < length; i++)
				{
					buffer.Append(separator).Append(pinyin[i][0]);
				}

				if (cursor < final)
				{
					buffer.Append(separator).Append(pinyin[length][0]).Append(separator);
				}
				else
				{
					buffer.Append(separator).Append(pinyin[length][0]);
				}
				return;
			}
			this.Next.WriteFirstLetter(ref cursor, final, buffer, separator);
		}

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标与索引到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="final">字符串中最后一个字符的位置</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="index">分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* final, string[] buffer, ref int index)
		{
			string word = this.Word;
			int length = this.Length;
			if (cursor + length - 1 <= final && *(cursor + 1) == word[1] && *(cursor + 2) == word[2])
			{
				for (int i = 3; i < length; i++)
				{
					if (*(cursor + i) != word[i])
					{
						this.Next.WritePinyin(ref cursor, final, buffer, ref index);
						return;
					}
				}

				string[] pinyin = this.pinyin;
				Array.Copy(pinyin, 0, buffer, index, pinyin.Length);
				index += pinyin.Length;
				cursor += length;
				return;
			}
			this.Next.WritePinyin(ref cursor, final, buffer, ref index);
		}
	}
}