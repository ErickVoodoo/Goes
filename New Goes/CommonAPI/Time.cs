using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Goes.CommonAPI
{
    public class Time
    {

        public List<List<TimeView>> getScheduleList(string days, string schedule)
        {
            List<List<TimeView>> result = new List<List<TimeView>>();
            string[] scheduleArray = schedule.Split('&');
            string[] daysArray = days.Split(',');
            int last_hour = -1;
            string minutes = "";
            for (int i = 0; i < daysArray.Length; i++)
            {
                List<TimeView> tempList = new List<TimeView>();
                if(int.Parse(daysArray[i]) != -1) {
                    string[] scheduleDayArray = scheduleArray[Int32.Parse(daysArray[i])].Split(',');
                    for (int j = 0; j < scheduleDayArray.Length; j++)
                    {
                        HM hm = getHourMinute(scheduleDayArray[j]);
                        if(last_hour == -1) {
                            last_hour = hm.hour;
                        }

                        if (hm.hour != last_hour || j == (scheduleDayArray.Length))
                        {
                            tempList.Add(new TimeView()
                            {
                                hour = last_hour.ToString(),
                                minute = minutes.Substring(0, minutes.Length - 1)
                            });
                            last_hour = hm.hour;
                            minutes = hm.minute.ToString() + ",";
                        }
                        else
                        {
                            minutes += hm.minute + ",";
                        }
                    }
                }
                result.Add(tempList);
            }

            return result;
        }

        public HM getHourMinute(string time)
        {
            HM hm = new HM();
            int timeInt = int.Parse(time);
            hm.hour = timeInt / 60;
            hm.minute = timeInt - hm.hour * 60;
            return hm;
        }

        public class TimeView {
            public string hour { get; set; }
            public string minute { get; set; }
            public string color { get; set; }
        }

        public class HM {
            public int hour { get; set;}
            public int minute { get; set; }
        }
    }
}
