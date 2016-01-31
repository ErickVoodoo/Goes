using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Goes.Model
{
    public class DirectionSQL
    {
        public double width { get; set; }
        public int type { get; set; }
        public int r_id { get; set; }
        public int d_id { get; set; }
        public string number { get; set; }
        public string name { get; set; }
    }

    public class DirectionStopSQL : INotifyPropertyChanged
    {
        public int r_id { get; set; }
        public int n_id { get; set; }
        public int d_id { get; set; }
        public int type { get; set; }
        public string next_bus { get; set; }
        public double width { get; set; }
        public string number { get; set; }
        public string name { get; set; }
        public string days { get; set; }
        public string favorite { get; set; }
        public string schedule { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public string Favorite
        {
            get { return this.favorite; }

            set
            {
                if (value != this.favorite)
                {
                    this.favorite = value;
                    NotifyPropertyChanged("favorite");
                }
            }
        }

    }
}
