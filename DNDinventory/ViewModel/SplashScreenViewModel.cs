using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDinventory.ViewModel
{
    public class SplashScreenViewModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public SplashScreenViewModel()
        {
            logger.Info(">< SplashScreenViewModel()");
            progress = 0;
        }

        private double progress;
        public double Progress
        {
            get 
            { 
                return progress; 
            }
            set 
            {
                if (progress != value)
                {
                    progress = value;
                    OnPropertyChange("Progress");
                }
            }
        }

        public Uri ImagePath
        {
            get { return new Uri("Images/SplashScreen.png", UriKind.Relative); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
