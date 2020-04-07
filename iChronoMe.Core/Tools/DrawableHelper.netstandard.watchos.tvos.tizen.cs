using System;
using System.IO;

using iChronoMe.Core.Classes;

namespace iChronoMe.Tools
{
    public static partial class DrawableHelper
    {
        public static bool ResizeImage(string input, string output, int maxSize)
        {
            try
            {
                File.Copy(input, output, true);
                return true;
            }
            catch (Exception ex)
            {
                sys.LogException(ex);
                return false;
            }
        }
    }
}