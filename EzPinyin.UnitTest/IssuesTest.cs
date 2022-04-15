using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EzPinyin.UnitTest
{
	[TestClass]
	public class IssuesTest
	{
		[TestMethod]
		public void VerifyIssue2()
		{
			DateTime start = DateTime.Now;
			Console.WriteLine(PinyinHelper.GetPinyin("藏宝室"));
			Console.WriteLine($"{DateTime.Now - start}");
			Console.WriteLine(PinyinHelper.GetPinyin("Hello,World!!"));
			Console.WriteLine(PinyinHelper.GetPinyin("世界你好!!Hello,World!!"));

			Console.WriteLine(TinyPinyin.PinyinHelper.GetPinyin("世界你好!!Hello,World!!"));
		}
	}
}
