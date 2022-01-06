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
		public bool Verified { get; set; }

		/// <summary>
		/// 该读音的常见用法数量。
		/// </summary>
		[JsonIgnore]
		public int ReferenceCount { get; set; }

		/// <summary>
		/// 经过评估后得出的该读音的最大词性加权。
		/// </summary>
		public double WeightedType { get; set; }

		/// <summary>
		/// 额外加分，如义项数量等。
		/// </summary>
		public double ExtraEvaluation { get; set; }

		public PinyinInfo()
		{

		}

		public PinyinInfo(string text)
		{
			this.Text = text;
			this.Verified = App.StandardPinyinList.Contains(text);
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
			double value = (int)type * 0.1D;
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

		/// <summary>
		/// 评估当前拼音的分数。
		/// </summary>
		/// <param name="info">字符信息。</param>
		public double Evaluate(CharacterInfo info)
		{
			double result = this.ReferenceCount * this.WeightedType + this.WeightedType + this.ExtraEvaluation;
			double extra = 0D;
			if (this.Text == info.ZPinyin)
			{
				extra += 0.5D;
			}
			if (this.Text == info.YPinyin)
			{
				extra += 0.5D;
			}

			return result + extra;
		}
	}
}