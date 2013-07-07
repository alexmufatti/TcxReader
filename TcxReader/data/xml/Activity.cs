using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcxReader.data.xml
{
    public class Activity
    {
        public string Id { set; get; }
        public string Sport { set; get; }
        public List<Lap> Laps { set; get; }
    }
}
