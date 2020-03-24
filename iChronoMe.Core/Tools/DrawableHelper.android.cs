using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using iChronoMe.Core.Classes;

namespace iChronoMe.Tools
{
    public static partial class DrawableHelper
    {
        public static bool ResizeImage(string input, string output, int maxSize)
        {
            string tmp = output + "_" + DateTime.Now.Ticks;
            try
            {
                BitmapFactory.Options options = new BitmapFactory.Options();// Create object of bitmapfactory's option method for further option use
                options.InPurgeable = true; // inPurgeable is used to free up memory while required
                Bitmap originalImage = BitmapFactory.DecodeFile(input);

                float newHeight = 0;
                float newWidth = 0;

                float originalHeight = originalImage.Height;
                float originalWidth = originalImage.Width;

                if (originalHeight > originalWidth)
                {
                    newHeight = maxSize;
                    float ratio = originalHeight / maxSize;
                    newWidth = originalWidth / ratio;
                }
                else
                {
                    newWidth = maxSize;
                    float ratio = originalWidth / maxSize;
                    newHeight = originalHeight / ratio;
                }

                Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)newWidth, (int)newHeight, false);

                originalImage.Recycle();

                using (FileStream ms = new FileStream(tmp, FileMode.CreateNew))
                {
                    resizedImage.Compress(Bitmap.CompressFormat.Png, 100, ms);

                    resizedImage.Recycle();
                    ms.Close();

                    if (File.Exists(output))
                        File.Delete(output);
                    File.Move(tmp, output);

                    return true;
                }
            } 
            catch (ThreadAbortException) { return false; }
            catch(Exception ex)
            {
                sys.LogException(ex);
                return false;
            }
            finally
            {
                if (File.Exists(tmp))
                    File.Delete(tmp);
            }
        }
    }
}