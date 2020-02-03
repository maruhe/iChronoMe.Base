﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using iChronoMe.Core.Interfaces;

namespace iChronoMe.Core.Classes
{
    public class ImageLoader
    {
        public const string filter_clockfaces = "clockface";

        static bool bDone;
        int iPrevSize = 150;
        public const string cUrlDir = "http://test.ichrono.me/_appdata/";

        public string GetImagePathThumb(string imageFilter)
        {
            string cPath = Path.Combine(sys.PathShare, "imgCache_" + imageFilter);
            if (!Directory.Exists(cPath))
                Directory.CreateDirectory(cPath);
            cPath = Path.Combine(cPath, "thumb_" + iPrevSize);
            if (!Directory.Exists(cPath))
                Directory.CreateDirectory(cPath);

            return cPath;
        }

        public bool CheckImageThumbCache(IProgressChangedHandler handler, string imageFilter)
        {
            string cBasePath = GetImagePathThumb(imageFilter);
            try
            {

                handler.StartProgress(ml.strings.ImageLoader_progress_title);
                string cImgList = sys.GetUrlContent(cUrlDir + "_imglist.php?filter=" + imageFilter + "&size=" + iPrevSize).Result;

                if (string.IsNullOrEmpty(cImgList))
                    throw new Exception(ml.strings.ImageLoader_error_list_unloadable);

                cImgList = cImgList.Trim().Replace("<br>", "").Replace("<BR>", "");

                if (!cImgList.StartsWith("path:"))
                    throw new Exception(ml.strings.ImageLoader_error_list_broken);

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
                                if (cFile.EndsWith(".png"))
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
                    handler.SetProgress(0, 0, sys.EzMzText(cLoadImgS.Count, ml.strings.ImageLoader_progress_one_image, ml.strings.ImageLoader_progress_n_images));

                    WebClient webClient = new WebClient();
                    int iImg = 0;
                    string cParentPath = Path.GetDirectoryName(cBasePath);
                    foreach (string cLoadImage in cLoadImgS)
                    {
                        if (bDone)
                            break;
                        try
                        {
                            iImg++;

                            string cDestPath = Path.Combine(cBasePath, cLoadImage);
                            webClient.DownloadFile(cUrlDir + "_imageprev.php?image=" + cLoadImage + "&max=" + iPrevSize, cDestPath + "_");

                            if (File.Exists(cDestPath))
                                File.Delete(cDestPath);
                            File.Move(cDestPath + "_", cDestPath);

                            try
                            {
                                if (File.Exists(Path.Combine(cParentPath, cLoadImage)))
                                    File.Delete(Path.Combine(cParentPath, cLoadImage));
                            }
                            catch { }

                            iSuccess++;
                            handler.SetProgress(iSuccess, cLoadImgS.Count,
                                sys.EzMzText(cLoadImgS.Count, ml.strings.ImageLoader_success_one_image, string.Format(ml.strings.ImageLoader_success_n_images, iSuccess, cLoadImgS.Count)));

#if DEBUG
                            if (iSuccess > 200)
                                break;
#endif
                        }
                        catch (Exception exLoad)
                        {
                            exLoad.ToString();
                        }
                    }
                }
                if (iSuccess == cLoadImgS.Count && "clockface".Equals(imageFilter))
                {
                    AppConfigHolder.MainConfig.LastCheckClockFaces = DateTime.Now;
                    AppConfigHolder.SaveMainConfig();
                }
                handler.SetProgressDone();
            }
            catch (Exception e)
            {
                xLog.Error(e);
                handler.ShowError(e.Message);
                return false;
            }

            return true; ;
        }
    }
}