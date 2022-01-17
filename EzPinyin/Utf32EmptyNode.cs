using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示空的UTF32字符节点。
	/// </summary>
	internal sealed class Utf32EmptyNode : PinyinNode
	{
		/// <summary>
		/// 获得空白的UTF32字符节点的实例。
		/// </summary>
		public static readonly Utf32EmptyNode Instance = new Utf32EmptyNode();

		/// <summary>
		/// 获得当前节点的拼音字符串。
		/// </summary>
		public override string Pinyin => null;

		private Utf32EmptyNode() { }

		/// <summary>
		/// 获得拼音字符串。
		/// </summary>
		/// <param name="cursor">指向输入字符当前串位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的字符串。</returns>
		public override unsafe string GetPinyin(char* cursor)
		{
			return new string(cursor, 0, 2);
		}

		/// <summary>
		/// 获得拼音首字母。
		/// </summary>
		/// <param name="cursor">指向输入字符当前串位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的首字母。</returns>
		public override unsafe string GetInitial(char* cursor)
		{
			return new string(cursor, 0, 2);
		}

		/// <summary>
		/// 将游标处的两个字符写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符当前串位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, StringBuilder buffer, string separator)
		{
			buffer.Append(*cursor).Append(*(cursor + 1)).Append(separator);
			cursor += 2;
		}

		/// <summary>
		/// 将游标处的两个字符写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符当前串位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WriteInitial(ref char* cursor, char* end, StringBuilder buffer, string separator)
		{
			buffer.Append(*cursor).Append(*(cursor + 1)).Append(separator);
			cursor += 2;
		}

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标与索引到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符当前串位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="index">指示操作结果在缓存区中存储位置的索引值。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, string[] buffer, ref int index)
		{
			buffer[index++] = new string(cursor, 0, 2);
			cursor += 2;
		}
	}
}