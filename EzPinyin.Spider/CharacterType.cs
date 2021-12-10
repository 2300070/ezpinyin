namespace EzPinyin.Spider
{
	/// <summary>
	/// 用来描述某个拼音义项的用途类型，大多数情况下即是其词性。
	/// </summary>
	internal enum CharacterType
	{
		/// <summary>
		/// 其它
		/// </summary>
		Other = 1,
		/// <summary>
		/// 叹词
		/// </summary>
		Interjections = 2,
		/// <summary>
		/// 副词
		/// </summary>
		Adverb = 3,
		/// <summary>
		/// 助词
		/// </summary>
		Auxiliary = 5,
		/// <summary>
		/// 动词
		/// </summary>
		Verb = 7,
		/// <summary>
		/// 形容词
		/// </summary>
		Adjective = 8,
		/// <summary>
		/// 名词
		/// </summary>
		Noun = 10
	}
}