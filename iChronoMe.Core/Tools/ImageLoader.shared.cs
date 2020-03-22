using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using iChronoMe.Core.Interfaces;

namespace iChronoMe.Core.Classes
{
    public static class ImageLoader
    {
        public const string filter_clockfaces = "clockfaces";

        public static string GetImagePathThumb(string imageGroup)
        {
            string cPath = Path.Combine(sys.PathShare, "imgCache_" + imageGroup);
            if (!Directory.Exists(cPath))
                Directory.CreateDirectory(cPath);

            return cPath;
        }

        public static bool CheckImageThumbCache(IProgressChangedHandler handler, string imageFilter, int size = 150, bool bOnlyOnePerGroup = false, string cGroupFilter = null, Action<ImageLoadetEventArgs> imageLoadet = null)
        {
            string cBasePath = GetImagePathThumb(imageFilter);
            string cIndexPath = Path.Combine(cBasePath, "index");
            try
            {
                handler?.StartProgress(localize.ImageLoader_progress_title);
                string cImgList = string.Empty;

                if (!bOnlyOnePerGroup && File.Exists(cIndexPath) && File.GetLastWriteTime(cIndexPath).AddDays(3) > DateTime.Now)
                    cImgList = File.ReadAllText(cIndexPath);

                if (string.IsNullOrEmpty(cImgList))
                {
                    cImgList = sys.GetUrlContent(Secrets.zAppDataUrl + "filelist.php?filter=" + imageFilter + "&size=" + size).Result;

                    if (string.IsNullOrEmpty(cImgList))
                        throw new Exception(localize.ImageLoader_error_list_unloadable);

                    cImgList = cImgList.Trim().Replace("<br>", "").Replace("<BR>", "");

                    if (!cImgList.StartsWith("group:") && !cImgList.StartsWith("path:"))
                        throw new Exception(localize.ImageLoader_error_list_broken);
                }

                File.WriteAllText(cIndexPath, cImgList);

                List<string> cLoadImgS = new List<string>();
                var list = cImgList.Split(new char[] { '\n' });
                string cLastGroup = string.Empty;

                string cGroup = "";
                string cFile = "";
                string cMd5 = "";
                string cMd5Thumb = "";
                foreach (string cLine in list)
                {
                    if (cLine.StartsWith("group:"))
                    {
                        cGroup = cLine.Substring(cLine.IndexOf(" ") + 1);
                    }
                    else if (cLine.StartsWith("path:"))
                    {
                        cFile = cLine.Substring(cLine.IndexOf(" ") + 1);
                    }
                    else if (cLine.StartsWith("md5_thumb:"))
                    {
                        cMd5Thumb = cLine.Substring(cLine.IndexOf(" ") + 1);
                    }
                    else if (cLine.StartsWith("md5:"))
                    {
                        cMd5 = cLine.Substring(cLine.IndexOf(" ") + 1);
                        try
                        {
                            if (!string.IsNullOrEmpty(cFile) && !string.IsNullOrEmpty(cMd5))
                            {
                                if (cFile.EndsWith(".png"))
                                {
                                    if (bOnlyOnePerGroup && Equals(cLastGroup, cGroup))
                                        continue;
                                    cLastGroup = cGroup;
                                    if (!string.IsNullOrEmpty(cGroupFilter) && !Equals(cGroupFilter, cGroup))
                                        continue;

                                    bool bLoadFile = true;
                                    string cLocal = Path.Combine(string.IsNullOrEmpty(cGroup) ? cBasePath : Path.Combine(cBasePath, cGroup), cFile);
                                    if (File.Exists(cLocal))
                                    {
                                        string cLocalMd5 = sys.CalculateFileMD5(cLocal);
                                        if (cMd5.Equals(cLocalMd5) || cMd5Thumb.Equals(cLocalMd5))
                                            bLoadFile = false;
                                    }

                                    if (bLoadFile)
                                        cLoadImgS.Add(string.IsNullOrEmpty(cGroup) ? cFile : cGroup + "/" + cFile);
                                }
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        cGroup = cFile = cMd5Thumb = cMd5 = "";
                    }
                }

                int iSuccess = 0;
                if (cLoadImgS.Count > 0)
                {
                    handler?.SetProgress(0, 0, sys.EzMzText(cLoadImgS.Count, localize.ImageLoader_progress_one_image, localize.ImageLoader_progress_n_images));

                    WebClient webClient = new WebClient();
                    int iImg = 0;
                    foreach (string cLoadImage in cLoadImgS)
                    {
                        try
                        {
                            iImg++;

                            string cDestPath = Path.Combine(cBasePath, cLoadImage);
                            Directory.CreateDirectory(Path.GetDirectoryName(cDestPath));
                            var x = cLoadImage.Split('/');
                            string grp = x[0];
                            webClient.DownloadFile(Secrets.zAppDataUrl + "imageprev.php?filter=" + imageFilter + "&group=" + grp + "&image=" + x[1] + "&max=" + size, cDestPath + "_");

                            if (File.Exists(cDestPath))
                                File.Delete(cDestPath);
                            File.Move(cDestPath + "_", cDestPath);

                            iSuccess++;
                            handler?.SetProgress(iSuccess, cLoadImgS.Count,
                                sys.EzMzText(cLoadImgS.Count, localize.ImageLoader_success_one_image, string.Format(localize.ImageLoader_success_n_images, iSuccess, cLoadImgS.Count)));                            

                            imageLoadet?.Invoke(new ImageLoadetEventArgs(cDestPath, cLoadImgS.Count - iSuccess));
#if DEBUG
                            if (iSuccess >= 200)
                                break;
#endif
                        }
                        catch (Exception exLoad)
                        {
                            exLoad.ToString();
                        }
                    }
                }

                if (iSuccess == cLoadImgS.Count && filter_clockfaces.Equals(imageFilter))
                {
                    AppConfigHolder.MainConfig.LastCheckClockFaces = DateTime.Now;
                    AppConfigHolder.SaveMainConfig();
                }
                if (iSuccess == cLoadImgS.Count && !bOnlyOnePerGroup && string.IsNullOrEmpty(cGroupFilter) && File.Exists(cIndexPath))
                    File.Delete(cIndexPath);
                handler?.SetProgressDone();
                imageLoadet?.Invoke(new ImageLoadetEventArgs(null, 0));
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                xLog.Error(e);
                handler?.ShowToast(e.Message);
                handler?.SetProgressDone();
                imageLoadet?.Invoke(new ImageLoadetEventArgs(null, 0));
                return false;
            }

            return true;
        }
    }

    public class ImageLoadetEventArgs
    {
        public string ImagePath { get; }
        public int InQue { get; }

        public ImageLoadetEventArgs(string path, int que)
        {
            ImagePath = path;
            InQue = que;
        }
    }
}