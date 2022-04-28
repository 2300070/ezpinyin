namespace EzPinyin
{
	/// <summary>
	/// 用来表示某个异体字特性的类。
	/// </summary>
	internal sealed class VariantInfo
	{
		/// <summary>
		/// 对应文本形式的字符。
		/// </summary>
		public string Text => new string(this.Character, 1);

		/// <summary>
		/// 异体字字符。
		/// </summary>
		public char Character { get; }

		/// <summary>
		/// 异体字字符的类型。
		/// </summary>
		public VariantType VariantType { get; }

		/// <summary>
		/// 实例化新的<see cref="VariantInfo"/>。
		/// </summary>
		/// <param name="character">字符信息。</param>
		/// <param name="variantType">字符类型。</param>
		public VariantInfo(char character, VariantType variantType)
		{
			this.Character = character;
			this.VariantType = variantType;
		}
	}
}