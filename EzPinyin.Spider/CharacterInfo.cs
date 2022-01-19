using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 表示一个字符的全部发音、词汇信息的集合。
	/// </summary>
	internal sealed class CharacterInfo
	{
		private static readonly char[] trimCharacters = new[] { ' ', '	', '\r', '\n', ' ', '̀' };
		private List<PinyinInfo> pinyinList = new List<PinyinInfo>(2);

		private PinyinInfo prefered;
		private bool isValid = true;

		/// <summary>
		/// 获得最常用的拼音字符串。
		/// </summary>
		[JsonIgnore]
		public string PreferedPinyin => this.CustomPinyin ?? this.Prefered?.Text;

		/// <summary>
		/// 获得最常用的读音信息。
		/// </summary>
		[JsonIgnore]
		public PinyinInfo Prefered
		{
			get => this.prefered ?? (this.prefered = this.ComputePrefered());
			internal set => this.prefered = value;
		}

		/// <summary>
		/// 获得当前集合中的读音总数。
		/// </summary>
		[JsonIgnore]
		public int Count => this.pinyinList.Count;

		/// <summary>
		/// 所有拼音集合。
		/// </summary>
		public List<PinyinInfo> PinyinList
		{
			get => this.pinyinList;
			set
			{
				this.pinyinList.Clear();
				if (value != null)
				{
					this.pinyinList.AddRange(value);
				}
			}
		}

		/// <summary>
		/// 指示这个字符对应的简体字。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public char Simplified { get; set; }

		/// <summary>
		/// 指示这个字的异体字。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<char> Variants { get; set; }
		/// <summary>
		/// 指示这个字符对应的繁体字。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public char Traditional { get; set; }

		/// <summary>
		/// 相关联的字符。
		/// </summary>
		[JsonIgnore]
		public string Character { get; set; }

		/// <summary>
		/// 此字符是否有标准拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsStandard { get; set; }

		/// <summary>
		/// 自定的拼音
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string CustomPinyin { get; set; }

		/// <summary>
		/// 汉典最佳拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ZDictPinyin { get; set; }

		/// <summary>
		/// 叶典最佳拼音。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string YDictPinyin { get; set; }

		/// <summary>
		/// 国学网最佳拼音
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string GuoxuePinyin { get; set; }

		/// <summary>
		/// 指示该字符信息是否合规。
		/// </summary>
		[JsonIgnore]
		public bool IsValid
		{
			get
			{
				if (this.Character.Length == 0 || this.Character[0] < 0xFF || this.Character.Length > 2)
				{
					return false;
				}
				return this.isValid;
			}
			set => this.isValid = value;
		}

		/// <summary>
		/// 此字符是否被可信来源收录。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsTrusted { get; set; }

		/// <summary>
		/// 根据索引Id获得读音信息。
		/// </summary>
		/// <param name="index">索引值。</param>
		/// <returns>对应的读音信息。</returns>
		public string this[int index] => this.pinyinList[index].Text;

		/// <summary>
		/// 根据拼音字符串获得读音信息。
		/// </summary>
		/// <param name="index">索引值。</param>
		/// <returns>对应的读音信息。</returns>
		public PinyinInfo this[string pinyin] => this.pinyinList.Find(x => x.Text == pinyin);

		public CharacterInfo()
		{
		}
		public CharacterInfo(string character)
		{
			this.Character = character;
		}

		/// <summary>
		/// 注册一个读音信息。
		/// </summary>
		/// <param name="pinyin">需要注册的拼音。</param>
		/// <returns>所注册的拼音信息。</returns>
		public PinyinInfo Register(string pinyin)
		{
			if (string.IsNullOrEmpty(pinyin))
			{
				return null;
			}
			List<PinyinInfo> list = this.pinyinList;
			lock (list)
			{
				int index = list.FindIndex(x => x.Text == pinyin);
				PinyinInfo info;
				if (index == -1)
				{
					info = new PinyinInfo(pinyin);
					if (info.IsStandard)
					{
						this.IsStandard = true;
					}
					list.Add(info);
					return info;
				}

				return list[index];

			}
		}

		/// <summary>
		/// 注册所有可能拼音并返回第一个有效的读音信息。
		/// </summary>
		/// <param name="pinyin">需要注册的拼音。</param>
		/// <returns>所注册的拼音信息。</returns>
		public PinyinInfo RegisterAll(string pinyin)
		{
			if (string.IsNullOrEmpty(pinyin))
			{
				return null;
			}

			string[] items = Regex.Split(WebUtility.HtmlDecode(pinyin.ToLower()).Trim(trimCharacters), @"\s*[，,]\s*", RegexOptions.Compiled);
			PinyinInfo result = null;
			for (int i = 0; i < items.Length; i++)
			{
				string item = App.FixPinyin(items[i]);
				if (!string.IsNullOrEmpty(item))
				{
					PinyinInfo info = this.Register(item);
					if (result == null || !result.IsStandard && info.IsStandard)
					{
						result = info;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// 清除所有读音信息。
		/// </summary>
		/// <remarks>
		/// 目前主要用于调试。
		/// </remarks>
		public void Clear()
		{
			this.pinyinList.Clear();
			this.prefered = null;
		}

		/// <summary>
		/// 充值拼音信息，以便重新统计。
		/// </summary>
		public void Reset()
		{
			this.prefered = null;
		}

		/// <summary>
		/// 计算最佳拼音信息
		/// </summary>
		/// <returns>最佳拼音信息。</returns>
		public PinyinInfo ComputePrefered()
		{
			if (this.ZDictPinyin != null && (this.ZDictPinyin == this.YDictPinyin || this.ZDictPinyin == this.GuoxuePinyin))
			{
				return this.Register(this.ZDictPinyin);
			}
			if (this.YDictPinyin != null && this.YDictPinyin == this.GuoxuePinyin)
			{
				return this.Register(this.YDictPinyin);
			}

			if (this.pinyinList.Count == 0)
			{
				if (this.YDictPinyin != null)
				{
					return this.Register(this.YDictPinyin);
				}
				if (this.ZDictPinyin != null)
				{
					return this.Register(this.ZDictPinyin);
				}
				if (this.GuoxuePinyin != null)
				{
					return this.Register(this.GuoxuePinyin);
				}
			}
			PinyinInfo result = null;
			foreach (PinyinInfo pinyin in this.pinyinList)
			{
				if (result == null || result.Evaluate(this) < pinyin.Evaluate(this))
				{
					result = pinyin;
				}
			}

			return result;
		}

		/// <summary>
		/// 从其它字符复制拼音信息。
		/// </summary>
		/// <param name="other">需要复制的字符。</param>
		public void CopyFrom(CharacterInfo other)
		{
			if (other.IsStandard)
			{
				this.IsStandard = true;
			}
			this.pinyinList = new List<PinyinInfo>(other.pinyinList);
		}

		/// <summary>
		/// 获得指定拼音的索引。
		/// </summary>
		/// <param name="pinyin">拼音信息。</param>
		/// <returns>索引值，如果不存在，则返回-1.</returns>
		public int IndexOf(string pinyin) => this.pinyinList.FindIndex(x => x.Text == pinyin);
	}
}