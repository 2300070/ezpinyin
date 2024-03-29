﻿namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面汉字部首扩展区、康熙字典部首区的字典。
	/// </summary>
	internal static class Radicals
	{
		internal static readonly PinyinNode[] Dictionary;

		static Radicals()
		{
			try
			{
				Dictionary = Common.LoadDictionary("dict_rad", Common.Utf16Templates, 0x2E80);
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			System.Console.WriteLine("Radicals dictionary is loaded.");
#endif
		}
	}
}