using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using HtmlAgilityPack;
using System.IO;
using System.Data.SQLite;
using System.Data;
using System.Threading;
using System.Runtime.InteropServices;
using WpfAnimatedGif;
using System.Windows.Threading;
namespace Knife
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            client.Connected += client_Connected;
            client.Disconnected += client_Disconnected;
            client.PacketReceived += client_PacketReceived;

        }
        string accid = "";
        string Cookie = "";
        string sessionId = "";
        bool setts = false;
        Point lastwndsize = new Point();
        bool Steam_waitforlog = false;
        Nading.Network.Client.clsClient client = new Nading.Network.Client.clsClient();
        string ServerIp = "94.19.181.93";
        string ServerPort = "18346";
        ConnState ClientState = ConnState.Disconnected;
        SteamAuthState steamAuthState = SteamAuthState.NotAuth;
        enum ConnState
        {
            Disconnected,
            Connecting,
            Connected
        }
        enum SteamAuthState
        {
            Auth,
            NotAuth,
            Authing,
            NotLogged
        }
        bool initrun = false;

        void client_PacketReceived(byte PacketType, string Packet)
        {

        }
        void client_Disconnected()
        {
            ClientState = ConnState.Disconnected;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
            {
                Img_ConnectToServer.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "Error.png"));
                Img_ConnectToServer.ToolTip = "Disconnected";
            }));
            steamAuthState = SteamAuthState.NotAuth;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
            {
                Img_SteamAuth.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png"));
                Img_SteamAuth.ToolTip = "NotAuth";
            }));
        }
        void client_Connected()
        {
            ClientState = ConnState.Connected;
        }
        void Init()
        {
            if (initrun)
                return;
            initrun = true;
            if (ClientState == ConnState.Disconnected)
            {
                InitConn();
            }
            if(ClientState == ConnState.Connected && steamAuthState == SteamAuthState.NotAuth)
            {
                SteamAuth();
            }
            initrun = false;
        }
        void InitConn()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Img_ConnectToServer.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "loading.gif"));
                Img_ConnectToServer.ToolTip = "Connecting";
            }));
            ClientState = ConnState.Connecting;
            
            try
            {
                client.Connect(ServerIp, ServerPort, false);
                Dispatcher.Invoke(new Action(() =>
                {
                    Img_ConnectToServer.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "ok.png"));
                    Img_ConnectToServer.ToolTip = "Connected";
                }));
                ClientState = ConnState.Connected;
            }
            catch
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Img_ConnectToServer.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png"));
                    Img_ConnectToServer.ToolTip = "Disconnected";
                }));
                ClientState = ConnState.Disconnected;
            }
        }
        void SteamAuth()
        {
            steamAuthState = SteamAuthState.Authing;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
            {
                Img_SteamAuth.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "loading.gif"));
                Img_SteamAuth.ToolTip = "Authing";
            }));
            wbload = false;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
            {
                wb.Source = new Uri("http://google.com");
                wb.Source = new Uri("http://steamcommunity.com/market/");
            }));
            while (!wbload)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                {
                }));
            }
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
            {
                if (wb.Source.AbsoluteUri == "http://steamcommunity.com/market/" || wb.Source.AbsoluteUri == "https://steamcommunity.com/market/")
                {
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(wb.ExecuteJavascriptWithResult("document.documentElement.outerHTML").ToString());
                    if (doc.DocumentNode.Descendants("span").Where(x => x.Id == "marketWalletBalanceAmount").Count() != 0)
                    {
                        double mon = Convert.ToDouble(doc.DocumentNode.Descendants("span").Where(x => x.Id == "marketWalletBalanceAmount").First().InnerText.Split(' ')[0]);
                        accid = doc.DocumentNode.Descendants("span").Where(c => c.Id == "account_pulldown").First().InnerText;
                        if (GetCookies())
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
                            {
                                steamAuthState = SteamAuthState.Auth;
                                Img_SteamAuth.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "ok.png"));
                                Img_SteamAuth.ToolTip = "Auth";
                            }));
                        }
                        else
                        {
                            steamAuthState = SteamAuthState.NotAuth;
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
                            {
                                Img_SteamAuth.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png"));
                                Img_SteamAuth.ToolTip = "NotAuth";
                            }));
                        }
                    }
                    else
                    {
                        steamAuthState = SteamAuthState.NotLogged;
                        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate
                        {
                            Img_SteamAuth.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "warning.png"));
                            Img_SteamAuth.ToolTip = "You are not logged in steam. Please click here to login";
                        }));
                    }
                }
            }));
            

        }
        bool wbload = false;
        private void wb_InitializeView(object sender, Awesomium.Core.WebViewEventArgs e)
        {
            wb.WebSession = Awesomium.Core.WebCore.CreateWebSession(System.Environment.CurrentDirectory, new Awesomium.Core.WebPreferences());
        }

        private void wb_LoadingFrameComplete(object sender, Awesomium.Core.FrameEventArgs e)
        {
            wbload = true;
            if(Steam_waitforlog)
            {
                if (wb.Source.AbsoluteUri == "http://steamcommunity.com/market/" || wb.Source.AbsoluteUri == "https://steamcommunity.com/market/")
                {
                    Steam_waitforlog = false;
                    steamAuthState = SteamAuthState.NotAuth;
                    wb.Visibility = System.Windows.Visibility.Hidden;
                    Width = lastwndsize.X;
                    Height = lastwndsize.Y;
                }
            }
        }
        bool GetCookies()
        {
            if (!File.Exists("Cookies"))
                return false;
            else
            {
                int i = 0;
                while (i < 3)
                {
                    try
                    {

                        FileStream fs = new FileStream("Cookies", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        byte[] buf = new byte[fs.Length];
                        fs.Read(buf, 0, (int)fs.Length);
                        MemoryStream ms = new MemoryStream(buf);
                        StreamReader sr = new StreamReader(ms);
                        string str = sr.ReadToEnd();
                        sr.Close();
                        ms.Close();
                        fs.Close();
                        sessionId = str.Remove(0, str.IndexOf("steamcommunity.comsessionid")).Substring(0, str.Remove(0, str.IndexOf("steamcommunity.comsessionid")).IndexOf("/")).Replace("steamcommunity.comsessionid", "");
                        Cookie = "sessionid=" + sessionId + ";";
                        Cookie += "steamLogin=" + str.Remove(0, str.IndexOf("steamcommunity.comsteamLogin")).Substring(0, str.Remove(0, str.IndexOf("steamcommunity.comsteamLogin")).IndexOf("/")).Replace("steamcommunity.comsteamLogin", "") + ";";
                        Cookie += "steamLoginSecure=" + str.Remove(0, str.IndexOf("steamcommunity.comsteamLoginSecure")).Substring(0, str.Remove(0, str.IndexOf("steamcommunity.comsteamLoginSecure")).IndexOf("/")).Replace("steamcommunity.comsteamLoginSecure", "") + ";";
                        Cookie += "comsteamMachineAuth" + str.Remove(0, str.IndexOf("steamcommunity.comsteamMachineAuth")).Substring(0, str.Remove(0, str.IndexOf("steamcommunity.comsteamMachineAuth")).IndexOf("/")).Replace("steamcommunity.comsteamMachineAuth", "").Insert(17, "=") + ";";
                        return true;
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                        i++;
                        if (i > 5)
                            return false;
                    }
                }
                return false;

            }
        }
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ClientState = ConnState.Disconnected;
            steamAuthState = SteamAuthState.NotAuth;
            Img_ConnectToServer.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png"));
            Img_ConnectToServer.ToolTip = "Disconnected";
            Img_SteamAuth.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png"));
            Img_SteamAuth.ToolTip = "NotAuth";
            sett.Margin = new Thickness(0, 0, -sett.Width, 30);
            System.Timers.Timer CheckAllTimer = new System.Timers.Timer(1000);
            CheckAllTimer.Elapsed += (ss, ee) =>
            {
                Init();
            };
            CheckAllTimer.Enabled = true;
            

        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
        }
        private void MW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!setts)
            {
                setts = true;
                //GetCountSlider.Value = Convert.ToDouble(GetCount);
                //bthsSlider.Value = Convert.ToDouble(bths);
                //TB_Money.Text = money.ToString();
                ThicknessAnimation ta = new ThicknessAnimation();
                ta.From = sett.Margin;
                ta.To = new Thickness(0, 0, 0, 30);
                ta.Duration = TimeSpan.FromMilliseconds(500);
                ta.EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut
                };
                sett.BeginAnimation(MarginProperty, ta);
            }
            else
            {
                setts = false;
                ThicknessAnimation ta = new ThicknessAnimation();
                ta.From = sett.Margin;
                ta.To = new Thickness(0, 0, -sett.Width, 30);
                ta.Duration = TimeSpan.FromMilliseconds(500);
                ta.EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut
                };
                sett.BeginAnimation(MarginProperty, ta);
            }
        }
        private void Img_SteamAuth_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(steamAuthState == SteamAuthState.NotLogged)
            {
                lastwndsize = new Point(Width, Height);
                wb.Visibility = System.Windows.Visibility.Visible;
                Width = 1280;
                Height = 720;
                wb.Source = new Uri("https://steamcommunity.com/login/home/?goto=market%2F");
                Steam_waitforlog = true;
            }
        }
    }
    public enum ConnMessType
    {
        Auth,
        Log,
        Com,
        AccessGranted,
        AccessDenied,
        Replace,
        Alivereq,
        Aliveres,
        Update,
    }
}
