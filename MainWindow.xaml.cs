//000000000000、、#define unsafe if(true)

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
    struct CTActv
    {
        enum type { CLASS,REST,IDLE };
        DateTime actStart,actEnd;
    };
    enum animStat
    {
        DISAPPEAR,
        PREENTER,
        ENTERING,
        POSTDISPR,
        DISAPPEARING,
        IDLE
    };
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private animStat E_STAT;
        private int 
            p_width = 425,
            cur_width = 50,
            p_height = 60,
            p_slantDelta = 25;
        private int
            tmp_frmcnt = 0,
            ent_frames = 25,
            dspr_frames = 50,
            min_length = 50;
        private int
            ent_lenpf = 0,
            dspr_lenpf = 0;
        private delegate void calcUI();
        private delegate void setVisible(Visibility vi,FrameworkElement target);
        private void sVisible(Visibility vi,FrameworkElement target)
        {
            target.Visibility = vi;
        }
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
            unsafe { CntMin.Text = stime; }
        }
        private void initTrayIcon()
        {
            ContextMenuStrip ctxMS= new ContextMenuStrip();
            ToolStripMenuItem exitMI=new ToolStripMenuItem();
            exitMI.Text = "退出";
            exitMI.Click += ExitMI_Click;
            ctxMS.Items.Add(exitMI);
            nfIco.ContextMenuStrip= ctxMS;
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
        private void checkAvtivity()
        {

        }
        private void calcAnimation()
        {
            // swaitch
            switch (E_STAT)
            {
                case animStat.PREENTER:
                    // this.Visibility = Visibility.Visible;
                    Dispatcher.Invoke(new setVisible(sVisible),Visibility.Visible,this.Visibility);
                    E_STAT = animStat.ENTERING;
                    tmp_frmcnt = 0;
                    break;
                case animStat.ENTERING:
                    cur_width = min_length + ent_lenpf * tmp_frmcnt;
                    ++tmp_frmcnt;
                    if (tmp_frmcnt >= ent_frames)
                    {
                        E_STAT = animStat.IDLE;
                        // if(E_STAT==animStat.IDLE)
                        Dispatcher.Invoke(new setVisible(sVisible),Visibility.Visible,HintsText);
                        tmp_frmcnt = 0;
                    }
                    break;
                case animStat.POSTDISPR:
                    // this.Visibility= Visibility.Hidden;
                    Dispatcher.Invoke(new setVisible(sVisible),Visibility.Hidden,this);
                    E_STAT = animStat.DISAPPEAR;
                    break;
                case animStat.DISAPPEARING:
                    cur_width = min_length + dspr_lenpf * (dspr_frames - tmp_frmcnt);
                    ++tmp_frmcnt;
                    if (tmp_frmcnt == dspr_frames)E_STAT=animStat.POSTDISPR;
                    break;
                case animStat.IDLE:
                case animStat.DISAPPEAR:
                default:
                    
                    checkActity();
                    break;
            }
        }

        private void checkActity()
        {
            // throw new NotImplementedException();
        }

        private void calcParlg()
        {
            // p_width to be changed by calcAnimation()
            this.Width = cur_width;
            this.Left = (Screen.PrimaryScreen.Bounds.Width - this.ActualWidth) / 2;
            pt_lu.StartPoint = new Point(p_slantDelta,0);
            pt_ru.Point=new Point(cur_width,0);
            pt_rd.Point=new Point(cur_width-p_slantDelta,p_height);
            pt_ld.Point=new Point(0,p_height);
            // GC.Collect();
        }
        private void backMainLoop()
        {
            E_STAT = animStat.PREENTER;
            // var tc = 0;
            while (true)
            {
                // tc++;
                //if(tc==1)
                    unsafe
                {
                    calcAnimation();
                    // updateTimeHint(10);
                    // calcParlg(); replace to: v
                    // cur_width = 320 + tc * 2;
                    Dispatcher.Invoke(new calcUI(calcParlg));
                    // 20FPS=50ms
                    DelayMS(1000/30-2);
                    // consitnue
                    continue;
                }
                // DelayMS(1000);
                // if (tc > 5 ) tc = 0;
            }
        }
        public MainWindow()
        {
            ent_lenpf = (p_width - min_length) / ent_frames;
            dspr_lenpf = (p_width - min_length) / dspr_frames;
            //Task.Run(async () =>{
                InitializeComponent();
                nfIco = new NotifyIcon();
                nfIco.Text = "课表展示";
            // nfIco.Icon;
                nfIco.Visible = true;
                initTrayIcon();
            //while (true)
            Task.Run(backMainLoop);
                //initMouseCLTH();
                /*
                Thread t = new Thread(() =>
                {
                    for (int i= 0; i < 100; ++i)
                    {
                        updateTimeHint(i);
                        DelayMS(100);
                    }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();*/
                // await 
            //});
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
            this.Visibility = Visibility.Visible;
            HintsText.Visibility = Visibility.Hidden;
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
