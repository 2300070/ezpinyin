using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示“虚假”的，仅仅用来临时标记词典节点的节点。
	/// </summary>
	/// <remarks>
	/// 如果一开始即加载所有的词典节点，将导致应用程序启动时出现较大的延迟，为此，在初始化时，将所有与词条相关的节点标记为需要加载词典节点的节点，当需要真正解析节点对应的字符时，再由该节点加载词典来完成真正的解析任务。
	/// </remarks>
	internal sealed class LexiconFakeNode : PinyinNode
	{
		private readonly string character;
		private readonly PinyinNode original;
		private readonly PinyinNode[] dictionary;
		private readonly int index;

		/// <summary>
		/// 获得当前节点的拼音字符串。
		/// </summary>
		public override string Pinyin => this.original.Pinyin;

		public LexiconFakeNode(string character, PinyinNode original, PinyinNode[] dictionary, int index)
		{
			this.character = character;
			this.original = original;
			this.dictionary = dictionary;
			this.index = index;
		}

		/// <summary>
		/// 获得拼音字符串。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的字符串。</returns>
		public override unsafe string GetPinyin(char* cursor) => this.LoadActualNode().GetPinyin(cursor);

		/// <summary>
		/// 获得拼音首字母。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的首字母。</returns>
		public override unsafe string GetInitial(char* cursor) => this.LoadActualNode().GetInitial(cursor);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">额外指定的分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, StringBuilder buffer, string separator) => this.LoadActualNode().WritePinyin(ref cursor, end, buffer, separator);

		/// <summary>
		/// 将拼音首字母写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">额外指定的分隔符。</param>
		public override unsafe void WriteInitial(ref char* cursor, char* end, StringBuilder buffer, string separator) => this.LoadActualNode().WriteInitial(ref cursor, end, buffer, separator);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标与索引到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="index">指示操作结果在缓存区中存储位置的索引值。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, string[] buffer, ref int index) => this.LoadActualNode().WritePinyin(ref cursor, end, buffer, ref index);

		/// <summary>
		/// 加载当前节点真正需要用来处理业务的节点。
		/// </summary>
		/// <returns>实际加载的节点。</returns>
		public PinyinNode LoadActualNode()
		{
			/**
			 * 加载应用最终应该实际使用的节点以便来完成相应任务。
			 */

			lock (this)
			{
				PinyinNode[] dictionary = this.dictionary;
				int index = this.index;
				if (dictionary[index] == this)
				{
					dictionary[index] = this.original;

					//先加载简体字对应的词汇
					if (Common.TryConvert(this.character, CharacterType.Simplified, out string simplified))
					{
						LexiconLoader.LoadLexicon(simplified, dictionary, index);
					}
					LexiconLoader.LoadLexicon(this.character, dictionary, index);
				}
				return dictionary[index];
			}
		}


	}
}