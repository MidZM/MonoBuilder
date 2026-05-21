using MonoBuilder.Models.generics.interfaces;
using System;

namespace MonoBuilder.Models.character_management
{
    public abstract class Character : BaseViewModel, INamedEntity, IMultiFile
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
                if (SetProperty(ref _id, value))
                {
                    OnPropertyChanged(nameof(EntityID));
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Tag
        {
            get => _tag;
            set
            {
                if (SetProperty(ref _tag, value))
                {
                    OnPropertyChanged(nameof(Tag));
                }
            }
        }

        public string? Color
        {
            get => _color;
            set
            {
                if (SetProperty(ref _color, value))
                {
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        public string? Directory
        {
            get => _directory;
            set
            {
                if (SetProperty(ref _directory, value))
                {
                    OnPropertyChanged(nameof(Directory));
                }
            }
        }

        public string FileKey
        {
            get => _fileKey;
            set
            {
                if (SetProperty(ref _fileKey, value))
                {
                    OnPropertyChanged(nameof(FileKey));
                }
            }
        }

        public bool IsSynced
        {
            get => _synced;
            set
            {
                if (SetProperty(ref _synced, value))
                {
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
    }
}
