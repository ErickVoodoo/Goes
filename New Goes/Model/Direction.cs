using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Goes.Model
{
    public class DirectionSQL
    {
        public int type { get; set; }
        public int r_id { get; set; }
        public int d_id { get; set; }
        public string number { get; set; }
        public string name { get; set; }
    }

    public class DirectionStopSQL
    {
        public int r_id { get; set; }
        public int n_id { get; set; }
        public int d_id { get; set; }
        public int type { get; set; }
        public string number { get; set; }
        public string name { get; set; }
        public string days { get; set; }
        public string favorite { get; set; }
        public string schedule { get; set; }
    }
}
