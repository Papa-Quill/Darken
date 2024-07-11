using Darken.Properties;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Darken
{
    public partial class Darken : Form
    {
        #region Variables
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x20;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private Timer bringToFrontTimer;
        private NotifyIcon notifyIcon;
        private Timer closeTimer;
        private Timer fadeTimer;
        private bool isContextMenuOpen;
        #endregion

        public Darken()
        {
            InitializeComponent();
            InitializeForm();
            InitializeNotifyIcon();
            InitializeTimers();
        }

        #region Initialization
        private void InitializeForm()
        {
            this.Location = new Point(0, 0);
            this.Width = Screen.PrimaryScreen.WorkingArea.Width;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height * 2;
            this.Opacity = Settings.Default.Opacity;
            MakeClickThrough();
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[]
                {
                    new MenuItem("Raise Opacity", RaiseOpacity),
                    new MenuItem("Lower Opacity", LowerOpacity),
                    new MenuItem("Exit", ExitDarken)
                }),
                Visible = true,
                Text = "Darken"
            };
            notifyIcon.MouseUp += NotifyIcon_MouseUp;
            notifyIcon.ContextMenu.Popup += ContextMenu_Popup;
            notifyIcon.ContextMenu.Collapse += ContextMenu_Collapse;
        }

        private void InitializeTimers()
        {
            bringToFrontTimer = new Timer { Interval = 1000 };
            bringToFrontTimer.Tick += BringToFrontTimer_Tick;
            bringToFrontTimer.Start();

            closeTimer = new Timer { Interval = 50 };
            fadeTimer = new Timer { Interval = 50 };
        }

        private void MakeClickThrough()
        {
            int extendedStyle = GetWindowLong(Handle, GWL_EXSTYLE);
            SetWindowLong(Handle, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
        #endregion

        #region Application functions
        private void BringToFrontTimer_Tick(object sender, EventArgs e)
        { if (!isContextMenuOpen) SetForegroundWindow(Handle); }

        private void NotifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (bringToFrontTimer.Enabled)
                    bringToFrontTimer.Stop();
                else
                    bringToFrontTimer.Start();
            }
        }

        private void ContextMenu_Popup(object sender, EventArgs e)
        { isContextMenuOpen = true; }

        private void ContextMenu_Collapse(object sender, EventArgs e)
        { isContextMenuOpen = false; }

        private void ExitDarken(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            closeTimer.Tick += FadeOut;
            closeTimer.Start();
        }

        private void LowerOpacity(object sender, EventArgs e)
        { ChangeOpacity(-0.1); }

        private void RaiseOpacity(object sender, EventArgs e)
        { ChangeOpacity(0.1); }

        private void ChangeOpacity(double delta)
        {
            Opacity = Math.Max(0, Math.Min(1, Opacity + delta));
            Settings.Default.Opacity = this.Opacity;
            Settings.Default.Save();
        }

        private void FadeClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            fadeTimer.Tick += FadeOut;
            fadeTimer.Start();
        }

        private void FadeOut(object sender, EventArgs e)
        {
            if (Opacity > 0)
                Opacity -= 0.03;
            else
            {
                fadeTimer.Stop();
                closeTimer.Stop();
                notifyIcon.Dispose();
                Application.Exit();
            }
        }
        #endregion
    }
}