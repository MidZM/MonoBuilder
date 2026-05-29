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
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string ImageLayer
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public ObservableCollection<Layer> Layers
        {
            get => _layers;
            set => SetProperty(ref _layers, value);
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
