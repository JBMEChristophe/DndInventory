using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DNDinventory.ViewModel
{
    public class SplashScreenViewModel : INotifyPropertyChanged
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public SplashScreenViewModel()
        {
            logger.Info(">< SplashScreenViewModel()");
            var randomGen = new Random();
            var images = Directory.GetFiles("Images/SplashScreen");
            var random = randomGen.Next(0, images.Length);
            Bitmap img = new Bitmap(images[random]);
            var color = img.GetPixel(randomGen.Next(img.Width), randomGen.Next(img.Height));
            ShadowColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
            ImagePath = Path.GetFullPath(images[random]);

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

        private System.Windows.Media.Color shadowColor;
        public System.Windows.Media.Color ShadowColor
        {
            get 
            { 
                return shadowColor; 
            }
            set 
            {
                if (shadowColor != value)
                {
                    shadowColor = value;
                    OnPropertyChange("ShadowColor");
                }
            }
        }

        string imagePath;
        public string ImagePath
        {
            get 
            {
                return imagePath; 
            }
            set
            {
                if (imagePath != value)
                {
                    imagePath = value;
                    OnPropertyChange("ImagePath");
                }
            }
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
