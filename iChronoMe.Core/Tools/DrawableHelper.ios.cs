using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using iChronoMe.Core.Classes;
using UIKit;

namespace iChronoMe.Tools
{
    public static partial class DrawableHelper
    {
        public static bool ResizeImage(string input, string output, int maxSize)
        {
            try
            {                
                UIImage originalImage = UIImage.FromFile(input);

                var originalHeight = originalImage.Size.Height;
                var originalWidth = originalImage.Size.Width;

                nfloat newHeight = 0;
                nfloat newWidth = 0;

                if (originalHeight > originalWidth)
                {
                    newHeight = maxSize;
                    nfloat ratio = originalHeight / maxSize;
                    newWidth = originalWidth / ratio;
                }
                else
                {
                    newWidth = maxSize;
                    nfloat ratio = originalWidth / maxSize;
                    newHeight = originalHeight / ratio;
                }

                var width = (float)newWidth;
                var height = (float)newHeight;

                UIGraphics.BeginImageContext(new SizeF(width, height));
                originalImage.Draw(new RectangleF(0, 0, width, height));
                var resizedImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                var bytesImagen = resizedImage.AsPNG().ToArray();
                resizedImage.Dispose();
                
                using (FileStream ms = new FileStream(output, FileMode.CreateNew))
                {
                    ms.Write(bytesImagen);
                    ms.Close();

                    return true;
                }
            } 
            catch(Exception ex)
            {
                sys.LogException(ex);
                return false;
            }
        }
    }
}