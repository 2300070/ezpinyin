using System;

namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面汉字部首扩展区、康熙字典部首区的字典。
	/// </summary>
	internal static class Radicals
	{
		internal static readonly PinyinNode[] Dictionary;

		static Radicals()
		{
			try
			{
				Dictionary = App.LoadDictionary("dict_rad", App.Utf16NodeTemplates);
			}
			finally
			{
				App.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("Radicals dictionary is loaded.");
#endif
		}
	}
}