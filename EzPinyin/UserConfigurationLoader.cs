using System;
using System.IO;
using System.Reflection;

namespace EzPinyin
{
	/// <summary>
	/// 表示用来加载用户自定义拼音的类。
	/// </summary>
	internal static class UserConfigurationLoader
	{


		static UserConfigurationLoader()
		{
			try
			{
				UserConfigurationLoader.LoadConfigurationFiles();
			}
			catch
			{
				//可能因访问权限原因而被拒绝。
			}

			UserConfigurationLoader.LoadConfigurationResources();
		}

		private static void LoadConfigurationResources()
		{
			try
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				Assembly current = typeof(UserConfigurationLoader).Assembly;
				foreach (Assembly assembly in assemblies)
				{
					if (assembly == current)
					{
						/**
						 * 直接略过当前应用程序
						 */
						continue;
					}

					AssemblyName[] names = assembly.GetReferencedAssemblies();
					if (Array.Find(names, x => x.FullName == current.FullName) != null)
					{
						UserConfigurationLoader.LoadConfigurationResources(assembly);
					}
				}
			}
			catch
			{
#if DEBUG
				throw;
#endif
				//直接略过所有异常。
			}

		}

		private static void LoadConfigurationResources(Assembly assembly)
		{
			try
			{
				string[] names = assembly.GetManifestResourceNames();
				foreach (string name in names)
				{
					if (name.Contains("dict"))
					{
						using (StreamReader sr = new StreamReader(assembly.GetManifestResourceStream(name)))
						{
							Common.LoadFrom(sr, PinyinPriority.High);
						}
#if DEBUG
						System.Console.WriteLine($"Load custom resource: {name}.");
#endif
					}
				}
			}
			catch
			{
#if DEBUG
				throw;
#endif
				//直接略过所有异常。
			}
		}


		public static void LoadAll()
		{
			/**
			 * 静态方法中已经完成加载
			 */
		}

		private static void LoadConfigurationFiles()
		{
			string[] files;

			/**
			 * 搜索并应用用户的自定义字典文件
			 */
			files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*dict*");

			foreach (string file in files)
			{
				try
				{
					Common.LoadFrom(file, PinyinPriority.High);
				}
				catch
				{
#if DEBUG
					throw;
#endif
				}
#if DEBUG
				System.Console.WriteLine($"Load custom file: {file}.");
#endif
			}
		}
	}
}