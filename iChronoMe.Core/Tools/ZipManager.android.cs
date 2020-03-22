using System;
using System.IO;

using Java.Util.Zip;

namespace iChronoMe.Core.Classes
{
    public static partial class ZipManager
    {
        static String _location;

        private static void DirChecker(String dir)
        {
            var file = new Java.IO.File(_location + dir);

            if (!file.IsDirectory)
            {
                file.Mkdirs();
            }
        }

        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
        {
            try
            {
                _location = destinationDirectoryName;
                if (!_location.EndsWith("/"))
                    _location += "/";
                var fileInputStream = new FileStream(sourceArchiveFileName, FileMode.Open);
                var zipInputStream = new ZipInputStream(fileInputStream);
                ZipEntry zipEntry = null;

                while ((zipEntry = zipInputStream.NextEntry) != null)
                {
                    xLog.Debug("UnZipping : " + zipEntry.Name);

                    if (zipEntry.IsDirectory)
                    {
                        DirChecker(zipEntry.Name);
                    }
                    else
                    {
                        var fileOutputStream = new Java.IO.FileOutputStream(_location + zipEntry.Name);

                        for (int i = zipInputStream.Read(); i != -1; i = zipInputStream.Read())
                        {
                            fileOutputStream.Write(i);
                        }

                        zipInputStream.CloseEntry();
                        fileOutputStream.Close();
                    }
                }
                zipInputStream.Close();
            }
            catch (Exception ex)
            {
                xLog.Error(ex);
            }
        }
    }
}
