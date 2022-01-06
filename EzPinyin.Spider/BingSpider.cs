using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EzPinyin.Spider
{
	internal static class BingSpider
	{
		private const string CACHE_FILE = "../cache/bing.json";
		private static readonly ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();
		private static readonly bool latestCache;
		private static bool saveCache;

		static BingSpider()
		{
			if (File.Exists(CACHE_FILE))
			{
				cache = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(File.ReadAllText(CACHE_FILE));
				latestCache = File.GetLastWriteTime(CACHE_FILE).Date == DateTime.Today;
			}
		}

		/// <summary>
		 /// 以异步方式加载指定样本的信息。
		 /// </summary>
		 /// <param name="sample">样本信息</param>
		public static async Task LoadSampleAsync(WordInfo sample)
		{
			if (sample.BingPinyin != null)
			{
				return;
			}

			string word = sample.ActualWord;
			if (cache.TryGetValue(word, out string pinyin))
			{
				if (pinyin != null || latestCache)
				{
					sample.BingPinyin = pinyin;
					return;
				}

				cache.TryRemove(word, out pinyin);
			}
			

			try
			{
				string html = await App.DownloadAsync($"https://cn.bing.com/dict/search?q={Uri.EscapeDataString(word)}");
				if (html == null)
				{
					return;
				}

				Match match = Regex.Match(html, @"<div[^>]+>\[([^]]+)\]\s*</div>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				if (match.Success)
				{
					sample.BingPinyin = App.ParseWordPinyin(word, match.Groups[1].Value);
				}
			}
			finally
			{
				if (sample.BingPinyin != null)
				{
					cache[word] = sample.BingPinyin;
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

	}
}