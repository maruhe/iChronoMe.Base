using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SkiaSharp;
using SkiaSharp.Views.Android;

//mapp's any SKCanvas-stuff to and Android Canvas
//based on SkiaSharp.Views.Android.SKCanvasView
namespace iChronoMe.Widgets.AndroidHelpers
{
    public class SKCanvasMapper : Java.Lang.Object
    {
		private Bitmap bitmap;
		private SKImageInfo info;

		public SKCanvasMapper()
		{
			// create the initial info
			info = new SKImageInfo(0, 0, SKColorType.Rgba8888, SKAlphaType.Premul);
		}

		public SKSize CanvasSize => info.Size;

		public void Draw(Canvas canvas)
		{
			if (info.Width == 0 || info.Height == 0)
			{
				FreeBitmap();
				return;
			}

			// create the bitmap data if we need it
			if (bitmap == null || bitmap.Handle == IntPtr.Zero || bitmap.Width != info.Width || bitmap.Height != info.Height)
			{
				FreeBitmap();
				bitmap = Bitmap.CreateBitmap(info.Width, info.Height, Bitmap.Config.Argb8888);
			}

			// create a surface
			using (var surface = SKSurface.Create(info, bitmap.LockPixels(), info.RowBytes))
			{
				// draw using SkiaSharp
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
				surface.Canvas.Flush();
			}
			bitmap.UnlockPixels();

			// draw bitmap to canvas
			//canvas.DrawBitmap(bitmap, info.Rect.ToRect(), new RectF(0, 0, Width, Height), null);
			canvas.DrawBitmap(bitmap, 0, 0, null);
		}

		public void UpdateCanvasSize(int width, int height)
		{
			info.Width = width;
			info.Height = height;
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		public void DetachedFromWindow()
		{
			FreeBitmap();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				FreeBitmap();

			base.Dispose(disposing);
		}

		private void FreeBitmap()
		{
			if (bitmap != null)
			{
				// free and recycle the bitmap data
				if (bitmap.Handle != IntPtr.Zero && !bitmap.IsRecycled)
					bitmap.Recycle();
				bitmap.Dispose();
				bitmap = null;
			}
		}
	}
}
