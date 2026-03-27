using System.Runtime.InteropServices;
using System.Windows.Forms;
using MonoBuilder.Utils;

namespace MonoBuilder.Screens.ScreenUtils
{
    internal static class RichTextBoxHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WM_SETREDRAW = 0x000B;

        /// <summary>
        /// Populates a <see cref="RichTextBox"/> with colored lines while suppressing
        /// all repaints until the operation is complete, avoiding per-line flicker.
        /// </summary>
        public static void DisplayFormattedLines(RichTextBox box, List<LineFormatInfo> lines)
        {
            box.Clear();

            // Suspend repainting at the Win32 level — SuspendLayout only suppresses
            // layout, not paint messages, so individual Select+SelectionColor calls
            // would still trigger a redraw on each iteration without this.
            if (box.IsHandleCreated)
                SendMessage(box.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

            try
            {
                foreach (var line in lines)
                {
                    int startPos = box.TextLength;
                    box.AppendText(line.Text + Environment.NewLine);

                    if (line.TextColor.HasValue)
                    {
                        int endPos = box.TextLength;
                        box.Select(startPos, endPos - startPos - Environment.NewLine.Length + 1);
                        box.SelectionColor = line.TextColor.Value;
                    }
                }

                box.Select(0, 0);
                box.ScrollToCaret();
            }
            finally
            {
                if (box.IsHandleCreated)
                {
                    SendMessage(box.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                    box.Invalidate();
                }
            }
        }
    }
}
