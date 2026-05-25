using MonoBuilder.Models.generics.interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MonoBuilder.Models.image_management
{
    public class MonoImage : BaseViewModel, INamedEntity, IMultiFile
    {
        private int _id = -1;
		private string _name = string.Empty;
        private string _path = string.Empty;
        private string _fileKey = string.Empty;
        private bool _isSynced = false;

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

        public string Path
        {
            get => _path;
            set
            {
                if (SetProperty(ref _path, value))
                {
                    OnPropertyChanged(nameof(Path));
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
                    OnPropertyChanged(nameof(_fileKey));
                }
            }
        }

        public bool IsSynced
        {
            get => _isSynced;
            set
            {
                if (SetProperty(ref _isSynced, value))
                {
                    OnPropertyChanged(nameof(IsSynced));
                }
            }
        }

        public MonoImage(string name, string path)
        {
            _name = name;
            _path = path;
        }
    }
}
