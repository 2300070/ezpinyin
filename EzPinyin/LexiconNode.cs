using System;
using System.Text;

namespace EzPinyin
{
	/**
	 * 当前算法并未考虑UTF32词汇导致的杂凑效率降低的问题:
	 * 1.当第一个字符是UTF32字符时，由于读取的第二个char是第一个字符的低半代理，必然导致所有的链表节点获得一样的杂凑码，从而导致只有一个链表生效。
	 * 2.当第一个字符为UTF16字符，而第二个字符是UTF32字符时，由于读取的第二个char是第二个字符的高半代理，因此同样容易出现杂凑效果严重下降的问题。，
	 * 但由于目前基本没有UTF32词汇，个别词汇不会影响整体效果，因此暂时不做修补。
	 */

	/// <summary>
	/// 表示一个词汇节点。
	/// </summary>
	/// <remarks>
	/// 词汇节点与一般的拼音节点的差别在于：词汇节点在进行解析时首先遍历其关联的链表，链表中的每个节点代表了某个匹配第一个字符的某个词汇的信息，如果词汇与输入字符串匹配，则优先使用链表节点来完成解析处理，当所有链表节点都不满足时，由于每个链表的最后节点即是当前词汇节点的字符节点，因此会自然调用字符节点来做默认处理。
	/// </remarks>
	internal sealed class LexiconNode : PinyinNode
	{
		private static readonly int[] primeTable = { 3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369 };

		private PinyinNode[] linkedLists;//所有以character节点所表示的词汇开头的词汇所组成的链表，以词汇的第二个字符与数组长度的余数作为依据来进行分组，每个组即构成一个链表，链表中的节点按照词汇的字数多少来倒序排列，每个链表的最后一个节点永远是character字段。
		private readonly PinyinNode character;//所有词汇公共的头部词汇。
		private int count;//实际节点数量。
		private int size;//nodes长度。
		private int index;//size在质数表中的索引位置。

		/// <summary>
		/// 获得当前节点的拼音字符串，始终抛出<see cref="NotSupportedException"/>。
		/// </summary>
		public override string Pinyin => this.character.Pinyin;


		internal LexiconNode(PinyinNode character)
		{
			this.character = character;
			this.Resize(0);
		}

		/// <summary>
		/// 为当前节点注册一个词汇与拼音。
		/// </summary>
		/// <param name="word">需要注册的词汇。</param>
		/// <param name="pinyin">该词汇对应的拼音。</param>
		/// <param name="priority">优先级别。</param>
		public void Add(string word, string[] pinyin, LinkNodePriority priority)
		{
			if (string.IsNullOrEmpty(word))
			{
				throw new ArgumentNullException(nameof(word));
			}
			if (word.Length == pinyin.Length)
			{
				switch (pinyin.Length)
				{
					case 2:
						this.Insert(new LinkNode2(word, pinyin, priority));
						return;
					case 3:
						this.Insert(new LinkNode3(word, pinyin, priority));
						return;
					case 4:
						this.Insert(new LinkNode4(word, pinyin, priority));
						return;
				}
			}
			this.Insert(new LinkNodeX(word, pinyin, priority));
		}

		/// <summary>
		/// 为当前节点注册一个词汇与拼音。
		/// </summary>
		/// <param name="word">需要注册的词汇。</param>
		/// <param name="pinyin">该词汇对应的拼音。</param>
		/// <param name="priority">优先级别。</param>
		public void Add(string word, string pinyin, LinkNodePriority priority) => this.Add(word, pinyin.Split(Common.CharacterSeparator, StringSplitOptions.RemoveEmptyEntries), priority);

		/// <summary>
		/// 获得拼音字符串。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的字符串。</returns>
		public override unsafe string GetPinyin(char* cursor) => this.character.GetPinyin(cursor);

		/// <summary>
		/// 获得拼音首字母。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的首字母。</returns>
		public override unsafe string GetInitial(char* cursor) => this.character.GetInitial(cursor);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, StringBuilder buffer, string separator) => this.linkedLists[*(cursor + 1) % this.size].WritePinyin(ref cursor, end, buffer, separator);

		/// <summary>
		/// 将拼音首字母写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WriteInitial(ref char* cursor, char* end, StringBuilder buffer, string separator) => this.linkedLists[*(cursor + 1) % this.size].WriteInitial(ref cursor, end, buffer, separator);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标与索引到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="index">指示操作结果在缓存区中存储位置的索引值。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, string[] buffer, ref int index) => this.linkedLists[*(cursor + 1) % this.size].WritePinyin(ref cursor, end, buffer, ref index);

		private void Resize(int index)
		{
			/**
			 * 重新调整当前节点的容积。
			 */
			if (index < 0 || index >= primeTable.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			PinyinNode[] oldLists = this.linkedLists;

			/**
			 * 使用新的容积大小对节点数组进行初始化
			 */
			int size = primeTable[index];
			PinyinNode character = this.character;
			PinyinNode[] newLists = new PinyinNode[size];
			for (int i = 0; i < size; i++)
			{
				newLists[i] = character;
			}

			this.linkedLists = newLists;
			this.index = index;
			this.size = size;
			this.count = 0;

			/**
			 * 将原有的节点插入到新的节点数组中。
			 */
			if (oldLists != null)
			{
				for (int i = 0; i < oldLists.Length; i++)
				{
					PinyinNode node = oldLists[i];
					while (node is LinkNode linkNode)
					{
						node = linkNode.Next;
						this.Insert(linkNode);
					}
				}
			}
		}

		private void Insert(LinkNode node)
		{
			if (this.count + 1 >= this.size && this.size < 0x10)
			{
				this.Resize(this.index + 1);
			}
			else if (this.count + 1 > this.size * 2)
			{
				this.Resize(this.index + 1);
			}

			int index = node.Word[1] % this.size;
			PinyinNode target = this.linkedLists[index];
			if (!(target is LinkNode))
			{
				/**
				 * 当前链表为空。
				 */
				node.Next = target;
				this.linkedLists[index] = node;
				this.count++;
				return;
			}

			LinkNode item = (LinkNode)target;
			LinkNode prev = null;
			do
			{
				if (item.Word == node.Word)
				{
					if (node.Priority < item.Priority)
					{
						return;
					}
					/**
					 * 替换现有的节点。
					 */
					if (prev == null)
					{
						/**
						 * 替换链表中的第一个元素。
						 */
						node.Next = item.Next;
						this.linkedLists[index] = node;
					}
					else
					{
						/**
						 * 插入到链表内部时，应先更新node.Next，再更新prev.Next，这样可以避免线程不同步的场景下可能出现NullReferenceException的出现，下同。
						 */
						node.Next = item.Next;
						prev.Next = node;
					}

					return;
				}

				if (node.Length > item.Length)
				{
					/**
					 * 插入新的节点。
					 */
					if (prev == null)
					{
						/**
						 * 插入到链表的头部。
						 */
						node.Next = target;
						this.linkedLists[index] = node;
					}
					else
					{
						node.Next = item;
						prev.Next = node;
					}

					this.count++;
					return;
				}

				prev = item;
				item = item.Next as LinkNode;
			} while (item != null);

			prev.Next = node;
			node.Next = this.character;
			this.count++;
		}
	}
}