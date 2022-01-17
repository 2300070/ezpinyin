using System;

namespace EzPinyin
{
	/// <summary>
	/// 表示一个适用于UTF16字符的链表节点。
	/// </summary>
	internal abstract class LexiconLinkNode : PinyinNode
	{
		/// <summary>
		/// 当前节点相关联的词汇。
		/// </summary>
		internal readonly string Word;

		/// <summary>
		/// 当前节点相关联的词汇的长度。
		/// </summary>
		internal int Length => this.Word.Length;

		/// <summary>
		/// 当前节点的下一个节点。
		/// </summary>
		internal PinyinNode Next;

		/// <summary>
		/// 获得当前节点的拼音字符串。
		/// </summary>
		public override string Pinyin => throw new NotSupportedException();

		/// <summary>
		/// 初始化新的实例。
		/// </summary>
		/// <param name="word">词汇信息。</param>
		protected LexiconLinkNode(string word)
		{
			this.Word = word;
		}

		/// <summary>
		/// 获得拼音字符串，永远抛出<see cref="NotSupportedException"/>异常。
		/// </summary>
		/// <param name="cursor">指向输入字符当前串位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的字符串。</returns>
		public override unsafe string GetPinyin(char* cursor) => throw new NotSupportedException();

		/// <summary>
		/// 获得拼音首字母，永远抛出<see cref="NotSupportedException"/>异常。
		/// </summary>
		/// <param name="cursor">指向输入字符当前串位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的首字母。</returns>
		public override unsafe string GetInitial(char* cursor) => throw new NotSupportedException();
	}
}