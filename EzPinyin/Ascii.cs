using System;
using System.Globalization;

namespace EzPinyin
{
	/// <summary>
	/// 表示ASCII区字典。
	/// </summary>
	internal sealed class Ascii
	{
		internal static readonly PinyinNode[] Dictionary;

		static Ascii()
		{
			Dictionary = new PinyinNode[0x100];
			for (int i = 0; i < 0x100; i++)
			{
				char c = (char)i;
				switch (char.GetUnicodeCategory(c))
				{
					//字母
					case UnicodeCategory.UppercaseLetter:
					case UnicodeCategory.LowercaseLetter:
					case UnicodeCategory.TitlecaseLetter:
					case UnicodeCategory.ModifierLetter:
					case UnicodeCategory.OtherLetter:
					//数字
					case UnicodeCategory.DecimalDigitNumber:
					case UnicodeCategory.LetterNumber:
					case UnicodeCategory.OtherNumber:
						Dictionary[i] = LetterOrNumberNode.Instance;
						break;
					default:
						Dictionary[i] = UnknownNode.Instance;
						break;
				}
			}
		}
	}
}