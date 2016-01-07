using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Goes.Model
{
    public class DirectionSQL
    {
        public string r_id { get; set; }
        public string d_id { get; set; }
        public string number { get; set; }
        public string name { get; set; }
    }

    public class DirectionStopSQL
    {
        public string s_id { get; set; }
        public string n_id { get; set; }
        public string d_id { get; set; }
        public string name { get; set; }
        public string days { get; set; }
        public string schedule { get; set; }
    }
}
