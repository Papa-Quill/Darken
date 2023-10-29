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
        private void MakeClickThrough()
        {
            int extendedStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x20;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private readonly NotifyIcon TrayIcon;

        void Exit(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            CloseTimer.Tick += new EventHandler(FadeOut);
            CloseTimer.Start();
        }
        void LowerOpacity(object sender, EventArgs e)
        {
            Opacity -= .1;
            global::Darken.Properties.Settings.Default.Opacity = this.Opacity;
            global::Darken.Properties.Settings.Default.Save();
        }
        void RaiseOpacity(object sender, EventArgs e)
        {
            Opacity += .1;
            global::Darken.Properties.Settings.Default.Opacity = this.Opacity;
            global::Darken.Properties.Settings.Default.Save();
        }

        public Darken()
        {
            InitializeComponent();
            TrayIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Increase Opacity", RaiseOpacity),
                new MenuItem("Decrease Opacity", LowerOpacity),
                new MenuItem("Close Darken", Exit)
            }),
                Visible = true
            };
            this.Location = new Point(0, 0);
            this.Width = Screen.PrimaryScreen.WorkingArea.Size.Width;
            this.Height = Screen.PrimaryScreen.WorkingArea.Size.Height * 2;
            Opacity = global::Darken.Properties.Settings.Default.Opacity;
            MakeClickThrough();
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
                Application.Exit();
            }
            else
                Opacity -= 0.3;
        }

    }
}
