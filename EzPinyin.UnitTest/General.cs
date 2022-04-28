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
			Console.WriteLine(PinyinHelper.GetPinyin("川藏").AssertToBe("chuan zang"));
			Console.WriteLine(PinyinHelper.GetPinyin("兙呣瓱").AssertToBe("shike m maowa"));
			Console.WriteLine(PinyinHelper.GetPinyin("朝阳").AssertToBe("chao yang"));
			Console.WriteLine(PinyinHelper.GetPinyin("一鿿").AssertToBe("yi ying"));
			Console.WriteLine(PinyinHelper.GetPinyin("成都").AssertToBe("cheng du"));
			Console.WriteLine(PinyinHelper.GetPinyin("〇").AssertToBe("ling"));
			Console.WriteLine(PinyinHelper.GetPinyin("重庆银行川藏大区成都分行朝阳区长厦路重工大厦行动处九楼张朝阳董事长藏宝室").AssertToBe("chong qing yin hang chuan zang da qu cheng du fen hang chao yang qu chang xia lu zhong gong da sha xing dong chu jiu lou zhang chao yang dong shi zhang cang bao shi"));
			Console.WriteLine(PinyinHelper.GetPinyin("长城重工").AssertToBe("chang cheng zhong gong"));
			Console.WriteLine(PinyinHelper.GetPinyin("一埄憈歌甐绔袘鉜鰠龥㐀㲒䔤䶵龦鿕").AssertToBe("yi beng qu ge lin ku yi fu sao yu qiu bao pa chi chang dan"));
			Console.WriteLine(PinyinHelper.GetPinyin("㐀㲒䔤䶵").AssertToBe("qiu bao pa chi"));
			Console.WriteLine(PinyinHelper.GetPinyin("𠀀𠧄𡎈𡵌𢜐𣃔𣪘𤑜𤸠𥟤𦆨𦭬𧔰𧻴𨢸𩉼𩱀𪜀𪻐𫜴𫝀𫠝").AssertToBe("he gan feng cha wei duan gui zhu miao li li dai juan lang ke shen peng deng cong 𫜴 wu bie"));
			Console.WriteLine(PinyinHelper.GetPinyin("㐀㲒䔤䶵𠀀𠧄𡎈𡵌𢜐𣃔𣪘𤑜𤸠𥟤𦆨𦭬𧔰𧻴𨢸𩉼𩱀𪜀𪻐𫜴𫝀𫠝𫠠𫿰𬟀𬺰𭡫𮈦𰀀𱍊").AssertToBe("qiu bao pa chi he gan feng cha wei duan gui zhu miao li li dai juan lang ke shen peng deng cong 𫜴 wu bie yi shou teng 𬺰𭡫 ling zui chang"));
		}
		[TestMethod]
		public void GetInitial()
		{
			Console.WriteLine(PinyinHelper.GetInitial("川藏", " ").AssertToBe("c z"));
			Console.WriteLine(PinyinHelper.GetInitial("兙呣瓱", " ").AssertToBe("s m m"));
			Console.WriteLine(PinyinHelper.GetInitial("朝阳", " ").AssertToBe("c y"));
			Console.WriteLine(PinyinHelper.GetInitial("一鿿", " ").AssertToBe("y y"));
			Console.WriteLine(PinyinHelper.GetInitial("成都", " ").AssertToBe("c d"));
			Console.WriteLine(PinyinHelper.GetInitial("〇", " ").AssertToBe("l"));
			Console.WriteLine(PinyinHelper.GetInitial("重庆银行川藏大区成都分行朝阳区长厦路重工大厦行动处九楼张朝阳董事长藏宝室").AssertToBe("cqyhczdqcdfhcyqcxlzgdsxdcjlzcydszcbs"));
			Console.WriteLine(PinyinHelper.GetInitial("长城重工").AssertToBe("cczg"));
			Console.WriteLine(PinyinHelper.GetInitial("一埄憈歌甐绔袘鉜鰠龥㐀㲒䔤䶵龦鿕").AssertToBe("ybqglkyfsyqbpccd"));
			Console.WriteLine(PinyinHelper.GetInitial("㐀㲒䔤䶵").AssertToBe("qbpc"));
			Console.WriteLine(PinyinHelper.GetInitial("𠀀𠧄𡎈𡵌𢜐𣃔𣪘𤑜𤸠𥟤𦆨𦭬𧔰𧻴𨢸𩉼𩱀𪜀𪻐𫜴𫝀𫠝").AssertToBe("hgfcwdgzmlldjlkspdc𫜴wb"));
			Console.WriteLine(PinyinHelper.GetInitial("㐀㲒䔤䶵𠀀𠧄𡎈𡵌𢜐𣃔𣪘𤑜𤸠𥟤𦆨𦭬𧔰𧻴𨢸𩉼𩱀𪜀𪻐𫜴𫝀𫠝𫠠𫿰𬟀𬺰𭡫𮈦𰀀𱍊").AssertToBe("qbpchgfcwdgzmlldjlkspdc𫜴wbyst𬺰𭡫lzc"));
		}

		[TestMethod]
		public void TestRuntimeDefine()
		{
			PinyinHelper.Define("𫜴", "lun");
			PinyinHelper.Define("𫜴吧", new[] { "lun", "biu" });
			Console.WriteLine(PinyinHelper.GetPinyin("𫜴").AssertToBe("lun"));
			Console.WriteLine(PinyinHelper.GetPinyin("𫜴吧").AssertToBe("lun biu"));
			Console.WriteLine(PinyinHelper.GetPinyin("𫜴吧一一").AssertToBe("lun biu yi yi"));
		}

		[TestMethod]
		public void TestDefineTradional()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("厂长").AssertToBe("chang zhang"));
			PinyinHelper.Define("厂长", new[] { "chang", "chang" });
			Console.WriteLine(PinyinHelper.GetPinyin("厂长").AssertToBe("chang chang"));//此处应该输出chang chang
			Console.WriteLine(PinyinHelper.GetPinyin("廠長"));//此处应该同样输出chang chang，因为定义简体词汇会应用到繁体。
			PinyinHelper.Define("廠長", new[] { "chang", "zhang" });
			Console.WriteLine(PinyinHelper.GetPinyin("廠長").AssertToBe("chang zhang"));//此处应该输出chang zhang，因为繁体已经重新定义。
		}

		[TestMethod]
		public void TestRuntimeLoad()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("𬺰"));
			Console.WriteLine(PinyinHelper.GetPinyin("𬺰𭡫"));
			PinyinHelper.Load("𬺰 lun\n𬺰𭡫 lun biu");
			Console.WriteLine(PinyinHelper.GetPinyin("𬺰").AssertToBe("lun"));
			Console.WriteLine(PinyinHelper.GetPinyin("𬺰𭡫").AssertToBe("lun biu"));
		}

		[TestMethod]
		public void GetFirstLetter()
		{
			Console.WriteLine(PinyinHelper.GetInitial("一").AssertToBe("y"));
			Console.WriteLine(PinyinHelper.GetInitial("1234567").AssertToBe("1234567"));
			Console.WriteLine(PinyinHelper.GetInitial("1234567", " ").AssertToBe("1234567"));
			Console.WriteLine(PinyinHelper.GetInitial("一鿕").AssertToBe("yd"));
			Console.WriteLine(PinyinHelper.GetInitial("成都", " ").AssertToBe("c d"));
		}

		[TestMethod]
		public void Test3CharacterWord()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("当涂县").AssertToBe("dang tu xian"));
		}

		[TestMethod]
		public void Test4CharacterWord()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("倒打一耙").AssertToBe("dao da yi pa"));
		}

		[TestMethod]
		public void Test5CharacterWord()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("刀口上舔血").AssertToBe("dao kou shang tian xue"));
		}

		[TestMethod]
		public void TestXCharacterWord()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("到什么山上唱什么歌").AssertToBe("dao shen me shan shang chang shen me ge"));
			Console.WriteLine(PinyinHelper.GetPinyin("到什么山上唱什么歌Test Word到什么山上唱什么歌").AssertToBe("dao shen me shan shang chang shen me ge Test Word dao shen me shan shang chang shen me ge"));
		}

		[TestMethod]
		public void TestMixed()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("Tony老师说：“你这个发型很cool！”").AssertToBe("Tony lao shi shuo：“ni zhe ge fa xing hen cool！”"));
		}

		[TestMethod]
		public void TestMixedWithExtended()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("Tony老师说：“你这个发型很𐌂𐌏𐌏𐌉！”").AssertToBe("Tony lao shi shuo：“ni zhe ge fa xing hen 𐌂𐌏𐌏𐌉！”"));
		}

		[TestMethod]
		public void TestExtensionA()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("㐀㲒䔤䶵").AssertToBe("qiu bao pa chi"));
		}

		[TestMethod]
		public void TestExtensionBCD()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("𠀀𠧄𡎈𡵌𢜐𣃔𣪘𤑜𤸠𥟤𦆨𦭬𧔰𧻴𨢸𩉼𩱀𪜀𪻐𫜴𫝀𫠝").AssertToBe("he gan feng cha wei duan gui zhu miao li li dai juan lang ke shen peng deng cong 𫜴 wu bie"));
		}

		[TestMethod]
		public void TestExtensionEFG()
		{
			Console.WriteLine(PinyinHelper.GetInitial("𫠠𫿰𬟀𬺰𭡫𮈦𰀀𱍊").AssertToBe("yst𬺰𭡫lzc"));
		}

		[TestMethod]
		public void TestRadicals()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("⼀⿕").AssertToBe("yi yue"));
		}


		[TestMethod]
		public void TestCompatibility()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("豈龎").AssertToBe("qi pang"));
		}

		[TestMethod]
		public void TestCompatibilitySupplement()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("丽犕𪘀").AssertToBe("li bei pian"));
		}

		[TestMethod]
		public void TestTraditionalChinese()
		{
			string pinyin = "chong qing yin hang chuan zang da qu cheng du fen hang chao yang qu chang xia lu zhi hang zhong gong da sha ban shi chu jiu lou dong shi zhang ban gong shi";
			Console.WriteLine(PinyinHelper.GetPinyin("重庆银行川藏大区成都分行朝阳区长厦路支行重工大厦办事处九楼董事长办公室").AssertToBe(pinyin));
			Console.WriteLine(PinyinHelper.GetPinyin("重慶銀行川藏大區成都分行朝陽區長廈路支行重工大廈辦事處九樓董事長辦公室").AssertToBe(pinyin));
			Console.WriteLine(PinyinHelper.GetPinyin("重慶銀行川藏大區成都分行朝阳區长廈路支行重工大廈辦事处九樓董事長辦公室").AssertToBe(pinyin));
		}

		[TestMethod]
		public void TestCustomDictionary()
		{
			Console.WriteLine(PinyinHelper.GetPinyin("啊"));
		}

		[TestMethod]
		public void TestCustomLexicon()
		{
			/**
			 * 此处测试需要启用dictionary模板中的配置。
			 */
			Console.WriteLine(PinyinHelper.GetPinyin("啊博"));
		}

	}
}
