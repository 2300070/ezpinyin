namespace EzPinyin
{
	/// <summary>
	/// 表示链表中的某个词汇节点所定义的拼音的优先级别。
	/// </summary>
	internal enum PinyinPriority
	{
		/// <summary>
		/// 低优先级别，一般是自动推导的异体词汇所使用的优先级别。
		/// </summary>
		Low,
		/// <summary>
		/// 一般优先级别，一般是添加项目内置词汇节点时所使用的优先级别。
		/// </summary>
		Normal,
		/// <summary>
		/// 高优先级别，一般是从自定义配置文件读取词条或在运行时添加自定义的词汇节点时所使用的优先级别。
		/// </summary>
		High,
	}
}
