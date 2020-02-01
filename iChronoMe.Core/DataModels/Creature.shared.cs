using iChronoMe.Core.Classes;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Core.DataModels
{
    public enum CreatureType
    {
        Human = 10,
        Cat = 1010,
        Dog = 1020,
        Horse = 2010,
        Other = 9990
    };

    public class Creature : dbObject
    {
        public static Creature GetOrCreateCreature(DateTime tDate, TimeSpan tTime, int iCreatureID)
        {
            Creature creature = new Creature();
            if (iCreatureID > 0)
            {
                var res = db.dbConfig.Query<Creature>("select * from Creature where RecNo = ?", iCreatureID);
                if (res.Count > 0)
                    creature = res[0];
            }
            creature.LifeStartTime = tDate.Date + tTime;
            return creature;
        }

        public string Name { get; set; }

        public CreatureType Type { get; set; }

        public DateTime LifeStartTime { get; set; }

        [Ignore]
        public DateTime LifeStartTime_Date { get => LifeStartTime.Date; set => LifeStartTime = value.Date + LifeStartTime.TimeOfDay; }
        [Ignore]
        public TimeSpan LifeStartTime_Time { get => LifeStartTime.TimeOfDay; set => LifeStartTime = LifeStartTime.Date + value; }
    }
}
