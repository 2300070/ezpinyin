using System;
using System.IO;
using System.Reflection;

namespace EzPinyin
{
	internal static class Basic
	{
		internal static readonly PinyinNode[] Dictionary;

		static Basic()
		{
			try
			{
				Dictionary = Common.LoadDictionary("dict_basic", Common.Utf16Templates);

				Common.LoadLexicon(Dictionary, 0x4E00);

				Common.LoadUserFiles();
			}
			finally
			{
				Common.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("Basic dictionary is loaded.");
#endif
		}

	}
}