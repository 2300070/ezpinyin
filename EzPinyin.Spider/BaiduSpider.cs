﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 用于从百度汉语进行数据抓取的类。
	/// </summary>
	internal static class BaiduSpider
	{

		/// <summary>
		/// 以异步方式抓取常用的含多音字的词语列表。
		/// </summary>
		/// <returns>任务信息</returns>
		public static async Task LoadSamplesAsync()
		{
			Console.WriteLine();
			Console.WriteLine("扫描百度汉语词汇数据。");

			await App.ForEachAsync(LexiconSpider.Characters, BaiduSpider.LoadSamplesAsync);
		}

		/// <summary>
		/// 以异步方式加载指定样本的信息。
		/// </summary>
		/// <param name="sample">样本信息</param>
		public static async Task LoadSampleAsync(WordInfo sample)
		{
			if (sample.BPinyin != null)
			{
				return;
			}
			string html = await App.DownloadAsync($"https://hanyu.baidu.com/s?wd={Uri.EscapeDataString(sample.Word)}&ptype=zici");
			if (html == null)
			{
				return;
			}

			Match match = Regex.Match(html, @"<b>\[\s*([^]]+)\s*\]</b>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				sample.BPinyin = App.ParseWordPinyin(sample.Word, match.Groups[1].Value);
			}
			else
			{
				match = Regex.Match(html, @"<div[^>]+>\s*<p>[^<>]*[拼读]音[为是]?\s*([\u0041-\u024F\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				if (match.Success)
				{
					sample.BPinyin = App.ParseWordPinyin(sample.Word, match.Groups[1].Value);
				}
			}
			match = Regex.Match(html, "<meta [^>]+description[^>]+content=\"([^\"]+)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				sample.ProcessMeaning(match.Groups[1].Value.Trim());
			}
			else
			{
				match = Regex.Match(html, @"<dd>(([^<>]|<p>|</p>)+)</dd>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				if (match.Success)
				{
					sample.ProcessMeaning(Regex.Replace(match.Groups[1].Value.Trim(), @"<[^>]+>|\s+", string.Empty, RegexOptions.Compiled));
				}
			}
		}

		
		/// <summary>
		/// 以异步方式从百度百科加载指定样本的信息。
		/// </summary>
		/// <param name="sample">样本信息</param>
		public static async Task LoadSampleFromBaikeAsync(WordInfo sample)
		{
			if (sample.InBaiduBaike.HasValue)
			{
				return;
			}
			string html = await App.DownloadAsync($"https://baike.baidu.com/item/{Uri.EscapeDataString(sample.Word)}");
			if (html == null)
			{
				sample.InBaiduBaike = false;
				return;
			}

			Match match = Regex.Match(html, @"[拼发][^<]*音</dt>\s*<dd[^>]+>\s*([^<]+?)\s*</dd>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				sample.InBaiduBaike = true;
				sample.BKPinyin = App.ParseWordPinyin(sample.Word, match.Groups[1].Value);
				return;
			}
			match = Regex.Match(html, @"content=.[^\n]*[拼发读]音[是为]?([^,，。、\n<>]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				sample.InBaiduBaike = true;
				sample.BKPinyin = App.ParseWordPinyin(sample.Word, match.Groups[1].Value);
				return;
			}
			match = Regex.Match(html, @"<span[^>]+pinyin[^>]+>\s*<span[^>]+>\[([^\]]+)\]</span>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				sample.InBaiduBaike = true;
				sample.BKPinyin = App.ParseWordPinyin(sample.Word, match.Groups[1].Value);
				return;
			}
			match = Regex.Match(html, @"<div[^>]+para[^>]+>[^\n]*[拼发读]音[是为]?([^,，。、\n<>]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match.Success)
			{
				sample.InBaiduBaike = true;
				sample.BKPinyin = App.ParseWordPinyin(sample.Word, match.Groups[1].Value);
				return;
			}

			sample.InBaiduBaike = !html.Contains("百度百科错误页");
		}

		internal static async Task LoadSamplesAsync(string character)
		{
			int page = 1;
			while (true)
			{
				string json = await App.DownloadAsync($"https://hanyu.baidu.com/hanyu/ajax/search_list?wd={Uri.EscapeDataString($"{character}组词")}&cf=zuci&pn={page}");
				if (string.IsNullOrEmpty(json))
				{
					return;
				}

				JToken obj = JToken.Parse(json);
				if (obj == null || obj.Type != JTokenType.Object)
				{
					return;
				}
				if (obj?["ret_array"] is JArray array)
				{
					foreach (JObject item in array)
					{
						JArray value = item["name"] as JArray;
						if (value == null || value.Count == 0 || value[0].Type != JTokenType.String)
						{
							continue;
						}
						string name = value[0].ToString();
						if (name.IndexOfAny(App.Dots) > -1)
						{
							continue;
						}
						value = item["pinyin"] as JArray;
						if (value == null || value.Count == 0 || value[0].Type != JTokenType.String)
						{
							continue;
						}

						WordInfo word = LexiconSpider.FindOrRegister(name);

						if (!word.IsValid)
						{
							continue;
						}

						string pinyin = null;
						if (value.Count == 1)
						{
							pinyin = value[0].ToString();
						}
						else
						{
							string prefered = string.Join(" ", Array.ConvertAll(name.ToCharArray(), c => App.Dictionary[new string(c, 1)].PreferedPinyin));
							JArray meanings = item["mean_list"] as JArray;
							if (meanings != null && meanings.Count > 0)
							{
								int max = 0;
								foreach (JObject meaning in meanings)
								{
									JArray definitions = meaning["definition"] as JArray;
									int score = definitions.Count;
									foreach (JValue definition in definitions)
									{
										Match match = Regex.Match(definition.ToString(CultureInfo.InvariantCulture), @"[〈\(\[（【]\s*([^〉\)\]】）]+)\s*[〉\)\]】）]", RegexOptions.Compiled);
										if (match.Success)
										{
											string type = match.Groups[1].Value;
											if (type.Contains("名"))
											{
												score += (int) CharacterType.Noun;
											}
											if (type.Contains("形"))
											{
												score += (int) CharacterType.Adjective;
											}
											if (type.Contains("副"))
											{
												score += (int) CharacterType.Adverb;
											}
											if (type.Contains("助"))
											{
												score += (int) CharacterType.Auxiliary;
											}
											if (type.Contains("叹"))
											{
												score += (int) CharacterType.Interjections;
											}
											if (type.Contains("动"))
											{
												score += (int) CharacterType.Verb;
											}
										}
									}

									string current = App.FixPinyin(meaning["pinyin"]?[0]?.ToString(), true);
									if (current == prefered)
									{
										pinyin = prefered;
										break;
									}

									if (current == word.ZPinyin)
									{
										pinyin = current;
										break;
									}

									if (current == word.HPinyin && current == word.CPinyin)
									{
										pinyin = current;
										break;
									}
									if (score > max)
									{
										max = score;
										pinyin = current;
									}
								}
							}

							if (pinyin == null)
							{
								pinyin = value[0].ToString();
							}
						}

						if (item.TryGetValue("verified", out JToken verified))
						{
							if (verified is JArray array2 && array2.Count > 0 && array2[0].Type == JTokenType.String && array2[0].ToString().Length > 0)
							{
								word.Verified = true;
							}
						}

						word.InBaiduHanyu = true;
						word.BPinyin = App.ParseWordPinyin(name, pinyin);
						word.ProcessMeaning(string.Join(string.Empty, item["definition"] as JArray));

					}
					page++;

					if (page > obj["extra"].Value<int>("total-page"))
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
		}
	}
}