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
				Dictionary = Common.LoadDictionary("dict_cmp_sup", Common.Utf32Templates, 0x2F800);
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			System.Console.WriteLine("Compatibility supplement dictionary is loaded.");
#endif
		}
	}
}