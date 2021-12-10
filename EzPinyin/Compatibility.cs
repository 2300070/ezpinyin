using System;

namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面兼容汉字区的字典。
	/// </summary>
	internal static class Compatibility
	{
		public static readonly PinyinNode[] Dictionary;

		static Compatibility()
		{
			try
			{
				Dictionary = App.LoadDictionary("dict_cmp", App.Utf16NodeTemplates);
			}
			finally
			{
				App.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("Compatibility dictionary is loaded.");
#endif
		}
	}
}