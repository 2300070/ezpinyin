using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 用于从叶典（cidianwang.com）进行数据抓取的类。
	/// </summary>
	internal static class CDictSpider
	{

		/// <summary>
		/// 以异步方式抓取常用的含多音字的词语列表。
		/// </summary>
		/// <returns>任务信息</returns>
		public static async Task LoadSamplesAsync()
		{
			Console.WriteLine();
			Console.WriteLine("扫描词典网词汇数据。");

			List<string> arguments = new List<string>(new[] { "a", "b", "c", "d", "e", "f", "g", "h", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "w", "x", "y", "z" });
			arguments.AddRange(LexiconSpider.Characters);

			await App.ForEachAsync(arguments, async key =>
			{
				if (key[0] >= 'a' && key[0] <= 'z')
				{
					await CDictSpider.LoadSamplesByLetterAsync(key);
				}
				else
				{
					await CDictSpider.LoadSamplesByKeyAsync(key);
				}
			});
		}

		/// <summary>
		/// 以异步方式加载指定样本的信息。
		/// </summary>
		/// <param name="sample">样本信息</param>
		public static async Task LoadSampleAsync(WordInfo sample)
		{
			string url = sample.CSource;
			if (url == null)
			{
				return;
			}
			if (sample.CPinyin != null)
			{
				return;
			}

			string html = await App.DownloadAsync(url);

			if (string.IsNullOrEmpty(html))
			{
				return;
			}
			Match match = Regex.Match(html, @"<h1>([^<]+)</h1>拼音：<b>([^>]+)</b>");
			if (match.Success)
			{
				string text = WebUtility.HtmlDecode(match.Groups[1].Value);
				if (text == sample.Word)
				{
					sample.CPinyin = App.ParseWordPinyin(text, match.Groups[2].Value);
					match = Regex.Match(html, "<meta[^>]+description[^>]+?content=\"([^\"]+)\"\\s*/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
					if (match.Success)
					{
						sample.ProcessMeaning(match.Groups[1].Value);
					}
				}

			}
		}

		private static async Task LoadSamplesByKeyAsync(string key)
		{
			if (key.IndexOfAny(App.Dots) > -1)
			{
				return;
			}
			int page = 1;

			do
			{
				string html = await App.DownloadAsync($"https://search.cidianwang.com/?m=1&q={Uri.EscapeDataString(key)}&page={page}&y=0");
				if (string.IsNullOrEmpty(html))
				{
					return;
				}
				Match match = Regex.Match(html, @"<h1>([^<]{2,})</h1>拼音：<b>([^>]+)</b>");
				if (match.Success)
				{
					string text = WebUtility.HtmlDecode(match.Groups[1].Value);
					if (text == key)
					{
						LexiconSpider.FindOrRegister(text).CPinyin = App.ParseWordPinyin(text, match.Groups[2].Value);
					}

					return;
				}
				MatchCollection matches = Regex.Matches(html, "<a href=\"([^\"]+)\" class=.s_link.[^>]+>词语《([^》]+)》的解释</a>.+?<b>拼音：</b>([^<]+?)(<b>五笔：</b>\\w+)?<br>([^<]+)");
				if (matches.Count > 0)
				{
					for (int i = 0; i < matches.Count; i++)
					{
						match = matches[i];
						string text = Regex.Replace(match.Groups[2].Value.Trim(), @"[ <>a-z=0-9&;/]+", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);
						if (text.IndexOfAny(App.Dots) > -1)
						{
							continue;
						}

						WordInfo word = LexiconSpider.FindOrRegister(text);
						word.CPinyin = App.ParseWordPinyin(text, match.Groups[3].Value);
						string meaning = match.Groups[5].Value.Trim();
						if (meaning.Length > 0)
						{
							word.ProcessMeaning(meaning);
						}
					}
				}
				page++;
				if (html.IndexOf($">{page}<", StringComparison.Ordinal) == -1)
				{
					break;
				}
			} while (page < 6);
		}

		private static async Task LoadSamplesByLetterAsync(string letter)
		{
			int page = 0;
			string html;
			string file = $"{letter}.htm";
			do
			{
				html = await App.DownloadAsync($"https://www.cidianwang.com/cd/{file}");
				if (html != null)
				{
					MatchCollection matches = Regex.Matches(html, @"<li>\s*<a\s*href=.(/cd/\w/\w+\.htm).[^>]+>\s*(\w+)\s*</a>\s*</li>");
					foreach (Match match in matches)
					{
						string url = match.Groups[1].Value.Trim();
						string text = match.Groups[2].Value.Trim();
						LexiconSpider.FindOrRegister(text).EnableCSource($"https://www.cidianwang.com/{url}");
					}
				}
				page++;
				file = $"{letter}{page}.htm";
			}
			while (Regex.IsMatch(html, $"<a[^>]+{file}.[^>]+>{page + 1}</a>"));
		}

	}
}