using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBuilder.Utils
{
    public class ScreenTransitioner
    {
        private int Width { get; set; } = 50;
        private int Height { get; set; } = 50;
        private string Text { get; set; } = "Loading...";
        private Font Font { get; set; } = new Font(FontFamily.GenericSansSerif, 15, FontStyle.Bold);
        private Color ForeColor { get; set; } = Color.White;
        private Color BackColor { get; set; } = Color.FromArgb(20, 20, 20);
        private Point Location { get; set; }

        public KryptonPanel? Panel { get; set; }
        public KryptonLabel? Label { get; set; }

        /// <param name="location">Position of the loading panel on the parent.</param>
        /// <param name="width">Panel width added on top of the label's natural width. Defaults to 50.</param>
        /// <param name="height">Panel height. Defaults to 50.</param>
        /// <param name="text">Loading text displayed in the panel. Defaults to "Loading...".</param>
        /// <param name="font">Font used for the loading label. Defaults to GenericSansSerif 15pt Bold.</param>
        /// <param name="foreColor">Label text color. Defaults to White.</param>
        /// <param name="backColor">Panel background color. Defaults to RGB(20,20,20).</param>
        public ScreenTransitioner(
            Point location,
            int width = 50,
            int height = 50,
            string text = "Loading...",
            Font? font = null,
            Color? foreColor = null,
            Color? backColor = null)
        {
            Location = location;
            Width = width;
            Height = height;
            Text = text;
            Font = font ?? new Font(FontFamily.GenericSansSerif, 15, FontStyle.Bold);
            ForeColor = foreColor ?? Color.White;
            BackColor = backColor ?? Color.FromArgb(20, 20, 20);
        }

        public KryptonPanel Show()
        {
            var panel = new KryptonPanel();
            var intLabel = new KryptonLabel();

            intLabel.Text = Text;
            intLabel.StateCommon.ShortText.Font = Font;
            intLabel.StateCommon.ShortText.Color1 = ForeColor;
            intLabel.StateCommon.ShortText.TextH = PaletteRelativeAlign.Center;
            intLabel.StateCommon.ShortText.TextV = PaletteRelativeAlign.Center;
            intLabel.Dock = DockStyle.Fill;

            panel.StateCommon.Color1 = BackColor;
            panel.Width = intLabel.Width + Width;
            panel.Height = Height;
            panel.Location = Location;

            Panel = panel;
            Label = intLabel;

            panel.Controls.Add(intLabel);

            return panel;
        }

        public void Hide()
        {
            Panel?.Dispose();
            Label?.Dispose();

            Panel = null;
            Label = null;
        }
    }
}
