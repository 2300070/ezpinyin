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
		/// <param name="cursor">游标信息。</param>
		/// <returns>所获得的字符串。</returns>
		public override unsafe string GetPinyin(char* cursor)
		{
			return new string(cursor, 0, 2);
		}

		/// <summary>
		/// 获得拼音首字母。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <returns>所获得的首字母。</returns>
		public override unsafe string GetInitial(char* cursor)
		{
			return new string(cursor, 0, 2);
		}

		/// <summary>
		/// 将游标处的两个字符写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="final">字符串中最后一个字符的位置。</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* final, StringBuilder buffer, string separator)
		{
			buffer.Append(*cursor).Append(*(cursor + 1)).Append(separator);
			cursor += 2;
		}

		/// <summary>
		/// 将游标处的两个字符写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="final">字符串中最后一个字符的位置。</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public override unsafe void WriteInitial(ref char* cursor, char* final, StringBuilder buffer, string separator)
		{
			buffer.Append(*cursor).Append(*(cursor + 1)).Append(separator);
			cursor += 2;
		}

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标与索引到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="final">字符串中最后一个字符的位置。</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="index">分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* final, string[] buffer, ref int index)
		{
			buffer[index++] = new string(new char[] { *cursor, *(cursor + 1) });
			cursor += 2;
		}
	}
}