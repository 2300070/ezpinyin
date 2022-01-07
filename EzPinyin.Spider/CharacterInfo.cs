using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 表示一个字符的全部发音、词汇信息的集合。
	/// </summary>
	internal sealed class CharacterInfo
	{
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
		/// 此字符是否有专业可信来源。
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool Verified { get; set; }

		/// <summary>
		/// 自定的拼音
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string CustomPinyin { get; set; }

		/// <summary>
		/// 汉典最佳拼音。
		/// </summary>
		public string ZPinyin { get; set; }

		/// <summary>
		/// 叶典最佳拼音。
		/// </summary>
		public string YPinyin { get; set; }

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
		public PinyinInfo FindOrRegister(string pinyin)
		{
			List<PinyinInfo> list = this.pinyinList;
			lock (list)
			{
				int index = list.FindIndex(x => x.Text == pinyin);
				PinyinInfo info;
				if (index == -1)
				{
					info = new PinyinInfo(pinyin);
					if (info.Verified)
					{
						this.Verified = true;
					}
					list.Add(info);
					return info;
				}

				return list[index];

			}
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
			if (this.ZPinyin == this.YPinyin && this.YPinyin != null)
			{
				this.Verified = true;
			}
			this.prefered = null;
		}

		/// <summary>
		/// 计算最佳拼音信息
		/// </summary>
		/// <returns>最佳拼音信息。</returns>
		public PinyinInfo ComputePrefered()
		{
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
			if (other.Verified)
			{
				this.Verified = true;
			}
			this.pinyinList = new List<PinyinInfo>(other.pinyinList);
		}
	}
}