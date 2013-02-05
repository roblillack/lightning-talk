using System;
using System.Collections.Concurrent;
using System.Text;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.IO;
using System.Threading;
using BlackBerry;
using LEDs = BlackBerry.NotificationLight.Color;

namespace BarefootPresenter
{
	public class ImageCache
	{
		int prefetch;
		FileInfo[] slides;
        ConcurrentDictionary<int, Image> cacheMap = new ConcurrentDictionary<int, Image>();
		volatile int currentIndex;
		Thread prefetcher;
		int width, height;

		public ImageCache (FileInfo[] files, Size size, int prefetchSize = 10)
		{
			prefetch = prefetchSize;
			slides = files;
			this.width = size.Width;
			this.height = size.Height;
			prefetcher = new Thread (Prefetch);
			prefetcher.Start ();
			lock (cacheMap) {
				Monitor.Pulse (cacheMap);
			}
		}

        public Image get (int idx)
        {
            Image image;

			lock (cacheMap) {
				currentIndex = idx;
				Monitor.Pulse (cacheMap);
			}

            if (cacheMap.TryGetValue(idx, out image)) {
                return image;
            }

            return Load (idx);
        }

		public int Length { get { return slides.Length; } }

		private int MinPrefetch { get { return Math.Max (0, currentIndex - prefetch); } }
		private int MaxPrefetch { get { return Math.Min (slides.Length - 1, currentIndex + prefetch); } }

		private Image Load (int idx)
		{
			try {
				var img = Image.FromFile (slides [idx].FullName);
				if (img.Width != width || img.Height != height) {
					ScaleImage (ref img);
				}
				cacheMap.TryAdd (idx, img);
				return img;
			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}

			return null;
		}

		private void ScaleImage (ref Image img)
		{
			var bitmap = new Bitmap (width, height);
			using (var g = Graphics.FromImage (bitmap)) {
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.Clear (Color.Black);
				var size = FixedRatioScale (img.Size);
				g.DrawImage (img, (width - size.Width) / 2, (height - size.Height) / 2, size.Width, size.Height);
			}
			img.Dispose ();
			img = bitmap;
		}

		private Size FixedRatioScale (Size original)
		{
			int newW = width;
			int newH = original.Height * newW / original.Width;

			if (newH > height) {
				newH = height;
				newW = original.Width * newH / original.Height;
			}

			Console.WriteLine ("[{0}, {1}] --> [{2}, {3}]", original.Width, original.Height, newW, newH);

			return new Size (newW, newH);
    	}

		private void Prefetch ()
		{
			while (true) {
				int min, max;
				Console.WriteLine ("Prefetcher Waiting...");
				NotificationLight.Blink (LEDs.Green);
				lock (cacheMap) {
					Monitor.Wait (cacheMap);
					min = MinPrefetch;
					max = MaxPrefetch;
				}
				using (new Vibration (Vibration.Intensity.High)) {
					Console.WriteLine ("Prefetching {0} to {1}", min, max);
					foreach (var i in cacheMap) {
						if (i.Key < min || i.Key > max) {
							Image img;
							if (cacheMap.TryRemove (i.Key, out img)) {
								img.Dispose ();
							}
						}
					}

					for (int i = min; i <= max; i++) {
						if (cacheMap.ContainsKey (i)) {
							continue;
						}
						cacheMap.TryAdd (i, Load (i));
					}
				}
			}
		}

		public string Status {
			get {
				return String.Format ("Idx: {0}, {1} images cached: {2}",
				                      currentIndex,
				                      cacheMap.Count,
				                      String.Join (", ", new System.Collections.Generic.SortedSet<int> (cacheMap.Keys)));
			}
		}
    }
}

