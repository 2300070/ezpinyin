namespace EzPinyin
{
	/// <summary>
	/// 用来标记字符特性的类。
	/// </summary>
	internal sealed class CharacterInfo
	{
		/// <summary>
		/// 对应文本形式的字符。
		/// </summary>
		public string Text => new string(this.Character, 1);

		/// <summary>
		/// 对应字符。
		/// </summary>
		public char Character { get; }

		/// <summary>
		/// 字符类型。
		/// </summary>
		public CharacterType CharacterType { get; }

		/// <summary>
		/// 实例化新的<see cref="CharacterInfo"/>。
		/// </summary>
		/// <param name="character">字符信息。</param>
		/// <param name="characterType">字符类型。</param>
		public CharacterInfo(char character, CharacterType characterType)
		{
			this.Character = character;
			this.CharacterType = characterType;
		}
	}
}