using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using iChronoMe.Core.Classes;

namespace iChronoMe.Core.DynamicCalendar
{
    public class CalendarModelCfgHolder
    {
        static object oLock = new object();

        public static Dictionary<string, CalendarModelInfo> LastModelCfgList { get; private set; } = null;

        public static string CfgPath { get; } = sys.ConfigPathCalCfg;
        public static string CfgFile { get; } = Path.Combine(sys.ConfigPathCalCfg, "calendarmodelcfg.xml");

        public CalendarModelCfgHolder()
        {
            LoadListFromFile();
            LastModelCfgList = ModelCfgList;
        }

        string cDefaultId;
        Dictionary<string, CalendarModelInfo> ModelCfgList = null;

        public int ModelCount { get => ModelCfgList.Count; }

        public DynamicCalendarModel GetModelCfg(string cModelId, bool bGetDefaultIfNotFound = true)
        {
            lock (oLock)
            {
                if ("_baseGregorian".Equals(cModelId))
                    return BaseGregorian;
                if (!string.IsNullOrEmpty(cModelId) && ModelCfgList.ContainsKey(cModelId))
                {
                    try
                    {
                        string cCachedModel = DynamicCalendarModel.GetCachedModelVersion(cModelId);
                        if (!string.IsNullOrEmpty(cCachedModel) && cCachedModel.Equals(ModelCfgList[cModelId].VersionID))
                        {
                            var cache = DynamicCalendarModel.GetCachedModel(cModelId);
                            xLog.Debug("load model from cache: " + cache.Name + ", Version: " + cache.VersionID);
                            return cache;
                        }
                    }
                    catch (Exception ex)
                    {
                        xLog.Debug(ex, "load model from cache: " + cModelId);
                    }

                    try
                    {
                        var model = LoadModelFromFile(cModelId);
                        if (model != null)
                        {
                            DynamicCalendarModel.AddCachedModel(model);
                            xLog.Debug("load model from file: " + model.Name + ", Version: " + model.VersionID);
                            return model;
                        }
                    }
                    catch (Exception ex)
                    {
                        xLog.Debug(ex, "load model from file: " + cModelId);
                    }
                }
                if (bGetDefaultIfNotFound)
                    return GetDefaultModelCfg();
                return null;
            }
        }

        public DynamicCalendarModel GetEditableModelCfg(string cModelId)
        {
            xLog.Debug("load model editable: " + cModelId);

            if ("_baseGregorian".Equals(cModelId))
            {
                var model = DynamicCalendarModelAssistent.CreateModel(CalendarModelSample.Gregorian);
                model.EnterEditableMode();
                xLog.Debug("loaded model model editable: " + model.Name + ", Version: " + model.VersionID);
                return model;
            }

            if (!string.IsNullOrEmpty(cModelId) && ModelCfgList.ContainsKey(cModelId))
            {
                var model = LoadModelFromFile(cModelId);
                if (model == null)
                    model = DynamicCalendarModelAssistent.CreateModel(CalendarModelSample.Gregorian);
                model.EnterEditableMode();
                xLog.Debug("loaded model model editable: " + model.Name + ", Version: " + model.VersionID);

                return model;
            }
            return null;
        }

        public DynamicCalendarModel GetTemporaryModelCfg(string cModelId)
        {
            xLog.Debug("load model editable: " + cModelId);

            var model = LoadModelFromFile(cModelId);
            if (model != null)
            {
                model.EnterTemporaryMode();
                xLog.Debug("loaded model model editable: " + model.Name + ", Version: " + model.VersionID);
                return model;
            }

            model = DynamicCalendarModelAssistent.CreateModel(CalendarModelSample.Gregorian);
            model.EnterTemporaryMode();
            xLog.Debug("loaded model model editable: " + model.Name + ", Version: " + model.VersionID);
            return model;
        }

        public bool ConfigExists(string cModelId)
        {
            return ModelCfgList.ContainsKey(cModelId);
        }

        public List<String> AllIds()
        {
            return new List<string>(ModelCfgList.Keys);
        }

        public List<CalendarModelInfo> AllInfos()
        {
            var res = new List<CalendarModelInfo>(ModelCfgList.Values);
            return res;
        }

        public CalendarModelInfo GetModelInfo(string cModelId)
        {
            if (ModelCfgList.ContainsKey(cModelId))
                return ModelCfgList[cModelId];
            return null;
        }

        public void SetModelCfg(DynamicCalendarModel cfg)
        {
            if (string.IsNullOrEmpty(cfg.ModelID))
                return;

            if (ModelCfgList.ContainsKey(cfg.ModelID))
                ModelCfgList.Remove(cfg.ModelID);

            cfg.VersionID = Guid.NewGuid().ToString();
            ModelCfgList.Add(cfg.ModelID, CalendarModelInfo.FromCalendarModel(cfg, cfg.ModelID.Equals(cDefaultId)));
            CheckModelList();

            if (SaveModelToFile(cfg))
                SaveListToFile();
        }

