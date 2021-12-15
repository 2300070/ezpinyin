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

			string key = (character.Length == 1 ? character[0] : char.ConvertToUtf32(character[0], character[1])).ToString("X");

			if (App.Dictionary.TryGetValue(character, out CharacterInfo result))
			{
				if (result.Verified)
				{
					return result;
				}
			}
			else
			{
				result = new CharacterInfo(character);
				App.Dictionary[character] = result;
			}

			await YDictSpider.LoadByKeyAsync(character, key);

			if (result != null && (result.Count > 0 || result.Verified))
			{
				return result;
			}

			App.Dictionary.TryRemove(character, out result);
			return null;

		}

		private static async Task<CharacterInfo> LoadByKeyAsync(string character, string key, HashSet<string> stacks = null)
		{

			if (!App.Dictionary.TryGetValue(character, out CharacterInfo result))
			{
				result = new CharacterInfo(character);
				App.Dictionary[character] = result;
			}


			string html, pinyin;
			bool validate = false;
			Match match;

			/**
			 * 下载叶典的页面
			 */
			TRY_AGAIN:
			html = await App.DownloadAsync($"http://yedict.com/zscontent.asp?uni={key}", validate);
			if (html != null)
			{
				bool verified = Regex.IsMatch(html, @"参考资料：《([^《》]|《[^《》]+》)+》|中华字海：第\d+页第\d+字", RegexOptions.Compiled);
				result.Verified = verified;

				if (html.Contains("非unicode临时码"))
				{
					result.IsValid = false;
				}

				int index = html.IndexOf("<table border=\"0\" width=\"100%\" cellpadding=\"2\" cellspacing=\"4\"  >", StringComparison.Ordinal);
				if (index > 0)
				{
					/**
					 * 截取到解释的内容并进行分析。
					 */
					string content = html.Substring(index, html.IndexOf("</table>", index, StringComparison.Ordinal) - index);
					index = content.IndexOf("<font size=3>", StringComparison.Ordinal);
					string explain;
					if (index > 0)
					{
						explain = content.Substring(index, content.IndexOf("</font>", index, StringComparison.Ordinal) - index);
					}
					else
					{
						explain = content;
					}
					string[] lines = explain.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
					pinyin = null;
					PinyinInfo info = null;
					foreach (string line in lines)
					{
						MatchCollection matches = Regex.Matches(line, @"拼音((\s*[a-z\u0080-\u024F]+\d?\s*)+)");
						if (matches.Count > 0)
						{
							for (int i = 0; i < matches.Count; i++)
							{
								/**
								 * 此处可以得到一个匹配一个或多个拼音的字符串，对于这个情形可能存在两种情况，一是这个字符是个多音字，二是这个字符本身的读音是包含多个音节的，例如兙读作shike，叶典本身没有对这些进行区分，由此需要用汉典的数据来相互印证
								 */
								string pinyinText = matches[i].Groups[1].Value;
								if (result.ZPinyin != null)
								{
									pinyin = App.FixPinyin(pinyinText);
									if (pinyin == result.ZPinyin)
									{
										/**
										 * 如果汉典的拼音与叶典的经过连接拼音是一致的，则认为此拼音是可信的，可以直接退出了。
										 */
										result.Verified = true;
										result.FindOrRegister(pinyin);
										result.YPinyin = pinyin;
										return result;
									}
								}

								/**
								 * 如果叶典与汉典无法匹配，则尽量寻找一个合适的，如果没有合适的，则使用第一个作为后备选择。
								 */
								MatchCollection collection = Regex.Matches(pinyinText, @"\s*[a-z\u0080-\u024F]+\d?\s*", RegexOptions.Compiled);
								string back = null;
								info = null;
								for (int j = 0; j < collection.Count; j++)
								{
									string item = App.FixPinyin(collection[j].Value);
									if (item != null)
									{
										if (App.PinyinList.Contains(item))
										{
											pinyin = item;
											info = result.FindOrRegister(item);
										}
										else if (back == null)
										{
											back = item;
										}
									}
								}

								if (info == null && back != null)
								{
									pinyin = back;
									info = result.FindOrRegister(back);
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
						result.YPinyin = result.ComputePrefered()?.Text;
						return result;
					}

					if (stacks == null)
					{
						stacks = new HashSet<string> { key };
					}

					match = Regex.Match(explain, @"古?同【<a[^>]+uni=([0-9A-F]{4,})>((?!</a>).)+</a>】");
					if (match.Success)
					{
						if (await YDictSpider.LoadVariantAsync(result, match.Groups[1].Value, stacks))
						{
							return result;
						}
					}
					match = Regex.Match(explain, @"【<a[^>]+uni=([0-9A-F]{4,})>((?!</a>).)+</a>】的[^<]+[字体]");
					if (match.Success)
					{
						if (await YDictSpider.LoadVariantAsync(result, match.Groups[1].Value, stacks))
						{
							return result;
						}
					}
					match = Regex.Match(content, @"异体：((<a[^>]+>((?!</a>).)+</a>)+)");
					if (match.Success)
					{
						MatchCollection matches = Regex.Matches(match.Groups[1].Value, @"<a[^>]+uni=([0-9A-F]{4,})>((?!</a>).)+</a>");
						if (matches.Count > 0)
						{
							for (int i = 0; i < matches.Count; i++)
							{
								if (await YDictSpider.LoadVariantAsync(result, matches[i].Groups[1].Value, stacks))
								{
									return result;
								}
							}
						}
					}
				}
			}

			if (!validate)
			{
				validate = true;
				goto TRY_AGAIN;
			}
			return null;
		}

		private static async Task<bool> LoadVariantAsync(CharacterInfo target, string key, HashSet<string> stacks)
		{
			if (stacks.Add(key))
			{
				string ch = null;
				bool valid = true;
				if (int.TryParse(key, System.Globalization.NumberStyles.HexNumber, null, out int code))
				{
					if (code < 0xFFFF)
					{
						if (key.Length == 4)
						{
							ch = new string((char)code, 1);
						}
						else
						{
							valid = false;
							ch = key;
						}
					}
					else
					{
						try
						{
							ch = char.ConvertFromUtf32(code);
						}
						catch
						{
							ch = key;
							valid = false;
						}
					}
					if (ch != null)
					{
						if (!App.Dictionary.TryGetValue(ch, out CharacterInfo variant))
						{
							variant = await YDictSpider.LoadByKeyAsync(ch, key, stacks);
						}

						if (variant != null)
						{
							variant.IsValid = valid;
							if (variant.Count > 0)
							{
								target.CopyFrom(variant);
								target.YPinyin = variant.ComputePrefered().Text;
								return true;
							}
						}
					}
				}
			}

			return false;
		}
	}
}