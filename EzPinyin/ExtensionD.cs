using System;

namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面2汉字D扩展区的字典。
	/// </summary>
	internal static class ExtensionD
	{
		internal static readonly PinyinNode[] Dictionary;

		static ExtensionD()
		{
			try
			{
				Dictionary = App.LoadDictionary("dict_ext_d", App.Utf32NodeTemplates);
			}
			finally
			{
				App.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("ExtensionD dictionary is loaded.");
#endif
		}
	}
}