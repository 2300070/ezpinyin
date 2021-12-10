namespace EzPinyin.Spider
{
	/// <summary>
	/// 表示某个读音的凭据，它既反映了读音的来源，又代表了这个读音的可信度。
	/// </summary>
	internal enum PinyinConfidence
	{
		/// <summary>
		/// 汉典
		/// </summary>
		ZDict = 60,
		/// <summary>
		/// 汉文学网
		/// </summary>
		Hwxnet = 25,
		/// <summary>
		/// 词典网
		/// </summary>
		Cidianwang = 30,
		/// <summary>
		/// 百度搜索
		/// </summary>
		Baidu = 60,
		///// <summary>
		///// Bing搜索
		///// </summary>
		//Bing = 50,
		/// <summary>
		/// 新华字典或者现代汉语词典等信任来源
		/// </summary>
		Professional = 1000,
		/// <summary>
		/// 推测
		/// </summary>
		Prediction = 25
	}
}