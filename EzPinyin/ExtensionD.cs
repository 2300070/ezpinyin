namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面2汉字D扩展区的字典。
	/// </summary>
	internal static class ExtensionD
	{
		internal static readonly PinyinNode[] Dictionary;

		static ExtensionD()
		{
			try
			{
				Dictionary = Common.LoadDictionary("dict_ext_d", Common.Utf32Templates, 0x2B740);
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			System.Console.WriteLine("ExtensionD dictionary is loaded.");
#endif
		}
	}
}