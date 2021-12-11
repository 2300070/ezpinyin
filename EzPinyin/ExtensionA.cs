
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
				Dictionary = Common.LoadDictionary("dict_ext_a", Common.Utf16NodeTemplates);
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}

#if DEBUG
			Console.WriteLine("ExtensionA dictionary is loaded.");
#endif
		}
	}
}