using System.Collections.Generic;

namespace EzPinyin
{
	/// <summary>
	/// 表示存储了其它未知字符的字典。
	/// </summary>
	internal static class Unknown
	{
		/// <summary>
		/// 存储了所有未知UTF16字符的字典。
		/// </summary>
		internal static readonly Dictionary<int, PinyinNode> Utf16 = new Dictionary<int, PinyinNode>();

		/// <summary>
		/// 存储了所有未知UTF32字符的字典。
		/// </summary>
		internal static readonly Dictionary<int, PinyinNode> Utf32 = new Dictionary<int, PinyinNode>();
	}
}