using System;

namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面2汉字B扩展区的字典。
	/// </summary>
	internal static class ExtensionB
	{
		internal static readonly PinyinNode[] Dictionary;

		static ExtensionB()
		{
			try
			{
				Dictionary = App.LoadDictionary("dict_ext_b", App.Utf32NodeTemplates);
			}
			finally
			{
				App.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("ExtensionB dictionary is loaded.");
#endif
		}
	}
}