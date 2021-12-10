
using System;

namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面1汉字A扩展区的字典。
	/// </summary>
	internal static class ExtensionA
	{
		internal static readonly PinyinNode[] Dictionary;

		static ExtensionA()
		{
			try
			{
				Dictionary = App.LoadDictionary("dict_ext_a", App.Utf16NodeTemplates);
			}
			finally
			{
				App.ResourceManager.ReleaseAllResources();
			}

#if DEBUG
			Console.WriteLine("ExtensionA dictionary is loaded.");
#endif
		}
	}
}