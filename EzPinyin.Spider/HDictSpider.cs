using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 用于从汉文学网（hwxnet.com）进行数据抓取的类。
	/// </summary>
	internal static class HDictSpider
	{
		private const string CACHE_FILE = "../cache/hdict.json";
		private static readonly ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();
		private static readonly bool latestCache;
		private static bool saveCache;

		static HDictSpider()
		{
			if (File.Exists(CACHE_FILE))
			{
				cache = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(File.ReadAllText(CACHE_FILE));
				latestCache = File.GetLastWriteTime(CACHE_FILE).Date == DateTime.Today;
			}
		}

		/// <summary>
		/// 以异步方式抓取常用的含多音字的词语列表。
		/// </summary>
		/// <returns>任务信息</returns>
		public static async Task LoadSamplesAsync()
		{
			Console.WriteLine();
			Console.WriteLine("扫描汉文学网词汇数据。");

			await App.ForEachAsync(LexiconSpider.Characters, HDictSpider.LoadSamplesAsync);
		}

		/// <summary>
		/// 以异步方式加载指定样本的信息。
		/// </summary>
		/// <param name="sample">样本信息</param>
		public static async Task LoadSampleAsync(WordInfo sample)
		{
			string url = sample.HSource;
			if (url == null)
			{
				return;
			}
			if (sample.HPinyin != null)
			{
				return;
			}
			string word = sample.ActualWord;
			if (cache.TryGetValue(word, out string pinyin))
			{
				if (pinyin != null || latestCache)
				{
					sample.HPinyin = pinyin;
					return;
				}

				cache.TryRemove(word, out pinyin);
			}

			try
			{
				string html = await App.DownloadAsync(url);
				if (html == null)
				{
					return;
				}
				Match match = Regex.Match(html, @"<span[^>]+.pinyin f20.>([^<]+)</span>");
				if (match.Success)
				{
					sample.HPinyin = App.ParseWordPinyin(sample.Word, match.Groups[1].Value);
				}
			}
			finally
			{
				if (sample.HPinyin != null)
				{
					cache[word] = sample.HPinyin;
					saveCache = true;
				}
			}
		}
		
		/// <summary>
		/// 保存缓存文件。
		/// </summary>
		public static void SaveCache()
		{
			if (saveCache)
			{
				File.WriteAllText(CACHE_FILE, JsonConvert.SerializeObject(cache));
			}
		}

		private static async Task LoadSamplesAsync(string character)
		{
			int page = 1;
			while (true)
			{
				string html = await App.DownloadAsync($"https://cd.hwxnet.com/search.do?wd={Uri.EscapeDataString(character)}&pageno={page}");
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
					if (word.IndexOfAny(App.Dots) > -1)
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
						info.HPinyin = App.ParseWordPinyin(word, text);
					}
					info.EnableHSource($"https://cd.hwxnet.com/{match.Groups[1].Value}");
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