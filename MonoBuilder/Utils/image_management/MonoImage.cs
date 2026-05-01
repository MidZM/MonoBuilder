using MonoBuilder.Utils.interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MonoBuilder.Utils.image_management
{
    public class MonoImage : INotifyPropertyChanged, INamedEntity, IMultiFile
    {
        private int _id = -1;
        private string _name = "";
        private string _path = "";
        private string _fileKey = "";
        private bool _synced = false;

        public int EntityID
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(EntityID));
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if ( _name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged(nameof(Path));
                }
            }
        }

        public string FileKey
        {
            get => _fileKey;
            set
            {
                if ( _fileKey != value)
                {
                    _fileKey = value;
                    OnPropertyChanged(nameof(_fileKey));
                }
            }
        }

        public bool IsSynced
        {
            get => _synced;
            set
            {
                if (_synced != value)
                {
                    _synced = value;
                    OnPropertyChanged(nameof(IsSynced));
                }
            }
        }

        public MonoImage(string name, string path)
        {
            _name = name;
            _path = path;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
