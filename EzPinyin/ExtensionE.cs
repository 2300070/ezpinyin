namespace EzPinyin
{
	/// <summary>
	/// 表示Unicode平面2汉字E扩展区的字典。
	/// </summary>
	internal static class ExtensionE
	{
		internal static readonly PinyinNode[] Dictionary;

		static ExtensionE()
		{
			try
			{
				Dictionary = Common.LoadDictionary("dict_ext_e", Common.Utf32Templates, 0x2B820);
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			System.Console.WriteLine("ExtensionE dictionary is loaded.");
#endif
		}
	}
}