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
				Dictionary = Common.LoadDictionary("dict_cmp", Common.Utf16Templates, 0xF900);
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("Compatibility dictionary is loaded.");
#endif
		}
	}
}