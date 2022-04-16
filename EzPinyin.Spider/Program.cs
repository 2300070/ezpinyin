using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EzPinyin.Spider
{
	class Program
	{


#if DEBUG
		public const string TEST_SAMPLE = "银";
#endif
		private static readonly DictionarySpider basic;
		private static readonly DictionarySpider extA;
		private static readonly DictionarySpider extB;
		private static readonly DictionarySpider extC;
		private static readonly DictionarySpider extD;
		private static readonly DictionarySpider extE;
		private static readonly DictionarySpider extF;
		private static readonly DictionarySpider extG;
		private static readonly DictionarySpider radicals;
		private static readonly DictionarySpider cmp;
		private static readonly DictionarySpider cmpSup;

		/// <summary>
		/// 下载数据时的并发数量
		/// </summary>
#if DEBUG
		public const int CONCURRENCY_LEVEL = 0x010;
#else
		public const int CONCURRENCY_LEVEL = 0x10;
#endif
		static Program()
		{
			basic = new DictionarySpider("基本汉字", "dict_basic", 0x4E00, 0x9FFF) { Check = true };
			basic.Add("〇");

			extA = new DictionarySpider("扩展A", "dict_ext_a", 0x3400, 0x4DBF) { Check = true };

			extB = new DictionarySpider("扩展B", "dict_ext_b", 0x20000, 0x2A6DF);

			extC = new DictionarySpider("扩展C", "dict_ext_c", 0x2A700, 0x2B738);

			extD = new DictionarySpider("扩展D", "dict_ext_d", 0x2B740, 0x2B81D);

			extE = new DictionarySpider("扩展E", "dict_ext_e", 0x2B820, 0x2CEA1);

			extF = new DictionarySpider("扩展F", "dict_ext_f", 0x2CEB0, 0x2EBE0);

			extG = new DictionarySpider("扩展G", "dict_ext_g", 0x30000, 0x3134A);

			radicals = new DictionarySpider("部首", "dict_rad", 0x2E80, 0x2FDF);

			cmp = new DictionarySpider("兼容汉字", "dict_cmp", 0xF900, 0xFAFF);

			cmpSup = new DictionarySpider("兼容汉字扩展", "dict_cmp_sup", 0x2F800, 0x2FA1F);
		}

		static void Main(string[] args)
		{
			Task.Run(async delegate
				{
					Console.WriteLine("EzPinyin数据生成程序。");

					if (!Directory.Exists("../cache"))
					{
						Console.Write("这是一个用来更新字典与词典的程序，第一次启动时需要从不同网站下载10-100G的数据，由此会耗费大量的时间与带宽，如果你仍然决定继续，请按下'y'键");

						ConsoleKeyInfo key = await Common.ReadKeyAsync();
						Console.WriteLine();
						if (key.Key != ConsoleKey.Y)
						{
							return;
						}
					}

					try
					{
						Console.WriteLine("开始生成字典。");
						/**
						 * 首先生成字典数据。
						 */
						await Program.GenerateDictionaryAsync();

						Console.WriteLine();
						Console.WriteLine("开始生成词典。");

						/**
						 * 接着收集词汇样本。
						 */
						await Program.GenerateSamplesAsync();
						Console.WriteLine();

						/**
						 * 词汇数据就绪，对字典进行校正。
						 */
						await Program.CorrectDictionaryAsync();

						/**
						 * 加载自定义的字典。
						 */
						await Program.LoadDictionaryFromTemplateAsync();

						/**
						 * 字典数据已经校正，重新对词典校正。
						 */
						await Program.CorrectLexiconAsync();

						/**
						 * 加载自定义的词典模板。
						 */
						await Program.LoadLexiconFromTemplateAsync();

						/**
						 * 生成词典。
						 */
						await Program.GenerateLexiconAsync();

						/**
						 * 保存数据
						 */
						await Program.SaveAsync();


						Console.WriteLine("所有操作全部完成。");
						Console.WriteLine("按任意键退出。");

					}
					catch (Exception e)
					{
						Console.WriteLine();
						Console.WriteLine(e);
						Console.WriteLine("按任意键继续。");
					}
					finally
					{
						if (DateTime.Now.Hour < 5)
						{
							Process.Start("shutdown", "/s /f /t 0");
						}
						else
						{
							Console.ReadKey();
						}

					}
				})
				.Wait();


		}

		private static async Task SaveAsync()
		{
			Console.WriteLine();
			Console.WriteLine("生成最终数据。");

			await basic.SaveAsync();

			await extA.SaveAsync();

			await extB.SaveAsync();

			await extC.SaveAsync();

			await extD.SaveAsync();

			await extE.SaveAsync();

			await extF.SaveAsync();

			await extG.SaveAsync();

			await radicals.SaveAsync();

			await cmp.SaveAsync();

			await cmpSup.SaveAsync();

			Console.WriteLine();
			Console.Write("生成词典...");
			await Program.WriteLexiconAsync("basic", true);

			Console.WriteLine("完成。");

			Console.WriteLine();
			Console.Write("生成补充词典...");
			await Program.WriteLexiconAsync("auxilliary", false);

			Console.WriteLine("完成。");

			Console.WriteLine();
			Console.WriteLine("生成拼音列表。");

			File.WriteAllLines("pinyin.txt", Common.PinyinList);

			Console.WriteLine("完成。");

			Console.WriteLine();
			Console.WriteLine("生成简繁字典。");

			using (FileStream fs = new FileStream("simplified", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				byte[] buffer = new byte[4];
				foreach (KeyValuePair<char, char> item in Common.Simplifield)
				{
					int val = item.Key;
					buffer[0] = (byte)((val >> 8) & 0xFF);
					buffer[1] = (byte)(val & 0xFF);

					val = item.Value;
					buffer[2] = (byte)((val >> 8) & 0xFF);
					buffer[3] = (byte)(val & 0xFF);

					fs.Write(buffer, 0, 4);
				}
			}


			Console.WriteLine("完成。");
		}

		private static async Task WriteLexiconAsync(string group, bool flag)
		{
			try
			{
				await Common.WaitAsync(Task.Run(delegate
				{
					/**
					 * 定义4个数据流分别存储长度为2,3,4的词汇，定义一个额外的数据流存储长度超过4的词汇。
					 */
					MemoryStream[] streams = { new MemoryStream(), new MemoryStream(), new MemoryStream() };
					MemoryStream x = new MemoryStream();
					byte[] buffer = new byte[2];
					List<WordInfo> samples = Common.ResultSamples;

					foreach (WordInfo sample in samples)
					{
#if DEBUG
						if (sample.ActualWord == TEST_SAMPLE)
						{
							Debugger.Break();
						}
#endif

						if (sample.HasRarePinyin != flag)
						{
							continue;
						}

						/**
						 * 写入词汇
						 */
						int[] indexes = sample.PreferedPinyinIndexes;
						if (indexes == null)
						{
							continue;
						}
						string word = sample.ActualWord;
						MemoryStream ms;

						if (word.Length < 5)
						{
							try
							{
								ms = streams[word.Length - 2];
							}
							catch (Exception e)
							{
								Console.WriteLine(e);
								throw;
							}
						}
						else
						{
							ms = x;
							ms.WriteByte((byte)word.Length);
						}

						foreach (char ch in word)
						{
							buffer[0] = (byte)((ch >> 8) & 0xFF);
							buffer[1] = (byte)(ch & 0xFF);
							ms.Write(buffer, 0, 2);
						}

						if (flag)
						{
							/**
							 * 写入拼音
							 */
							for (int i = 0; i < indexes.Length; i++)
							{
								int index = indexes[i] + 1;

								buffer[0] = (byte)((index >> 8) & 0xFF);
								buffer[1] = (byte)(index & 0xFF);
								ms.Write(buffer, 0, 2);
							}
						}
					}

					Console.WriteLine("保存词典。");
					for (int i = 0; i < streams.Length; i++)
					{
						MemoryStream stream = streams[i];
						using (FileStream fs = new FileStream($"lex_{group}_{i + 2}", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
						{
							stream.Seek(0, SeekOrigin.Begin);
							stream.CopyTo(fs);
							fs.SetLength(fs.Position);
							stream.Dispose();
							fs.Flush();
						}
					}
					using (FileStream fs = new FileStream($"lex_{group}_x", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
					{
						x.Seek(0, SeekOrigin.Begin);
						x.CopyTo(fs);
						fs.SetLength(fs.Position);
						x.Dispose();
						fs.Flush();
					}
				}));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		private static int CompateWord(WordInfo w1, WordInfo w2)
		{
			string x = w1.ActualWord;
			string y = w2.ActualWord;

			int a, b;
			if (char.IsHighSurrogate(x[0]) && char.IsLowSurrogate(x[1]))
			{
				a = char.ConvertToUtf32(x[0], x[1]);
			}
			else
			{
				a = x[0];
			}
			if (char.IsHighSurrogate(y[0]) && char.IsLowSurrogate(y[1]))
			{
				b = char.ConvertToUtf32(y[0], y[1]);
			}
			else
			{
				b = y[0];
			}

			if (a != b)
			{
				return a.CompareTo(b);
			}
			return string.CompareOrdinal(x, y);
		}

		private static async Task CorrectLexiconAsync()
		{
			Console.WriteLine();
			Console.Write("重新校正样本...");


			await Common.WaitAsync(Task.Run(async delegate
			{
				await Common.ForEachAsync(Common.Samples.Values, word =>
				{
					word.ResetPinyin();

					string name = word.Word;
					string[] explain = new string[name.Length];
					word.Explain?.CopyTo(explain, 0);
					for (int i = 0; i < explain.Length; i++)
					{
						if (explain[i] == null)
						{
							if (!Common.Dictionary.TryGetValue(new string(name[i], 1), out CharacterInfo info))
							{
								return;
							}
							explain[i] = info.PreferedPinyin;
						}
					}

					word.PredicatePinyin = string.Join(" ", explain);
				});
			}));


			Console.WriteLine("完成。");
		}

		private static async Task CorrectDictionaryAsync()
		{
			Console.WriteLine();
			Console.Write("校正字典...");

			await Common.WaitAsync(Task.Run(async delegate
			{

				await Common.ForEachAsync(Common.Dictionary.Values, ch =>
				{
					ch.Reset();
				});

				await Common.ForEachAsync(Common.Samples.Values, sample =>
				{
					string[] array = sample.PreferedPinyinArray;
					if (array == null)
					{
						return;
					}

					string name = sample.Word;
					for (int i = 0; i < name.Length; i++)
					{
						if (Common.Dictionary.TryGetValue(new string(name[i], 1), out CharacterInfo info))
						{
							info.Register(array[i]).AddReferenceCount(1);
						}
					}
				});

			}));

			Console.WriteLine("完成。");
		}

		private static async Task GenerateLexiconAsync()
		{
			Common.IsDataReloaded = true;

			Common.PinyinList.Sort();

			/**
			 * 添加行政区划
			 */
			foreach (string name in Common.GeographicalNames)
			{
				Common.Samples.GetOrAdd(name, new WordInfo(name)).EnsurePreferedPinyin();
			}

			#region 分析并生成词典数据
			/**
			 * 先筛选出一个需要用于分析的样本。
			 */

			Console.WriteLine();

			Console.Write("筛选样本...");
			List<WordInfo> items = new List<WordInfo>();
			await Common.WaitAsync(Task.Run(delegate
			{
				foreach (KeyValuePair<string, WordInfo> item in Common.Samples)
				{
					string key = item.Key;
					WordInfo sample = item.Value;
#if DEBUG
					if (key == TEST_SAMPLE)
					{
						Debugger.Break();
					}
#endif
					if (!sample.Validate())
					{
						continue;
					}

					if (sample.ActualWord != key)
					{
						if (Common.Samples.ContainsKey(sample.ActualWord))
						{
							continue;
						}
					}
					if (Common.ContainsPolyphones(key))
					{
						if (sample.CheckCanRemoveByHead())
						{
							continue;
						}
						if (sample.CheckCanRemoveByTail())
						{
							continue;
						}
						items.Add(sample);
					}
				}
			}));
			Console.WriteLine($"剩余{items.Count}样本。");
			Console.WriteLine();


			/**
			 * 清除多余的多音词样本。
			 */
			Console.Write("清除多余样本...");
			List<WordInfo> list = new List<WordInfo>();

			await Common.WaitAsync(Task.Run(delegate
			{
				foreach (WordInfo sample in items)
				{
#if DEBUG
					if (sample.ActualWord == TEST_SAMPLE)
					{
						Debugger.Break();
					}
#endif
					if (sample.HasRarePinyin)
					{
						sample.IsSelected = true;
						list.Add(sample);
					}
				}
			}));

			Console.WriteLine($"剩余{list.Count}样本。");

			Console.WriteLine();
			items.Clear();
			Console.Write("收缩样本...");

			await Common.WaitAsync(Task.Run(delegate
			{
				int index = 0;
				while (index < list.Count)
				{
					WordInfo sample = list[index];

					WordInfo result = sample.Shrink();
					if (result != null)
					{
						if (list.Contains(result))
						{
							list.RemoveAt(index);
							continue;
						}
						list[index] = result;
						sample = result;
					}


					string word = sample.ActualWord;

					if (word.Length > 2 && Regex.IsMatch(word, @"[省市港县区乡]$", RegexOptions.Compiled))
					{
						if (Common.GeographicalNames.Contains(word))
						{
							/**
							 * 尝试简化地理词汇。
							 */
							string newWord = Regex.Replace(word, @"(((\w|蒙古|维吾尔|布依|朝鲜|土家|哈尼|哈萨克|僳僳|高山|拉祜|东乡|纳西|景颇|柯尔克孜|达斡尔|仫佬|布朗|撒拉|毛南|仡佬|锡伯|阿昌|普米|塔吉克|乌孜别克|俄罗斯|鄂温克|德昂|保安|裕固|塔塔尔|独龙|鄂伦春|赫哲|门巴|珞巴|基诺)族)*自治)?[省市区州县]", string.Empty, RegexOptions.Compiled);
							if (newWord.Length > 1)
							{
								string[] array = new string[newWord.Length];
								Array.Copy(sample.PreferedPinyinArray, array, array.Length);
								if (!Common.CheckRarePinyin(newWord, array))
								{
									/**
									 * 如果简化后拼音不再有生僻读音，则放弃。
									 */
									list.RemoveAt(index);
									continue;
								}
								string newPinyin = string.Join(" ", array);
								if (Common.Samples.TryGetValue(newWord, out sample))
								{
									if (list.Contains(sample))
									{
										list.RemoveAt(index);
										continue;
									}

									list[index] = sample;
								}
								else
								{
									sample = new WordInfo(newWord) { SpecifiedPinyin = newPinyin };
									list[index] = sample;
								}
							}
						}
					}
					int index2;
					do
					{
						index2 = list.FindIndex(index + 1, other => sample.CanReplace(other));
						if (index2 > -1)
						{
							list.RemoveAt(index2);
						}
					} while (index2 > -1);

					items.Add(sample);
					index++;
				}
			}));

			Console.WriteLine($"剩余{items.Count}样本。");

			Console.WriteLine();
			Console.Write("收集补充样本...");
			/**
			 * 收集补充词汇样本。
			 */
			HashSet<WordInfo> auxilliaries = new HashSet<WordInfo>();

			await Common.WaitAsync(Task.Run(delegate
			{
				foreach (WordInfo sample in Common.Samples.Values)
				{
#if DEBUG
					if (sample.ActualWord == TEST_SAMPLE)
					{
						Debugger.Break();
					}
#endif
					if (sample.IsSelected || !sample.IsValid)
					{
						continue;
					}

					if (sample.CheckCanRemoveByTail())
					{
						continue;
					}

					foreach (WordInfo item in items)
					{
						if (item.CheckIntersect(sample))
						{
							sample.IsAuxilliary = true;
							sample.IsSelected = true;
							lock (auxilliaries)
							{
								auxilliaries.Add(sample);
							}

							break;
						}
					}
				}
			}));

			Console.WriteLine("完成。");

			Console.WriteLine($"共收集{auxilliaries.Count}补充样本。");

			foreach (WordInfo word in items)
			{
				Program.AddToLexicon(word);
			}

			foreach (WordInfo word in auxilliaries)
			{
				Program.AddToLexicon(word);
			}

			Console.WriteLine($"合并所有样本后得到{Common.Lexicon.Count}必要词汇。");
			#endregion

			#region 将得到的数据输出到文件

			StringBuilder buffer = new StringBuilder();

			Console.Write("输出词典样张");
			buffer.Clear();
			await Common.WaitAsync(Task.Run(delegate
			{
				List<WordInfo> samples = new List<WordInfo>(Common.Lexicon.Values);
				samples.Sort(Program.CompateWord);
				Common.ResultSamples = samples;

				foreach (WordInfo sample in samples)
				{
#if DEBUG
					if (sample.ActualWord == TEST_SAMPLE)
					{
						Debugger.Break();
					}
#endif
					buffer.Append($"{sample.ActualWord}		{sample.PreferedPinyin}	#{sample.Source}");
					if (sample.IsAuxilliary)
					{
						buffer.Append("	X");
					}
					buffer.AppendLine();
				}
			}));
			File.WriteAllText("lexicon.txt", buffer.ToString());
			Console.WriteLine("完成。");
			Console.WriteLine();
			Console.WriteLine("样张已经保存到lexicon.txt文件。");

			#endregion

		}

		private static void AddToLexicon(WordInfo word)
		{
			string ch = word.ActualWord.Substring(0, 1);
#if DEBUG
			if (ch == TEST_SAMPLE)
			{
				Debugger.Break();
			}
#endif
			if (Common.Traditional.TryGetValue(ch[0], out char traditional))
			{
				Common.Dictionary[traditional.ToString()].HasLexiconItem = true;
			}
			else if (Common.Simplifield.TryGetValue(ch[0], out char simplified))
			{
				Common.Dictionary[simplified.ToString()].HasLexiconItem = true;
			}

			Common.Dictionary[ch].HasLexiconItem = true;

			Common.Lexicon.TryAdd(word.ActualWord, word);
		}

		private static async Task GenerateSamplesAsync()
		{
			//if (File.Exists(App.SAMPLE_CACHE_FILE) && !App.IsDataReloaded)
			//{
			//	Console.Write("如果你想要重新扫描样本，请按下'y'键");
			//	ConsoleKeyInfo key = await App.ReadKeyAsync();
			//	Console.WriteLine();
			//	if (key.Key != ConsoleKey.Y)
			//	{
			//		Console.WriteLine();
			//		await App.LoadSamplesAsync();
			//		return;
			//	}
			//}

			Common.IsDataReloaded = true;

			Console.WriteLine("扫描词汇样本。");

			await LexiconSpider.LoadSamplesAsync();

			Console.Write("简单校正样本...");

			await Common.WaitAsync(Task.Run(async delegate
			{
				await Common.ForEachAsync(Common.Samples.Values, sample =>
				{
					if (sample.ProfessionalPinyin != null)
					{
						//在分析义项时可能已经根据工具书对某些字进行了解释，此处不能覆写已有的专业解释。
						return;
					}
					string[] explain = sample.Explain;
					if (explain == null)
					{
						return;
					}
					string word = sample.ActualWord;
					for (int i = 0; i < explain.Length; i++)
					{
						string item = explain[i];
						if (item == null)
						{
							if (!Common.Dictionary.TryGetValue(new string(word[i], 1), out CharacterInfo info) || info.Count != 1)
							{
								return;
							}

							explain[i] = info[0];
						}
					}
					sample.ProfessionalPinyin = string.Join(" ", explain);
				});
			}));

			Console.WriteLine("完成。");

			Console.WriteLine($@"共处理{ Common.Samples.Count}个词汇。");

		}

		private static async Task GenerateDictionaryAsync()
		{
			//await DictionarySpider.DownloadAsync("瓱");

			Common.IsDataReloaded = true;
			Common.PinyinList.Clear();
			Common.PinyinList.AddRange(Common.StandardPinyinList);

			Console.WriteLine("抓取字符信息。");

			await basic.LoadSimplifiedAsync();

			await extA.LoadSimplifiedAsync();

			await basic.DownloadAsync();

			await extA.DownloadAsync();

			await extB.DownloadAsync();

			await extC.DownloadAsync();

			await extD.DownloadAsync();

			await extE.DownloadAsync();

			await extF.DownloadAsync();

			await extG.DownloadAsync();

			await radicals.DownloadAsync();

			await cmp.DownloadAsync();

			await cmpSup.DownloadAsync();

			Console.WriteLine("分析校正信息。");

			DictionarySpider.UpdateConvertion();


			Console.WriteLine($@"共扫描{
					basic.Count
					+ extA.Count
					+ extB.Count
					+ extC.Count
					+ extD.Count
					+ extE.Count
					+ extF.Count
					+ extG.Count
					+ radicals.Count
					+ cmp.Count
					+ cmpSup.Count
				}，收集{Common.Dictionary.Count}个字符。");


			StringBuilder buffer = new StringBuilder();
			Console.Write("输出字典样张");
			await Common.WaitAsync(Task.Run(delegate
			{
				List<CharacterInfo> list = new List<CharacterInfo>(Common.Dictionary.Values);
				list.Sort((x, y) => string.Compare(x.Character, y.Character, StringComparison.Ordinal));

				foreach (CharacterInfo ch in list)
				{
					if (!ch.IsValid)
					{
						continue;
					}
					buffer.AppendLine($"{ch.Character} {ch.PreferedPinyin}");
				}
			}));
			File.WriteAllText("dictionary.txt", buffer.ToString());
			Console.WriteLine("完成。");
			Console.WriteLine();
			Console.WriteLine("样张已经保存到dictionary.txt文件。");

			Console.WriteLine("完成。");

		}

		private static async Task LoadDictionaryFromTemplateAsync()
		{
			Console.WriteLine();
			Console.Write("从template.txt加载自定义字典...");


			string file = "template.txt";
			if (!File.Exists(file))
			{
				file = "../template.txt";
			}

			if (File.Exists(file))
			{
				await Common.WaitAsync(Task.Run(delegate
				{
					Program.LoadDictionaryFrom(file);
				}));

				Console.WriteLine("完成。");
			}
			else
			{
				Console.WriteLine("不存在。");
			}

		}

		private static async Task LoadLexiconFromTemplateAsync()
		{
			Console.WriteLine();
			Console.Write("从template.txt加载自定义词典...");

			string file = "template.txt";
			if (!File.Exists(file))
			{
				file = "../template.txt";
			}

			if (File.Exists(file))
			{
				await Common.WaitAsync(Task.Run(delegate
				{
					Program.LoadLexiconFrom(file);
				}));
				Console.WriteLine("完成。");
			}
			else
			{
				Console.WriteLine("不存在。");
			}
		}

		private static void LoadLexiconFrom(string file)
		{
			Program.EnumerateFile(file, (word, pinyin, disabled) =>
			{
				/**
				 * 如果是字符定义，忽略。
				 */
				if (word.Length < 2 || (!Char.IsHighSurrogate(word[0]) && !Char.IsLowSurrogate(word[1])))
				{
					return;
				}

				if (!Common.Lexicon.TryGetValue(word, out WordInfo info))
				{
					info = LexiconSpider.FindOrRegister(word);
					info.IsSelected = true;
					Common.Lexicon.TryAdd(word, info);
					info.CustomPinyin = Common.ParseWordPinyin(word, pinyin);
				}
				else if (info.PreferedPinyin != pinyin)
				{
					info.CustomPinyin = pinyin;
				}

				info.IsDisabled = disabled;
			});
		}

		private static void LoadDictionaryFrom(string file)
		{
			Program.EnumerateFile(file, (ch, pinyin, disabled) =>
			{
				if (ch.Length == 2)
				{
					if (!Char.IsHighSurrogate(ch[0]) || !Char.IsLowSurrogate(ch[1]))
					{
						return;
					}
				}
				else if (ch.Length > 2)
				{
					/**
					 * 忽略词汇定义
					 */
					return;
				}

				pinyin = Common.FixPinyin(pinyin);

				if (pinyin == null)
				{
					throw new FormatException($"自定义拼音错误：字符'{ch}'未指定拼音。");
				}

				if (Common.Dictionary.TryGetValue(ch, out CharacterInfo info))
				{
					if (info.PreferedPinyin == pinyin)
					{
						return;
					}

					info.Character = ch;
					info.CustomPinyin = pinyin;

				}

				Common.EnsurePinyin(pinyin);
			});
		}

		private static void EnumerateFile(string file, Action<string, string, bool> handler)
		{
			char[] separators = new[] { ' ', ' ', '　', ' ' };
			using (StreamReader sr = new StreamReader(file, Encoding.UTF8, true))
			{
				/**
				 * 逐行读取模板文件，并且解析出其中的字符定义并添加到字典集合。
				 */
				while (!sr.EndOfStream)
				{
					string content = sr.ReadLine();
					string line = content?.Trim();
					if (String.IsNullOrEmpty(line))
					{
						continue;
					}

					int index = line.IndexOf('#');
					if (index > 0)
					{
						line = line.Substring(0, index).Trim();
					}
					else if (index == 0)
					{
						continue;
					}

					if (line.Length == 0)
					{
						continue;
					}

					index = line.IndexOfAny(separators);

					string word, pinyin;
					bool disabled = false;
					if (index < 1)
					{
						word = line;
						pinyin = null;
					}
					else
					{
						word = line.Substring(0, index);
						pinyin = line.Substring(index + 1).Trim();
					}

					if (word[0] == '-')
					{
						word = word.Substring(1);
						disabled = true;
					}

					handler(word, pinyin, disabled);
				}
			}
		}
	}
}
