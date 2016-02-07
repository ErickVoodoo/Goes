using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Goes.Model
{

    public class TaxiItem
    {
        public string name { get; set; }
        public string description { get; set; }
        public string place { get; set; }
        public List<object> phone { get; set; }
    }

    public class TaxiObj
    {
        public List<TaxiItem> taxi { get; set; }
    }

    public class PhoneObj
    {
        public string phone { get; set; }
        public string name { get; set; }
    }

    public class TaxiList
    {
        public string name { get; set; }
        public string description { get; set; }
        public string phone { get; set; }
        public string place { get; set; }
        public string place_schedule { get; set; }
        public string name_schedule { get; set; }
        public string description_schedule { get; set; }
        public List<PhoneObj> phone_schedule { get; set; }
        public double width { get; set; }
    }

    public class TaxiSQL
    {
        public int id { get; set; }
        public string city { get; set; }
        public string json { get; set; }
    }
}
