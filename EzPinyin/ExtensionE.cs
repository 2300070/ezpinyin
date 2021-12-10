using System;

namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面2汉字E扩展区的字典。
	/// </summary>
	internal static class ExtensionE
	{
		internal static readonly PinyinNode[] Dictionary;

		static ExtensionE()
		{
			try
			{
				Dictionary = App.LoadDictionary("dict_ext_e", App.Utf32NodeTemplates);
			}
			finally
			{
				App.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("ExtensionE dictionary is loaded.");
#endif
		}
	}
}