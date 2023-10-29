using Darken.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Darken
{
    public partial class Darken : Form
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x20;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private void MakeClickThrough()
        {
            int extendedStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
        private void BringToFrontTimer_Tick(object sender, EventArgs e)
        {
            SetForegroundWindow(this.Handle);
        }
        private void NotifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (bringToFrontTimer.Enabled)
                {
                    bringToFrontTimer.Stop();
                }
                else
                {
                    bringToFrontTimer.Start();
                }
            }
        }

        private readonly Timer bringToFrontTimer;
        private readonly NotifyIcon notifyIcon;

        public Darken()
        {
            InitializeComponent();
            this.Location = new Point(0, 0);
            this.Width = Screen.PrimaryScreen.WorkingArea.Size.Width;
            this.Height = Screen.PrimaryScreen.WorkingArea.Size.Height * 2;
            Opacity = global::Darken.Properties.Settings.Default.Opacity;
            MakeClickThrough();
            notifyIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Raise Opacity", RaiseOpacity),
                new MenuItem("Lower Opacity", LowerOpacity),
                new MenuItem("Exit", ExitDarken)
            }),
                Visible = true,
                Text = "Darken"
            };
            notifyIcon.MouseUp += NotifyIcon_MouseUp;
            bringToFrontTimer = new Timer();
            bringToFrontTimer.Interval = 1000;
            bringToFrontTimer.Tick += BringToFrontTimer_Tick;
            bringToFrontTimer.Start();
        }

        private void ExitDarken(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            CloseTimer.Tick += new EventHandler(FadeOut);
            CloseTimer.Start();
        }
        private void LowerOpacity(object sender, EventArgs e)
        {
            Opacity -= .1;
            global::Darken.Properties.Settings.Default.Opacity = this.Opacity;
            global::Darken.Properties.Settings.Default.Save();
        }
        private void RaiseOpacity(object sender, EventArgs e)
        {
            Opacity += .1;
            global::Darken.Properties.Settings.Default.Opacity = this.Opacity;
            global::Darken.Properties.Settings.Default.Save();
        }

        readonly Timer CloseTimer = new Timer();
        readonly Timer T1 = new Timer();
        void FadeClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            T1.Tick += new EventHandler(FadeOut);
            T1.Start();


            if (Opacity == 0)
                e.Cancel = false;

        }
        void FadeOut(object sender, EventArgs e)
        {
            CloseTimer.Interval = 50;
            if (Opacity <= 0)
            {
                T1.Stop();
                notifyIcon.Dispose();
                Application.Exit();
            }
            else
                Opacity -= 0.3;
        }

    }
}
