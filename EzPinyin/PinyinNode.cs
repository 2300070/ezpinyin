using System.Text;

namespace EzPinyin
{
	/// <summary>
	/// 表示一个拼音节点的基类。
	/// </summary>
	internal abstract unsafe class PinyinNode
	{
		/// <summary>
		/// 获得当前节点的拼音字符串。
		/// </summary>
		public abstract string Pinyin { get; }

		/// <summary>
		/// 获得拼音字符串。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <returns>所获得的字符串。</returns>
		public abstract string GetPinyin(char* cursor);
		
		/// <summary>
		/// 获得拼音首字母。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <returns>所获得的首字母。</returns>
		public abstract string GetInitial(char* cursor);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="end">字符串中最后一个字符的位置</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public abstract void WritePinyin(ref char* cursor, char* end, StringBuilder buffer, string separator);

		/// <summary>
		/// 将拼音首字母写入到指定的缓存区，并且自动移动游标到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="end">字符串中最后一个字符的位置</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="separator">分隔符。</param>
		public abstract void WriteInitial(ref char* cursor, char* end, StringBuilder buffer, string separator);

		/// <summary>
		/// 将拼音字符串写入到指定的缓存区，并且自动移动游标与索引到下一个字符的位置。
		/// </summary>
		/// <param name="cursor">游标信息。</param>
		/// <param name="end">字符串中最后一个字符的位置</param>
		/// <param name="buffer">目标缓存区。</param>
		/// <param name="index">分隔符。</param>
		public abstract void WritePinyin(ref char* cursor, char* end, string[] buffer, ref int index);
	}
}