using System.Collections.Generic;

namespace EzPinyin
{
	/// <summary>
	/// 表示其它的杂项字典。
	/// </summary>
	internal static class Miscellaneous
	{
		/// <summary>
		/// 存储了UTF16字符的杂项字典。
		/// </summary>
		internal static readonly Dictionary<int, PinyinNode> Dictionary16 = new Dictionary<int, PinyinNode>();

		/// <summary>
		/// 存储了UTF32字符的杂项字典。
		/// </summary>
		internal static readonly Dictionary<int, PinyinNode> Dictionary32 = new Dictionary<int, PinyinNode>();
	}
}