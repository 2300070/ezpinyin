using System;
using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示一个词汇节点。
	/// </summary>
	/// <remarks>
	/// 词汇节点与一般的拼音节点的差别在于：词汇节点在进行解析时首先遍历其关联的链表，链表中的每个节点代表了某个匹配第一个字符的某个词汇的信息，如果词汇与输入字符串匹配，则优先使用链表节点来完成解析处理，当所有链表节点都不满足时，由于每个链表的最后节点即是当前词汇节点的字符节点，因此会自然调用字符节点来做默认处理。
	/// </remarks>
	internal sealed class LexiconNode : PinyinNode
	{
		private PinyinNode[] buckets;
		private readonly PinyinNode characterNode;//所有词汇公共的头部词汇。
		private int count;//实际节点数量。
		private int size;//nodes长度。
		private int index;//size在质数表中的索引位置。

		/// <summary>
		/// 获得当前节点的拼音字符串，始终抛出<see cref="NotSupportedException"/>。
		/// </summary>
		public override string Pinyin => this.characterNode.Pinyin;


		internal LexiconNode(PinyinNode characterNode)
		{
			this.characterNode = characterNode;
			this.Resize(0);
		}

		/// <summary>
		/// 为当前节点注册一个词汇与拼音。
		/// </summary>
		/// <param name="word">需要注册的词汇。</param>
		/// <param name="pinyin">该词汇对应的拼音。</param>
		/// <param name="priority">优先级别</param>
		public void Add(string word, string[] pinyin, int priority)
		{
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
		/// <param name="priority">优先级别</param>
		public void Add(string word, string pinyin, int priority) => this.Add(word, pinyin.Split(Common.CharacterSeparator, StringSplitOptions.RemoveEmptyEntries), priority);

		/// <summary>
		/// 获得拼音字符串。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的字符串。</returns>
		public override unsafe string GetPinyin(char* cursor) => this.characterNode.GetPinyin(cursor);

		/// <summary>
		/// 获得拼音首字母。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的首字母。</returns>
		public override unsafe string GetInitial(char* cursor) => this.characterNode.GetInitial(cursor);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, StringBuilder buffer, string separator) => this.buckets[*(cursor + 1) % this.size].WritePinyin(ref cursor, end, buffer, separator);

		/// <summary>
		/// 将拼音首字母写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WriteInitial(ref char* cursor, char* end, StringBuilder buffer, string separator) => this.buckets[*(cursor + 1) % this.size].WriteInitial(ref cursor, end, buffer, separator);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标与索引到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="index">指示操作结果在缓存区中存储位置的索引值。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, string[] buffer, ref int index) => this.buckets[*(cursor + 1) % this.size].WritePinyin(ref cursor, end, buffer, ref index);

		private void Resize(int index)
		{
			/**
			 * 重新调整当前节点的容积。
			 */
			if (index < 0 || index >= Common.PrimeTable.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			PinyinNode[] old = this.buckets;

			/**
			 * 使用新的容积大小对节点数组进行初始化
			 */
			int size = Common.PrimeTable[index];
			PinyinNode character = this.characterNode;
			PinyinNode[] nodes = new PinyinNode[size];
			for (int i = 0; i < size; i++)
			{
				nodes[i] = character;
			}

			this.buckets = nodes;
			this.index = index;
			this.size = size;
			this.count = 0;

			/**
			 * 将原有的节点插入到新的节点数组中。
			 */
			if (old != null)
			{
				for (int i = 0; i < old.Length; i++)
				{
					PinyinNode node = old[i];
					while (node is LinkNode link)
					{
						node = link.Next;
						this.Insert(link);
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
			else if (this.count + 1 >= this.size * 2)
			{
				this.Resize(this.index + 1);
			}

			int index = node.Word[1] % this.size;
			PinyinNode target = this.buckets[index];
			if (!(target is LinkNode))
			{
				node.Next = target;
				this.buckets[index] = node;
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
						node.Next = item.Next;
						this.buckets[index] = node;
					}
					else
					{
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
						node.Next = target;
						this.buckets[index] = node;
					}
					else
					{
						prev.Next = node;
						node.Next = item;
					}

					this.count++;
					return;
				}

				prev = item;
				item = item.Next as LinkNode;
			} while (item != null);

			prev.Next = node;
			node.Next = this.characterNode;
			this.count++;
		}
	}
}