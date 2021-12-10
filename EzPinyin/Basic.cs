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
				Dictionary = App.LoadDictionary("dict_basic", App.Utf16NodeTemplates);

				App.LoadLexicon(Dictionary, 0x4E00);
				

				/**
				 * 搜索并应用用户的自定义字典文件
				 */
				string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*dict*");

				foreach (string file in files)
				{
					App.LoadCustomFile(file);
				}
			}
			finally
			{
				App.ResourceManager.ReleaseAllResources();
			}
#if DEBUG
			Console.WriteLine("Basic dictionary is loaded.");
#endif
		}

	}
}