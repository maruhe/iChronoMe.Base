using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

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

        private static Thread loaderThread = null;
        private static List<string> imagesToLoad = new List<string>();
        private static List<Action> actionsAfterLoad = new List<Action>();
        public static void AddImageToLoadQue(string cImageFilter, string cGroup, string cImageName, Action afterComplete = null)
        {
            try
            {
                string cID = cImageFilter + "/" + cGroup + "/" + cImageName;
                lock (imagesToLoad)
                {
                    if (imagesToLoad.Contains(cID))
                        return;
                    imagesToLoad.Add(cID);
                    if (afterComplete != null)
                        actionsAfterLoad.Add(afterComplete);

                    if (loaderThread != null)
                        return;

                    loaderThread = GetNewLoaderThread();
                    loaderThread.Start();
                }
            }
            catch (Exception ex)
            {
                sys.LogException(ex);
                imagesToLoad = new List<string>();
                actionsAfterLoad = new List<Action>();
                loaderThread = null;
            }
        }

        private static Thread GetNewLoaderThread()
            => new Thread(() =>
            {
                Thread.Sleep(150);
                try
                {
                    while (imagesToLoad.Count > 0)
                    {
                        string cImgId = null;
                        try
                        {
                            lock (imagesToLoad)
                            {
                                if (imagesToLoad.Count > 0)
                                    cImgId = imagesToLoad[0];
                            }
                            if (string.IsNullOrEmpty(cImgId))
                                return;

                            var x = cImgId.Split('/');
                            string cImageFilter = x[0];
                            string cImageGroup = x[1];
                            string cImageName = x[2];

                            string cFolder = Path.Combine(ImageLoader.GetImagePathThumb(cImageFilter), cImageGroup);
                            string cImagePath = Path.Combine(cFolder, cImageName);

                            WebClient webClient = new WebClient();
                            webClient.DownloadFile(Secrets.zAppDataUrl + "imageprev.php?id=" + cImageFilter + "/" + cImageGroup + "/" + cImageName, cImagePath + "_");

                            if (File.Exists(cImagePath))
                                File.Delete(cImagePath);
                            File.Move(cImagePath + "_", cImagePath);
                        }
                        catch
                        {

                        }
                        finally
                        {
                            lock (imagesToLoad)
                            {
                                if (imagesToLoad.Contains(cImgId))
                                    imagesToLoad.Remove(cImgId);
                            }
                        }
                    }
                }
                finally
                {
                    lock (imagesToLoad)
                    {
                        loaderThread = null;
                    }
                    Thread.Sleep(100);
                    lock (imagesToLoad)
                    {
                        if (imagesToLoad.Count > 0 && loaderThread == null)
                        {
                            sys.LogException(new Exception("ImageThread was stopped while an Image was added => restart loaderThread"));
                            loaderThread = GetNewLoaderThread();
                            loaderThread.Start();
                        }
                    }
                }
            });

        public static bool CheckImageThumbCache(IProgressChangedHandler handler, string imageFilter, int size = 150)
        {
            string cBasePath = GetImagePathThumb(imageFilter);
            try
            {
                handler.StartProgress(localize.ImageLoader_progress_title);
                string cImgList = sys.GetUrlContent(Secrets.zAppDataUrl + "filelist.php?filter=" + imageFilter + "&size=" + size).Result;

                if (string.IsNullOrEmpty(cImgList))
                    throw new Exception(localize.ImageLoader_error_list_unloadable);

                cImgList = cImgList.Trim().Replace("<br>", "").Replace("<BR>", "");

                if (!cImgList.StartsWith("group:") && !cImgList.StartsWith("path:"))
                    throw new Exception(localize.ImageLoader_error_list_broken);

                List<string> cLoadImgS = new List<string>();
                var list = cImgList.Split(new char[] { '\n' });

                string cGroup = "";
                string cFile = "";
                string cMd5 = "";
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
                    else if (cLine.StartsWith("md5:"))
                    {
                        cMd5 = cLine.Substring(cLine.IndexOf(" ") + 1);
                        try
                        {
                            if (!string.IsNullOrEmpty(cFile) && !string.IsNullOrEmpty(cMd5))
                            {
                                if (cFile.EndsWith(".png"))
                                {
                                    bool bLoadFile = true;
                                    string cLocal = Path.Combine(string.IsNullOrEmpty(cGroup) ? cBasePath : Path.Combine(cBasePath, cGroup), cFile);
                                    if (File.Exists(cLocal))
                                    {
                                        string cLocalMd5 = sys.CalculateFileMD5(cLocal);
                                        if (cMd5.Equals(cLocalMd5))
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
                        cFile = cMd5 = "";
                    }
                }

                int iSuccess = 0;
                if (cLoadImgS.Count > 0)
                {
                    handler.SetProgress(0, 0, sys.EzMzText(cLoadImgS.Count, localize.ImageLoader_progress_one_image, localize.ImageLoader_progress_n_images));

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
                            webClient.DownloadFile(Secrets.zAppDataUrl + "imageprev.php?filter=" + imageFilter + "&group=" + x[0] + "&image=" + x[1] + "&max=" + size, cDestPath + "_");

                            if (File.Exists(cDestPath))
                                File.Delete(cDestPath);
                            File.Move(cDestPath + "_", cDestPath);

                            iSuccess++;
                            handler.SetProgress(iSuccess, cLoadImgS.Count,
                                sys.EzMzText(cLoadImgS.Count, localize.ImageLoader_success_one_image, string.Format(localize.ImageLoader_success_n_images, iSuccess, cLoadImgS.Count)));

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
                handler.SetProgressDone();
            }
            catch (Exception e)
            {
                xLog.Error(e);
                handler.ShowToast(e.Message);
                handler.SetProgressDone();
                return false;
            }

            return true; ;
        }
    }
}