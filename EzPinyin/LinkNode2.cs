using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示适用于一个词汇长度为2的链表节点。
	/// </summary>
	internal sealed class LinkNode2 : LinkNode
	{
		private readonly char char1;
		private readonly string pinyin0;
		private readonly string pinyin1;

		internal LinkNode2(string word, string[] pinyin, LinkNodePriority priority)
			: base(word, priority)
		{
			this.char1 = word[1];
			this.pinyin0 = pinyin[0];
			this.pinyin1 = pinyin[1];
		}

		/// <summary>
		/// 验证词汇是否匹配，将拼音字符串写入到指定的缓存区，并且自动移动游标到词汇末尾下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">额外指定的分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, StringBuilder buffer, string separator)
		{
			/**
			 * 只需验证后一个字符。
			 */
			if (cursor + 1 <= end && *(cursor + 1) == this.char1)
			{
				if (separator != null && buffer.Length > 0)
				{
					buffer.Append(separator).Append(this.pinyin0).Append(separator).Append(this.pinyin1);
				}
				else
				{
					buffer.Append(this.pinyin0).Append(separator).Append(this.pinyin1);
				}
				cursor += 2;
				return;
			}

			this.Next.WritePinyin(ref cursor, end, buffer, separator);
		}

		/// <summary>
		/// 验证词汇是否匹配，将拼音首字母写入到指定的缓存区，并且自动移动游标到词汇末尾下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">额外指定的分隔符。</param>
		public override unsafe void WriteInitial(ref char* cursor, char* end, StringBuilder buffer, string separator)
		{
			/**
			 * 只需验证后一个字符。
			 */
			if (cursor + 1 <= end && *(cursor + 1) == this.char1)
			{
				if (separator != null && buffer.Length > 0)
				{
					buffer.Append(separator).Append(this.pinyin0[0]).Append(separator).Append(this.pinyin1[0]);
				}
				else
				{
					buffer.Append(this.pinyin0[0]).Append(separator).Append(this.pinyin1[0]);
				}
				cursor += 2;
				return;
			}


			this.Next.WriteInitial(ref cursor, end, buffer, separator);
		}

		/// <summary>
		/// 验证词汇是否匹配，将拼音字符串写入到指定的缓存区，并且自动移动游标到词汇末尾下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="index">指示操作结果在缓存区中存储位置的索引值。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, string[] buffer, ref int index)
		{
			/**
			 * 只需验证后一个字符。
			 */
			if (cursor + 1 <= end && *(cursor + 1) == this.char1)
			{
				buffer[index] = this.pinyin0;
				buffer[index + 1] = this.pinyin1;
				index += 2;
				cursor += 2;
				return;
			}
			

			this.Next.WritePinyin(ref cursor, end, buffer, ref index);
		}
	}
}