using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

using iChronoMe.Core.Interfaces;

namespace iChronoMe.Core.Classes
{
    public static class DataLoader
    {
        public const string filter_clockhands = "clockhands";

        private static Thread loaderThread = null;
        private static List<string> imagesToLoad = new List<string>();
        private static List<Action> actionsAfterLoad = new List<Action>();

        public static bool CheckDataPackage(IProgressChangedHandler handler, string dataFilter, string localPath, string title)
        {
            string cBasePath = localPath;
            try
            {
                handler.StartProgress(title);
                string cImgList = sys.GetUrlContent(Secrets.zAppDataUrl + "filelist.php?filter=" + dataFilter).Result;

                if (string.IsNullOrEmpty(cImgList))
                    throw new Exception(localize.DataLoader_error_list_unloadable);

                cImgList = cImgList.Trim().Replace("<br>", "").Replace("<BR>", "");

                if (!cImgList.StartsWith("group:") && !cImgList.StartsWith("path:"))
                    throw new Exception(localize.DataLoader_error_list_broken);

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
                                //if (cFile.EndsWith(".png"))
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
                            webClient.DownloadFile(Secrets.zAppDataUrl + dataFilter + "/" + cLoadImage, cDestPath + "_");

                            if (File.Exists(cDestPath))
                                File.Delete(cDestPath);
                            File.Move(cDestPath + "_", cDestPath);

                            iSuccess++;
                        }
                        catch (Exception exLoad)
                        {
                            exLoad.ToString();
                        }
                    }
                }
                handler.SetProgressDone();
                return iSuccess == cLoadImgS.Count;
            }
            catch (Exception e)
            {
                xLog.Error(e);
                handler.ShowToast(e.Message);
                handler.SetProgressDone();
                return false;
            }
        }
    }
}