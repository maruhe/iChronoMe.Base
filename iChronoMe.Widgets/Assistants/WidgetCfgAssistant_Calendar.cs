using System;
using System.Collections.Generic;

using iChronoMe.Core.DynamicCalendar;
using iChronoMe.Core.Interfaces;
using iChronoMe.Core.Types;

namespace iChronoMe.Widgets
{
    public static class WidgetCfgAssistantData_Calendar
    {
        public static DynamicCalendarModel CalendarModel { get; set; } = CalendarModelCfgHolder.BaseGregorian;
        public static List<WidgetCfg_Calendar> DeletedWidgets { get; set; } = new List<WidgetCfg_Calendar>();
    }

    public class WidgetCfgAssistant_Calendar_Start : WidgetConfigAssistant<WidgetCfg_Calendar>
    {
        public WidgetCfgAssistant_Calendar_Start(WidgetCfgSample<WidgetCfg_Calendar> baseSample)
        {
            Title = localize.CalendarType;

            //if (baseSample.WidgetConfig != null)
            //  Samples.Add(new WidgetCfgSample<WidgetCfg_Calendar>(localize.current, baseSample.GetConfigClone()));
            string c = localize.WidgetTitleAgenda;
            c = localize.WidgetTitleCircleWave;
            c = localize.WidgetTitleMonthView;
            c = localize.current;
            Samples.Add(new WidgetCfgSample<WidgetCfg_Calendar>(localize.WidgetTitleAgenda, new WidgetCfg_CalendarTimetable()));
            Samples.Add(new WidgetCfgSample<WidgetCfg_Calendar>(localize.WidgetTitleCircleWave, new WidgetCfg_CalendarCircleWave()));
            Samples.Add(new WidgetCfgSample<WidgetCfg_Calendar>(localize.WidgetTitleMonthView, new WidgetCfg_CalendarMonthView()));
            //if (WidgetCfgAssistantData_Calendar.DeletedWidgets.Count > 0)
            //  Samples.Add(new WidgetCfgSample<WidgetCfg_Calendar>(localize.DeletedWidgets, null));

            NextStepAssistantType = typeof(WidgetCfgAssistant_Calendar_Theme);
        }
    }

    public class WidgetCfgAssistant_Calendar_Theme : WidgetConfigAssistant<WidgetCfg_Calendar>
    {
        public WidgetCfgAssistant_Calendar_Theme(WidgetCfgSample<WidgetCfg_Calendar> baseSample)
        {
            Title = localize.Theme;

            foreach (var o in Enum.GetValues(typeof(WidgetTheme)))
            {
                var cfg = baseSample.GetConfigClone();
                cfg.SetTheme((WidgetTheme)o);
                Samples.Add(new WidgetCfgSample<WidgetCfg_Calendar>(o.ToString(), cfg));
            }

            NextStepAssistantType = null;
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_Calendar> sample)
        {
            base.AfterSelect(handler, sample);
        }
    }

    public class WidgetCfgAssistant_CalendarCircleWave_OptionsBase : WidgetConfigAssistant<WidgetCfg_CalendarCircleWave>
    {
        public WidgetCfgAssistant_CalendarCircleWave_OptionsBase(WidgetCfgSample<WidgetCfg_CalendarCircleWave> baseSample)
        {
            Title = localize.title_EditWidget;
            BaseSample = baseSample;

            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.action_SaveAndQuit, BaseSample.GetConfigClone()));

