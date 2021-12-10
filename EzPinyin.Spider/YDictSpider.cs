using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 用于从叶典（yedict.com）进行数据抓取的类。
	/// </summary>
	internal static class YDictSpider
	{

		public static async Task<CharacterInfo> LoadCharacterAsync(string character)
		{

			if (App.Dictionary.TryGetValue(character, out CharacterInfo info))
			{
				return info;
			}

			string key = (character.Length == 1 ? character[0] : char.ConvertToUtf32(character[0], character[1])).ToString("X");

			info = await YDictSpider.LoadByKeyAsync(key);

			if (info != null && info.Count > 0)
			{
				App.Dictionary[character] = info;
			}

			return info;
		}

		private static async Task<CharacterInfo> LoadByKeyAsync(string key, HashSet<string> stacks = null)
		{

			CharacterInfo ch = new CharacterInfo(key);


			string html, pinyin;
			Match match;

			html = await App.DownloadAsync($"http://yedict.com/zscontent.asp?uni={key}");
			if (html != null)
			{
				int index = html.IndexOf("<table border=\"0\" width=\"100%\" cellpadding=\"2\" cellspacing=\"4\"  >", StringComparison.Ordinal);
				if (index > 0)
				{
					string content = html.Substring(index, html.IndexOf("</table>", index, StringComparison.Ordinal) - index);
					index = content.IndexOf("<font size=3>", StringComparison.Ordinal);
					string para;
					if (index > 0)
					{
						para = content.Substring(index, content.IndexOf("</font>", index, StringComparison.Ordinal) - index);
					}
					else
					{
						para = content;
					}
					string[] lines = para.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
					pinyin = null;
					PinyinInfo info = null;
					foreach (string line in lines)
					{
						MatchCollection matches = Regex.Matches(line, @"拼音((\s*[a-z\u0080-\u024F]+\d?\s*)+)");
						if (matches.Count > 0)
						{
							for (int i = 0; i < matches.Count; i++)
							{
								match = matches[i];
								string newPinyin = Regex.Replace(match.Groups[1].Value, @"\s*[a-z\u0080-\u024F]+\d?\s*", m => App.FixPinyin(m.Value));

								if (App.PinyinList.Contains(newPinyin))
								{
									pinyin = newPinyin;
									ch.FindOrRegister(newPinyin);
								}
							}

							if (ch.Count == 0)
							{
								for (int i = 0; i < matches.Count; i++)
								{
									match = matches[i];
									string newPinyin = Regex.Replace(match.Groups[1].Value, @"\s*[a-z\u0080-\u024F]+\d?\s*", m => App.FixPinyin(m.Value));

									if (Regex.IsMatch(html, @"参考资料：《汉语大字典.?|中华字海|汉语大字典》", RegexOptions.Compiled))
									{
										App.PinyinList.Add(newPinyin);
										pinyin = newPinyin;
										info = ch.FindOrRegister(newPinyin);
										break;
									}
								}
							}
						}
						else if (Regex.IsMatch(line, "[①②③④⑤⑥⑦⑧⑨⑩⑪⑫⑬⑭⑮⑯⑰⑱⑲⑳]", RegexOptions.Compiled))
						{
							info?.AddEvaluation(1D);
						}
					}

					if (pinyin != null)
					{
						return ch;
					}

					match = Regex.Match(html, @"【<a[^>]+uni=([0-9A-F]+)>.+</a>】");
					if (match.Success)
					{
						if (stacks == null)
						{
							stacks = new HashSet<string> { key };
						}

						key = match.Groups[1].Value;
						if (stacks.Add(key))
						{
							return await YDictSpider.LoadByKeyAsync(key, stacks);
						}
					}
				}
			}

			return null;
		}
	}
}