using MonoBuilder.Models.generics.interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace MonoBuilder.Models.character_management
{
	public class Layer : BaseViewModel, INamedEntity
    {
        private int _id;
        private string _name;
        private int _index;
        private ObservableCollection<LayerAsset> _layers = new();

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

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public ObservableCollection<LayerAsset> Layers
        {
            get => _layers;
            set => SetProperty(ref _layers, value);
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
    }

    public class LayerAsset : BaseViewModel
    {
        private string _name;
        private string _layer;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Layer
        {
            get => _layer;
            set => SetProperty(ref _layer, value);
        }

        public LayerAsset(string name, string layer)
        {
            _name = name;
            _layer = layer;
        }
    }
}
