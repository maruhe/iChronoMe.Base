using System;
using System.Collections.Generic;
using System.IO;
using iChronoMe.Core.Classes;
using Windows.UI.Xaml.Media.Imaging;

namespace iChronoMe.Tools
{
    public static partial class DrawableHelper
    {
        public static bool ResizeImage(string input, string output, int maxSize)
        {
            throw new NotImplementedException();
        }
    }
}
            /*
            try
            {
                using (MemoryStream streamIn = new MemoryStream(imageData))
                {
                    Picture
                    WriteableBitmap bitmap = PictureDecoder.DecodeJpeg(streamIn, (int)width, (int)height);

                    float Height = 0;
                    float Width = 0;

                    float originalHeight = bitmap.PixelHeight;
                    float originalWidth = bitmap.PixelWidth;

                    if (originalHeight > originalWidth)
                    {
                        Height = height;
                        float ratio = originalHeight / height;
                        Width = originalWidth / ratio;
                    }
                    else
                    {
                        Width = width;
                        float ratio = originalWidth / width;
                        Height = originalHeight / ratio;
                    }

                    using (MemoryStream streamOut = new MemoryStream())
                    {
                        bitmap.SaveJpeg(streamOut, (int)Width, (int)Height, 0, 100);
                        resizedData = streamOut.ToArray();
                    }
                }
                return resizedData;

                using (FileStream ms = new FileStream(output, FileMode.CreateNew))
                {
                    resizedImage.Compress(Bitmap.CompressFormat.Png, 100, ms);

                    resizedImage.Recycle();
                    ms.Flush();
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
}*/