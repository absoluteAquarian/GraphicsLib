using ReLogic.Content;
using System;
using System.Threading;
using Terraria;

namespace GraphicsLib.Utility {
	public static class ThreadUtils {
		public static void InvokeOnMainThread(Action action) {
			if (!AssetRepository.IsMainThread) {
				ManualResetEvent evt = new(false);

				Main.QueueMainThreadAction(() => {
					action();
					evt.Set();
				});

				evt.WaitOne();
			} else
				action();
		}
	}
}
