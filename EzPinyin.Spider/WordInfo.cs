﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 描述了一个词汇的源信息，以便供词典生成工具分析并生成词典。
	/// </summary>
	internal sealed class WordInfo
	{
		private string preferedPinyin;
		private string[] preferedPinyinArray;
		private string zPinyin;
		private string hPinyin;
		private string bPinyin;
		private string cPinyin;
		private string gPinyin;
		private string pPinyin;
		private string customPinyin;
		private string specifiedPinyin;
		private bool? hasRarePinyin;
		private string actualWord;
		private string bkPinyin;

		/// <summary>
		/// 词汇的文本信息。
		/// </summary>
		public string Word { get; set; }

		/// <summary>
		/// 获得相关词汇经过评估的理想的拼音。
		/// </summary>
		[JsonIgnore]
		public string PreferedPinyin => this.preferedPinyin ?? (this.preferedPinyin = this.EvaluatePreferedPinyin());

		/// <summary>
		/// 获得相关词汇经过评估的理想的拼音的数组。
		/// </summary>
		[JsonIgnore]
		public string[] PreferedPinyinArray => this.preferedPinyinArray ?? (this.preferedPinyinArray = this.PreferedPinyin?.Split(' '));

		/// <summary>
		/// 这个词汇在汉典的详细资料地址。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ZSource { get; set; }

		/// <summary>
		/// 这个词汇在词典网的详细资料地址。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string CSource { get; set; }

		/// <summary>
		/// 这个词汇在汉文学网的详细资料地址。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string HSource { get; set; }

		/// <summary>
		/// 经过处理后得到的新华字典或者现代汉语词典对这个词汇的拼音解释。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string[] Explain { get; set; }

		/// <summary>
		/// 自定的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string CustomPinyin
		{
			get => this.customPinyin;
			set
			{
				this.customPinyin = value;

				string[] array = Regex.Split(value.Trim(), @"\s+");
				if (array.Length == this.Word.Length)
				{
					this.customPinyin = value;

					this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
					this.preferedPinyinArray = null;
				}
				else
				{
					throw new FormatException($"词汇'{this.Word}'与拼音'{value}'不匹配。");
				}
			}
		}

		/// <summary>
		/// 在运行过程中指定的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string SpecifiedPinyin
		{
			get => this.specifiedPinyin;
			set
			{
				this.specifiedPinyin = value;


				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 用于标识此词汇是否包含与其它词汇交叉的内容。
		/// </summary>
		[JsonIgnore]
		public bool IsIntersection { get; set; }

		/// <summary>
		/// 用于标识此词汇是否被选中为最终词典中的项目。
		/// </summary>
		[JsonIgnore]
		public bool IsSelected { get; set; }

		/// <summary>
		/// 汉典提供的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ZPinyin
		{
			get => this.zPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.zPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 汉文学网提供的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string HPinyin
		{
			get => this.hPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.hPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 百度汉语提供的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string BPinyin
		{
			get => this.bPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.bPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 百度百科提供的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string BKPinyin
		{
			get => this.bkPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}

				this.bkPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 词典网提供的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string CPinyin
		{
			get => this.cPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.cPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 根据工具书解释猜测的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string GPinyin
		{
			get => this.gPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.gPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 根据工具书解释计算出的拼音。
		/// </summary>
		[JsonIgnore]
		public string PPinyin
		{
			get => this.pPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.pPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 此词汇是否有专业可信来源。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool Verified { get; set; }

		/// <summary>
		/// 检测此词汇是否合规。
		/// </summary>
		[JsonIgnore]
		public bool IsValid { get; set; }

		/// <summary>
		/// 拼音信息的来源。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Source { get; set; }

		/// <summary>
		/// 最终可以用于处理的词汇。
		/// </summary>
		[JsonIgnore]
		public string ActualWord => this.actualWord ?? (this.actualWord = App.Normalize(this.Word));

		/// <summary>
		/// 指示这个词汇是否因为头部部分可以组成词汇而被移除。
		/// </summary>
		/// <remarks>
		/// 例如“一把辛酸泪”的前两个字符是“一把”，若存在“一把”这个词汇，那么当“一把辛酸泪”不存在多音字时，就属于可以安全移除的词汇。
		/// </remarks>
		[JsonIgnore]
		public bool? CanRemoveByHead { get; set; }

		/// <summary>
		/// 指示这个词汇是否因为尾部部分可以组成词汇而被移除。
		/// </summary>
		/// <remarks>
		/// 例如“利民商店”的后两个字符是“商店”，若存在“商店”这个词汇，那么当“利民商店”不存在多音字时，就属于可以安全移除的词汇。
		/// </remarks>
		[JsonIgnore]
		public bool? CanRemoveByTail { get; set; }

		/// <summary>
		/// 指示这个词汇是否包含生僻读音。
		/// </summary>
		[JsonIgnore]
		public bool HasRarePinyin
		{
			get => (this.hasRarePinyin ?? (this.hasRarePinyin = App.CheckRarePinyin(this.ActualWord, this.PreferedPinyinArray))).Value;
			set => this.hasRarePinyin = value;
		}

		/// <summary>
		/// 指示这个词汇是否应该被忽略。
		/// </summary>
		public bool IsDisabled { get; set; }

		/// <summary>
		/// 创建新的词汇信息的实例。
		/// </summary>
		/// <param name="word">相关词汇。</param>
		public WordInfo(string word)
		{
			if (Regex.IsMatch(word, @"[^\u4E00-\u9FFF\W]") || word.Length < 2)
			{
				this.IsValid = false;
			}
			else
			{
				this.IsValid = true;
			}

			this.Word = word;
		}

		/// <summary>
		/// 重设最佳拼音。
		/// </summary>
		public void ResetPinyin()
		{
			this.preferedPinyin = null;
			this.preferedPinyinArray = null;
		}

		/// <summary>
		/// 指示相关的词汇在汉典有详细资料。
		/// </summary>
		public void EnableZSource()
		{
			this.ZSource = $"https://www.zdic.net/hans/{Uri.EscapeDataString(this.Word)}";
		}

		/// <summary>
		/// 以异步方式检查此词汇是否来自可信来源。
		/// </summary>
		/// <returns>如果来源可信，则返回true。</returns>
		public async Task<bool> CheckVerfiedAsync()
		{
			if (this.Verified)
			{
				return true;
			}

			await BaiduSpider.LoadSampleFromBaikeAsync(this);


			int score = 0;
			if (this.BPinyin != null)
			{
				score++;
			}
			if (this.BKPinyin != null)
			{
				score++;
			}
			if (this.ZSource != null)
			{
				score++;
			}
			if (this.HSource != null)
			{
				score++;
			}
			if (this.CSource != null)
			{
				score++;
			}
			return score > 2;
		}


		/// <summary>
		/// 检查某个词汇被另一个词汇包含。
		/// </summary>
		/// <param name="other">需要检查的词汇。</param>
		/// <returns>如果其它词汇与以当前词汇开始或结尾，且生僻读音与当前词汇一致，则返回true。</returns>
		public bool CanReplace(WordInfo other)
		{
			string pinyinA = this.PreferedPinyin;
			string pinyinB = other.PreferedPinyin;
			if (pinyinA == null || pinyinB == null)
			{
				return false;
			}
			string name = this.ActualWord;
			string otherName = other.ActualWord;
			int index = otherName.IndexOf(name, StringComparison.Ordinal);
			if (index == -1)
			{
				return false;
			}

			string[] array = other.PreferedPinyinArray;

			for (int i = 0; i < index; i++)
			{
				if (App.Dictionary[new string(otherName[i], 1)].PreferedPinyin != array[i])
				{
					return false;
				}
			}
			for (int i = index + name.Length; i < otherName.Length; i++)
			{
				if (App.Dictionary[new string(otherName[i], 1)].PreferedPinyin != array[i])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// 检查某个词汇是否可以安全排除。
		/// </summary>
		/// <param name="word">需要检查的词汇。</param>
		/// <returns>如果一个词汇可以拆解为一个或多个更小的部分，则返回true。</returns>
		public bool CheckCanRemoveByHead()
		{
			if (this.CanRemoveByHead.HasValue)
			{
				return this.CanRemoveByHead.Value;
			}

			string name = this.ActualWord;
			if (name.Length > 2)
			{
				int len = 2;
				while (len < name.Length)
				{
					string child = name.Substring(0, 2);
					if (App.Samples.TryGetValue(child, out WordInfo info) && info.CanReplace(this))
					{
						return (this.CanRemoveByHead = true).Value;
					}

					len++;
				}
			}

			return (this.CanRemoveByHead = false).Value;
		}

		/// <summary>
		/// 检查某个词汇是否可以安全排除。
		/// </summary>
		/// <param name="word">需要检查的词汇。</param>
		/// <returns>如果一个词汇可以拆解为一个或多个更小的部分，则返回true。</returns>
		public bool CheckCanRemoveByTail()
		{
			if (this.CanRemoveByTail.HasValue)
			{
				return this.CanRemoveByTail.Value;
			}

			string name = this.ActualWord;
			if (name.Length > 2)
			{
				int len = 2;
				while (len < name.Length)
				{
					string child = name.Substring(name.Length - len);
					if (App.Samples.TryGetValue(child, out WordInfo info) && info.CanReplace(this))
					{
						return (this.CanRemoveByTail = true).Value;
					}

					len++;
				}
			}

			return (this.CanRemoveByTail = false).Value;
		}

		/// <summary>
		/// 比对指定的词汇，检查是否存在交集。
		/// </summary>
		/// <param name="other">需要比对的词汇。</param>
		/// <returns>如果存在交集，返回true。</returns>
		public bool CheckIntersect(WordInfo other)
		{
			string name = this.ActualWord;
			string otherName = other.ActualWord;
			string[] otherPinyin = other.PreferedPinyinArray;
			string[] pinyin = this.PreferedPinyinArray;

			if (otherPinyin == null)
			{
				return false;
			}
			int index = otherName.IndexOf(name[0]);
			while (index > 0)
			{
				if (string.CompareOrdinal(otherName, index, name, 0, otherName.Length - index) == 0)
				{
					for (int i = 0, max = otherName.Length - index; i < max; i++, index++)
					{
						if (otherPinyin[index] != pinyin[i])
						{
							return true;
						}

					}
					return false;
				}
				index = otherName.IndexOf(name[0], index + 1);
			}

			return false;
		}

		/// <summary>
		/// 收缩词汇。
		/// </summary>
		/// <param name="samples">样本集合范围。</param>
		/// <param name="result">收缩后的词汇。</param>
		/// <returns>如果可以收缩，则返回收缩后的词汇，否则返回null。</returns>
		public WordInfo Shrink()
		{
			string name = this.ActualWord;
			if (name.Length > 2)
			{
				int index = name.Length - 1;
				char ch = name[index];
				/**
				 * 尾部带虚词的，可以考虑收缩。
				 */
				if ("的地得着了过啊哦呵哈了么呢吧啦咧咯啰喽吗嘛".IndexOf(ch) > -1)
				{
					CharacterInfo info = App.Dictionary[new string(ch, 1)];
					if (info.PreferedPinyin == this.PreferedPinyinArray[index])
					{
						name = name.Substring(0, index);
						if (App.Samples.TryGetValue(name, out WordInfo result))
						{
							if (result.PreferedPinyin == null)
							{
								string[] array = new string[index];
								Array.Copy(this.PreferedPinyinArray, 0, array, 0, index);
								result.SpecifiedPinyin = string.Join(" ", array);
							}
							return result;
						}
					}
				}

				string child;
				WordInfo other;
				for (int len = 2; len < name.Length; len++)
				{
					for (int i = 0, max = name.Length - len; i <= max; i++)
					{
						child = name.Substring(i, len);
						if (App.Samples.TryGetValue(child, out other) && this.CheckShrink(other))
						{
							return other;
						}
					}
					child = name.Substring(len);
					if (App.Samples.TryGetValue(child, out other) && !other.HasRarePinyin)
					{
						WordInfo result = new WordInfo(name.Substring(0, len));
						string[] array = new string[len];
						Array.Copy(this.PreferedPinyinArray, array, len);
						result.SpecifiedPinyin = string.Join(" ", array);
						return result;
					}
				}
			}
			return null;
		}


		/// <summary>
		/// 启用词典网资源。
		/// </summary>
		/// <param name="url">资源的下载地址。</param>
		public void EnableCSource(string url)
		{
			if (this.CSource == null)
			{
				this.CSource = url;
			}
		}

		/// <summary>
		/// 启用汉文学网资源。
		/// </summary>
		/// <param name="url">资源的下载地址。</param>
		public void EnableHSource(string url)
		{
			if (this.HSource == null)
			{
				this.HSource = url;
			}
		}

		/// <summary>
		/// 解释这个词汇中某个字符的拼音。
		/// </summary>
		/// <param name="character">需要解释的字符。</param>
		/// <param name="pinyin">字符对应的拼音。</param>
		public void ExplainPinyin(string character, string pinyin)
		{
			string word = this.Word;
			string[] explain = this.Explain ?? (this.Explain = new string[word.Length]);
			int index = word.IndexOf(character, StringComparison.Ordinal);
			while (index > -1)
			{
				explain[index] = pinyin;
				index = word.IndexOf(character, index + 1, StringComparison.Ordinal);
			}
		}

		/// <summary>
		/// 计算一个最佳拼音
		/// </summary>
		public void ComputePreferedPinyin()
		{
			if (this.PreferedPinyin != null)
			{
				return;
			}
			string word = this.ActualWord;
			List<string> blocks = new List<string>(word.Length);
			foreach (char ch in word)
			{
				if (App.Dictionary.TryGetValue(new string(ch, 1), out CharacterInfo character))
				{
					blocks.Add(character.PreferedPinyin);
				}
			}
			this.SpecifiedPinyin = string.Join(" ", blocks);
		}

		private string EvaluatePreferedPinyin()
		{
			if (this.customPinyin != null)
			{
				this.Source = "模板";
				return this.customPinyin;
			}
			if (this.specifiedPinyin != null)
			{
				this.Source = "计算";
				return this.specifiedPinyin;
			}
			if (this.preferedPinyin != null)
			{
				return this.preferedPinyin;
			}
			if (!this.IsValid)
			{
				return null;
			}

			this.Source = null;
			if (this.Verified)
			{
				this.Source = "百度+";
				return this.BPinyin;
			}

			if (this.PPinyin != null)
			{
				this.Source = "字典";
				return this.PPinyin;
			}


			if (this.EvaluatePinyin(this.ZPinyin) >= 7D)
			{
				this.Source = "汉典";
				return this.ZPinyin;
			}

			if (this.EvaluatePinyin(this.BPinyin) >= 7D)
			{
				this.Source = "百度";
				return this.BPinyin;
			}

			if (this.EvaluatePinyin(this.CPinyin) >= 7D)
			{
				this.Source = "词典";
				return this.CPinyin;
			}

			if (this.EvaluatePinyin(this.HPinyin) >= 7D)
			{
				this.Source = "汉学";
				return this.HPinyin;
			}

			if (this.EvaluatePinyin(this.BKPinyin) >= 7D)
			{
				this.Source = "百科";
				return this.BKPinyin;
			}

			return null;
		}

		private double EvaluatePinyin(string p)
		{
			if (p == null)
			{
				return 0D;
			}
			double score = 0D;
			if (p == this.ZPinyin)
			{
				score = 6D;
			}

			if (p == this.BPinyin)
			{
				score += 6D;
			}

			if (p == this.CPinyin)
			{
				score += 4D;
			}

			if (p == this.HPinyin)
			{
				score += 3D;
			}

			if (p == this.BKPinyin)
			{
				score += 4D;
			}

			if (p == this.GPinyin)
			{
				score += 2D;
			}

			return score;
		}


		private bool CheckShrink(WordInfo other)
		{
			/**
			 * 如果这个词汇包含一个其它的包含生僻读音的词汇，且若去除这两个词汇相同的部分，剩余部分没有生僻的读音，则这个词汇是可以收缩的，例如‘阿拉伯联合酋长国’可以收缩为‘酋长’。
			 * 如果某个词汇的生僻读音集中在某个部分，且这个部分可以单独成词，或者剩余部分可以单独成词，则这个词汇是可以收缩的，例如‘青藏铁路’可以收缩为‘青藏’。
			 */
			string name = this.ActualWord;
			string[] pinyin = this.PreferedPinyinArray;

			string otherName = other.ActualWord;
			if (name.Length <= otherName.Length)
			{
				return false;
			}
			int index = name.IndexOf(otherName, StringComparison.Ordinal);
			if (index == -1)
			{
				return false;
			}
			if (other.PreferedPinyin == null || other.IsDisabled)
			{
				return false;
			}

			string[] otherPinyin = other.PreferedPinyinArray;
			for (int i = 0; i < otherPinyin.Length; i++)
			{
				if (otherPinyin[i] != pinyin[index + i])
				{
					return false;
				}
			}

			if (!App.CheckRarePinyin(name, pinyin, 0, index) && !App.CheckRarePinyin(name, pinyin, index + otherName.Length, name.Length - index - otherName.Length))
			{
				return true;
			}

			return false;
		}

	}
}