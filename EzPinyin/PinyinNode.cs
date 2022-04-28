using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示一个拼音节点的基类。
	/// </summary>
	internal abstract unsafe class PinyinNode
	{
		/// <summary>
		/// 指示是否是否是一个符号。
		/// </summary>
		public virtual bool IsSymbol => false;

		/// <summary>
		/// 指示是否不包含拼音信息。
		/// </summary>
		public virtual bool NoPinyin => false;

		/// <summary>
		/// 获得当前节点的拼音字符串。
		/// </summary>
		public abstract string Pinyin { get; }

		/// <summary>
		/// 获得拼音字符串。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的字符串。</returns>
		public abstract string GetPinyin(char* cursor);

		/// <summary>
		/// 获得拼音首字母。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的首字母。</returns>
		public abstract string GetInitial(char* cursor);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">额外指定的分隔符。</param>
		public abstract void WritePinyin(ref char* cursor, char* end, StringBuilder buffer, string separator);

		/// <summary>
		/// 将拼音首字母写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">额外指定的分隔符。</param>
		public abstract void WriteInitial(ref char* cursor, char* end, StringBuilder buffer, string separator);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标与索引到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="index">指示操作结果在缓存区中存储位置的索引值。</param>
		public abstract void WritePinyin(ref char* cursor, char* end, string[] buffer, ref int index);

		/// <summary>
		/// 填充分隔符。
		/// </summary>
		/// <param name="prev">当前节点的前一个节点，如果当前节点为字符串第一个节点，则此参数值为<see cref="UnknownNode.Instance"/>。</param>
		/// <param name="buffer">需要填充分隔符的可变字符串。</param>
		/// <param name="separator">需要填充的分隔符。</param>
		/// <returns>填充分隔符之后的可变字符串。</returns>
		public virtual StringBuilder FillSeparator(PinyinNode prev, StringBuilder buffer, string separator) => prev.IsSymbol ? buffer : buffer.Append(separator);
	}
}