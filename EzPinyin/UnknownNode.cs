using System;
using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示未知的拼音节点。
	/// </summary>
	/// <remarks>
	/// 与<see cref="Utf16EmptyNode"/>与<see cref="Utf32EmptyNode"/>不一样的是，<see cref="UnknownNode"/>对应的是非汉字字符。
	/// </remarks>
	internal sealed class UnknownNode : PinyinNode
	{
		/// <summary>
		/// <see cref="UnknownNode"/>的实例。
		/// </summary>
		public static UnknownNode Instance = new UnknownNode();

		/// <summary>
		/// 获得当前节点的拼音字符串。
		/// </summary>
		public override string Pinyin => throw new NotSupportedException();

		private UnknownNode(){}

		/// <summary>
		/// 获得拼音字符串。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的字符串。</returns>
		public override unsafe string GetPinyin(char* cursor)
		{
			return new string(*cursor, 1);
		}

		/// <summary>
		/// 获得拼音首字母。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <returns>所获得的首字母。</returns>
		public override unsafe string GetInitial(char* cursor)
		{
			return new string(*cursor, 1);
		}

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">额外指定的分隔符。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, StringBuilder buffer, string separator)
		{
			buffer.Append(*cursor);
			cursor += 1;
		}

		/// <summary>
		/// 将拼音首字母写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="separator">额外指定的分隔符。</param>
		public override unsafe void WriteInitial(ref char* cursor, char* end, StringBuilder buffer, string separator)
		{
			buffer.Append(*cursor);
			cursor += 1;
		}

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标与索引到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">指向输入字符串当前位置的指针，可以作为游标来遍历整个字符串。</param>
		/// <param name="end">指向输入字符串最后一个字符位置的指针。</param>
		/// <param name="buffer">用来存储操作结果的缓存区。</param>
		/// <param name="index">指示操作结果在缓存区中存储位置的索引值。</param>
		public override unsafe void WritePinyin(ref char* cursor, char* end, string[] buffer, ref int index)
		{
			buffer[index] = new string(*cursor, 1);
			index += 1;
			cursor += 1;
		}
		/// <summary>
		/// 向指定的可变字符串填充一个分隔符。
		/// </summary>
		/// <param name="buffer">需要填充的可变字符串。</param>
		/// <param name="separator">需要填充的分隔符。</param>
		/// <returns>返回填充后的可变字符串。</returns>
		public override StringBuilder FillSeperator(StringBuilder buffer, string separator) => buffer;
	}
}