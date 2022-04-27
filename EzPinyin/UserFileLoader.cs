using System;
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
			try
			{
				UserFileLoader.InternalLoad();
			}
			catch
			{
				//可能因访问权限原因而被拒绝。
			}
		}

		public static void LoadAll()
		{
			/**
			 * 静态方法中已经完成加载
			 */
		}

		private static void InternalLoad()
		{
			string[] files;

			/**
			 * 搜索并应用用户的自定义字典文件
			 */
			files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*dict*");

			foreach (string file in files)
			{
				Common.LoadFrom(file, PinyinPriority.High);
#if DEBUG
				System.Console.WriteLine($"Load custom file: {file}.");
#endif
			}
		}
	}
}