using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBuilder.CustomComponents
{
    public class EnhancedRichTextBox : RichTextBox
    {
        private const int WM_ENABLE = 0x000A;

        private bool _wasReadOnly;
        private bool _isFakeDisabled;

        private Color _normalBackColor;
        private Color _disabledBackColor;

        private Color _normalForeColor;
        private Color _disabledForeColor;

        public EnhancedRichTextBox()
        {
            _normalBackColor = BackColor;
            _normalForeColor = ForeColor;
        }

        protected override void WndProc(ref Message m)
        {
            // If the OS tries to disable the control (e.g., because the Form is disabled),
            // we intercept the message and do nothing.
            if (m.Msg == WM_ENABLE)
            {
                bool shouldDisable = (m.WParam == IntPtr.Zero);

                if (shouldDisable)
                {
                    if (!_isFakeDisabled)
                    {
                        _wasReadOnly = this.ReadOnly;
                        _isFakeDisabled = true;

                        this.ReadOnly = true;
                        this.BackColor = _disabledBackColor;
                        this.ForeColor = _disabledForeColor;
                        this.TabStop = false;
                    }
                }
                else
                {
                    if (_isFakeDisabled)
                    {
                        _isFakeDisabled = false;

                        this.ReadOnly = _wasReadOnly;
                        this.BackColor = _normalBackColor;
                        this.ForeColor = _normalForeColor;
                        this.TabStop = true;
                    }
                }
                // Do not call base.WndProc(ref m);
                // This prevents the control from entering the 'Disabled' state visually.
                return;
            }

            base.WndProc(ref m);
        }

        public Color DisabledBackColor
        {
            get => _disabledBackColor;
            set => _disabledBackColor = value;
        }

        public Color DisabledForeColor
        {
            get => _disabledForeColor;
            set => _disabledForeColor = value;
        }

        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                if (!_isFakeDisabled)
                {
                    _normalBackColor = value;
                }
                base.BackColor = value;
            }
        }

        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                if (!_isFakeDisabled)
                {
                    _normalForeColor = value;
                }
                base.ForeColor = value;
            }
        }

        public void EnterFakeDisabledMode()
        {
            var m = new Message { Msg = WM_ENABLE, WParam = IntPtr.Zero };
            WndProc(ref m);
        }

        public void ExitFakeDisabledMode()
        {
            var m = new Message { Msg = WM_ENABLE, WParam = new IntPtr(1) };
            WndProc(ref m);
        }
    }
}
