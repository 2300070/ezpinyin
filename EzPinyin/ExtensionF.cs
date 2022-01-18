using System;

namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面2汉字F扩展区的字典。
	/// </summary>
	internal static class ExtensionF
	{
		internal static readonly PinyinNode[] Dictionary;

		static ExtensionF()
		{
			try
			{
				Dictionary = Common.LoadDictionary("dict_ext_f", Common.Utf32Templates);
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("ExtensionF dictionary is loaded.");
#endif
		}
	}
}