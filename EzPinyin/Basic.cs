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
				

				/**
				 * 搜索并应用用户的自定义字典文件
				 */
				string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*dict*");

				foreach (string file in files)
				{
					Common.LoadFrom(file);
#if DEBUG
					Console.WriteLine($"Load custom file: {file}.");
#endif
				}
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