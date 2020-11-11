using DNDinventory.SocketFileTransfer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDinventory.Model
{
    public enum TransferState
    {
        Running,
        Paused,
        Stopped,
        Completed
    }

    public class Transfer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string id;
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChange("Id");
                }
            }
        }

        private string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    OnPropertyChange("FileName");
                }
            }
        }

        private string type;
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChange("Type");
                }
            }
        }

        private string progress;
        public string Progress 
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

        private TransferState state;
        public TransferState State
        {
            get
            {
                return state;
            }
            set
            {
                if (state != value)
                {
                    state = value;
                    OnPropertyChange("State");
                }
            }
        }

        public TransferQueue Queue { get; set; }

        protected void OnPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
