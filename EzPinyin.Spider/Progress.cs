using System;
using System.Diagnostics;
using System.Threading;

namespace EzPinyin.Spider
{
	/// <summary>
	/// 描述了一个进度管理呈现类，用来在终端程序上打出进度条。
	/// </summary>
	internal sealed class Progress
	{
		private static readonly long interval = TimeSpan.FromMilliseconds(500).Ticks;
		private readonly int maximum;
		private readonly int step;
		private readonly int top;
		private int previous;
		private int progress;
		private long ticks;

		/// <summary>
		/// 创建一个新的进度实例。
		/// </summary>
		/// <param name="maximum">此实例所关联的过程最大执行次数，亦即将调用<see cref="Progress.Increment"/>方法的次数。</param>
		public Progress(int maximum)
		{
			this.maximum = maximum;
			this.previous = 0;
			this.step = maximum / 1000;
			this.top = Console.CursorTop;
			this.UpdateProgress(0);
		}

		/// <summary>
		/// 使进度增加1，并且自动触发进度条更新。
		/// </summary>
		public void Increment()
		{
			int value = Interlocked.Increment(ref this.progress);
			if (value >= this.maximum)
			{
				this.FinishProgress();
				return;
			}

			if (this.previous + this.step == value)
			{
				this.previous = value;
				this.UpdateProgress(value);
			}
		}

		private void UpdateProgress(int progress)
		{
			long ticks = Stopwatch.GetTimestamp();

			if (ticks - interval < this.ticks)
			{
				return;
			}

			this.ticks = ticks;

			double value = progress * 100D / this.maximum;
			string label = $"{value:0.0}%";
			int num = (int)((Console.WindowWidth - 7D) * progress / this.maximum);
			int prevLeft = Console.CursorLeft;

			Console.SetCursorPosition(0, this.top);

			if (num > 0)
			{
				Console.Write(new string('▆', num / 2));
			}
			Console.Write(label);
			num = prevLeft - Console.CursorLeft;
			if (num > 0)
			{
				Console.Write(new string(' ', num));
			}
			Console.CursorVisible = false;
		}

		private void FinishProgress()
		{
			Console.CursorVisible = true;
			int total = Console.WindowWidth;
			Console.SetCursorPosition(0, this.top);
			Console.Write(new string('▆', (total - 6) / 2));
			Console.WriteLine("100%");
		}
	}
}