            var cfg = BaseSample.GetConfigClone();
            var cfgPrev = BaseSample.GetConfigClone();
            if (cfgPrev.TimeUnit == TimeUnit.Month)
            {
                cfgPrev.TimeUnit = TimeUnit.Week;
                cfgPrev.TimeUnitCount = 3;
            }
            else
            {
                cfgPrev.TimeUnit = TimeUnit.Month;
                cfgPrev.TimeUnitCount = 3;
            }
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.TimeSpan, null, cfg, typeof(WidgetCfgAssistant_CalendarCircleWave_Length), cfgPrev));

            cfg = BaseSample.GetConfigClone();
            cfgPrev = BaseSample.GetConfigClone();
            var clrs = DynamicColors.SampleColorSetS[5];
            cfgPrev.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(cfg.TimeUnit, new xColor[] { xColor.HotPink }) } };
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.DayColorType, null, cfg, typeof(WidgetCfgAssistant_CalendarCircleWave_DayColorType), cfgPrev));

            cfg = BaseSample.GetConfigClone();
            cfgPrev = BaseSample.GetConfigClone();
            cfgPrev.ColorTodayBackground = xColor.HotPink;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.TodayColor, null, cfg, typeof(WidgetCfgAssistant_CalendarCircleWave_TodayDayColor), cfgPrev));

            NextStepAssistantType = null;
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_CalendarCircleWave> sample)
        {
            base.AfterSelect(handler, sample);

            NextStepAssistantType = (sample.Tag as Type);
        }
    }

    public class WidgetCfgAssistant_CalendarCircleWave_Length : WidgetConfigAssistant<WidgetCfg_CalendarCircleWave>
    {
        public WidgetCfgAssistant_CalendarCircleWave_Length(WidgetCfgSample<WidgetCfg_CalendarCircleWave> baseSample)
        {
            Title = localize.TimeSpan;
            BaseSample = baseSample;

            var cfg = baseSample.GetConfigClone();
            cfg.FirstDayType = FirstDayType.WeekStart;
            cfg.TimeUnit = TimeUnit.Week;
            cfg.TimeUnitCount = 2;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.twoWeeks, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.FirstDayType = FirstDayType.WeekStart;
            cfg.TimeUnit = TimeUnit.Week;
            cfg.TimeUnitCount = 3;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.threeWeeks, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.FirstDayType = FirstDayType.MonthStart;
            cfg.TimeUnit = TimeUnit.Month;
            cfg.TimeUnitCount = 1;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.oneMonth, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.FirstDayType = FirstDayType.MonthStart;
            cfg.TimeUnit = TimeUnit.Month;
            cfg.TimeUnitCount = 2;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.twoMonths, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.FirstDayType = FirstDayType.MonthStart;
            cfg.TimeUnit = TimeUnit.Month;
            cfg.TimeUnitCount = 3;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.threeMonths, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.FirstDayType = FirstDayType.MonthStart;
            cfg.TimeUnit = TimeUnit.Month;
            cfg.TimeUnitCount = WidgetCfgAssistantData_Calendar.CalendarModel.GetMonthsOfYear(WidgetCfgAssistantData_Calendar.CalendarModel.GetYearFromUtcDate(DateTime.Now).Year) / 2;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(cfg.TimeUnitCount + " " + localize.Months, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.FirstDayType = FirstDayType.YearStart;
            cfg.TimeUnit = TimeUnit.Year;
            cfg.TimeUnitCount = 1;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.oneYear, cfg));

            NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_OptionsBase);
        }
    }

    public class WidgetCfgAssistant_CalendarCircleWave_DayColorType : WidgetConfigAssistant<WidgetCfg_CalendarCircleWave>
    {
        public WidgetCfgAssistant_CalendarCircleWave_DayColorType(WidgetCfgSample<WidgetCfg_CalendarCircleWave> baseSample)
        {
            Title = localize.DayColorType;
            AllowCustom = true;
            BaseSample = baseSample;

            var clr = xColor.FromHex(DynamicColors.SampleColorSetS[5][2]);

            var cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(cfg.TimeUnit, Core.DynamicCalendar.GradientType.StaticColor) { CustomColors = new xColor[] { clr } } } };
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.SingleColor, cfg));

            var clr1 = clr.AddLuminosity(.1);
            var clr2 = clr.AddLuminosity(-.1);

            cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(cfg.TimeUnit, new xColor[] { clr1, clr2 }) } };
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.DarkToLight, cfg));

            clr1 = clr.AddLuminosity(-.1);
            clr2 = clr.AddLuminosity(.1);

            cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(cfg.TimeUnit, new xColor[] { clr1, clr2 }) } };
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.LightToDark, cfg));

            var clrs = DynamicColors.SampleColorSetS[5];

            cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(cfg.TimeUnit, new xColor[] { clrs[0], clrs[1] }) } };
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.twoColors, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(cfg.TimeUnit, new xColor[] { clrs[0], clrs[1], clrs[2] }) } };
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.threeColors, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(cfg.TimeUnit, new xColor[] { clrs[0], clrs[1], clrs[2], clrs[3] }) } };
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.fourColors, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(cfg.TimeUnit, new xColor[] { clrs[0], clrs[1], clrs[2], clrs[3], clrs[4] }) } };
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.fiveColors, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(TimeUnit.Month, Core.DynamicCalendar.GradientType.Rainbow) } };
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.aRainbowPerMonth, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(TimeUnit.Year, Core.DynamicCalendar.GradientType.Rainbow) } };
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.aRainbowPerYear, cfg));

            NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_DayColors);
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_CalendarCircleWave> sample)
        {
            base.AfterSelect(handler, sample);

            if (sample.WidgetConfig.DayBackgroundGradient.GradientS[0].CustomColors.Length == 0)
                NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_OptionsBase);
        }

        public override async void ExecCustom(IUserIO handler)
        {
            base.ExecCustom(handler);
            NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_OptionsBase);
            if (await handler.UserShowYesNoMessage(localize.DayColorType, localize.DayColorTypeCustomInfo))
            {
                List<xColor> xclrs = new List<xColor>();
                int iClr = 0;
                while (true)
                {
                    iClr++;

                    var clr = await handler.UserSelectColor("Color number " + iClr);

                    if (clr == null)
                        break;
                    else
                        xclrs.Add(clr.Value);
                }
                if (xclrs.Count > 0)
                {
                    var cfg = BaseSample.GetConfigClone();
                    cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(cfg.TimeUnit, xclrs.ToArray()) } };
                    BaseSample = new WidgetCfgSample<WidgetCfg_CalendarCircleWave>("custom", xclrs.ToArray(), cfg);
                }

                NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_CutomDayColorGradientTimeSpan);
                handler.TriggerPositiveButtonClicked();
            }
        }
    }

    public class WidgetCfgAssistant_CalendarCircleWave_CutomDayColorGradientTimeSpan : WidgetConfigAssistant<WidgetCfg_CalendarCircleWave>
    {
        public WidgetCfgAssistant_CalendarCircleWave_CutomDayColorGradientTimeSpan(WidgetCfgSample<WidgetCfg_CalendarCircleWave> baseSample)
        {
            Title = localize.DayColorsTimeSpan;
            BaseSample = baseSample;

            var cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient.GradientS[0].TimeUnit = TimeUnit.Week;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.perWeek, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient.GradientS[0].TimeUnit = TimeUnit.Month;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.perMonth, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.DayBackgroundGradient.GradientS[0].TimeUnit = TimeUnit.Year;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.perYear, cfg));

            NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_OptionsBase);
        }
    }

    public class WidgetCfgAssistant_CalendarCircleWave_DayColors : WidgetConfigAssistant<WidgetCfg_CalendarCircleWave>
    {
        public WidgetCfgAssistant_CalendarCircleWave_DayColors(WidgetCfgSample<WidgetCfg_CalendarCircleWave> baseSample)
        {
            Title = localize.DayColors;
            AllowCustom = true;
            ShowColors = true;
            BaseSample = baseSample;

            var cfgTemplate = baseSample.GetConfigClone();

            int iClrs = cfgTemplate.DayBackgroundGradient.GradientS[0].CustomColors.Length;

            int i1 = Math.Max(4 - iClrs, 0);
            int i2 = Math.Min(4, i1 + iClrs - 1);
            float nLum1 = 0;
            float nLum2 = 0;
            if (iClrs == 1 && cfgTemplate.DayBackgroundGradient.GradientS[0].CustomColors.Length == 2)
            {
                if (cfgTemplate.DayBackgroundGradient.GradientS[0].CustomColors[0].Luminosity > cfgTemplate.DayBackgroundGradient.GradientS[0].CustomColors[1].Luminosity)
                {
                    nLum1 = .1F;
                    nLum2 = -.1F;
                }
                else
                {
                    nLum1 = -.1F;
                    nLum2 = .1F;
                }
            }

            int i = 0;
            foreach (var clrs in DynamicColors.SampleColorSetS)
            {
                i++;
                var cfg = (WidgetCfg_CalendarCircleWave)cfgTemplate.Clone();
                List<xColor> xclrs = new List<xColor>();

                for (int iClr = i1; iClr <= i2; iClr++)
                {
                    xclrs.Add(xColor.FromHex(clrs[iClr]));
                }
                if (iClrs == 1 && nLum1 != 0)
                {
                    var clr1 = xclrs[0].AddLuminosity(nLum1);
                    var clr2 = xclrs[0].AddLuminosity(nLum2);
                    xclrs.Clear();
                    xclrs.Add(clr1);
                    xclrs.Add(clr2);
                }
                cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(cfg.TimeUnit, xclrs.ToArray()) } };
                Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.Sample + " " + i, cfg));

            }

            NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_OptionsBase);
        }

        public override async void ExecCustom(IUserIO handler)
        {
            base.ExecCustom(handler);
            NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_OptionsBase);
            if (await handler.UserShowYesNoMessage(localize.DayColorType, localize.DayColorTypeCustomInfo))
            {
                List<xColor> xclrs = new List<xColor>();
                int iClr = 0;
                while (true)
                {
                    iClr++;

                    var clr = await handler.UserSelectColor("Color number " + iClr);

                    if (clr == null)
                        break;
                    else
                        xclrs.Add(clr.Value);
                }
                if (xclrs.Count > 0)
                {
                    var cfg = BaseSample.GetConfigClone();
                    cfg.DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(cfg.TimeUnit, xclrs.ToArray()) } };
                    BaseSample = new WidgetCfgSample<WidgetCfg_CalendarCircleWave>("custom", xclrs.ToArray(), cfg);
                }

                NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_CutomDayColorGradientTimeSpan);
                handler.TriggerPositiveButtonClicked();
            }
        }
    }

    public class WidgetCfgAssistant_CalendarCircleWave_TodayDayColor : WidgetConfigAssistant<WidgetCfg_CalendarCircleWave>
    {
        public WidgetCfgAssistant_CalendarCircleWave_TodayDayColor(WidgetCfgSample<WidgetCfg_CalendarCircleWave> baseSample)
        {
            Title = localize.TodayColor;
            BaseSample = baseSample;
            AllowCustom = true;

            var cfgTemplate = baseSample.GetConfigClone();
            cfgTemplate.ColorTodayBackground = xColor.Transparent; //??
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(localize.NoHighlite, (WidgetCfg_CalendarCircleWave)cfgTemplate.Clone()));


            var cfg = (WidgetCfg_CalendarCircleWave)cfgTemplate.Clone();
            cfg.ColorTodayBackground = WidgetCfg.tcLight;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(WidgetCfg.tcLight.HexString, cfg));

            cfg = (WidgetCfg_CalendarCircleWave)cfgTemplate.Clone();
            cfg.ColorTodayBackground = WidgetCfg.tcDark;
            Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(WidgetCfg.tcDark.HexString, cfg));

            List<string> doneClrS = new List<string>();
            int i = 0;
            foreach (var clrs in DynamicColors.SampleColorSetS)
            {
                i++;
                var clr = xColor.FromHex(clrs[0]);
                if (!doneClrS.Contains(clr.HexString))
                {
                    doneClrS.Add(clr.HexString);
                    cfg = (WidgetCfg_CalendarCircleWave)cfgTemplate.Clone();
                    cfg.ColorTodayBackground = clr;
                    Samples.Add(new WidgetCfgSample<WidgetCfg_CalendarCircleWave>(clr.HexString, cfg));
                }
            }

            NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_OptionsBase);
        }

        public override async void ExecCustom(IUserIO handler)
        {
            base.ExecCustom(handler);
            NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_OptionsBase);
            var clr = await handler.UserSelectColor(localize.TodayColor);

            if (clr != null)
            {
                var cfg = BaseSample.GetConfigClone();
                cfg.ColorTodayBackground = clr.Value;
                BaseSample = new WidgetCfgSample<WidgetCfg_CalendarCircleWave>("custom", cfg);
            }

            NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_OptionsBase);
            handler.TriggerPositiveButtonClicked();
        }
    }
}