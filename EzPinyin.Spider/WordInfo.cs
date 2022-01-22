using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
		private string zDictPinyin;
		private string hDictPinyin;
		private string baiduHanyuPinyin;
		private string cDictPinyin;
		private string predicatePinyin;
		private string professinalPinyin;
		private string customPinyin;
		private string specifiedPinyin;
		private bool? hasRarePinyin;
		private string baiduBaikePinyin;
		private string bingPinyin;
		private string guoxuePinyin;
		private int[] preferedPinyinIndexes;

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
		/// 获得相关词汇经过评估的理想的拼音的索引数组。
		/// </summary>
		[JsonIgnore]
		public int[] PreferedPinyinIndexes
		{
			get
			{
				if (this.preferedPinyinIndexes == null && this.IsValid)
				{
					this.Validate();
				}
				return this.preferedPinyinIndexes;
			}
		}

		/// <summary>
		/// 这个词汇在汉典的详细资料地址。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ZDictSource { get; set; }

		/// <summary>
		/// 这个词汇在国学网的详细资料地址。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string GuoxueSource { get; set; }

		/// <summary>
		/// 这个词汇在词典网的详细资料地址。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string CDictSource { get; set; }

		/// <summary>
		/// 这个词汇在汉文学网的详细资料地址。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string HDictSource { get; set; }

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
		public string ZDictPinyin
		{
			get => this.zDictPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.zDictPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 国学网提供的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string GuoxuePinyin
		{
			get => this.guoxuePinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.guoxuePinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 汉文学网提供的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string HDictPinyin
		{
			get => this.hDictPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.hDictPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 百度汉语提供的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string BaiduHanyuPinyin
		{
			get => this.baiduHanyuPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.baiduHanyuPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 百度百科提供的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string BaiduBaikePinyin
		{
			get => this.baiduBaikePinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}

				this.baiduBaikePinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 词典网提供的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string CDictPinyin
		{
			get => this.cDictPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.cDictPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 根据工具书解释猜测的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string PredicatePinyin
		{
			get => this.predicatePinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.predicatePinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 根据工具书解释计算出的拼音。
		/// </summary>
		[JsonIgnore]
		public string ProfessionalPinyin
		{
			get => this.professinalPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.professinalPinyin = value;

				this.preferedPinyin = this.EvaluatePreferedPinyin() ?? this.preferedPinyin;
				this.preferedPinyinArray = null;
			}
		}

		/// <summary>
		/// 必应词典提供的拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string BingPinyin
		{
			get => this.bingPinyin;
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					return;
				}
				this.bingPinyin = value;

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
		public string ActualWord { get; set; }

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
			get => (this.hasRarePinyin ?? (this.hasRarePinyin = Common.CheckRarePinyin(this.ActualWord, this.PreferedPinyinArray))).Value;
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
			word = Regex.Replace(word, @"\s", string.Empty, RegexOptions.Compiled);

			bool isValid = true;

			/**
			 * 对词汇进行整理，将简体字转换为繁体字，去除非汉字字符。
			 */
			StringBuilder buffer = new StringBuilder(word.Length);
			for (int i = 0; i < word.Length; i++)
			{
				char ch = word[i];
				if (!Common.Simplified.TryGetValue(ch, out char simpilfied))
				{
					if (ch >= 0x4E00 && ch <= 0x9FFF || ch == '〇' //基本区+‘〇’
						 || ch >= 0x3400 && ch <= 0x4DBF //扩展A
						 || ch > 0xF8FF && ch < 0xFB00 //兼容
						 || ch > 0x2E7F && ch < 0x2FE0 //部首及部首扩展
					)
					{
						buffer.Append(ch);
					}
					else if (char.IsHighSurrogate(ch) && i + 1 < word.Length && char.IsLowSurrogate(word[i + 1]))
					{
						int code = char.ConvertToUtf32(ch, word[i + 1]);
						if (code > 0x1FFFF && code < 0x2A6E0 //扩展B
							|| code > 0x2A6FF && code < 0x2B739 //扩展C
							|| code > 0x2B73F && code < 0x2B81E //扩展D
							|| code > 0x2B81F && code < 0x2CEA2 //扩展E
							|| code > 0x2CEAF && code < 0x2EBE1 //扩展F
							|| code > 0x2FFFF && code < 0x3134B //扩展G
							|| code > 0x2F7FF && code < 0x2FA20 //兼容扩展
						)
						{
							buffer.Append(word, i, 2);
						}
						else
						{
							isValid = false;
						}

						i += 1;
					}
					else
					{
						isValid = false;
					}
				}
				else
				{
					buffer.Append(simpilfied);
				}

			}

			this.Word = word;
			this.ActualWord = buffer.ToString();
			this.IsValid = isValid;
		}

		/// <summary>
		/// 重新验证关键信息
		/// </summary>
		public bool Validate()
		{
			if (!this.IsValid)
			{
				return false;
			}

			if (this.PreferedPinyinArray != null && !this.IsDisabled)
			{
				if (this.Validate(this.ActualWord, this.PreferedPinyinArray))
				{
					return true;
				}
				if (this.Validate(this.Word, this.PreferedPinyinArray))
				{
					this.ActualWord = this.Word;
					return true;
				}
			}

			return this.IsValid = false;
		}

		private bool Validate(string word, string[] array)
		{
			if (word == null || word.Length < 2 || array == null || array.Length != word.Length)
			{
				return false;
			}

			int[] indexes = new int[word.Length];

			for (int i = 0; i < word.Length; i++)
			{
				string pinyin = array[i];
				if (pinyin == null || !Common.Dictionary.TryGetValue(new string(word[i], 1), out CharacterInfo ch) || ch.IndexOf(pinyin) == -1 || (indexes[i] = Common.PinyinList.IndexOf(pinyin)) == -1)
				{
					return false;
				}

			}

			this.preferedPinyinIndexes = indexes;
			return true;
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
		public void EnableZDictSource(string word = null)
		{
			this.ZDictSource = $"https://www.zdic.net/hans/{Uri.EscapeDataString(word ?? this.Word)}";
		}

		/// <summary>
		/// 指示相关的词汇在国学网有详细资料。
		/// </summary>
		public void EnableGuoxueSource(string key)
		{
			this.GuoxueSource = $"http://www.guoxuedashi.net/hydcd/{key}.html";
		}

		/// <summary>
		/// 以异步方式检查此词汇是否来自可信来源。
		/// </summary>
		/// <returns>如果来源可信，则返回true。</returns>
		public bool CheckVerified()
		{
			if (this.Verified)
			{
				return true;
			}


			int score = 0;
			if (this.BaiduHanyuPinyin != null)
			{
				score++;
			}
			if (this.BaiduBaikePinyin != null)
			{
				score++;
			}
			if (this.ZDictSource != null)
			{
				score++;
			}
			if (this.HDictSource != null)
			{
				score++;
			}
			if (this.CDictSource != null)
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
				if (Common.Dictionary.TryGetValue(new string(otherName[i], 1), out CharacterInfo info) && info.PreferedPinyin != array[i])
				{
					return false;
				}
			}
			for (int i = index + name.Length; i < otherName.Length; i++)
			{
				if (Common.Dictionary.TryGetValue(new string(otherName[i], 1), out CharacterInfo info) && info.PreferedPinyin != array[i])
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
					if (Common.Samples.TryGetValue(child, out WordInfo info) && info.CanReplace(this))
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
					if (Common.Samples.TryGetValue(child, out WordInfo info) && info.CanReplace(this))
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
			while (index > -1)
			{
				int length = Math.Min(otherName.Length - index, name.Length);
				if (string.CompareOrdinal(otherName, index, name, 0, length) == 0)
				{
					for (int i = 0; i < length; i++, index++)
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
					CharacterInfo info = Common.Dictionary[new string(ch, 1)];
					if (info.PreferedPinyin == this.PreferedPinyinArray[index])
					{
						name = name.Substring(0, index);
						if (Common.Samples.TryGetValue(name, out WordInfo result))
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
						if (Common.Samples.TryGetValue(child, out other) && this.CheckShrink(other))
						{
							return other;
						}
					}
					child = name.Substring(len);
					if (Common.Samples.TryGetValue(child, out other) && !other.HasRarePinyin)
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
		/// 指示相关的词汇在词典网有详细资料。
		/// </summary>
		/// <param name="url">资源的下载地址。</param>
		public void EnableCDictSource(string url)
		{
			if (this.CDictSource == null)
			{
				this.CDictSource = url;
			}
		}

		/// <summary>
		/// 指示相关的词汇在汉文学网有详细资料。
		/// </summary>
		/// <param name="url">资源的下载地址。</param>
		public void EnableHDictSource(string url)
		{
			if (this.HDictSource == null)
			{
				this.HDictSource = url;
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
		/// 确保当前词汇拥有最佳拼音
		/// </summary>
		public void EnsurePreferedPinyin()
		{
			if (this.PreferedPinyin != null)
			{
				return;
			}
			string word = this.ActualWord;
			List<string> blocks = new List<string>(word.Length);
			foreach (char ch in word)
			{
				if (Common.Dictionary.TryGetValue(new string(ch, 1), out CharacterInfo character))
				{
					blocks.Add(character.PreferedPinyin);
				}
				else
				{
					return;
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
			if (!this.IsValid)
			{
				return null;
			}

			this.Source = null;
			if (this.Verified && this.BaiduHanyuPinyin != null)
			{
				this.Source = "百度+";
				return this.BaiduHanyuPinyin;
			}
			if (this.Verified && this.ZDictPinyin != null)
			{
				this.Source = "汉典+";
				return this.ZDictPinyin;
			}

			if (this.ProfessionalPinyin != null)
			{
				this.Source = "字典";
				return this.ProfessionalPinyin;
			}


			if (this.EvaluatePinyin(this.ZDictPinyin) >= 7D)
			{
				this.Source = "汉典";
				return this.ZDictPinyin;
			}

			if (this.EvaluatePinyin(this.BaiduHanyuPinyin) >= 7D)
			{
				this.Source = "百度";
				return this.BaiduHanyuPinyin;
			}

			if (this.EvaluatePinyin(this.GuoxuePinyin) >= 7D)
			{
				this.Source = "国学";
				return this.GuoxuePinyin;
			}

			if (this.EvaluatePinyin(this.CDictPinyin) >= 7D)
			{
				this.Source = "词典";
				return this.CDictPinyin;
			}

			if (this.EvaluatePinyin(this.HDictPinyin) >= 7D)
			{
				this.Source = "汉学";
				return this.HDictPinyin;
			}

			if (this.EvaluatePinyin(this.BaiduBaikePinyin) >= 7D)
			{
				this.Source = "百科";
				return this.BaiduBaikePinyin;
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
			if (p == this.ZDictPinyin)
			{
				score = 6D;
			}

			if (p == this.BaiduHanyuPinyin)
			{
				score += 6D;
			}

			if (p == this.BingPinyin)
			{
				score += 6D;
			}

			if (p == this.GuoxuePinyin)
			{
				score += 5D;
			}

			if (p == this.CDictPinyin)
			{
				score += 4D;
			}

			if (p == this.HDictPinyin)
			{
				score += 3D;
			}

			if (p == this.BaiduBaikePinyin)
			{
				score += 4D;
			}

			if (p == this.PredicatePinyin)
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
			if (!other.IsValid)
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

			if (!Common.CheckRarePinyin(name, pinyin, 0, index) && !Common.CheckRarePinyin(name, pinyin, index + otherName.Length, name.Length - index - otherName.Length))
			{
				return true;
			}

			return false;
		}

	}
}