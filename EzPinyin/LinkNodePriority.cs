using System;
using System.Collections.Generic;
using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示链表中的某个词汇节点的优先级别。
	/// </summary>
	internal enum LinkNodePriority
	{
		/// <summary>
		/// 低优先级别，一般是添加简体字词汇时同步添加的繁体词汇节点所使用的优先级别。
		/// </summary>
		Low = 0,
		/// <summary>
		/// 一般优先级别，一般是添加项目内置词汇节点时所使用的优先级别。
		/// </summary>
		Normal,
		/// <summary>
		/// 高优先级别，一般是在运行时添加自定义的词汇节点时所使用的优先级别。
		/// </summary>
		High,
	}
}
