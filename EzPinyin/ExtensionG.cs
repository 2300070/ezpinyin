namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面2汉字G扩展区的字典。
	/// </summary>
	internal static class ExtensionG
	{
		internal static readonly PinyinNode[] Dictionary;

		static ExtensionG()
		{
			try
			{
				Dictionary = Common.LoadDictionary("dict_ext_g", Common.Utf32Templates, 0x30000);
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			System.Console.WriteLine("ExtensionG dictionary is loaded.");
#endif
		}
	}
}