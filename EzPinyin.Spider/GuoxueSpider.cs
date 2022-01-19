using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 用于从国学网进行数据抓取的类。
	/// </summary>
	internal static class GuoxueSpider
	{
		private static readonly PinyinCache cache = new PinyinCache("../cache/guoxue.json");

		/// <summary>
		/// 以异步方式加载指定样本的信息。
		/// </summary>
		/// <param name="sample">样本信息</param>
		public static async Task LoadSampleAsync(WordInfo sample)
		{
			string url = sample.GuoxueSource;
			if (url == null)
			{
				return;
			}
			if (sample.GuoxuePinyin != null)
			{
				return;
			}
			string word = sample.ActualWord;
			if (GuoxueSpider.cache.TryGetValue(word, out string pinyin))
			{
				sample.GuoxuePinyin = pinyin;
				return;
			}

			try
			{

				string html = await App.DownloadAsync(url);

				if (string.IsNullOrEmpty(html))
				{
					return;
				}

				Match match = Regex.Match(html, @"<font class=.titci.>([^<]+)<", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				if (match.Success)
				{
					string text = match.Groups[1].Value.Trim();
					if (text.Length > word.Length)
					{
						sample.GuoxuePinyin = App.ParseWordPinyin(word, text.Substring(word.Length));
					}
				}
			}
			finally
			{
				GuoxueSpider.cache.Add(word, sample.GuoxuePinyin);
			}
		}

		/// <summary>
		/// 以异步方式抓取指定字符的拼音信息。
		/// </summary>
		/// <param name="character">需要抓取的字符。</param>
		/// <returns>该字符的拼音信息，如果没有抓取到拼音，则返回null。</returns>
		public static async Task<CharacterInfo> LoadCharacterAsync(string character)
		{
			if (App.Dictionary.TryGetValue(character, out CharacterInfo result))
			{
				if (result.Count > 0 && (result.IsTrusted || result.IsStandard))
				{
					return result;
				}
			}
			else
			{
				result = new CharacterInfo(character);
				App.Dictionary[character] = result;
			}

			if (result.GuoxuePinyin != null)
			{
				return result;
			}

			string html = await App.DownloadAsync($"http://www.guoxuedashi.net/zidian/so.php?sokeyzi={Uri.EscapeDataString(character)}&submit=&kz=1");
			if (html == null)
			{
				return null;
			}

			if (html.TrimStart().StartsWith("<script"))
			{
				int index = html.IndexOf('\'');
				if (index > 0)
				{
					string url = html.Substring(index + 1, html.IndexOf('\'', index + 1) - index - 1);
					html = await App.DownloadAsync($"http://www.guoxuedashi.net{url}");
					if (html == null)
					{
						return null;
					}
				}
			}

			Match match = Regex.Match(html, @"<td colspan=.2.>拼音[:：]([^<]+)</td>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				if ((result.GuoxuePinyin = result.RegisterAll(match.Groups[1].Value)?.Text) != null)
				{
					return result;
				}
			}

			return null;
		}

		/// <summary>
		/// 以异步方式抓取词汇列表。
		/// </summary>
		/// <returns>任务信息</returns>
		public static async Task LoadSamplesAsync()
		{
			Console.WriteLine();
			Console.WriteLine("扫描国学网词汇数据。");
			await App.ForEachAsync(new[] { "a", "ai", "an", "ang", "ao", "ba", "bai", "ban", "bang", "bao", "bei", "ben", "beng", "bi", "bian", "biao", "bie", "bin", "bing", "bo", "bu", "ca", "cai", "can", "cang", "cao", "ce", "cei", "cen", "ceng", "cha", "chai", "chan", "chang", "chao", "che", "chen", "cheng", "chi", "chong", "chou", "chu", "chua", "chuai", "chuan", "chuang", "chui", "chun", "chuo", "ci", "cong", "cou", "cu", "cuan", "cui", "cun", "cuo", "da", "dai", "dan", "dang", "dao", "de", "dei", "deng", "di", "dia", "dian", "diao", "die", "ding", "diu", "dong", "dou", "du", "duan", "dui", "dun", "duo", "e", "ei", "en", "eng", "er", "fa", "fan", "fang", "fei", "fen", "feng", "fiao", "fo", "fou", "fu", "ga", "gai", "gan", "gang", "gao", "ge", "gei", "gen", "geng", "gong", "gou", "gu", "gua", "guai", "guan", "guang", "gui", "gun", "guo", "ha", "hai", "han", "hang", "hao", "he", "hei", "hen", "heng", "hm", "hng", "hong", "hou", "hu", "hua", "huai", "huan", "huang", "hui", "hun", "huo", "ji", "jia", "jian", "jiang", "jiao", "jie", "jin", "jing", "jiong", "jiu", "ju", "juan", "jue", "jun", "ka", "kai", "kan", "kang", "kao", "ke", "ken", "keng", "kong", "kou", "ku", "kua", "kuai", "kuan", "kuang", "kui", "kun", "kuo", "la", "lai", "lan", "lang", "lao", "le", "lei", "leng", "li", "lia", "lian", "liang", "liao", "lie", "lin", "ling", "liu", "long", "lou", "lu", "luan", "lüe", "lun", "luo", "m", "ma", "mai", "man", "mang", "mao", "me", "mei", "men", "meng", "mi", "mian", "miao", "mie", "min", "ming", "miu", "mo", "mou", "mu", "n", "na", "nai", "nan", "nang", "nao", "ne", "nei", "nen", "neng", "ng", "ni", "nian", "niang", "niao", "nie", "nin", "ning", "niu", "nong", "nou", "nu", "nuan", "nüe", "nun", "nuo", "o", "ou", "pa", "pai", "pan", "pang", "pao", "pei", "pen", "peng", "pi", "pian", "piao", "pie", "pin", "ping", "po", "pou", "pu", "qi", "qia", "qian", "qiang", "qiao", "qie", "qin", "qing", "qiong", "qiu", "qu", "quan", "que", "qun", "ran", "rang", "rao", "re", "ren", "reng", "ri", "rong", "rou", "ru", "ruan", "rui", "run", "ruo", "sa", "sai", "san", "sang", "sao", "se", "sen", "seng", "sha", "shai", "shan", "shang", "shao", "she", "shei", "shen", "sheng", "shi", "shou", "shu", "shua", "shuai", "shuan", "shuang", "shui", "shun", "shuo", "si", "song", "sou", "su", "suan", "sui", "sun", "suo", "ta", "tai", "tan", "tang", "tao", "te", "teng", "ti", "tian", "tiao", "tie", "ting", "tong", "tou", "tu", "tuan", "tui", "tun", "tuo", "wa", "wai", "wan", "wang", "wei", "wen", "weng", "wo", "wu", "xi", "xia", "xian", "xiang", "xiao", "xie", "xin", "xing", "xiong", "xiu", "xu", "xuan", "xue", "xun", "ya", "yai", "yan", "yang", "yao", "ye", "yi", "yin", "ying", "yo", "yong", "you", "yu", "yuan", "yue", "yun", "za", "zai", "zan", "zang", "zao", "ze", "zei", "zen", "zeng", "zha", "zhai", "zhan", "zhang", "zhao", "zhe", "zhei", "zhen", "zheng", "zhi", "zhong", "zhou", "zhu", "zhua", "zhuai", "zhuan", "zhuang", "zhui", "zhun", "zhuo", "zi", "zong", "zou", "zu", "zuan", "zui", "zun", "zuo" }, async pinyin =>
			  {
				  string html = await App.DownloadAsync($"http://www.guoxuedashi.net/hydcd/py/{pinyin}.html");
				  if (html != null)
				  {
					  MatchCollection matches = Regex.Matches(html, @"<a target=._blank. href=./hydcd/zi_([^>]+)\.html", RegexOptions.Compiled | RegexOptions.IgnoreCase);
					  foreach (Match match in matches)
					  {
						  await GuoxueSpider.LoadSamplesAsync(match.Groups[1].Value);
					  }
				  }
			  });
		}

		private static async Task LoadSamplesAsync(string header)
		{
			string html = await App.DownloadAsync($"http://www.guoxuedashi.net/hydcd/zi_{header}.html");
			if (html != null)
			{
				MatchCollection matches = Regex.Matches(html, @"<a target=._blank. href=./hydcd/([^>]+)\.html.>([^<]+)</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				foreach (Match match in matches)
				{
					LexiconSpider.FindOrRegister(match.Groups[2].Value.Trim()).EnableGuoxueSource(match.Groups[1].Value);
				}
			}
		}
	}
}