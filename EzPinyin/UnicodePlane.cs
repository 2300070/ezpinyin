using System;
using System.Collections;

namespace EzPinyin
{
	internal delegate ICollection DictionaryFunc();

	/// <summary>
	/// 表示某个汉字相关的Unicode平面。
	/// </summary>
	internal class UnicodePlane
	{
		private readonly DictionaryFunc loadDictionary;
		private ICollection dictionary;

		public int Head { get; }

		public PlaneType Type { get; }
		public ICollection Dictionary => this.dictionary ?? (this.dictionary = this.loadDictionary());
		public int Count => this.Dictionary.Count;

		internal UnicodePlane(PlaneType type, int head, DictionaryFunc loadDictionary)
		{
			this.loadDictionary = loadDictionary;
			this.Type = type;
			this.Head = head;
		}


		internal void LoadLexicon() => throw new NotImplementedException();
		internal void Add(string word, string[] pinyin) => throw new NotImplementedException();
	}
}