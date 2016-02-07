using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Goes.Model
{

    public class TitleObj
    {
        public string number { get; set; }
        public string direction { get; set; }
        public string name { get; set; }
        public string time_available { get; set; }
        public string interval { get; set; }
        public string alternative { get; set; }
    }

    public class ScheduleObj
    {
        public string number { get; set; }
        public string direction { get; set; }
        public string name { get; set; }
        public string time_available { get; set; }
        public string interval { get; set; }
        public string alternative { get; set; }
    }

    public class FBus
    {
        public TitleObj title { get; set; }
        public List<ScheduleObj> schedule { get; set; }
    }

    public class FBusList
    {
        public string number { get; set; }
        public string direction { get; set; }
        public string interval { get; set; }
        public string time_available { get; set; }
        public string alternative { get; set; }
        public string name { get; set; } 
        public string number_schedule { get; set; }
        public string direction_schedule { get; set; }
        public string interval_schedule { get; set; }
        public string alternative_schedule { get; set; }
        public string time_available_schedule { get; set; }
        public string name_schedule { get; set; }
        public string isNameVisible { get; set; }
        public string isAltVisible { get; set; }
        public string isTimeAvVisible { get; set; }
        public double width { get; set; }
    }

    public class FBusSQL
    {
        public int id { get; set; }
        public string city { get; set; }
        public string json { get; set; }
    }
}