        public void SetDefaultModelCfg(string cId)
        {
            if (string.IsNullOrEmpty(cId))
                return;

            cDefaultId = cId;
            foreach (var inf in ModelCfgList.Values)
                inf.IsDefault = inf.ID.Equals(cDefaultId);

            SaveListToFile();
        }

        public string GetDefaultModelId()
        {
            if (string.IsNullOrEmpty(cDefaultId))
                return GetDefaultModelCfg().ID;
            return cDefaultId;
        }

        static DynamicCalendarModel _baseGregorian = null;
        public static DynamicCalendarModel BaseGregorian
        {
            get
            {
                lock (oLock)
                {
                    if (_baseGregorian == null)
                    {
                        _baseGregorian = DynamicCalendarModelAssistent.CreateModel(CalendarModelSample.Gregorian);
                        _baseGregorian.Name = "Gregorian";
                        _baseGregorian.ID = "_baseGregorian";

                        DynamicCalendarModel.AddCachedModel(_baseGregorian);
                    }
                    return _baseGregorian;
                }
            }
        }

        public DynamicCalendarModel GetDefaultModelCfg()
        {
            if (!string.IsNullOrEmpty(cDefaultId) && (ModelCfgList.ContainsKey(cDefaultId)))
            {
                var ret = GetModelCfg(cDefaultId, false);
                if (ret != null)
                    return ret;
            }
            return BaseGregorian;
        }

        public void DeleteModel(string cModelId, bool bSaveToFile = true)
        {
            if (string.IsNullOrEmpty(cModelId))
                return;

            if (ModelCfgList.ContainsKey(cModelId))
                ModelCfgList.Remove(cModelId);
            if (cModelId.Equals(cDefaultId))
                cDefaultId = null;
            CheckModelList();
            SaveListToFile();
            try
            {
                File.Delete(Path.Combine(sys.PathDBcache, "calendar_model_cache_" + cModelId + ".db"));
            }
            catch { }
        }

        private void LoadListFromFile()
        {
            try
            {
                using (var stream = new StreamReader(CfgFile))
                {
                    var serializer = new XmlSerializer(typeof(List<CalendarModelInfo>));
                    var questionData = serializer.Deserialize(stream);
                    ModelCfgList = new Dictionary<string, CalendarModelInfo>();
                    foreach (var inf in (ICollection<CalendarModelInfo>)questionData)
                    {
                        ModelCfgList.Add(inf.ID, inf);
                        if (inf.IsDefault)
                            cDefaultId = inf.ID;
                    }
                    stream.Close();
                }
                ModelCfgList.ToString();
            }
            catch (Exception e)
            {
                e.ToString();
                ModelCfgList = new Dictionary<string, CalendarModelInfo>();
            }
            CheckModelList();
        }

        void CheckModelList()
        {
            bool bHasBaseGregoian = false;
            bool bHasCustomGregorian = false;
            foreach (var m in ModelCfgList.Values)
            {
                if (m.ID.Equals(BaseGregorian.ModelID))
                    bHasBaseGregoian = true;
                else if (CalendarModelSample.Gregorian.ToString().Equals(m.BaseSample))
                    bHasCustomGregorian = true;
            }
            if (bHasCustomGregorian && bHasBaseGregoian)
                ModelCfgList.Remove(BaseGregorian.ModelID);
            else if (!bHasCustomGregorian && !bHasBaseGregoian)
                ModelCfgList.Add(BaseGregorian.ModelID, CalendarModelInfo.FromCalendarModel(BaseGregorian, ModelCfgList.Count == 0));
        }

        private void SaveListToFile()
        {
            try
            {
                try
                {
                    if (File.Exists(CfgFile))
                        File.Copy(CfgFile, CfgFile + ".bak", true);
                }
                catch { }
                DateTime swStart = DateTime.Now;
                XmlSerializer x = new XmlSerializer(typeof(List<CalendarModelInfo>));
                TextWriter writer = new StreamWriter(CfgFile + ".new");
                List<CalendarModelInfo> data = AllInfos();
                x.Serialize(writer, data);
                writer.Flush();
                writer.Close();
                TimeSpan tsSera = DateTime.Now - swStart;
                swStart = DateTime.Now;

                File.Delete(CfgFile);
                File.Move(CfgFile + ".new", CfgFile);

                TimeSpan tsRepl = DateTime.Now - swStart;
            }
            catch (Exception e)
            {
                e.ToString();
                try
                {
                    if (System.IO.File.Exists(CfgFile + ".bak"))
                        System.IO.File.Copy(CfgFile + ".bak", CfgFile, true);
                }
                catch { }
            }
        }

