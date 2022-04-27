using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EzPinyin.UnitTest
{
	[TestClass]
	public class IssuesTest
	{
		[TestMethod]
		public void VerifyIssue3()
		{
			//测试前需要去除test_issues3_dict.txt的访问权限。
			//此处由于在test_issues3_dict.txt文件中虽然定义了新拼音，但由于不具备访问该文件的权限，因此此处应该返回原来的拼音且测试过程不能报错。
			Console.WriteLine(PinyinHelper.GetPinyin("北市").AssertToBe("bei shi"));
		}
		[TestMethod]
		public void VerifyIssue2()
		{
			DateTime start = DateTime.Now;
			Console.WriteLine(PinyinHelper.GetPinyin("川藏"));
			Console.WriteLine($"{DateTime.Now - start}");
			Console.WriteLine(PinyinHelper.GetPinyin("Hello,World!!"));
			Console.WriteLine(PinyinHelper.GetPinyin("世界你好!!Hello,World!!"));

			Console.WriteLine(TinyPinyin.PinyinHelper.GetPinyin("世界你好!!Hello,World!!"));
		}
	}
}
