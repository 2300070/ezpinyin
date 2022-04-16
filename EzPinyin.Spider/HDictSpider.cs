using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 用于从汉文学网（hwxnet.com）进行数据抓取的类。
	/// </summary>
	internal static class HDictSpider
	{
		private static readonly PinyinCache cache  = new PinyinCache("../cache/hdict.json");

		/// <summary>
		/// 以异步方式抓取词汇列表。
		/// </summary>
		/// <returns>任务信息</returns>
		public static async Task LoadSamplesAsync()
		{
			Console.WriteLine();
			Console.WriteLine("扫描汉文学网词汇数据。");

			await Common.ForEachAsync(LexiconSpider.Characters, HDictSpider.LoadSamplesAsync);
		}

		/// <summary>
		/// 以异步方式加载指定样本的信息。
		/// </summary>
		/// <param name="sample">样本信息</param>
		public static async Task LoadSampleAsync(WordInfo sample)
		{
			string url = sample.HDictSource;
			if (url == null)
			{
				return;
			}
			if (sample.HDictPinyin != null)
			{
				return;
			}
			string word = sample.ActualWord;
			if (HDictSpider.cache.TryGetValue(word, out string pinyin))
			{
				sample.HDictPinyin = pinyin;
				return;
			}

			try
			{
				string html = await Common.DownloadAsync(url);
				if (html == null)
				{
					return;
				}
				Match match = Regex.Match(html, @"<span[^>]+.pinyin f20.>([^<]+)</span>");
				if (match.Success)
				{
					sample.HDictPinyin = Common.ParseWordPinyin(sample.Word, match.Groups[1].Value);
				}
			}
			finally
			{
				HDictSpider.cache.Add(word, sample.HDictPinyin);
			}
		}


		private static async Task LoadSamplesAsync(string character)
		{
			int page = 1;
			while (true)
			{
				string html = await Common.DownloadAsync($"https://cd.hwxnet.com/search.do?wd={Uri.EscapeDataString(character)}&pageno={page}");
				if (string.IsNullOrEmpty(html))
				{
					break;
				}

				MatchCollection matches = Regex.Matches(html, @"<li>\s+<a[^>]+href=.([^>]+html).>([^<]+)</a>\s*(<span[^>]+>([^<]+)</span>)?[^<]*</li>");
				if (matches.Count == 0)
				{
					break;
				}

				foreach (Match match in matches)
				{
					string word = match.Groups[2].Value.Trim();
					if (word.IndexOfAny(Common.Dots) > -1)
					{
						continue;
					}

					WordInfo info = LexiconSpider.FindOrRegister(word);
					if (!info.IsValid)
					{
						continue;
					}
					string text = match.Groups[4].Value;
					if (text.Length > 0)
					{
						info.HDictPinyin = Common.ParseWordPinyin(word, text);
					}
					info.EnableHDictSource($"https://cd.hwxnet.com/{match.Groups[1].Value}");
				}

				if (html.IndexOf(">下一页</a>", StringComparison.Ordinal) == -1)
				{
					break;
				}

				page++;
			}
		}
	}
}