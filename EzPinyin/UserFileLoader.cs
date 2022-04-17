using System.IO;
using System.Reflection;

namespace EzPinyin
{
	/// <summary>
	/// 表示用来加载用户自定义拼音的类。
	/// </summary>
	internal static class UserFileLoader
	{


		static UserFileLoader()
		{

			/**
			 * 搜索并应用用户的自定义字典文件
			 */
			string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*dict*");

			foreach (string file in files)
			{
				Common.LoadFrom(file, PinyinPriority.High);
#if DEBUG
				System.Console.WriteLine($"Load custom file: {file}.");
#endif
			}
		}

		public static void LoadAll()
		{
		}
	}
}