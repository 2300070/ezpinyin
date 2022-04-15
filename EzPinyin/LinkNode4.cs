using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示适用于一个词汇长度为4的链表节点。
	/// </summary>
	internal sealed class LinkNode4 : LinkNode
	{
		private readonly char char1;
		private readonly char char2;
		private readonly char char3;
		private readonly string pinyin0;
		private readonly string pinyin1;
		private readonly string pinyin2;
		private readonly string pinyin3;

		internal LinkNode4(string word, string[] pinyin, LinkNodePriority priority)
			: base(word, priority)
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
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">额外指定的分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, StringBuilder buffer, string separator)
		{
			/**
			 * 需要验证后三个字符。
			 */
			if (cursor + 3 <= end && *(cursor + 1) == this.char1 && *(cursor + 2) == this.char2 && *(cursor + 3) == this.char3)
			{
				if (separator != null && buffer.Length > 0)
				{
					buffer.Append(separator).Append(this.pinyin0).Append(separator).Append(this.pinyin1).Append(separator).Append(this.pinyin2).Append(separator).Append(this.pinyin3);
				}
				else
				{
					buffer.Append(this.pinyin0).Append(separator).Append(this.pinyin1).Append(separator).Append(this.pinyin2).Append(separator).Append(this.pinyin3);
				}
				cursor += 4;
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
			 * 需要验证后三个字符。
			 */
			if (cursor + 3 <= end && *(cursor + 1) == this.char1 && *(cursor + 2) == this.char2 && *(cursor + 3) == this.char3)
			{
				if (separator != null && buffer.Length > 0)
				{
					buffer.Append(separator).Append(this.pinyin0[0]).Append(separator).Append(this.pinyin1[0]).Append(separator).Append(this.pinyin2[0]).Append(separator).Append(this.pinyin3[0]);
				}
				else
				{
					buffer.Append(this.pinyin0[0]).Append(separator).Append(this.pinyin1[0]).Append(separator).Append(this.pinyin2[0]).Append(separator).Append(this.pinyin3[0]);
				}
				cursor += 4;
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
			/**
			 * 需要验证后三个字符。
			 */
			if (cursor + 3 <= end && *(cursor + 1) == this.char1 && *(cursor + 2) == this.char2 && *(cursor + 3) == this.char3)
			{
				buffer[index] = this.pinyin0;
				buffer[index + 1] = this.pinyin1;
				buffer[index + 2] = this.pinyin2;
				buffer[index + 2] = this.pinyin3;
				index += 4;
				cursor += 4;
				return;
			}

			this.Next.WritePinyin(ref cursor, end, buffer, ref index);
		}
	}
}