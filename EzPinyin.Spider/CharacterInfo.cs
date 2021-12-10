using System.Collections.Generic;
using Newtonsoft.Json;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 表示一个字符的全部发音、词汇信息的集合。
	/// </summary>
	internal sealed class CharacterInfo
	{
		private readonly List<PinyinInfo> pinyinList = new List<PinyinInfo>(2);

		private PinyinInfo prefered;

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
			get
			{
				PinyinInfo prefered = this.prefered;
				if (prefered == null)
				{
					foreach (PinyinInfo pinyin in this.pinyinList)
					{
						if (prefered == null || prefered.Evaluation < pinyin.Evaluation)
						{
							prefered = pinyin;
						}
					}
				}
				return prefered;
			}
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
		/// 自定的拼音
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string CustomPinyin { get; set; }

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
			if (!App.PinyinList.Contains(pinyin))
			{
				App.PinyinList.Add(pinyin);
			}

			List<PinyinInfo> definitions = this.pinyinList;
			int index = definitions.FindIndex(x => x.Text == pinyin);
			PinyinInfo info;
			if (index == -1)
			{
				info = new PinyinInfo(pinyin);
				definitions.Add(info);
			}
			else
			{
				info = definitions[index];
			}

			return info;
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
		/// 重设各个拼音的引用状态，以便重新统计。
		/// </summary>
		public void Reset()
		{
			this.prefered = null;
			foreach (PinyinInfo pinyin in this.pinyinList)
			{
				pinyin.ReferenceCount = 0;
			}
		}
	}
}