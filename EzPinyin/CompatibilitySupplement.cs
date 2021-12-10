using System;

namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面兼容汉字扩展区的字典。
	/// </summary>
	internal static class CompatibilitySupplement
	{
		public static readonly PinyinNode[] Dictionary;

		static CompatibilitySupplement()
		{
			try
			{
				Dictionary = App.LoadDictionary("dict_cmp_sup", App.Utf32NodeTemplates);
			}
			finally
			{
				App.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("Compatibility supplement dictionary is loaded.");
#endif
		}
	}
}