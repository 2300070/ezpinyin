using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示适用于一个包含四个UTF16字符的词汇的节点。
	/// </summary>
	internal sealed class LexiconLinkNode4 : LexiconLinkNode
	{
		private readonly char char1;
		private readonly char char2;
		private readonly char char3;
		private readonly string pinyin0;
		private readonly string pinyin1;
		private readonly string pinyin2;
		private readonly string pinyin3;

		internal LexiconLinkNode4(string word, string[] pinyin)
			: base(word)
		{
			this.char1 = word[1];
			this.char2 = word[2];
			this.char3 = word[3];
			this.pinyin0 = pinyin[0];
			this.pinyin1 = pinyin[1];
			this.pinyin2 = pinyin[2];
			this.pinyin3 = pinyin[3];
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
			 * 需要验证后三个字符。
			 */
			if (cursor + 3 < final && *(cursor + 1) == this.char1 && *(cursor + 2) == this.char2 && *(cursor + 3) == this.char3)
			{
				buffer.Append(this.pinyin0).Append(separator).Append(this.pinyin1).Append(separator).Append(this.pinyin2).Append(separator).Append(this.pinyin3).Append(separator);
				cursor += 4;
				return;
			}
			if (cursor + 3 == final && *(cursor + 1) == this.char1 && *(cursor + 2) == this.char2 && *(cursor + 3) == this.char3)
			{
				buffer.Append(this.pinyin0).Append(separator).Append(this.pinyin1).Append(separator).Append(this.pinyin2).Append(separator).Append(this.pinyin3);
				cursor += 4;
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
			 * 需要验证后三个字符。
			 */
			if (cursor + 3 < final && *(cursor + 1) == this.char1 && *(cursor + 2) == this.char2 && *(cursor + 3) == this.char3)
			{
				buffer.Append(this.pinyin0[0]).Append(separator).Append(this.pinyin1[0]).Append(separator).Append(this.pinyin2[0]).Append(separator).Append(this.pinyin3[0]).Append(separator);
				cursor += 4;
				return;
			}
			if (cursor + 3 == final && *(cursor + 1) == this.char1 && *(cursor + 2) == this.char2 && *(cursor + 3) == this.char3)
			{
				buffer.Append(this.pinyin0[0]).Append(separator).Append(this.pinyin1[0]).Append(separator).Append(this.pinyin2[0]).Append(separator).Append(this.pinyin3[0]);
				cursor += 4;
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
			/**
			 * 需要验证后三个字符。
			 */
			if (cursor + 3 <= final && *(cursor + 1) == this.char1 && *(cursor + 2) == this.char2 && *(cursor + 3) == this.char3)
			{
				buffer[index] = this.pinyin0;
				buffer[index + 1] = this.pinyin1;
				buffer[index + 2] = this.pinyin2;
				buffer[index + 2] = this.pinyin3;
				index += 4;
				cursor += 4;
				return;
			}

			this.Next.WritePinyin(ref cursor, final, buffer, ref index);
		}
	}
}