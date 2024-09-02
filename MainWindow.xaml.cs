using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Security.Permissions;
using System.Windows.Threading;
using System.Threading;


public static class DispatcherHelper
{
    [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    public static void DoEvents()
    {
        DispatcherFrame frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrames), frame);
        try { Dispatcher.PushFrame(frame); }
        catch (InvalidOperationException) { }
    }
    private static object ExitFrames(object frame)
    {
        ((DispatcherFrame)frame).Continue = false;
        return null;
    }
}

namespace swClassTableHint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        void DelayMS(int milliSecond){
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < milliSecond)//毫秒
            {
                DispatcherHelper.DoEvents();
            }
        }
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        // Win32 API declarations
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x80;
        protected NotifyIcon nfIco = null;
        private void updateTimeHint(int mins)
        {
            string stime=mins.ToString();
            if (mins < 10) stime = "0" + stime;
            CntMin.Text = stime;
        }
        private void initTrayIcon()
        {
            ContextMenuStrip ctxMS= new ContextMenuStrip();
            nfIco.ContextMenuStrip= ctxMS;
            ToolStripMenuItem exitMI=new ToolStripMenuItem();
            exitMI.Text = "退出";
            exitMI.Click += ExitMI_Click;
        }

        private void ExitMI_Click(object? sender, EventArgs e)
        {
            this.doClose();
        }
        private void OnSourceInit(object? sender, EventArgs e)
        {
            //base.OnSourceInitialized(e);
            IntPtr hWnd = new WindowInteropHelper(this).Handle;

            // Get the extended window style
            int exStyle = (int)GetWindowLong(hWnd, GWL_EXSTYLE);

            // Set the WS_EX_TOOLWINDOW style
            exStyle |= WS_EX_TOOLWINDOW;
            SetWindowLong(hWnd, GWL_EXSTYLE, (IntPtr)exStyle);
        }

        public MainWindow()
        {
            InitializeComponent();
            nfIco = new NotifyIcon();
            nfIco.Text = "课表展示";
            nfIco.Visible = true;
            initTrayIcon();
            //initMouseCLTH();
            Thread t = new Thread(() =>
            {
                for (int i= 0; i < 100; ++i)
                {
                    updateTimeHint(i);
                    DelayMS(100);
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            
        }
        public void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        public void MW_Loaded(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
            this.Left = (Screen.PrimaryScreen.Bounds.Width - this.ActualWidth)/2;
            this.Top = 0; // Top float window
        }
        public void MW_Closed(object sender, EventArgs e)
        {
            this.doClose();
        }

        private void MW_MRBdown(object sender, MouseButtonEventArgs e)
        {
        }
        public void C_EXIT(object sender, EventArgs e)
        {
            this.doClose();
        }
        private void doClose()
        {
            try { this.nfIco.Dispose(); }
            catch(Exception) { }
            this.Close();
        }
    }
}
