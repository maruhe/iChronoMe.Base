using System.IO.Compression;
using System.Text;

namespace iChronoMe.Core.Classes
{
    public static partial class ZipManager
    {

        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
            => ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName);

        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel compressionLevel, bool includeBaseDirectory)
            => ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, compressionLevel, includeBaseDirectory);

        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel compressionLevel, bool includeBaseDirectory, Encoding entryNameEncoding)
            => ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, compressionLevel, includeBaseDirectory, entryNameEncoding);

        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
            => ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName);

        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, Encoding entryNameEncoding)
            => ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, entryNameEncoding);

    }
}
