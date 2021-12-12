using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyP = TinyPinyin.PinyinHelper;

namespace EzPinyin.UnitTest
{
	[TestClass]
	public class PerformanceBenchmark
	{

		[TestMethod]
		public void TestDictionaryVsHashedLinkList()
		{
			/**
			 * 通过模拟对词汇"abc"的检索，
			 * 验证使用字典表与哈希链表之间的性能差异。
			 */
			const int TIMES = 0x7FFFFFF;
			long max = 0;
			string text = new string('a', 0xFF);
			Dictionary<char, char> dict = new Dictionary<char, char> { { 'a', 'a' }, { 'b', 'b' }, { 'c', 'c' }, { 'd', 'd' }, { 'e', 'e' } };
			char[] array = text.ToCharArray();
			char b = 'b';
			char c = 'c';

			Stopwatch sw = Stopwatch.StartNew();
			for (int i = 0; i < TIMES; i++)
			{
				if (dict.TryGetValue('a', out char ch) && dict.TryGetValue('b', out ch) && dict.TryGetValue('c', out ch))
				{

				}
			}

			sw.Stop();

			long t1 = sw.ElapsedTicks;
			max = Math.Max(max, t1);

			sw = Stopwatch.StartNew();
			for (int i = 0; i < TIMES; i++)
			{
				if (text['a' % array.Length] == 'a' && b == 'b' && c == 'c')
				{

				}
			}

			sw.Stop();
			long t2 = sw.ElapsedTicks;
			max = Math.Max(max, t2);

			sw = Stopwatch.StartNew();
			for (int i = 0; i < TIMES; i++)
			{
				if (text['a' & 0xFF] == 'a' && b == 'b' && c == 'c')
				{

				}
			}

			sw.Stop();
			long t3 = sw.ElapsedTicks;
			max = Math.Max(max, t3);

			Console.WriteLine($"Dictionary {TimeSpan.FromTicks(t1)} {(max * 100D / t1):00.00}%");
			Console.WriteLine($"Mode {TimeSpan.FromTicks(t2)} {(max * 100D / t2):00.00}%");
			Console.WriteLine($"Bitwise And {TimeSpan.FromTicks(t2)} {(max * 100D / t2):00.00}%");
		}

		[TestMethod]
		public void BasicBenchmark()
		{
			PerformanceBenchmark.ApplyBenchmark("重庆银行川藏大区成都分行驻厦门朝阳区长厦路支行");
		}
		[TestMethod]
		public void ExtensionABenchmark()
		{
			PerformanceBenchmark.ApplyBenchmark("重庆银行川藏大区成都分行朝阳区长厦路支行，㐀㲒䔤䶵。");
		}
		[TestMethod]
		public void ExtensionABCDBenchmark()
		{
			PerformanceBenchmark.ApplyBenchmark("重庆银行川藏大区成都分行朝阳区长厦路支行，㐀㲒䔤䶵，𠀀𠧄𡎈𡵌𢜐𣃔𣪘𤑜𤸠𥟤𦆨𦭬𧔰𧻴𨢸𩉼𩱀𪜀𪻐𫜴𫝀𫠝", false);
		}
		[TestMethod]
		public void ExtensionABCDEFGBenchmark()
		{
			PerformanceBenchmark.ApplyBenchmark("重庆银行川藏大区成都分行朝阳区长厦路支行，㐀㲒䔤䶵，𠀀𠧄𡎈𡵌𢜐𣃔𣪘𤑜𤸠𥟤𦆨𦭬𧔰𧻴𨢸𩉼𩱀𪜀𪻐𫜴𫝀𫠝𫠠𫿰𬟀𬺰𭡫𮈦𰀀𱍊", false);
		}


		private static void ApplyBenchmark(string text, bool compare = true)
		{
			Console.WriteLine($"测试样本：{text}");
			Console.WriteLine($"GetPinyin {PinyinHelper.GetPinyin(text)}");
			Console.WriteLine($"GetFirstLetters {PinyinHelper.GetFirstLetters(text)}");
			if (compare)
			{
				Console.WriteLine($"TinyPinyin {TinyP.GetPinyin(text)}");//##当文字包含UTF32汉字时，如果执行这一个方法，通过Reshaprer调用的测试会导致测试终止的异常，然而通过VS自带的测试管理器就不会。
			}
			const int TIMES = 0x7FFFFF;

			long max = 0;

			Stopwatch sw = Stopwatch.StartNew();
			for (int i = 0; i < TIMES; i++)
			{
				PinyinHelper.GetPinyin(text);
			}

			sw.Stop();


			long t1 = sw.ElapsedTicks;
			max = Math.Max(max, t1);

			long t2 = 0;
			if (compare)
			{
				sw = Stopwatch.StartNew();
				for (int i = 0; i < TIMES; i++)
				{
					TinyP.GetPinyin(text);
				}

				sw.Stop();
				t2 = sw.ElapsedTicks;
				max = Math.Max(max, t2);
			}

			sw = Stopwatch.StartNew();
			for (int i = 0; i < TIMES; i++)
			{
				PinyinHelper.GetPinyinArray(text);
			}

			sw.Stop();
			long t3 = sw.ElapsedTicks;
			max = Math.Max(max, t3);

			sw = Stopwatch.StartNew();
			for (int i = 0; i < TIMES; i++)
			{
				PinyinHelper.GetFirstLetters(text);
			}

			sw.Stop();
			long t4 = sw.ElapsedTicks;
			max = Math.Max(max, t4);



			int total = TIMES * text.Length;

			Console.WriteLine($"EzPinyin GetPinyin {(0.0001D * total / TimeSpan.FromTicks(t1).TotalSeconds):0.0}万/秒 {TimeSpan.FromTicks(t1)} {(max * 100D / t1):00.00}%");
			Console.WriteLine($"EzPinyin GetPinyinArray {(0.0001D * total / TimeSpan.FromTicks(t3).TotalSeconds):0.0}万/秒 {TimeSpan.FromTicks(t3)} {(max * 100D / t3):00.00}%");
			Console.WriteLine($"EzPinyin GetFirstLetters {(0.0001D * total / TimeSpan.FromTicks(t4).TotalSeconds):0.0}万/秒 {TimeSpan.FromTicks(t4)} {(max * 100D / t4):00.00}%");
			if (compare)
			{
				Console.WriteLine($"TinyPinyin {(0.0001D * total / TimeSpan.FromTicks(t2).TotalSeconds):0.0}万/秒 {TimeSpan.FromTicks(t2)} {(max * 100D / t2):00.00}%");
			}
		}
	}
}