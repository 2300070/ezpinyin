using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示一个拼音节点的基类。
	/// </summary>
	internal abstract unsafe class PinyinNode
	{
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
		/// 向指定的可变字符串填充一个分隔符。
		/// </summary>
		/// <param name="buffer">需要填充的可变字符串。</param>
		/// <param name="separator">需要填充的分隔符。</param>
		/// <returns>返回填充后的可变字符串。</returns>
		public virtual StringBuilder FillSeperator(StringBuilder buffer, string separator) => buffer.Length > 0 ? buffer.Append(separator) : buffer;
	}
}