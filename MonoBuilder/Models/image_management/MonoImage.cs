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
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public string FileKey
        {
            get => _fileKey;
            set => SetProperty(ref _fileKey, value);
        }

        public bool IsSynced
        {
            get => _isSynced;
            set => SetProperty(ref _isSynced, value);
        }

        public MonoImage(string name, string path)
        {
            _name = name;
            _path = path;
        }
    }
}
