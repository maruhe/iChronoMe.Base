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
	public class SKCanvasMapper : IDisposable
	{
		private Bitmap bitmap;
		private SKImageInfo info;

		public SKCanvasMapper()
		{
			// create the initial info
			info = new SKImageInfo(0, 0, SKColorType.Rgba8888, SKAlphaType.Premul);
		}

		public SKSize CanvasSize => info.Size;

		public void Draw(Canvas canvas, object param = null)
		{
			if (info.Width == 0 || info.Height == 0)
			{
				FreeBitmap();
				return;
			}

			// draw bitmap to canvas
			//canvas.DrawBitmap(bitmap, info.Rect.ToRect(), new RectF(0, 0, Width, Height), null);
			canvas.DrawBitmap(GetBitmap(param), 0, 0, null);
		}

		public Bitmap GetBitmap(object param = null)
		{
			if (info.Width == 0 || info.Height == 0)
			{
				FreeBitmap();
				return null;
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
				OnPaintSurface(new CMPaintSurfaceEventArgs(surface, info, param));
				surface.Canvas.Flush();
			}
			bitmap.UnlockPixels();

			return bitmap;
		}

		public void UpdateCanvasSize(int width, int height)
		{
			info.Width = width;
			info.Height = height;
		}

		public event EventHandler<CMPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(CMPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		public void DetachedFromWindow()
		{
			FreeBitmap();
		}

		public void Dispose()
		{
			FreeBitmap();
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

		public object ViewTag { get; set; }
		public object ConfigTag { get; set; }
	}

	public class CMPaintSurfaceEventArgs : EventArgs
	{
		public CMPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info, object param)
		{
			Surface = surface;
			Info = info;
			Param = param;
		}

		//
		// Zusammenfassung:
		//     Gets the surface that is currently being drawn on.
		public SKSurface Surface { get; }
		//
		// Zusammenfassung:
		//     Gets the information about the surface that is currently being drawn.
		public SKImageInfo Info { get; }

		public object Param { get; }
	}
}
