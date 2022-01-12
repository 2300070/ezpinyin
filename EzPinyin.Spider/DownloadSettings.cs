using System;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 表示下载设置
	/// </summary>
	internal sealed class DownloadSettings
	{
		public string Url { get; set; }
		/// <summary>
		/// 指示如果缓存的最后写入时间大于指定值，则直接忽略。
		/// </summary>
		public DateTime? IgnoreCache { get; set; }
		/// <summary>
		/// 需要使用POST方式发送的数据
		/// </summary>
		public string Data { get; set; }
		/// <summary>
		/// 指示如果缓存的最后写入时间小于指定值，则需要重新读取。
		/// </summary>
		public DateTime? CacheDate { get; set; }
		/// <summary>
		/// 请求的Authorization标头。
		/// </summary>
		public string Authorization{get; set; }
		/// <summary>
		/// 请求的Content-Type标头。
		/// </summary>
		public string ContentType { get; set; }

		public DownloadSettings(string url)
		{
			this.Url = url;
		}

		public static implicit operator DownloadSettings(string url)
		{
			return new DownloadSettings(url);
		}
	}
}