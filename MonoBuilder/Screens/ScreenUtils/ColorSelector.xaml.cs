using ColorPicker;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MonoBuilder.Screens.ScreenUtils
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorSelector : Window
    {
        private string CharacterName { get; set; } = string.Empty;
        private string PickedColor { get; set; } = "#FF0000";
        public bool SelectedColor { get; set; } = false;

        public ColorSelector(string? name = "")
        {
            InitializeComponent();
            if (name != string.Empty && name != null)
            {
                CharacterName = name;
                Title = $"{CharacterName} | Color";
            }
        }

        public string GetSelectedColor()
        {
            return PickedColor;
        }

        public void SetSelectedColor(string color)
        {
            PickedColor = color;
            if (color != string.Empty)
            {
                ColorPickerElm.SelectedColor = (Color)ColorConverter.ConvertFromString(color);
            }
        }

        private void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            var color = ColorPickerElm.SelectedColor.ToString();

            SelectedColor = true;
            PickedColor = color[0] + color[3..];
            Close();
        }

        private void NoColor_Click(object sender, RoutedEventArgs e)
        {
            SelectedColor = true;
            PickedColor = string.Empty;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
