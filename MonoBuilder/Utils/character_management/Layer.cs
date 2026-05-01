using MonoBuilder.Utils.interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace MonoBuilder.Utils.character_management
{
    public class Layer : INotifyPropertyChanged, INamedEntity
    {
        private int _id;
        private string _name;
        private int _index;
        private ObservableCollection<LayerAsset> _layers = new();

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

        public int Index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged(nameof(Index));
                }
            }
        }

        public ObservableCollection<LayerAsset> Layers
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

        public Layer(string name, int index, IEnumerable<LayerAsset>? layers = null)
        {
            _name = name;
            _index = index;

            if (layers != null)
            {
                _layers = new(layers);
            }
        }

        public bool AddLayer(LayerAsset layer)
        {
            if (_layers.FirstOrDefault(l => l.Name == layer.Name) is null)
            {
                _layers.Add(layer);
                return true;
            }

            return false;
        }

        public bool RemoveLayer(string layerName)
        {
            if (_layers.FirstOrDefault(l => l.Name == layerName) is LayerAsset layer)
            {
                return _layers.Remove(layer);
            }

            return false;
        }

        public LayerAsset? GetLayer(string layerName)
        {
            return _layers.FirstOrDefault(l => l.Name == layerName);
        }

        public LayerAsset? UpdateLayer(string layerName, LayerAsset layer)
        {
            if (_layers.FirstOrDefault(l => l.Name == layerName) is LayerAsset oldLayer)
            {
                int index = _layers.IndexOf(oldLayer);
                _layers.RemoveAt(index);
                _layers.Insert(index, layer);
                return layer;
            }

            return null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LayerAsset : INotifyPropertyChanged
    {
        private string _name;
        private string _layer;

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

        public string Layer
        {
            get => _layer;
            set
            {
                if (_layer != null)
                {
                    _layer = value;
                    OnPropertyChanged(nameof(Layer));
                }
            }
        }

        public LayerAsset(string name, string layer)
        {
            _name = name;
            _layer = layer;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
