using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using BlackBerry;
using BlackBerry.Screen;

namespace BarefootPresenter
{
	class Presenter
	{
		Window window;
		BlackBerry.Screen.Buffer buffer;
		Graphics graphics;
		int index;
		ImageCache images;
		int width, height;

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
			index = Math.Max (Math.Min (index, images.Length - 1), 0);
			Random r = new Random ();
			//System.Console.WriteLine ("Image Size: {0}", img == null ? "no image" : img.Size.ToString ());
			graphics.Clear (Color.Black);
			DateTime loaded, before = DateTime.Now;
			Image img;
			if ((img = images.get (index)) != null) {
				loaded = DateTime.Now;
				graphics.DrawImage (img, 0, 0, width, height);
			}
			Font font = new Font (FontFamily.GenericSansSerif, 36.0f, FontStyle.Bold, GraphicsUnit.Pixel);
			graphics.DrawString (images.Status, font, new SolidBrush (Color.DarkRed), new Point (20, 20));
			graphics.DrawString (index.ToString (), font, new SolidBrush (Color.DarkGreen), new Point (20, 70));
			//Dialog.Alert ("Info", "Loading this image took " + (loaded - before) + ", displaying " + (DateTime.Now - loaded) + " units.", new Button ("Ok"));
			graphics.Flush ();
			window.Render (buffer);
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
					Dialog.Alert ("ScreenOrientation Error", e.Message + "\n\n" + e.StackTrace, new Button ("Ack"));
				}
				try {
					nav.SetOrientation (Navigator.ApplicationOrientation.TopUp);
				} catch (Exception e) {
					Dialog.Alert ("ApplicationOrientation Error", e.Message + "\n\n" + e.StackTrace, new Button ("Ack"));
				}
				try {
					nav.OrientationLock = true;
				} catch (Exception e) {
					Dialog.Alert ("OrientationLock Error", e.Message + "\n\n" + e.StackTrace, new Button ("Ack"));
				}
				using (var ctx = Context.GetInstance (ContextType.Application))
				using (window = new Window (ctx)) {
					window.KeepAwake = true;
					window.AddBuffer ();
					buffer = window.Buffers [0];
					width = window.Width;
					height = window.Height;
					graphics = Graphics.FromImage (buffer.Bitmap);

					nav.OnSwipeDown = () => Dialog.Alert ("barefoot presenter",
				                                      "Mem: " + System.GC.GetTotalMemory (false) + "\n" +
					                                      images.Status,
				                                      new Button ("Previous", PreviousSlide),
				                                      new Button ("Next", NextSlide));				                                  

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
						System.Console.WriteLine ("I am asked to shutdown!?!");
						PlatformServices.Shutdown (0);
					};

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
