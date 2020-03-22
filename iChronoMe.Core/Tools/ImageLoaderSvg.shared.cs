using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using iChronoMe.Core.Interfaces;

namespace iChronoMe.Core.Classes
{
    public static class ImageLoaderSvg
    {
        public const string filter_clockfaces = "clockfaces";

        static bool bDone;

        public static string GetImagePathThumb(string imageGroup, int size = 150)
        {
            string cPath = Path.Combine(sys.PathShare, "imgCache_" + imageGroup);
            if (!Directory.Exists(cPath))
                Directory.CreateDirectory(cPath);
            return cPath;
        }

        public static bool CheckImageThumbCache(IProgressChangedHandler handler, string imageFilter, int size = 150, bool bOnlyOnePerGroup = false, string cGroupFilter = null, Action<ImageLoadetEventArgs> imageLoadet = null)
        {
            string cBasePath = GetImagePathThumb(imageFilter, size);
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

                string cFile = "";
                string cMd5 = "";
                foreach (string cLine in list)
                {
                    if (cLine.StartsWith("path:"))
                    {
                        cFile = cLine.Substring(cLine.IndexOf(" ") + 1);
                    }
                    else if (cLine.StartsWith("md5:"))
                    {
                        cMd5 = cLine.Substring(cLine.IndexOf(" ") + 1);
                        try
                        {
                            if (!string.IsNullOrEmpty(cFile) && !string.IsNullOrEmpty(cMd5))
                            {
                                if (cFile.EndsWith(".zip"))
                                {
                                    bool bLoadFile = true;
                                    if (File.Exists(Path.Combine(cBasePath, cFile)))
                                    {
                                        string cLocalMd5 = sys.CalculateFileMD5(Path.Combine(cBasePath, cFile));
                                        if (cMd5.Equals(cLocalMd5))
                                            bLoadFile = false;
                                    }

                                    if (bLoadFile)
                                        cLoadImgS.Add(cFile);
                                }
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        cFile = cMd5 = "";
                    }
                }

                int iSuccess = 0;
                if (cLoadImgS.Count > 0)
                {
                    handler.SetProgress(0, 0, sys.EzMzText(cLoadImgS.Count, localize.ImageLoader_progress_one_package, localize.ImageLoader_progress_n_packages));

                    WebClient webClient = new WebClient();
                    int iImg = 0;
                    foreach (string cLoadImage in cLoadImgS)
                    {
                        if (bDone)
                            break;
                        try
                        {
                            iImg++;

                            string cDestPath = Path.Combine(cBasePath, cLoadImage);
                            webClient.DownloadFile(Secrets.zAppDataUrl + imageFilter + "/" + cLoadImage, cDestPath + "_");

                            if (File.Exists(cDestPath))
                                File.Delete(cDestPath);
                            File.Move(cDestPath + "_", cDestPath);

                            iSuccess++;
                            handler.SetProgress(iSuccess, cLoadImgS.Count,
                                sys.EzMzText(cLoadImgS.Count, localize.ImageLoader_success_one_package, string.Format(localize.ImageLoader_success_n_packages, iSuccess, cLoadImgS.Count)));

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

                    handler.SetProgress(0, 0, "unpacking...");

                    foreach (string cOldDir in Directory.GetDirectories(cBasePath))
                        try { Directory.Delete(cOldDir, true); } catch { }

                    foreach (string cPackage in Directory.GetFiles(cBasePath, "*.zip"))
                        ZipManager.ExtractToDirectory(cPackage, cBasePath);
                }
                handler.SetProgressDone();
            }
            catch (Exception e)
            {
                xLog.Error(e);
                handler.ShowToast(e.Message);
                return false;
            }

            return true; ;
        }
    }
}