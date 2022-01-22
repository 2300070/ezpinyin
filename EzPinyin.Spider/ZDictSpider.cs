using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 用于从汉典（zdict.com）进行数据抓取的类。
	/// </summary>
	internal static class ZDictSpider
	{
		private static readonly PinyinCache cache = new PinyinCache("../cache/zdict.json");
		private static readonly ConcurrentDictionary<string, CharacterInfo> dictionary = new ConcurrentDictionary<string, CharacterInfo>();
		

		/// <summary>
		/// 抓取指定字符的拼音信息。
		/// </summary>
		/// <param name="character">需要抓取的字符。</param>
		/// <returns>该字符的拼音信息，如果没有抓取到拼音，则返回null。</returns>
		public static async Task LoadSimplifiedAsync(string character)
		{
			string html = await Common.DownloadAsync($"https://www.zdic.net/hans/{Uri.EscapeUriString(character)}");

			Match match;
			string header = null;
			if (character.Length == 1)
			{
				/**
				 * 分析繁体字，异体字信息
				 */
				header = ZDictSpider.ExtractHeader(html);
				if (header.Length > 0)
				{
					/**
					 * 分析简体字、繁体字
					 */
					match = Regex.Match(header, @"<p><span[^>]+z_ts2.>(\w)体</span>\s*<a[^>]+href=./hans/([^'])'[^>]+>((?!</a>).)+</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
					if (match.Success)
					{
						char ch = match.Groups[2].Value[0];
						switch (match.Groups[1].Value)
						{
							case "简":
							case "簡":
								Common.Simplified[character[0]] = ch;
								break;
							default:
								Common.Simplified[ch] = character[0];
								break;
						}
					}
				}
			}
		}

		/// <summary>
		/// 以异步方式抓取指定字符的拼音信息。
		/// </summary>
		/// <param name="character">需要抓取的字符。</param>
		/// <returns>该字符的拼音信息，如果没有抓取到拼音，则返回null。</returns>
		public static async Task<CharacterInfo> LoadCharacterAsync(string character)
		{
			if (dictionary.TryGetValue(character, out CharacterInfo result))
			{
				if (result.Count == 0)
				{
					return null;
				}

				return result;
			}

			if (!Common.Dictionary.TryGetValue(character, out result))
			{
				result = new CharacterInfo(character);
			}
			else if (result.Count > 0)
			{
				return result;
			}

			dictionary[character] = result;

			string html = await Common.DownloadAsync($"https://www.zdic.net/hans/{Uri.EscapeUriString(character)}");

			Match match;
			string header = null;
			if (character.Length == 1)
			{
				/**
				 * 尝试截取新华字典解释部分并分析。
				 */
				match = Regex.Match(html, @"<div[^>]+nr-box-shiyi jbjs[^>]+>([\W\w]+?)div copyright.>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				if (match.Success)
				{
					ZDictSpider.LoadFromXinhuaExplain(character, match.Groups[1].Value, result);
				}

				/**
				 * 尝试截取现代汉语词典解释部分并分析。
				 */
				match = Regex.Match(html, @"<div[^>]+nr-box-shiyi xxjs[^>]+>([\W\w]+?)div copyright.>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				if (match.Success)
				{
					ZDictSpider.LoadFromXianhanExplain(character, match.Groups[1].Value, result);
				}

				/**
				 * 分析繁体字，异体字信息
				 */
				header = ZDictSpider.ExtractHeader(html);
				if (header.Length > 0)
				{
					/**
					 * 分析简体字、繁体字
					 */
					match = Regex.Match(header, @"<p><span[^>]+z_ts2.>(\w)体</span>\s*<a[^>]+href=./hans/([^'])'[^>]+>((?!</a>).)+</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
					if (match.Success)
					{
						char ch = match.Groups[2].Value[0];
						switch (match.Groups[1].Value)
						{
							case "简":
							case "簡":
								result.Simplified = ch;
								break;
							default:
								result.Traditional = ch;
								break;
						}
					}

					/**
					 * 分析异体字
					 */

					match = Regex.Match(header, @"<td[^>]+z_ytz\d.>((<a href=./han./([^']+)'[^>]+>((?!</a>).)+</a>\s*)+)</td>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
					if (match.Success)
					{
						List<char> variants = result.Variants ?? (result.Variants = new List<char>());
						MatchCollection matches = Regex.Matches(match.Groups[1].Value, @"<a href=./han./([^']+)' target=_blank>((?!</a>).)+</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
						for (int i = 0; i < matches.Count; i++)
						{
							match = matches[i];
							string text = match.Groups[1].Value;
							if (text.Length == 1 && !variants.Contains(text[0]))
							{
								variants.Add(text[0]);
							}
						}
					}
				}
			}


			if (result.Count == 0)
			{
				/**
				 * 如果没找到基本解释，尝试从顶部的概览信息栏获取。
				 */
				ZDictSpider.LoadFromHeaderLine(header ?? ZDictSpider.ExtractHeader(html), result);
			}

			if (result.Count == 0)
			{
				return null;
			}
			Common.Dictionary[character] = result;
			result.ZDictPinyin = result.ComputePrefered()?.Text;
			result.IsTrusted = true;
			return result;
		}

		/// <summary>
		/// 以异步方式抓取词汇列表。
		/// </summary>
		/// <returns>任务信息</returns>
		public static async Task LoadSamplesAsync()
		{
			Console.WriteLine();
			Console.WriteLine("扫描汉典词汇数据。");
			await Common.ForEachAsync(LexiconSpider.Characters, ZDictSpider.LoadSamplesAsync);
		}

		/// <summary>
		/// 以异步方式加载指定样本的信息。
		/// </summary>
		/// <param name="sample">样本信息</param>
		public static async Task LoadSampleAsync(WordInfo sample)
		{
			string url = sample.ZDictSource;
			if (url == null)
			{
				return;
			}

			if (sample.ZDictPinyin != null)
			{
				return;
			}
			string word = sample.ActualWord;
			if (ZDictSpider.cache.TryGetValue(word, out string pinyin))
			{
				sample.ZDictPinyin = pinyin;
				return;
			}

			try
			{
				string html = await Common.DownloadAsync(url);
				if (html == null)
				{
					return;
				}

				/**
				 * 加载拼音。
				 */
				ZDictSpider.AnalysePinyin(html, sample);
			}
			finally
			{
				ZDictSpider.cache.Add(word, sample.ZDictPinyin);
			}

		}


		private static async Task LoadSamplesAsync(string character)
		{
			/**
			 * 先以词头的形式进行搜索。
			 */
			int page;
			string html;
			page = 0;
			while (true)
			{
				html = await Common.DownloadAsync($"https://www.zdic.net/e/sci/index.php?page={page}&keyboard={Uri.EscapeDataString(character)}E&classid=8&sear=1");
				if (html == null)
				{
					break;
				}
				page++;
				int index = html.IndexOf("<div class=\"sslist\">", StringComparison.Ordinal);
				if (index == -1)
				{
					break;
				}

				string content = html.Substring(index, html.IndexOf("</ul>", index, StringComparison.Ordinal) - index);
				MatchCollection matches = Regex.Matches(content, @"<li><a [^>]+/hans/[^>]+usual.>\s*([^<]+)\s*(<span[^>]+ef.>([^<]+)</span>)?</a></li>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				foreach (Match match in matches)
				{
					string word = match.Groups[1].Value.Trim();
					if (word.Length != 2)
					{
						continue;
					}

					WordInfo info = LexiconSpider.FindOrRegister(word);
					if (info.IsValid)
					{
						info.EnableZDictSource();
						info.ZDictPinyin = Common.ParseWordPinyin(word, match.Groups[3].Value);
					}

				}

				if (!html.Contains($">{page}<"))
				{
					break;
				}
			}

			/**
			 * 接着以部首的方式进行搜索。
			 */
			page = 1;
			while (true)
			{
				html = await Common.DownloadAsync($"https://www.zdic.net/cd/bs/ci/?z={Uri.EscapeDataString(character)}|{page}");
				page++;
				if (html == null)
				{
					break;
				}
				int index = html.IndexOf("<div class='cizilist'>", StringComparison.Ordinal);
				if (index == -1)
				{
					break;
				}

				string content = html.Substring(index, html.IndexOf("<div id='gg_bslot_a'", index, StringComparison.Ordinal) - index);
				MatchCollection matches = Regex.Matches(content, @"<li><a [^>]+/hans/(\w+)[^>]+>\1(<span class=.ef.>(.*)</span>)?</a></li>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				foreach (Match match in matches)
				{
					LexiconSpider.FindOrRegister(match.Groups[1].Value.Trim()).EnableZDictSource();
				}

				if (!html.Contains($">{page}<"))
				{
					break;
				}
			}

		}

		private static void LoadFromXinhuaExplain(string character, string html, CharacterInfo info)
		{
			html = Regex.Replace(html, "[\r\n]+", string.Empty, RegexOptions.Compiled);
			MatchCollection matches = Regex.Matches(html, @"<p>[◎●].*?<[^>]+dicpy.>([^<]+)<([^>]+ptr)?");
			if (matches.Count == 0)
			{
				matches = Regex.Matches(html, @"<[^>]+dicpy.>([^<]+)<([^>]+ptr)?");
			}
			for (int i = 0; i < matches.Count; i++)
			{
				Match match = matches[i];
				PinyinInfo pinyin = info.RegisterAll(match.Groups[1].Value);
				if (pinyin == null)
				{
					continue;
				}

				/**
				 * 截取解释信息
				 */
				string explain;
				int index = match.Index + match.Length;
				CharacterType type = CharacterType.Other;
				if (i + 1 < matches.Count)
				{
					explain = html.Substring(index, matches[i + 1].Index - index);
				}
				else
				{
					explain = html.Substring(index);
				}

				/**
				 * 尝试分析词性。
				 */
				if (Regex.IsMatch(explain, @"\b(.名|姓|名词)\b"))
				{
					type = CharacterType.Noun;
				}
				else if (Regex.IsMatch(explain, @"\b形容词\b"))
				{
					type = CharacterType.Adjective;
				}
				else if (Regex.IsMatch(explain, @"\b叹词\b"))
				{
					type = CharacterType.Interjections;
				}
				else if (Regex.IsMatch(explain, @"\b副词\b"))
				{
					type = CharacterType.Adverb;
				}
				else if (Regex.IsMatch(explain, @"\b助词\b"))
				{
					type = CharacterType.Auxiliary;
				}
				else if (Regex.IsMatch(explain, @"\b动词\b"))
				{
					type = CharacterType.Verb;
				}

				if (Regex.IsMatch(explain, @"([地省市州县]名)(?=\W)", RegexOptions.Compiled))
				{
					type = CharacterType.Noun;
					pinyin.AddEvaluation(Common.EXTRA_EVALUATION);
				}

				MatchCollection collection = Regex.Matches(explain, @"(\w+用字|译音字)", RegexOptions.Compiled);
				if (collection.Count > 0)
				{
					type = CharacterType.Noun;
					pinyin.AddEvaluation(Common.EXTRA_EVALUATION * collection.Count);
				}

				/**
				 * 尝试分析义项数量。
				 */

				/**
				 * 此处本来用正则表达式，但是不清楚用<li>.+?</li>或者<li>(((?!</li>)[^\n])+)</li>在RegexBuddy中都是正常，唯独程序中运行时总是少了最后一项，所以改成硬编码了。
				 */
				index = explain.IndexOf("<li>", StringComparison.Ordinal);
				int index2;
				if (index == -1)
				{
					index = explain.IndexOf("<ol>", StringComparison.Ordinal);
					if (index == -1)
					{
						continue;
					}
					index2 = explain.LastIndexOf("</ol>", StringComparison.Ordinal);
				}
				else
				{
					index2 = explain.LastIndexOf("</li>", StringComparison.Ordinal);
				}

				if (index2 == -1)
				{
					index2 = explain.LastIndexOf("</ol>", StringComparison.Ordinal);
				}

				if (index2 == -1)
				{
					index2 = explain.Length;
				}

				string[] meanings = explain.Substring(index, index2 + 5 - index).Split(new[] { "</li>" }, StringSplitOptions.RemoveEmptyEntries);

				pinyin.AddType(type, meanings.Length);

				MatchCollection words;
				foreach (string meaning in meanings)
				{
					/**
					 * 分析常见用法数量。
					 */
					words = Regex.Matches(meaning, @"[：。]([^。\s]*～[^。\s]*)", RegexOptions.Compiled);
					foreach (Match item in words)
					{
						string text = item.Groups[1].Value.Replace("～", character);
						if (text.Length < 6)
						{
							LexiconSpider.FindOrRegister(text).ExplainPinyin(character, pinyin.Text);
						}
					}


					match = Regex.Match(meaning, @">〔([^〕]+)〕([^<]+)", RegexOptions.Compiled);
					if (match.Success)
					{
						string text = match.Groups[1].Value.Replace("～", character);
						/**
						 * 如果词汇比较老，则防止覆盖现有拼音。
						 */
						string description = match.Groups[2].Value;
						if (Regex.IsMatch(description, "古[代时]|戏[曲文]|旧小说|封建|[帝王将相]") && Common.Samples.ContainsKey(text))
						{
							continue;
						}


						WordInfo word = LexiconSpider.FindOrRegister(text);
						if (word.IsValid)
						{
							word.ExplainPinyin(character, pinyin.Text);
						}

					}
				}

			}
		}

		private static void LoadFromXianhanExplain(string character, string html, CharacterInfo info)
		{
			html = Regex.Replace(html, "[\r\n]+", string.Empty, RegexOptions.Compiled);
			MatchCollection matches = Regex.Matches(html, @"[◎●].*?<[^>]+dicpy.>([^<]+)<([^>]+ptr)?");
			if (matches.Count == 0)
			{
				matches = Regex.Matches(html, @"<[^>]+dicpy.>([^<]+)<([^>]+ptr)?");
			}
			for (int i = 0; i < matches.Count; i++)
			{
				Match match = matches[i];

				PinyinInfo pinyin = info.RegisterAll(match.Groups[1].Value);
				if (pinyin == null)
				{
					continue;
				}


				pinyin.AddEvaluation(1D);//为义项加评估分。

				/**
				 * 截取解释信息
				 */
				string explain;
				int index = match.Index + match.Length;
				CharacterType type = CharacterType.Other;

				if (i + 1 < matches.Count)
				{
					explain = html.Substring(index, matches[i + 1].Index - index);
				}
				else
				{
					explain = html.Substring(index);
				}


				/**
				 * 分析词性。
				 */
				match = Regex.Match(explain, "<p>〈(.)〉</p>");
				if (!match.Success)
				{
					match = Regex.Match(explain, @"<span[^>]+xx_cx.>\s*(.)\s*</span>");
				}
				if (match.Success)
				{
					switch (match.Groups[1].Value)
					{
						case "名":
							type = CharacterType.Noun;
							break;
						case "形":
							type = CharacterType.Adjective;
							break;
						case "动":
							type = CharacterType.Verb;
							break;
						case "副":
							type = CharacterType.Adverb;
							break;
						case "助":
							type = CharacterType.Auxiliary;
							break;
						case "叹":
							type = CharacterType.Interjections;
							break;
					}
				}

				/**
				 * 额外加权
				 */
				if (Regex.IsMatch(explain, @"([地|省|市|州|县]名)(?=\W)", RegexOptions.Compiled))
				{
					type = CharacterType.Noun;
					pinyin.AddEvaluation(Common.EXTRA_EVALUATION);
				}

				pinyin.AddType(type, Regex.Matches(explain, @"<span[^>]+.cino.>").Count);


				/**
				 * 分析常见用词情况。
				 */
				ZDictSpider.AnalyseCommonUsagesForXianhan(character, pinyin, explain);
			}
		}

		private static void LoadFromHeaderLine(string html, CharacterInfo info) => ZDictSpider.RegisterPinyin(Regex.Matches(html, @"<span class=.z_d song.>([^<]+)<"), info);

		private static void AnalyseCommonUsagesForXianhan(string character, PinyinInfo pinyin, string explain)
		{
			MatchCollection matches;
			/**
			 * 匹配义项中的用例。
			 */
			matches = Regex.Matches(explain, @"<p><span class=.cino.>((?!</p>).)+如:([^<]+)</p>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			pinyin.AddEvaluation(matches.Count);//为义项加评估分。

			/**
			 * 匹配词组举例。
			 */
			matches = Regex.Matches(explain, @"<p><span[^>]+diczx3.>([^>]+)</span></p>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			pinyin.AddEvaluation(matches.Count);//为义项加评估分。
			foreach (Match match in matches)
			{
				/**
				 * 匹配出所有义项中的举例，提取出来变成词汇信息。
				 */
				MatchCollection words = Regex.Matches(match.Groups[1].Value, @"[:，;]([\w\s]{2,})(\([^)]+\))?", RegexOptions.Compiled);
				foreach (Match item in words)
				{
					WordInfo word = LexiconSpider.FindOrRegister(item.Groups[1].Value);
					if (word.IsValid)
					{
						word.ExplainPinyin(character, pinyin.Text);
					}
				}
			}

			/**
			 * 匹配常用词组。
			 */
			matches = Regex.Matches(explain, @"<span class=.crefe.><a[^>]+/hans/(\w+).>\1</a></span>", RegexOptions.Compiled);
			foreach (Match match in matches)
			{
				WordInfo word = LexiconSpider.FindOrRegister(match.Groups[1].Value);
				if (word.IsValid)
				{
					word.EnableZDictSource();
				}
			}
		}

		private static void AnalysePinyin(string html, WordInfo sample)
		{
			Match match;

			/**
			 * 根据优先顺序依次尝试解析出对应的拼音。
			 */

			/**
			 * 解析出现代汉语词典的拼音
			 */
			match = Regex.Match(html, @"<strong>[^<]+</strong>\s*<span[^>]+dicpy.>\s*([^<]+)\s*<", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				if ((sample.ZDictPinyin = Common.ParseWordPinyin(sample.Word, match.Groups[1].Value)) != null)
				{
					sample.Verified = true;
					return;
				}
			}

			/**
			 * 解析出国语辞典的拼音
			 */
			match = Regex.Match(html, @"<rt>([^<]+)<span[^>]+ptr.><a[^>]+audio_play_button", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				if ((sample.ZDictPinyin = Common.ParseWordPinyin(sample.Word, match.Groups[1].Value)) != null)
				{
					return;
				}
			}
			match = Regex.Match(html, @"<rt>([^<]+)</rt>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				if ((sample.ZDictPinyin = Common.ParseWordPinyin(sample.Word, match.Groups[1].Value)) != null)
				{
					return;
				}
			}

			/**
			 * 解析出头部的拼音
			 */
			match = Regex.Match(html, @"拼音[^\n]+dicpy[^>]+>([^<]+)</span>[^\n]+z_d song[^\n]+ptr[^\n]+audio_play_button", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				if ((sample.ZDictPinyin = Common.ParseWordPinyin(sample.Word, match.Groups[1].Value)) != null)
				{
					return;
				}
			}
		}
		
		private static void RegisterPinyin(MatchCollection matches, CharacterInfo info)
		{
			foreach (Match match in matches)
			{
				info.RegisterAll(match.Groups[1].Value);
			}
		}

		private static string ExtractHeader(string html)
		{
			int index = html.IndexOf("<table border=\"0\" class=\"dsk\">", StringComparison.Ordinal);
			if (index > 0)
			{
				return html.Substring(index, html.IndexOf("</table>", index, StringComparison.Ordinal) - index);
			}

			return string.Empty;
		}
	}
}