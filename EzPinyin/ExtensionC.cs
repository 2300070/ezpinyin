using System;

namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面2汉字C扩展区的字典。
	/// </summary>
	internal static class ExtensionC
	{
		internal static readonly PinyinNode[] Dictionary;

		static ExtensionC()
		{
			try
			{
				Dictionary = Common.LoadDictionary("dict_ext_c", Common.Utf32Templates);
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("ExtensionC dictionary is loaded.");
#endif
		}
	}
}