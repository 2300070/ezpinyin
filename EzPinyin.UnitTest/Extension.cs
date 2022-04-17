using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EzPinyin.UnitTest
{
	internal static class Extension
	{
		public static TValue AssertToBe<TValue>(this TValue value, TValue tobe)
		{
			Assert.AreEqual(tobe, value);
			return value;
		}
	}
}