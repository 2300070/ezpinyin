using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 用于下载，分析常用词以生成词典。
	/// </summary>
	internal static class LexiconSpider
	{
		public static readonly List<string> Characters = new List<string>();

		static LexiconSpider()
		{
			foreach (char ch in App.GB2312_CHARACTERS)
			{
				Characters.Add(new string(ch, 1));
			}

		}
		/// <summary>
		/// 查找或者注册指定的词汇样本。
		/// </summary>
		/// <param name="word">需要注册的词汇。</param>
		/// <returns>所找到的词汇，如果词汇不存在，则创建新的词汇样本，注册并返回。</returns>
		public static WordInfo FindOrRegister(string word)
		{
			if (App.Samples.TryGetValue(word, out WordInfo result))
			{
				return result;
			}

			result = new WordInfo(word);
			return result.IsValid ? App.Samples.GetOrAdd(result.ActualWord, result) : result;
		}

		/// <summary>
		/// 以异步的方式扫描词汇数据。
		/// </summary>
		/// <returns>任务信息。</returns>
		public static async Task LoadSamplesAsync()
		{
			Console.WriteLine("扫描样本数据。");

			await ZDictSpider.LoadSamplesAsync();

			await HDictSpider.LoadSamplesAsync();

			await CDictSpider.LoadSamplesAsync();

			await BaiduSpider.LoadSamplesAsync();

			foreach (string name in App.GeographicalNames)
			{
				if (!App.Samples.ContainsKey(name))
				{
					App.Samples.TryAdd(name, new WordInfo(name));
				}
			}

			Console.WriteLine();
			Console.WriteLine("下载并生成样本数据。");
			await App.ForEachAsync(App.Samples.Values, LexiconSpider.DownloadSampleAsync);

			BaiduSpider.SaveCache();
			ZDictSpider.SaveCache();
			CDictSpider.SaveCache();
			HDictSpider.SaveCache();
			BingSpider.SaveCache();

			Console.WriteLine("完成。");
		}

		/// <summary>
		/// 以异步方式下载指定的词汇信息。
		/// </summary>
		/// <param name="sample">需要下载的词汇样本。</param>
		/// <returns>任务信息</returns>
		public static async Task DownloadSampleAsync(WordInfo sample)
		{
			/**
			 * 如果是百度汉语从工具书中引用的，则不再进行剩余处理。
			 */
			if (sample.Verified && sample.BPinyin != null)
			{
				return;
			}

			/**
			 * 从汉典下载样本信息
			 */
			if (sample.ZPinyin == null)
			{
				await ZDictSpider.LoadSampleAsync(sample);
			}

			/**
			 * 从百度下载样本信息
			 */
			if (sample.BPinyin == null)
			{
				await BaiduSpider.LoadSampleAsync(sample);
			}

			/**
			 * 从词典网下载样本信息
			 */
			if (sample.PreferedPinyin == null && sample.CPinyin == null)
			{
				await CDictSpider.LoadSampleAsync(sample);
			}


			/**
			 * 从汉文学网下载样本信息
			 */
			if (sample.PreferedPinyin == null && sample.HPinyin == null)
			{
				await HDictSpider.LoadSampleAsync(sample);
			}

			/**
			 * 从必应词典下载样本信息
			 */
			if (sample.PreferedPinyin == null && sample.BingPinyin == null)
			{
				await BingSpider.LoadSampleAsync(sample);
			}

			if (sample.PreferedPinyin == null && sample.ZPinyin == null)
			{
				sample.EnableZSource(sample.ActualWord);
				await ZDictSpider.LoadSampleAsync(sample);
			}

			/**
			 * 从百度百科下载样本信息
			 */
			if (sample.PreferedPinyin == null && sample.BKPinyin == null)
			{
				await BaiduSpider.LoadSampleFromBaikeAsync(sample);
			}
		}
	}
}