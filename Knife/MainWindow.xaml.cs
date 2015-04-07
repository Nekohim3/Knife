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
using System.Threading;
using System.Runtime.InteropServices;
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
            states = new States(Img_ConnectToServer, Img_SteamAuth, Img_ServerAuth, Img_Main, ColorGrid, log, L_ConnectToServer, L_SteamAuth, L_ServerAuth);
            client.Connected += client_Connected;
            client.Disconnected += client_Disconnected;
            client.PacketReceived += client_PacketReceived;

        }
        States states;
        string ProfileImgLink = "";
        string accid = "";
        string Cookie = "";
        string sub = "";
        string sessionId = "";
        double MoneyLimit = 0;
        bool setts = false;
        bool wbload = false;
        bool IsInitRunned = false;
        Point lastwndsize = new Point();
        bool Steam_waitforlog = false;

        Nading.Network.Client.clsClient client = new Nading.Network.Client.clsClient();
        string ServerIp = "94.19.181.93";
        string ServerPort = "18346";

        public class States
        {
            Image Img_ConnectToServer;
            Image Img_SteamAuth;
            Image Img_ServerAuth;
            Image Img_Main; 
            Label L_ConnectToServer;
            Label L_SteamAuth;
            Label L_ServerAuth;
            Grid ColorGrid;
            Label Log;
            public States(Image img1, Image img2, Image img3, Image img4, Grid g, Label log, Label l1, Label l2, Label l3)
            {
                Img_ConnectToServer = img1;
                Img_SteamAuth = img2;
                Img_ServerAuth = img3;
                Img_Main = img4;
                ColorGrid = g;
                Log = log;
                L_ConnectToServer = l1;
                L_SteamAuth = l2;
                L_ServerAuth = l3;
            }

            private ClientState _clientState;
            public ClientState clientState
            {
                get { return _clientState; }
                set
                {
                    _clientState = value;
                    if (_clientState == ClientState.Connected)
                    {
                        Img_ConnectToServer.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "ok.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_ConnectToServer, image);
                            Img_ConnectToServer.ToolTip = "Connected";
                            Log.Content = "Connected to main server";
                            Img_ToPanel(Img_ConnectToServer,0, L_ConnectToServer);
                            Img_SteamAuth.Visibility = Visibility.Visible;
                            
                        }));
                    }
                    if (_clientState == ClientState.Connecting)
                    {
                        Img_ConnectToServer.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "loading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_ConnectToServer, image);
                            Img_ConnectToServer.ToolTip = "Connecting";
                            Log.Content = "Connecting to main server";
                            Img_ToCenter(Img_ConnectToServer, L_ConnectToServer);
                            Img_SteamAuth.Visibility = Visibility.Hidden;
                        }));
                    }
                    if (_clientState == ClientState.Disconnected)
                    {
                        steamAuthState = SteamAuthState.NotAuth;
                        serverState = ServerState.NotAuth;
                        Img_ConnectToServer.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_ConnectToServer, image);
                            Img_ConnectToServer.ToolTip = "Disconnected";
                            Log.Content = "Disconnected from main server";
                            Img_ToCenter(Img_ConnectToServer, L_ConnectToServer);
                            Img_SteamAuth.Visibility = Visibility.Hidden;
                        }));
                    }
                }
            }

            private SteamAuthState _steamAuthState;
            public SteamAuthState steamAuthState
            {
                get { return _steamAuthState; }
                set
                {
                    if (_clientState == ClientState.Connected)
                    {
                        _steamAuthState = value;
                    }
                    else
                    {
                        _steamAuthState = SteamAuthState.NotAuth;
                    }
                    if (_steamAuthState == SteamAuthState.Auth)
                    {
                        Img_SteamAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "ok.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_SteamAuth, image);
                            Img_SteamAuth.ToolTip = "Auth";
                            Log.Content = "Logged in Steam";
                            Img_ToPanel(Img_SteamAuth, 1, L_SteamAuth);
                            Img_ServerAuth.Visibility = Visibility.Visible;
                        }));
                    }
                    if (_steamAuthState == SteamAuthState.Authing)
                    {
                        Img_SteamAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "loading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_SteamAuth, image);
                            Img_SteamAuth.ToolTip = "Authing";
                            Log.Content = "Log in Steam";
                            Img_ToCenter(Img_SteamAuth, L_SteamAuth);
                            Img_ServerAuth.Visibility = Visibility.Hidden;
                        }));
                    }
                    if (_steamAuthState == SteamAuthState.NotAuth)
                    {
                        Img_SteamAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_SteamAuth, image);
                            Img_SteamAuth.ToolTip = "NotAuth";
                            Log.Content = "Not logged in Steam";
                            Img_ToCenter(Img_SteamAuth, L_SteamAuth);
                            Img_ServerAuth.Visibility = Visibility.Hidden;
                        }));
                    }
                    if (_steamAuthState == SteamAuthState.NotLogged)
                    {
                        Img_SteamAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "warning.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_SteamAuth, image);
                            Img_SteamAuth.ToolTip = "You are not logged in steam. Please click here to login";
                            Log.Content = "You are not logged in steam. Please click to image to login";
                            Img_ToCenter(Img_SteamAuth, L_SteamAuth);
                            Img_ServerAuth.Visibility = Visibility.Hidden;
                        }));
                    }
                }
            }

            private ServerState _serverState;
            public ServerState serverState
            {
                get { return _serverState; }
                set
                {
                    if (clientState == ClientState.Connected && steamAuthState == SteamAuthState.Auth)
                        _serverState = value;
                    else
                        _serverState = ServerState.NotAuth;
                    if (_serverState == ServerState.Auth)
                    {
                        Img_ServerAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "ok.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_ServerAuth, image);
                            Img_ServerAuth.ToolTip = "Auth";
                            //Log.Content = "Logged in main server";
                            Log.Content = "Coming soon...";
                            Img_ToPanel(Img_ServerAuth, 2, L_ServerAuth);
                            Img_Main.Visibility = Visibility.Visible;
                        }));
                        searchState = SearchState.Search;

                    } 
                    if (_serverState == ServerState.Authing)
                    {
                        Img_ServerAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "loading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_ServerAuth, image);
                            Img_ServerAuth.ToolTip = "Authing";
                            Log.Content = "Log in main server";
                            Img_ToCenter(Img_ServerAuth, L_ServerAuth);
                            Img_Main.Visibility = Visibility.Hidden;
                        }));
                    } 
                    if (_serverState == ServerState.Banned)
                    {
                        Img_ServerAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "banned.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_ServerAuth, image);
                            Img_ServerAuth.ToolTip = "You are banned";
                            Log.Content = "You are banned";
                            Img_ToCenter(Img_ServerAuth, L_ServerAuth);
                            Img_Main.Visibility = Visibility.Hidden;
                        }));
                    }
                    if (_serverState == ServerState.NotAuth)
                    {
                        Img_ServerAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_ServerAuth, image);
                            Img_ServerAuth.ToolTip = "NotAuth";
                            Log.Content = "Not logged in main server";
                            Img_ToCenter(Img_ServerAuth, L_ServerAuth);
                            Img_Main.Visibility = Visibility.Hidden;
                        }));
                    } 
                    if (_serverState == ServerState.NewUser)
                    {
                        Img_ServerAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "sloading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_ServerAuth, image);
                            Img_ServerAuth.ToolTip = "Wait until you authorize admin";
                            Log.Content = "Wait until you authorize admin";
                            Img_ToCenter(Img_ServerAuth, L_ServerAuth);
                            Img_Main.Visibility = Visibility.Hidden;
                        }));
                    }
                }
            }

            private SearchState _searchState;
            public SearchState searchState
            {
                get { return _searchState; }
                set
                {
                    _searchState = value;
                    if (_searchState == SearchState.Off)
                    {
                        Img_Main.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "sloading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(Img_Main, image);
                            Img_Main.ToolTip = "off";
                        }));
                        Log.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate
                        {
                            Log.Content = "Please allow enough time for all authorizations";
                        }));
                    }
                    if (_searchState == SearchState.Search)
                    {

                    }
                    if (_searchState == SearchState.GetKnife)
                    {

                    }
                    if (_searchState == SearchState.Buying)
                    {

                    }
                }
            }

            void Img_ToCenter(Image img, Label l)
            {
                ThicknessAnimation ta = new ThicknessAnimation();
                ta.From = img.Margin;
                ta.To = new Thickness(80, 10, 0, -1);
                ta.Duration = TimeSpan.FromMilliseconds(500);
                ta.EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut
                };

                DoubleAnimation da = new DoubleAnimation();
                da.From = img.Width;
                da.To = 350;
                da.Duration = TimeSpan.FromMilliseconds(500);
                da.EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut
                };
                img.BeginAnimation(MarginProperty, ta);
                img.BeginAnimation(WidthProperty, da);
                img.BeginAnimation(HeightProperty, da);

                DoubleAnimation dal = new DoubleAnimation();
                dal.From = l.Opacity;
                dal.To = 0;
                dal.Duration = TimeSpan.FromMilliseconds(500);
                dal.EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut
                };
                l.BeginAnimation(OpacityProperty, dal);
            }
            void Img_ToPanel(Image img, int n, Label l)
            {
                ThicknessAnimation ta = new ThicknessAnimation();
                ta.From = img.Margin;
                ta.To = new Thickness(515, 120*n, 0, 0);
                ta.Duration = TimeSpan.FromMilliseconds(500);
                ta.EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut
                };
                DoubleAnimation da = new DoubleAnimation();
                da.From = img.Width;
                da.To = 100;
                da.Duration = TimeSpan.FromMilliseconds(500);
                da.EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut
                };
                img.BeginAnimation(MarginProperty, ta);
                img.BeginAnimation(WidthProperty, da);
                img.BeginAnimation(HeightProperty, da);

                DoubleAnimation dal = new DoubleAnimation();
                dal.From = l.Opacity;
                dal.To = 1;
                dal.Duration = TimeSpan.FromMilliseconds(500);
                dal.EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut
                };
                l.BeginAnimation(OpacityProperty, dal);
            }
        }
        public enum ClientState
        {
            Disconnected,
            Connecting,
            Connected
        }
        public enum SteamAuthState
        {
            Auth,
            NotAuth,
            Authing,
            NotLogged
        }
        public enum ServerState
        {
            Auth,
            NotAuth,
            Authing,
            Banned,
            NewUser
        }
        public enum SearchState
        {
            Off,
            Search,
            GetKnife, 
            Buying,
        }

        void client_PacketReceived(byte PacketType, string Packet)
        {
            if(PacketType == (byte)ConnMessType.Auth)
            {
                states.serverState = (ServerState)Enum.Parse(typeof(ServerState), Packet);
            }
        }
        void client_Disconnected()
        {
            states.clientState = ClientState.Disconnected;
        }
        void client_Connected()
        {
            states.clientState = ClientState.Connected;
        }

        void Init()
        {
            if (IsInitRunned)
                return;
            IsInitRunned = true;
            try
            {
                if (states.clientState == ClientState.Disconnected)
                {
                    InitConn();
                }
                if (states.clientState == ClientState.Connected && states.steamAuthState == SteamAuthState.NotAuth)
                {
                    SteamAuth();
                }
                if (states.clientState == ClientState.Connected && states.steamAuthState == SteamAuthState.Auth && states.serverState == ServerState.NotAuth)
                {
                    ServerAuth();
                }
            }
            catch
            {
                states.clientState = ClientState.Disconnected;
                states.steamAuthState = SteamAuthState.NotAuth;
                states.serverState = ServerState.NotAuth;
            }
            IsInitRunned = false;
        }
        void InitConn()
        {
            states.clientState = ClientState.Connecting;
            try
            {
                client.Connect(ServerIp, ServerPort, false);
                states.clientState = ClientState.Connected;
            }
            catch
            {
                states.clientState = ClientState.Disconnected;
            }
        }
        void SteamAuth()
        {
            DateTime dt = DateTime.Now;
            states.steamAuthState = SteamAuthState.Authing;
            wbload = false;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
            {
                try
                {
                    wb.Source = new Uri("http://google.com");
                    wb.Source = new Uri("http://steamcommunity.com/market/");
                    while (!wbload)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate{ }));
                        if ((DateTime.Now - dt).TotalMilliseconds > 15000)
                        {
                            states.steamAuthState = SteamAuthState.NotAuth;
                            return;
                        }
                    }
                    if (wb.Source.AbsoluteUri == "http://steamcommunity.com/market/" || wb.Source.AbsoluteUri == "https://steamcommunity.com/market/")
                    {
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(wb.ExecuteJavascriptWithResult("document.documentElement.outerHTML").ToString()); 
                        if (doc.DocumentNode.Descendants("span").Where(x => x.Id == "marketWalletBalanceAmount").Count() != 0)
                        {
                            MoneyLimit = Convert.ToDouble(doc.DocumentNode.Descendants("span").Where(x => x.Id == "marketWalletBalanceAmount").First().InnerText.Split(' ')[0]);
                            if (MoneyLimit > 1500)
                                MoneyLimit = 1500;
                            accid = doc.DocumentNode.Descendants("span").Where(c => c.Id == "account_pulldown").First().InnerText; 
                            string ProfileLink = doc.DocumentNode.Descendants("img").Where(x => x.Id == "headerUserAvatarIcon").First().ParentNode.GetAttributeValue("href", ""); 
                            wb.Source = new Uri("http://google.com");
                            wb.Source = new Uri(ProfileLink);
                            wbload = false;
                            while (!wbload)
                            {
                                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                                if ((DateTime.Now - dt).TotalMilliseconds > 15000)
                                {
                                    states.steamAuthState = SteamAuthState.NotAuth;
                                    return;
                                }
                            }
                            doc.LoadHtml(wb.ExecuteJavascriptWithResult("document.documentElement.outerHTML").ToString()); 
                            ProfileImgLink = doc.DocumentNode.Descendants("div").Where(x => x.GetAttributeValue("class", "").IndexOf("playerAvatar profile_header_size") != -1).First().ChildNodes.Where(x => x.Name == "img").First().GetAttributeValue("src", "");
                            if (GetCookies())
                            {
                                states.steamAuthState = SteamAuthState.Auth;

                            }
                            else
                            {
                                states.steamAuthState = SteamAuthState.NotAuth;
                            }
                        }
                        else
                        {
                            states.steamAuthState = SteamAuthState.NotLogged;
                        }
                    }
                }
                catch(Exception e)
                {
                    states.steamAuthState = SteamAuthState.NotAuth;
                    MessageBox.Show(e.Message.ToString() +Environment.NewLine + e.ToString());
                }
            })); 
            

        }
        void ServerAuth()
        {
            client.Send((byte)ConnMessType.Auth, accid + "<:>" + ProfileImgLink + "<:>" + MoneyLimit.ToString());
            states.serverState = ServerState.Authing;
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
                    states.steamAuthState = SteamAuthState.NotAuth;
                    wb.Visibility = System.Windows.Visibility.Hidden;
                    Width = lastwndsize.X;
                    Height = lastwndsize.Y;
                }
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            states.clientState = ClientState.Disconnected;
            states.steamAuthState = SteamAuthState.NotAuth;
            states.serverState = ServerState.NotAuth;
            states.searchState = SearchState.Off;
            sett.Margin = new Thickness(0, 0, -sett.Width, 30);
            System.Timers.Timer CheckAllTimer = new System.Timers.Timer(1000);
            CheckAllTimer.Elapsed += (ss, ee) =>
            {
                Init();
            };
            CheckAllTimer.Enabled = true;
            

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
        private void Img_SteamAuth_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (states.steamAuthState == SteamAuthState.NotLogged)
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
        Sub
    }
}
//search ------- search sucsessfull--
//           |-- search error
//
//get knife ---- get sucsessfull--
//           |-- lot sold
//           |-- get error
//           |-- price err
//
//buying ------- buy succ
//           |-- buy err