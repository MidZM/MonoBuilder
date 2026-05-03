using MonoBuilder.Utils.interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBuilder.Utils.character_management
{
    public class Sprite : INotifyPropertyChanged, INamedEntity
    {
        private int _id;
        private string _name;
        private string _image;
        private ObservableCollection<Layer> _layers = new();

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

        public string ImageLayer
        {
            get => _image;
            set
            {
                if (_image != value)
                {
                    _image = value;
                    OnPropertyChanged(nameof(ImageLayer));
                }
            }
        }

        public ObservableCollection<Layer> Layers
        {
            get => _layers;
            set
            {
                if (_layers != value)
                {
                    _layers = value;
                    OnPropertyChanged(nameof(Layers));
                }
            }
        }

        public Sprite(string name, string imageLayer, IEnumerable<Layer>? layers = null)
        {
            _name = name;
            _image = imageLayer;

            if (layers != null)
            {
                _layers = new(layers);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
