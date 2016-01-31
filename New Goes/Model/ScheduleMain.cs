using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Goes.Model
{
    public class ScheduleSQL
    {
        public int type { get; set; }
        public double width { get; set; }
        public string days { get; set; }
        public string schedule { get; set; }
        public string number { get; set; }
        public string d_name { get; set; }
        public string s_name { get; set; }
        public int r_id { get; set; }
        public int d_id { get; set; }
        public int n_id { get; set; }
        public bool favorite { get; set; }
    }
}
