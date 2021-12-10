using System;
using Newtonsoft.Json;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 描述了一个拼音信息。
	/// </summary>
	internal sealed class PinyinInfo
	{
		/// <summary>
		/// 读音的拼音
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// 该读音的常见用法数量。
		/// </summary>
		[JsonIgnore]
		public int ReferenceCount { get; set; }

		/// <summary>
		/// 经过评估后所得到的此拼音的有效分数。
		/// </summary>
		public double Evaluation => this.ReferenceCount * 0.5D + this.WeightedType + this.ExtraEvaluation;

		/// <summary>
		/// 经过评估后得出的该读音的最大词性加权。
		/// </summary>
		public double WeightedType { get; set; }

		/// <summary>
		/// 额外加分，如义项数量等。
		/// </summary>
		public double ExtraEvaluation { get; set; }

		public PinyinInfo(string text)
		{
			this.Text = text;
		}

		/// <summary>
		/// 增加额外的分数。
		/// </summary>
		/// <param name="evaluation">分数值。</param>
		public void AddEvaluation(double evaluation)
		{
			this.ExtraEvaluation += evaluation;
		}

		/// <summary>
		/// 增加一个词性与义项数量。
		/// </summary>
		/// <param name="type">词性。</param>
		/// <param name="meaningCount">义项数量。</param>
		public void AddType(CharacterType type, int meaningCount)
		{
			this.ExtraEvaluation += meaningCount;
			double value = (int) type * 0.01D;
			if (value > this.WeightedType)
			{
				this.WeightedType = value;
			}

		}
		/// <summary>
		/// 添加此此读音的引用计数。
		/// </summary>
		/// <param name="count">计数。</param>
		public void AddReferenceCount(int count)
		{
			this.ReferenceCount += count;
		}
	}
}