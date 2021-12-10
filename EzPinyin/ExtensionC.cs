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
				Dictionary = App.LoadDictionary("dict_ext_c", App.Utf32NodeTemplates);
			}
			finally
			{
				App.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("ExtensionC dictionary is loaded.");
#endif
		}
	}
}