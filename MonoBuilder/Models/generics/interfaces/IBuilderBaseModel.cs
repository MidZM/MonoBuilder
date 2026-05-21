using System;
using System.Windows;

namespace MonoBuilder.Models.generics.interfaces
{
    public interface IBuilderBaseModel
    {
		public Window Owner { get; set; }
		protected AppSettings ApplicationSettings { get; set; }
    }
}
