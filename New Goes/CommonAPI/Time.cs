using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace New_Goes.CommonAPI
{
    public class Time
    {
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        public List<List<TimeView>> getScheduleList(double width, string days, string schedule)
        {
            List<List<TimeView>> result = new List<List<TimeView>>();
            string[] scheduleArray = schedule.Split('&');
            string[] daysArray = days.Split(',');
            int last_hour = -1;
            string minutes = "";
            for (int i = 0; i < daysArray.Length; i++)
            {
                List<TimeView> tempList = new List<TimeView>();
                last_hour = -1;
                minutes = "";
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
                                hour = last_hour - (last_hour/24) * 24,
                                width = width,
                                minute = minutes.Substring(0, minutes.Length - 1),
                            });
                            last_hour = hm.hour;
                            minutes = (hm.minute.ToString().Length == 1 ? "0" + hm.minute.ToString() : hm.minute.ToString()) + ",";
                        }
                        else
                        {
                            minutes += (hm.minute.ToString().Length == 1 ? "0" + hm.minute.ToString() : hm.minute.ToString()) + ",";
                        }
                    }
                }
                if(last_hour != -1)
                    tempList.Add(new TimeView()
                    {
                        hour = last_hour - (last_hour/24) * 24,
                        width = width,
                        minute = minutes.Substring(0, minutes.Length - 1),
                    });
                result.Add(tempList);
            }

            return result;
        }

        public string getNextBusTime(string schedule, string days)
        {
            int current_time = getUnixTime();
            int current_week_day = getWeekDay();
            int last_week_day = getLastWeekDay();

            int currentHour = DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 3 ? 24 + DateTime.Now.Hour : DateTime.Now.Hour;
            int currenMinute = DateTime.Now.Minute;

            string[] scheduleArray = schedule.Split('&');
            string[] daysArray = days.Split(',');
            string current_day = daysArray[getWeekDay()];

            if (int.Parse(daysArray[last_week_day]) != -1 && currentHour >= 24)
            {
                string[] scheduleDayArray = scheduleArray[Int32.Parse(daysArray[last_week_day])].Split(',');
                if (int.Parse(scheduleDayArray[scheduleDayArray.Length - 1]) > currentHour * 60 + currenMinute)
                    current_week_day = last_week_day;
            }

            if (current_week_day == getWeekDay())
                currentHour = DateTime.Now.Hour;

            if (int.Parse(daysArray[current_week_day]) != -1)
            {
                string[] scheduleDayArray = scheduleArray[Int32.Parse(daysArray[current_week_day])].Split(',');
                for (int j = 0; j < scheduleDayArray.Length; j++)
                {
                    if (scheduleDayArray[j] != "") { 
                        HM hm = getHourMinute(scheduleDayArray[j]);

                        if (hm.hour * 60 + hm.minute >= currentHour * 60 + currenMinute)
                        {
                            HM current = getHourMinute(((hm.hour * 60 + hm.minute) - (currentHour * 60 + currenMinute)).ToString());
                            return (current.hour == 0 ? "" : current.hour.ToString() + " ч.") + " " + current.minute + " м.";
                        }
                    }
                }
            }
            return this.resourceLoader.GetString("ScheduleFinished");
        }

        public static int getCurrentDaySchedule(string schedule, string days)
        {
            int current_week_day = getWeekDay();
            int last_week_day = getLastWeekDay();

            int currentHour = DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 3 ? 24 + DateTime.Now.Hour : DateTime.Now.Hour;
            int currenMinute = DateTime.Now.Minute;

            string[] scheduleArray = schedule.Split('&');
            string[] daysArray = days.Split(',');
            string current_day = daysArray[getWeekDay()];

            if (int.Parse(daysArray[last_week_day]) != -1 && currentHour >= 24)
            {
                string[] scheduleDayArray = scheduleArray[Int32.Parse(daysArray[last_week_day])].Split(',');
                if (int.Parse(scheduleDayArray[scheduleDayArray.Length - 1]) > currentHour * 60 + currenMinute)
                    return last_week_day;
            }
            return current_week_day;
        }

        public static int getWeekDay() 
        {
            DayOfWeek day = DateTime.Today.Date.DayOfWeek;
            if (DayOfWeek.Monday == day)
            {
                return 0;
            }
            else if (DayOfWeek.Tuesday == day)
            {
                return 1;
            }
            else if (DayOfWeek.Wednesday == day)
            {
                return 2;
            }
            else if (DayOfWeek.Thursday == day)
            {
                return 3;
            }
            else if (DayOfWeek.Friday == day)
            {
                return 4;
            }
            else if (DayOfWeek.Saturday == day)
            {
                return 5;
            }
            else if (DayOfWeek.Sunday == day)
            {
                return 6;
            }
            return 1;
        }

        public static int getLastWeekDay()
        {
            DayOfWeek day = DateTime.Today.Date.DayOfWeek;
            if (DayOfWeek.Monday == day)
            {
                return 6;
            }
            else if (DayOfWeek.Tuesday == day)
            {
                return 0;
            }
            else if (DayOfWeek.Wednesday == day)
            {
                return 1;
            }
            else if (DayOfWeek.Thursday == day)
            {
                return 2;
            }
            else if (DayOfWeek.Friday == day)
            {
                return 3;
            }
            else if (DayOfWeek.Saturday == day)
            {
                return 4;
            }
            else if (DayOfWeek.Sunday == day)
            {
                return 5;
            }
            return 6;
        }

        public Int32 getUnixTime()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
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
            public double width { get; set; }
            public int hour { get; set; }
            public string minute { get; set; }
            public string color { get; set; }
            public string cur_time { get; set; }
        }

        public class HM {
            public int hour { get; set;}
            public int minute { get; set; }
        }
    }
}
