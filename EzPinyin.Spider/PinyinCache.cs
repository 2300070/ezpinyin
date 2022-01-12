using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 表示拼音的缓存文件。
	/// </summary>
	internal sealed class PinyinCache
	{
		private static readonly List<PinyinCache> caches = new List<PinyinCache>();

		private readonly ConcurrentDictionary<string, string> cache;
		private readonly bool isLatest;
		private readonly string file;
		private bool changed;

		public PinyinCache(string file)
		{
			lock (caches)
			{
				caches.Add(this);
			}
			this.file = file;
			this.cache = new ConcurrentDictionary<string, string>();
			if (File.Exists(file))
			{
				Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(file));
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					if (item.Value != null)
					{
						this.cache.TryAdd(item.Key, item.Value);
					}
				}
				this.isLatest = File.GetLastWriteTime(file).Date == DateTime.Today;
			}
		}

		public void Save()
		{
			if (this.changed)
			{
				File.WriteAllText(this.file, JsonConvert.SerializeObject(this.cache));
			}
		}

		public void Add(string word, string pinyin)
		{
			if (pinyin == null)
			{
				return;
			}

			this.cache[word] = pinyin;
			this.changed = true;
		}

		public bool TryGetValue(string word, out string pinyin)
		{
			if (this.cache.TryGetValue(word, out pinyin))
			{
				if (pinyin != null || this.isLatest)
				{
					return true;
				}

				if (this.cache.TryRemove(word, out pinyin))
				{
					this.changed = true;
				}
			}

			return false;
		}

		public static void SaveAll()
		{
			foreach (PinyinCache cache in caches)
			{
				cache.Save();
			}
		}
	}
}