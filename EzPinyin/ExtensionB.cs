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
				Dictionary = Common.LoadDictionary("dict_ext_b", Common.Utf32Templates, 0x20000);
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("ExtensionB dictionary is loaded.");
#endif
		}
	}
}