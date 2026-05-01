using MonoBuilder.Utils.interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBuilder.Utils.character_management
{
    public abstract class Character : INotifyPropertyChanged, INamedEntity, IMultiFile
    {
        private int _id;
        private string _name = "";
        private string _tag = "";
        private string? _color;
        private string? _directory;
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
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Tag
        {
            get => _tag;
            set
            {
                if (_tag != value)
                {
                    _tag = value;
                    OnPropertyChanged(nameof(Tag));
                }
            }
        }

        public string? Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        public string? Directory
        {
            get => _directory;
            set
            {
                if (_directory != value)
                {
                    _directory = value;
                    OnPropertyChanged(nameof(Directory));
                }
            }
        }

        public string FileKey
        {
            get => _fileKey;
            set
            {
                if (_fileKey != value)
                {
                    _fileKey = value;
                    OnPropertyChanged(nameof(FileKey));
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

        public Character(string name, string tag)
        {
            Name = name;
            Tag = tag;
        }

        public Character(string name, string tag, string colorOrDirectory)
        {
            Name = name;
            Tag = tag;
            if (colorOrDirectory.StartsWith("#"))
            {
                Color = colorOrDirectory;
            }
            else
            {
                Directory = colorOrDirectory;
            }
        }

        public Character(string name, string tag, string? color, string? directory)
        {
            Name = name;
            Tag = tag;
            if (color != null) Color = color;
            if (directory != null) Directory = directory;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