        private DynamicCalendarModel LoadModelFromFile(string cID)
        {
            try
            {
#if DEBUG
                DateTime swStart = DateTime.Now;
                DynamicCalendarModel model1;
                using (var stream = new StreamReader(Path.Combine(CfgPath, cID)))
                {
                    var serializer = new SmoothXmlSerializer();
                    var questionData = serializer.Deserialize<DynamicCalendarModel>(stream);
                    stream.Close();
                    model1 = questionData;
                }
                TimeSpan tsParse1 = DateTime.Now - swStart;
                swStart = DateTime.Now;
                DynamicCalendarModel modelv2 = null;
                if (File.Exists(Path.Combine(CfgPath, cID + ".v2")))
                {
                    using (var stream = new StreamReader(Path.Combine(CfgPath, cID + ".v2")))
                    {
                        var serializer = new SmoothXmlSerializer();
                        var questionData = serializer.Deserialize<DynamicCalendarModel>(stream);
                        stream.Close();
                        modelv2 = questionData;
                    }
                }
                TimeSpan tsParsev2 = DateTime.Now - swStart;
                swStart = DateTime.Now;
#endif
                using (var stream = new StreamReader(Path.Combine(CfgPath, cID)))
                {
                    var serializer = new XmlSerializer(typeof(DynamicCalendarModel));
                    var questionData = serializer.Deserialize(stream);
                    stream.Close();

                    var model = questionData as DynamicCalendarModel;
                    if (string.IsNullOrEmpty(model.ID))
                        model.ID = cID;
                    if (string.IsNullOrEmpty(model.VersionID))
                        model.VersionID = Guid.NewGuid().ToString();

#if DEBUG
                    TimeSpan tsParseX = DateTime.Now - swStart;
                    swStart = DateTime.Now;
                    if (!sys.XmlEquals(model, model1))
                        this.ToString();
                    if (modelv2 != null && !sys.XmlEquals(model, modelv2))
                        this.ToString();
#endif

                    return model;
                }
            }
            catch (Exception e)
            {
                e.ToString();
                return null;
            }
        }

        private bool SaveModelToFile(DynamicCalendarModel model)
        {
            string cEditModeID = null;
            if (!string.IsNullOrEmpty(model.BackupID))
            {
                cEditModeID = model.ID;
                model.ID = model.BackupID;
            }
            string cCfgFile = Path.Combine(CfgPath, model.ID);
            try
            {
                DynamicCalendarModel.AddCachedModel(model);
                try
                {
                    if (File.Exists(cCfgFile))
                        File.Copy(cCfgFile, cCfgFile + ".bak", true);
                }
                catch { }
                DateTime swStart = DateTime.Now;
                XmlSerializer x = new XmlSerializer(typeof(DynamicCalendarModel));
                TextWriter writer = new StreamWriter(cCfgFile + ".new");
                x.Serialize(writer, model);
                writer.Flush();
                writer.Close();
                TimeSpan tsSera = DateTime.Now - swStart;

                swStart = DateTime.Now;

                SmoothXmlSerializer y = new SmoothXmlSerializer();
                writer = new StreamWriter(cCfgFile + ".v2");
                y.Serialize(writer, model);
                writer.Flush();
                writer.Close();

                TimeSpan tsSera2 = DateTime.Now - swStart;

                swStart = DateTime.Now;

                File.Delete(cCfgFile);
                File.Move(cCfgFile + ".new", cCfgFile);

                TimeSpan tsRepl = DateTime.Now - swStart;

                xLog.Debug("stored CalendarModel " + model.Name + ", " + model.ID + ", Version: " + model.VersionID);

                return true;
            }
            catch (Exception e)
            {
                e.ToString();
                try
                {
                    if (System.IO.File.Exists(cCfgFile + ".bak"))
                        System.IO.File.Copy(cCfgFile + ".bak", cCfgFile, true);
                }
                catch { }
            }
            finally
            {
                if (!string.IsNullOrEmpty(cEditModeID))
                    model.ID = cEditModeID;
            }
            return false;
        }
    }

    public class CalendarModelInfo
    {
        public static CalendarModelInfo FromCalendarModel(DynamicCalendarModel model, bool bIsDefault = false)
        {
            return new CalendarModelInfo { ID = model.ModelID, VersionID = model.VersionID, Name = model.Name, Description = model.Description, BaseSample = model.BaseSample, IsDefault = bIsDefault };
        }

        public string ID { get; set; }
        public string VersionID { get; set; }
        public bool IsDefault { get; set; }
        public string BaseSample { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}