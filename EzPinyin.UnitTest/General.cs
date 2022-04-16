using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EzPinyin.UnitTest
{
	[TestClass]
	public class General
	{

		[TestMethod]
		public void GetPinyin()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("川藏"));
			Console.WriteLine(PinyinHelper.GetPinyin("兙呣瓱"));
			Console.WriteLine(PinyinHelper.GetPinyin("朝阳"));
			Console.WriteLine(PinyinHelper.GetPinyin("一鿿"));
			Console.WriteLine(PinyinHelper.GetPinyin("成都"));
			Console.WriteLine(PinyinHelper.GetPinyin("长城"));
			Console.WriteLine(PinyinHelper.GetPinyin("〇"));
			Console.WriteLine(PinyinHelper.GetPinyin("重庆银行川藏大区成都分行朝阳区长厦路重工大厦行动处九楼张朝阳董事长藏宝室"));
			Console.WriteLine(PinyinHelper.GetPinyin("长城重工"));
			Console.WriteLine(PinyinHelper.GetPinyin("厦门和兴铝材"));
			Console.WriteLine(PinyinHelper.GetPinyin("一埄憈歌甐绔袘鉜鰠龥㐀㲒䔤䶵龦鿕"));
			Console.WriteLine(PinyinHelper.GetPinyin("㐀㲒䔤䶵"));
			Console.WriteLine(PinyinHelper.GetPinyin("𠀀𠧄𡎈𡵌𢜐𣃔𣪘𤑜𤸠𥟤𦆨𦭬𧔰𧻴𨢸𩉼𩱀𪜀𪻐𫜴𫝀𫠝"));
			Console.WriteLine(PinyinHelper.GetPinyin("㐀㲒䔤䶵𠀀𠧄𡎈𡵌𢜐𣃔𣪘𤑜𤸠𥟤𦆨𦭬𧔰𧻴𨢸𩉼𩱀𪜀𪻐𫜴𫝀𫠝𫠠𫿰𬟀𬺰𭡫𮈦𰀀𱍊"));
		}

		[TestMethod]
		public void TestRuntimeDefine()
		{
			PinyinHelper.Define("𫜴", "lun");
			PinyinHelper.Define("𫜴吧", new[] { "lun", "biu" });
			Console.WriteLine(PinyinHelper.GetPinyin("𫜴"));
			Console.WriteLine(PinyinHelper.GetPinyin("𫜴吧"));
			Console.WriteLine(PinyinHelper.GetPinyin("𫜴吧一一"));
		}

		[TestMethod]
		public void TestDefineTradional()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("厂长"));
			PinyinHelper.Define("厂长", new[] { "chang", "chang" });
			Console.WriteLine(PinyinHelper.GetPinyin("厂长"));//此处应该输出chang chang
			Console.WriteLine(PinyinHelper.GetPinyin("廠長"));//此处应该同样输出chang chang，因为定义简体词汇会应用到繁体。
			PinyinHelper.Define("廠長", new[] { "chang", "zhang" });
			Console.WriteLine(PinyinHelper.GetPinyin("廠長"));//此处应该输出chang zhang，因为繁体已经重新定义。
		}

		[TestMethod]
		public void TestRuntimeLoad()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("𬺰"));
			Console.WriteLine(PinyinHelper.GetPinyin("𬺰𭡫"));
			PinyinHelper.Load("𬺰 lun\n𬺰𭡫 lun biu");
			Console.WriteLine(PinyinHelper.GetPinyin("𬺰"));
			Console.WriteLine(PinyinHelper.GetPinyin("𬺰𭡫"));
		}

		[TestMethod]
		public void GetFirstLetter()
		{
			Console.WriteLine(PinyinHelper.GetInitial("一"));
			Console.WriteLine(PinyinHelper.GetInitial("1234567"));
			Console.WriteLine(PinyinHelper.GetInitial("一鿕"));
			Console.WriteLine(PinyinHelper.GetInitial("成都"));
			Console.WriteLine(PinyinHelper.GetInitial("长城"));
			Console.WriteLine(PinyinHelper.GetInitial("〇"));
		}

		[TestMethod]
		public void Test3CharacterWord()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("当涂县"));
		}

		[TestMethod]
		public void Test4CharacterWord()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("倒打一耙"));
		}

		[TestMethod]
		public void Test5CharacterWord()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("刀口上舔血"));
		}

		[TestMethod]
		public void TestXCharacterWord()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("到什么山上唱什么歌"));
			Console.WriteLine(PinyinHelper.GetPinyin("到什么山上唱什么歌到什么山上唱什么歌"));
		}

		[TestMethod]
		public void TestExtensionA()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("㐀㲒䔤䶵"));
		}

		[TestMethod]
		public void TestExtensionBCD()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("𠀀𠧄𡎈𡵌𢜐𣃔𣪘𤑜𤸠𥟤𦆨𦭬𧔰𧻴𨢸𩉼𩱀𪜀𪻐𫜴𫝀𫠝"));
		}

		[TestMethod]
		public void TestExtensionEFG()
		{
			Console.WriteLine(PinyinHelper.GetInitial("𫠠𫿰𬟀𬺰𭡫𮈦𰀀𱍊"));
		}

		[TestMethod]
		public void TestRadicals()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("⼀⿕"));
		}


		[TestMethod]
		public void TestCompatibility()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("豈龎"));
		}

		[TestMethod]
		public void TestCompatibilitySupplement()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("丽犕𪘀"));
		}

		[TestMethod]
		public void TestTraditionalChinese()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("重庆银行川藏大区成都分行朝阳区长厦路支行重工大厦办事处九楼董事长办公室"));
			Console.WriteLine(PinyinHelper.GetPinyin("重慶銀行川藏大區成都分行朝陽區長廈路支行重工大廈辦事處九樓董事長辦公室"));
		}

		[TestMethod]
		public void TestCustomDictionary()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("啊"));
		}

		[TestMethod]
		public void TestCustomLexicon()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("啊博"));
		}

		[TestMethod]
		public void TestOverridesLexicon()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("北京市"));
		}
	}
}
