﻿using System;

namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面2汉字G扩展区的字典。
	/// </summary>
	internal static class ExtensionG
	{
		internal static readonly PinyinNode[] Dictionary;

		static ExtensionG()
		{
			try
			{
				Dictionary = App.LoadDictionary("dict_ext_g", App.Utf32NodeTemplates);
			}
			finally
			{
				App.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("ExtensionG dictionary is loaded.");
#endif
		}
	}
}