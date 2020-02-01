using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Core.Classes
{
    public class SelectPositionResult : dbObject
    {
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public interface SelectPositionReceiver
    {
        void ReceiveSelectedPosition(SelectPositionResult pos);
    }
}
