using MonoBuilder.Models.generics.interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBuilder.Models.character_management
{
    public class Sprite : BaseViewModel, INamedEntity
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

        public string ImageLayer
        {
            get => _image;
            set
            {
                if (SetProperty(ref _image, value))
                {
                    OnPropertyChanged(nameof(ImageLayer));
                }
            }
        }

        public ObservableCollection<Layer> Layers
        {
            get => _layers;
            set
            {
                if (SetProperty(ref _layers, value))
                {
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
    }
}
