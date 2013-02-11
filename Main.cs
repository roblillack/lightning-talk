using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using BlackBerry;
using BlackBerry.Screen;

namespace LightningTalk
{
	class Presenter
	{
		const int LIGHTNING_TALK_TIME = 5 * 60 * 1000;
		Window window;
		BlackBerry.Screen.Buffer buffer;
		Graphics graphics;
		ImageCache images;
		int width, height;
		Thread renderThread;
		System.Timers.Timer timer;
		volatile int index;
		volatile int msecsLeft;
		Font timerFont;

		public static void Main (string[] args)
		{
			try {
				var p = new Presenter ();
				p.Run ();
			} catch (System.Exception e) {
				Dialog.Alert ("OH SNAP!", e.Message + (e.InnerException == null ? "": " ---- " + e.InnerException.Message), new Button ("Quit"));
			}
		}

		private void RenderSlide ()
		{
			lock (this) {
				Monitor.Pulse (this);
			}
		}

		private void DrawTimer ()
		{
			if (!timer.Enabled) {
				return;
			}

			if (timerFont == null) {
				timerFont = new Font (FontFamily.GenericSansSerif, 100.0f, FontStyle.Regular, GraphicsUnit.Pixel);
			}

			var left = new TimeSpan (0, 0, 0, 0, msecsLeft);
			var str = String.Format ("{0}′{1:00}.{2}″", left.Minutes, left.Seconds, left.Milliseconds / 100);
			graphics.DrawString (str, timerFont, new SolidBrush (Color.DarkRed), new Point (20, 20));
		}

		private void RenderLoop ()
		{
			while (true) {
				lock (this) {
					Monitor.Wait (this);
				}

				index = Math.Max (Math.Min (index, images.Length - 1), 0);
				Image img;
				if ((img = images.get (index)) != null) {
					graphics.DrawImage (img, 0, 0, width, height);
				}

				DrawTimer ();

				graphics.Flush ();
				window.Render (buffer);
			}
		}

		public void NextSlide ()
		{
			index++;
			RenderSlide ();
		}

		public void PreviousSlide ()
		{
			index--;
			RenderSlide ();
		}

		public void Run ()
		{
			using (var nav = new Navigator ()) {
				try {
					nav.SetOrientation (Navigator.ScreenOrientation.Landscape);
				} catch (Exception e) {
					Console.WriteLine ("ScreenOrientation Error: {0}\n{1}", e.Message, e.StackTrace);
				}
				try {
					nav.SetOrientation (Navigator.ApplicationOrientation.TopUp);
				} catch (Exception e) {
					Console.WriteLine ("ApplicationOrientation Error: {0}\n{1}", e.Message, e.StackTrace);
				}
				try {
					nav.OrientationLock = true;
				} catch (Exception e) {
					Console.WriteLine ("OrientationLock Error: {0}\n{1}", e.Message, e.StackTrace);
				}
				using (var ctx = Context.GetInstance (ContextType.Application))
				using (window = new Window (ctx)) {
					window.KeepAwake = true;
					window.AddBuffer ();
					buffer = window.Buffers [0];
					width = window.Width;
					height = window.Height;
					graphics = Graphics.FromImage (buffer.Bitmap);

					SoundPlayer.Prepare (SoundPlayer.SystemSound.AlarmBattery);

					msecsLeft = LIGHTNING_TALK_TIME;
					timer = new System.Timers.Timer ();
					timer.AutoReset = true;
					timer.Interval = 100;
					timer.Elapsed += (sender, e) => {
						msecsLeft -= 100;
						if (msecsLeft <= 0) {
							timer.Enabled = false;
							SoundPlayer.Play (SoundPlayer.SystemSound.AlarmBattery);
						}
						RenderSlide ();
					};

					nav.OnSwipeDown = () => Dialog.Alert ("Lightning Talk",
				                                          "A MonoBerry demonstration.",
					                                      new Button ("Countdown 5 mins", () => {
						msecsLeft = LIGHTNING_TALK_TIME;
						timer.Enabled = true;
					}));				                                  

					ctx.OnFingerTouch = (x, y) => {
						if (x < window.Width / 2) {
							PreviousSlide ();
						} else {
							NextSlide ();
						}
					};
					ctx.OnKeyDown = (code, _) => {
						if (code == KeyCode.Up) {
							PreviousSlide ();
						} else {
							NextSlide ();
						}
					};

					nav.OnExit = () => {
						PlatformServices.Shutdown (0);
					};

					renderThread = new Thread (RenderLoop);
					renderThread.Start ();

					DirectoryInfo dirInfo = new DirectoryInfo ("shared/documents");
					images = new ImageCache (dirInfo.GetFiles ("*.jpg", SearchOption.AllDirectories), new Size (width, height));
					//Dialog.Alert ("Info", images.Length + " files found in " + dirInfo.FullName,
				    //          new Button ("Nevermind"));

					RenderSlide ();

					PlatformServices.Run ();
					System.Console.WriteLine ("Event handler stopped. WTH?");
					PlatformServices.Shutdown (1);
				}
			}
		}
	}
}
