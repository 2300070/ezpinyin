using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EzPinyin.Spider
{
	internal static class BingSpider
	{
		private static readonly PinyinCache cache = new PinyinCache("../cache/bing.json");

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
			if (BingSpider.cache.TryGetValue(word, out string pinyin))
			{
				sample.BingPinyin = pinyin;
				return;
			}


			try
			{
				string html = await Common.DownloadAsync($"https://cn.bing.com/dict/search?q={Uri.EscapeDataString(word)}");
				if (html == null)
				{
					return;
				}

				Match match = Regex.Match(html, @"<div[^>]+>\[([^]]+)\]\s*</div>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				if (match.Success)
				{
					sample.BingPinyin = Common.ParseWordPinyin(word, match.Groups[1].Value);
				}
			}
			finally
			{
				BingSpider.cache.Add(word, sample.BingPinyin);
			}
		}

	}
}