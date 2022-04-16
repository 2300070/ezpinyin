
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
				Dictionary = Common.LoadDictionary("dict_ext_a", Common.Utf16Templates, 0x3400);

				Common.LoadUserFiles();
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}

#if DEBUG
			System.Console.WriteLine("ExtensionA dictionary is loaded.");
#endif
		}
	}
}