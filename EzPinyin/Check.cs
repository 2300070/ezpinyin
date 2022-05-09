using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace EzPinyin
{
	/// <summary>
	/// 用于检验一些信息。
	/// </summary>
	internal static class Check
	{
		/// <summary>
		/// 检测当前环境是否是开发环境。
		/// </summary>
		public static readonly bool IsIdeEnvironment = Check.CheckIdeEnvironment();

		static Check()
		{
		}

		private static bool CheckIdeEnvironment()
		{
			if (Debugger.IsAttached)
			{
				return true;
			}

			StackTrace trace = new StackTrace();
			StackFrame[] frames = trace.GetFrames();
			Assembly current = Assembly.GetExecutingAssembly();
			foreach (StackFrame frame in frames)
			{
				Assembly assembly = frame.GetMethod().DeclaringType.Assembly;
				if (assembly == current)
				{
					continue;
				}

				if (assembly.GetCustomAttributes(typeof(DebuggableAttribute), true).Length > 0)
				{
					return true;
				}
			}

			return false;
		}
	}
}