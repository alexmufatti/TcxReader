using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcxReader.data.xml
{
    public class Trackpoint
    {
        public DateTime Time { set; get; }
        public double AltitudeMeters { get; set; }
        public double DistanceMeters { get; set; }
        public int HeartRateBpm { get; set; }
        public int Cadence { get; set; }
        public string SensorState { get; set; }
        public List<Position> Positionx { get; set; }
    }
}
