using System;

using iChronoMe.Core.Classes;

using SQLite;

namespace iChronoMe.Core.DataModels
{
    public enum SpanType
    {
        Human = 10,
        Cat = 1010,
        Dog = 1020,
        Horse = 2010,
        Other = 9990
    };

    public class ChronoSpan : dbObject
    {
        public static ChronoSpan GetOrCreateCronoSpan(DateTime tDate, TimeSpan tTime, int iSpanID)
        {
            ChronoSpan spans = new ChronoSpan();
            if (iSpanID > 0)
            {
                var res = db.dbConfig.Query<ChronoSpan>("select * from CronoSpan where RecNo = ?", iSpanID);
                if (res.Count > 0)
                    spans = res[0];
            }
            spans.ChronoTime = tDate.Date + tTime;
            return spans;
        }

        public string Name { get; set; }

        //public SpanType Type { get; set; }

        public DateTime ChronoTime { get; set; }

        [Ignore]
        public DateTime ChronoTime_Date { get => ChronoTime.Date; set => ChronoTime = value.Date + ChronoTime.TimeOfDay; }
        [Ignore]
        public TimeSpan ChronoTime_Time { get => ChronoTime.TimeOfDay; set => ChronoTime = ChronoTime.Date + value; }
    }
}
