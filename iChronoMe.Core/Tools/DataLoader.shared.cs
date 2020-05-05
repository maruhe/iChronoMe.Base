using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using iChronoMe.Core.Interfaces;

namespace iChronoMe.Core.Classes
{
    public static class DataLoader
    {
        public const string filter_clockhands = "clockhands";

        public static bool CheckDataPackage(IProgressChangedHandler handler, string dataFilter, string localPath, string title)
        {
            string cBasePath = localPath;
            try
            {
                handler.StartProgress(title);
                string cDataList = sys.GetUrlContent(Secrets.zAppDataUrl + "filelist.php?filter=" + dataFilter).Result;

                if (string.IsNullOrEmpty(cDataList))
                    cDataList = sys.GetUrlContent(Secrets.zAppDataUrl + "filelist.php?filter=" + dataFilter).Result;

                if (string.IsNullOrEmpty(cDataList))
                    throw new Exception(localize.DataLoader_error_list_unloadable);

                cDataList = cDataList.Trim().Replace("<br>", "").Replace("<BR>", "");

                if (!cDataList.StartsWith("group:") && !cDataList.StartsWith("path:"))
                    throw new Exception(localize.DataLoader_error_list_broken);

                List<string> cLoadDataS = new List<string>();
                var list = cDataList.Split(new char[] { '\n' });

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
                                {
                                    bool bLoadFile = true;
                                    string cLocal = Path.Combine(string.IsNullOrEmpty(cGroup) ? cBasePath : Path.Combine(cBasePath, cGroup), cFile);
                                    if (File.Exists(cLocal))
                                    {
                                        string cLocalMd5 = sys.CalculateFileMD5(cLocal);
                                        if (cMd5.Equals(cLocalMd5))
                                        {
                                            bLoadFile = false;
                                            try
                                            {
                                                File.SetLastWriteTime(cLocal, DateTime.Now);
                                            }
                                            catch (Exception ex)
                                            {
                                                sys.LogException(ex);
                                            }
                                        }
                                    }

                                    if (bLoadFile)
                                        cLoadDataS.Add(string.IsNullOrEmpty(cGroup) ? cFile : cGroup + "/" + cFile);
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
                if (cLoadDataS.Count > 0)
                {
                    WebClient webClient = new WebClient();

                    int iData = 0;
                    foreach (string cLoadData in cLoadDataS)
                    {
                        try
                        {
                            iData++;

                            string cDestPath = Path.Combine(cBasePath, cLoadData);
                            Directory.CreateDirectory(Path.GetDirectoryName(cDestPath));
                            var x = cLoadData.Split('/');
                            webClient.DownloadFile(Secrets.zAppDataUrl + dataFilter + "/" + cLoadData, cDestPath + "_");

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
                return iSuccess == cLoadDataS.Count;
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