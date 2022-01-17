using System;
using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示一个适用于UTF16字符的词典节点。
	/// </summary>
	internal sealed class LexiconNode : PinyinNode
	{
		private PinyinNode[] nodes;
		private readonly PinyinNode firstNode;//所有词汇公共的头部词汇。
		private int count;//实际节点数量。
		private int size;//nodes长度。
		private int index;//size在质数表中的索引位置。

		/// <summary>
		/// 获得当前节点的拼音字符串，始终抛出<see cref="NotSupportedException"/>。
		/// </summary>
		public override string Pinyin => this.firstNode.Pinyin;


		internal LexiconNode(PinyinNode firstNode)
		{
			this.firstNode = firstNode;
			this.Resize(0);
		}

		/// <summary>
		/// 为当前节点注册一个词汇与拼音。
		/// </summary>
		/// <param name="word">需要注册的词汇。</param>
		/// <param name="pinyin">该词汇对应的拼音。</param>
		public void Add(string word, string[] pinyin)
		{
			if (word.Length == pinyin.Length)
			{
				switch (pinyin.Length)
				{
					case 2:
						this.Insert(new LexiconLinkNode2(word, pinyin));
						return;
					case 3:
						this.Insert(new LexiconLinkNode3(word, pinyin));
						return;
					case 4:
						this.Insert(new LexiconLinkNode4(word, pinyin));
						return;
				}
			}
			this.Insert(new LexiconLinkNodeX(word, pinyin));
		}

		/// <summary>
		/// 为当前节点注册一个词汇与拼音。
		/// </summary>
		/// <param name="word">需要注册的词汇。</param>
		/// <param name="pinyin">该词汇对应的拼音。</param>
		public void Add(string word, string pinyin) => this.Add(word, pinyin.Split(Common.CharacterSeparator, StringSplitOptions.RemoveEmptyEntries));

		/// <summary>
		/// 获得拼音字符串。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <returns>所获得的字符串。</returns>
		public override unsafe string GetPinyin(char* cursor) => this.firstNode.GetPinyin(cursor);

		/// <summary>
		/// 获得拼音首字母。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <returns>所获得的首字母。</returns>
		public override unsafe string GetInitial(char* cursor) => this.firstNode.GetInitial(cursor);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="end">字符串中最后一个字符的位置</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, StringBuilder buffer, string separator) => this.nodes[*(cursor + 1) % this.size].WritePinyin(ref cursor, end, buffer, separator);

		/// <summary>
		/// 将拼音首字母写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="end">字符串中最后一个字符的位置</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WriteInitial(ref char* cursor, char* end, StringBuilder buffer, string separator) => this.nodes[*(cursor + 1) % this.size].WriteInitial(ref cursor, end, buffer, separator);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标与索引到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="end">字符串中最后一个字符的位置</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="index">分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, string[] buffer, ref int index) => this.nodes[*(cursor + 1) % this.size].WritePinyin(ref cursor, end, buffer, ref index);

		private void Resize(int index)
		{
			/**
			 * 重新调整当前节点的容积。
			 */
			if (index < 0 || index >= Common.PrimeTable.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			PinyinNode[] old = this.nodes;

			/**
			 * 使用新的容积大小对节点数组进行初始化
			 */
			int size = Common.PrimeTable[index];
			PinyinNode origin = this.firstNode;
			PinyinNode[] nodes = new PinyinNode[size];
			for (int i = 0; i < size; i++)
			{
				nodes[i] = origin;
			}

			this.nodes = nodes;
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
					while (node is LexiconLinkNode link)
					{
						node = link.Next;
						this.Insert(link);
					}
				}
			}
		}

		private void Insert(LexiconLinkNode node)
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
			PinyinNode target = this.nodes[index];
			if (!(target is LexiconLinkNode))
			{
				node.Next = target;
				this.nodes[index] = node;
				this.count++;
				return;
			}

			LexiconLinkNode item = (LexiconLinkNode)target;
			LexiconLinkNode prev = null;
			do
			{
				if (item.Word == node.Word)
				{
					/**
					 * 替换现有的节点。
					 */
					if (prev == null)
					{
						node.Next = item.Next;
						this.nodes[index] = node;
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
						this.nodes[index] = node;
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
				item = item.Next as LexiconLinkNode;
			} while (item != null);
			
			prev.Next = node;
			node.Next = this.firstNode;
			this.count++;
		}
	}
}