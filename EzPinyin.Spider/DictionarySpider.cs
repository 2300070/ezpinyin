using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 用于下载，分析某个Unicode的汉字分区以便生成拼音字典
	/// </summary>
	internal sealed class DictionarySpider
	{
		private readonly string name;
		private readonly string file;
		private readonly List<string> characters = new List<string>();

		public int Count => this.characters.Count;

		public bool Check { get; set; }

		public DictionarySpider(string name, string file, int from, int to)
		{
			this.name = name;
			this.file = file;
			this.Add(from, to);

		}

		public void Add(int from, int to)
		{
			for (int code = from; code <= to; code++)
			{
				this.Add(code < 0x10000 ? new string((char)code, 1) : char.ConvertFromUtf32(code));
			}
		}


		public void Add(string ch)
		{
			this.characters.Add(ch);
		}

		public async Task LoadSimplifiedAsync()
		{
			Console.WriteLine();
			Console.WriteLine($"开始加载{this.name}的繁简数据。");

			await App.ForEachAsync(this.characters, async ch =>
			{
				await ZDictSpider.LoadSimplifiedAsync(ch);
			});
		}

		public async Task DownloadAsync()
		{
			Console.WriteLine();
			Console.WriteLine($"开始抓取{this.name}数据。");
			StringBuilder buffer = new StringBuilder();
			await App.ForEachAsync(this.characters, async character =>
			{
				CharacterInfo info = await DictionarySpider.DownloadAsync(character);
				if (this.Check && info != null && info.Count == 0 && info.IsTrusted)
				{
					buffer.Append(character);
				}
			});


			Console.WriteLine($"操作完成，抓取了{this.Count}个字符。");
			if (buffer.Length > 0)
			{
				string text = buffer.ToString();
				File.WriteAllText($"{this.file}_failed.txt", text);
				Console.WriteLine("警告，抓取下列字符失败：");
				Console.WriteLine(text);
				Console.Write("按返回键继续。");
				Console.ReadLine();
				Console.WriteLine();
			}
			Console.WriteLine();
		}

		public static async Task<CharacterInfo> DownloadAsync(string character)
		{
			CharacterInfo info = await ZDictSpider.LoadCharacterAsync(character);
			if (info == null || info.Count == 0 || !info.IsStandard)
			{
				info?.Clear();
				info = await YDictSpider.LoadCharacterAsync(character);
				if (info == null || info.Count == 0)
				{
					info = await GuoxueSpider.LoadCharacterAsync(character);
				}
				if (info != null && info.IsTrusted)
				{
					string pinyin = info.PreferedPinyin;
					if (pinyin != null)
					{
						App.EnsurePinyin(pinyin);
					}
				}
			}

			return info;
		}

		public async Task SaveAsync()
		{
			Console.WriteLine($"生成{this.name}字典。");

			App.PinyinList.Sort();
			using (FileStream fs = new FileStream(this.file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				List<string> characters = this.characters;
				for (int i = 0; i < characters.Count; i++)
				{
					string pinyin;
					string ch = this.characters[i];
					if (!App.Dictionary.TryGetValue(ch, out CharacterInfo info) || info.Count == 0)
					{
						pinyin = null;
					}
					else
					{
						pinyin = info.PreferedPinyin;
					}
					int index = App.PinyinList.IndexOf(pinyin) + 1;
					fs.WriteByte((byte)((index & 0xFF00) >> 8));
					fs.WriteByte((byte)(index & 0xFF));
				}
				fs.SetLength(fs.Position);
				await fs.FlushAsync();
			}
			Console.WriteLine($"已经保存{this.name}字典。");
		}

		/// <summary>
		/// 建立校正信息
		/// </summary>
		public static void UpdateCorrection()
		{
			foreach (KeyValuePair<string, CharacterInfo> item in App.Dictionary)
			{
				string character = item.Key;
				if (character.Length > 1)
				{
					continue;
				}

				char ch = character[0];
				CharacterInfo info = item.Value;
				if (info.Simplified > 0)
				{
					App.Simplified[ch] = info.Simplified;
				}
				else if (info.Traditional > 0)
				{
					App.Simplified[info.Traditional] = ch;
				}
			}
		}
	}
}