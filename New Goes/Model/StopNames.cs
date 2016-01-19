using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Goes.Model
{
    public class StopNameSQL
    {
        public string id { get; set; }
        public string name { get; set; }
        public double width { get; set; }
    }

    public class StopNameAllSQL
    {
        public string isBus { get; set; }
        public string isTroll { get; set; }
        public string isTramm { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public double width { get; set; }

    }
}
