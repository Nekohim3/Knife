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
using System.Net;
using Newtonsoft.Json;
namespace Knife
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        [DllImport("kernel32")]
        static extern bool AllocConsole();

        public MainWindow()
        {
            InitializeComponent();
            states = new States(this);//Img_ConnectToServer, Img_SteamAuth, Img_ServerAuth, Img_Main, ColorGrid, log, L_ConnectToServer, L_SteamAuth, L_ServerAuth,
            client.Connected += client_Connected;
            client.Disconnected += client_Disconnected;
            client.PacketReceived += client_PacketReceived;

        }
        States states;
        string ProfileImgLink = "";
        string accid = "";
        bool CanOffline = false;
        bool sub = false;
        string sessionId = "";
        string Cookie = "";

        string subc = "";
        string subs = "";

        double MoneyLimit = 0;
        double AccMoney = 0;
        double LastPrice = 0;
        bool setts = false;
        bool wbload = false;
        bool IsInitRunned = false;
        Point lastwndsize = new Point();
        bool Steam_waitforlog = false;
        int GetCount1 = 1;
        int GetCount2 = 1;

        List<SKnife> KnivesStats = new List<SKnife>();

        Nading.Network.Client.clsClient client = new Nading.Network.Client.clsClient();
        string ServerIp = "94.19.181.93";
        string ServerPort = "18346";

        public class States
        {
            MainWindow MainW;
            public States( MainWindow mw)
            {
                MainW = mw;
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
                        MainW.Img_ConnectToServer.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "ok.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_ConnectToServer, image);
                            MainW.Img_ConnectToServer.ToolTip = "Connected";
                            MainW.Log.Content = "Connected to main server";
                            Img_ToPanel(MainW.Img_ConnectToServer, 0, MainW.L_ConnectToServer);
                            MainW.Img_SteamAuth.Visibility = Visibility.Visible;
                            
                        }));
                    }
                    if (_clientState == ClientState.Connecting)
                    {
                        MainW.Img_ConnectToServer.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "loading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_ConnectToServer, image);
                            MainW.Img_ConnectToServer.ToolTip = "Connecting";
                            MainW.Log.Content = "Connecting to main server";
                            Img_ToCenter(MainW.Img_ConnectToServer, MainW.L_ConnectToServer);
                            MainW.Img_SteamAuth.Visibility = Visibility.Hidden;
                        }));
                    }
                    if (_clientState == ClientState.Disconnected)
                    {
                        steamAuthState = SteamAuthState.NotAuth;
                        serverState = ServerState.NotAuth;
                        searchState = SearchState.Off;
                        MainW.Img_ConnectToServer.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_ConnectToServer, image);
                            MainW.Img_ConnectToServer.ToolTip = "Disconnected";
                            MainW.Log.Content = "Disconnected from main server";
                            Img_ToCenter(MainW.Img_ConnectToServer, MainW.L_ConnectToServer);
                            MainW.Img_SteamAuth.Visibility = Visibility.Hidden;
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
                        MainW.Img_SteamAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "ok.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_SteamAuth, image);
                            MainW.Img_SteamAuth.ToolTip = "Auth";
                            MainW.Log.Content = "Logged in Steam";
                            Img_ToPanel(MainW.Img_SteamAuth, 1, MainW.L_SteamAuth);
                            MainW.Img_ServerAuth.Visibility = Visibility.Visible;
                        }));
                    }
                    if (_steamAuthState == SteamAuthState.Authing)
                    {
                        MainW.Img_SteamAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "loading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_SteamAuth, image);
                            MainW.Img_SteamAuth.ToolTip = "Authing";
                            MainW.Log.Content = "Log in Steam";
                            Img_ToCenter(MainW.Img_SteamAuth, MainW.L_SteamAuth);
                            MainW.Img_ServerAuth.Visibility = Visibility.Hidden;
                        }));
                    }
                    if (_steamAuthState == SteamAuthState.NotAuth)
                    {
                        MainW.Img_SteamAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_SteamAuth, image);
                            MainW.Img_SteamAuth.ToolTip = "NotAuth";
                            MainW.Log.Content = "Not logged in Steam";
                            Img_ToCenter(MainW.Img_SteamAuth, MainW.L_SteamAuth);
                            MainW.Img_ServerAuth.Visibility = Visibility.Hidden;
                        }));
                    }
                    if (_steamAuthState == SteamAuthState.NotLogged)
                    {
                        MainW.Img_SteamAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "warning.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_SteamAuth, image);
                            MainW.Img_SteamAuth.ToolTip = "You are not logged in steam. Please click here to login";
                            MainW.Log.Content = "You are not logged in steam. Please click to image to login";
                            Img_ToCenter(MainW.Img_SteamAuth, MainW.L_SteamAuth);
                            MainW.Img_ServerAuth.Visibility = Visibility.Hidden;
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
                        MainW.Img_ServerAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "ok.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_ServerAuth, image);
                            MainW.Img_ServerAuth.ToolTip = "Auth";
                            //MainW.Log.Content = "Coming soon...";
                            Img_ToPanel(MainW.Img_ServerAuth, 2, MainW.L_ServerAuth);
                            MainW.Img_Main.Visibility = Visibility.Visible;

                            DoubleAnimation dal = new DoubleAnimation();
                            dal.From = 0;
                            dal.To = 1;
                            dal.Duration = TimeSpan.FromMilliseconds(1500);
                            dal.EasingFunction = new PowerEase()
                            {
                                EasingMode = EasingMode.EaseOut
                            };
                            MainW.Img_Main.BeginAnimation(OpacityProperty, dal);
                        }));
                        searchState = SearchState.Search;

                    } 
                    if (_serverState == ServerState.Authing)
                    {
                        MainW.Img_ServerAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "loading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_ServerAuth, image);
                            MainW.Img_ServerAuth.ToolTip = "Authing";
                            MainW.Log.Content = "Log in main server";
                            Img_ToCenter(MainW.Img_ServerAuth, MainW.L_ServerAuth);
                            MainW.Img_Main.Visibility = Visibility.Hidden;
                            searchState = SearchState.Off;
                        }));
                    } 
                    if (_serverState == ServerState.Banned)
                    {
                        MainW.Img_ServerAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "banned.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_ServerAuth, image);
                            MainW.Img_ServerAuth.ToolTip = "You are banned";
                            MainW.Log.Content = "You are banned";
                            Img_ToCenter(MainW.Img_ServerAuth, MainW.L_ServerAuth);
                            MainW.Img_Main.Visibility = Visibility.Hidden;
                            searchState = SearchState.Off;
                        }));
                    }
                    if (_serverState == ServerState.NotAuth)
                    {
                        MainW.Img_ServerAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_ServerAuth, image);
                            MainW.Img_ServerAuth.ToolTip = "NotAuth";
                            MainW.Log.Content = "Not logged in main server";
                            Img_ToCenter(MainW.Img_ServerAuth, MainW.L_ServerAuth);
                            MainW.Img_Main.Visibility = Visibility.Hidden;
                            searchState = SearchState.Off;
                        }));
                    } 
                    if (_serverState == ServerState.NewUser)
                    {
                        MainW.Img_ServerAuth.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "sloading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_ServerAuth, image);
                            MainW.Img_ServerAuth.ToolTip = "Wait until you authorize admin";
                            MainW.Log.Content = "Wait until you authorize admin";
                            Img_ToCenter(MainW.Img_ServerAuth, MainW.L_ServerAuth);
                            MainW.Img_Main.Visibility = Visibility.Hidden;
                            searchState = SearchState.Off;
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
                    if (serverState != ServerState.Auth)
                        if(value != SearchState.Off)
                        return;
                    SearchState old = _searchState;
                    _searchState = value;
                    if (_searchState == SearchState.Off)
                    {
                        MainW.Img_Main.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "error.png");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_Main, image);
                            MainW.Img_Main.ToolTip = "off";

                            ColorAnimation ca = new ColorAnimation();
                            ca.From = ((SolidColorBrush)MainW.ColorGrid.Background).Color;
                            ca.To = Color.FromArgb(255, 16, 16, 16);
                            ca.Duration = TimeSpan.FromMilliseconds(500);
                            ca.EasingFunction = new PowerEase()
                            {
                                EasingMode = EasingMode.EaseOut
                            };
                            MainW.ColorGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

                            MainW.Log.Content = "Off";
                            if(clientState == ClientState.Connected)
                                MainW.client.Send((byte)ConnMessType.CAct, SearchState.Off.ToString());
                        }));
                    }
                    if (_searchState == SearchState.Search)
                    {
                        if(old == SearchState.Off)
                            new Thread(() => { MainW.KSearch(); }).Start();
                        MainW.Img_Main.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "sloading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_Main, image);
                            MainW.Img_Main.ToolTip = "off";
                            MainW.Log.Content = "Search. Current lowest price: " + MainW.LastPrice.ToString();

                            ColorAnimation ca = new ColorAnimation();
                            ca.From = ((SolidColorBrush)MainW.ColorGrid.Background).Color;
                            ca.To = Color.FromArgb(255, 16, 16, 16);
                            ca.Duration = TimeSpan.FromMilliseconds(500);
                            ca.EasingFunction = new PowerEase()
                            {
                                EasingMode = EasingMode.EaseOut
                            };
                            MainW.ColorGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                            if (clientState == ClientState.Connected)
                                MainW.client.Send((byte)ConnMessType.CAct, SearchState.Search.ToString());
                        }));
                    }
                    if (_searchState == SearchState.GetKnife)
                    {
                        MainW.Img_Main.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "loading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_Main, image);
                            MainW.Img_Main.ToolTip = "off";
                            MainW.Log.Content = "Get knife lot";

                            ColorAnimation ca = new ColorAnimation();
                            ca.From = ((SolidColorBrush)MainW.ColorGrid.Background).Color;
                            ca.To = Color.FromArgb(255, 255, 255, 0);
                            ca.Duration = TimeSpan.FromMilliseconds(500);
                            ca.EasingFunction = new PowerEase()
                            {
                                EasingMode = EasingMode.EaseOut
                            };
                            MainW.ColorGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                            if (clientState == ClientState.Connected)
                                MainW.client.Send((byte)ConnMessType.CAct, SearchState.GetKnife.ToString());
                        }));
                    }
                    if (_searchState == SearchState.Buying)
                    {
                        MainW.Img_Main.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "loading.gif");
                            image.EndInit();
                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(MainW.Img_Main, image);
                            MainW.Img_Main.ToolTip = "off";
                            MainW.Log.Content = "Buying";

                            ColorAnimation ca = new ColorAnimation();
                            ca.From = ((SolidColorBrush)MainW.ColorGrid.Background).Color;
                            ca.To = Color.FromArgb(255, 255, 0, 0);
                            ca.Duration = TimeSpan.FromMilliseconds(500);
                            ca.EasingFunction = new PowerEase()
                            {
                                EasingMode = EasingMode.EaseOut
                            };
                            MainW.ColorGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                            if (clientState == ClientState.Connected)
                                MainW.client.Send((byte)ConnMessType.CAct, SearchState.Buying.ToString());
                        }));
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
            if(PacketType == (byte)ConnMessType.Sub)
            {
                if (Packet != "")
                {
                    sub = true;
                    subc = Packet;
                    subs = subc.Split(';').Where(x => x.Split('=')[0] == "sessionid").First().Split('=')[1];
                }
                else
                {
                    sub = false;
                    subc = "";
                    subs = "";
                }
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
                try
                {
                    wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        wb.Source = new Uri("http://google.com");
                        wb.Source = new Uri("http://steamcommunity.com/market/"); 
                    }));
                    while (!wbload)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                        if ((DateTime.Now - dt).TotalMilliseconds > 15000)
                        {
                            states.steamAuthState = SteamAuthState.NotAuth;
                            return;
                        }
                    } 
                    string qwe = "";
                    wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        qwe = wb.Source.AbsoluteUri;
                    }));
                    if (qwe == "http://steamcommunity.com/market/" || qwe == "https://steamcommunity.com/market/")
                    {
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            doc.LoadHtml(wb.ExecuteJavascriptWithResult("document.documentElement.outerHTML").ToString());
                        }));
                        if (doc.DocumentNode.Descendants("span").Where(x => x.Id == "marketWalletBalanceAmount").Count() != 0)
                        {
                            AccMoney = Convert.ToDouble(doc.DocumentNode.Descendants("span").Where(x => x.Id == "marketWalletBalanceAmount").First().InnerText.Split(' ')[0]);
                            if (AccMoney > 1500)
                                MoneyLimit = 1500;
                            else
                                MoneyLimit = AccMoney;
                            accid = doc.DocumentNode.Descendants("span").Where(c => c.Id == "account_pulldown").First().InnerText; 
                            string ProfileLink = doc.DocumentNode.Descendants("img").Where(x => x.Id == "headerUserAvatarIcon").First().ParentNode.GetAttributeValue("href", "");
                            wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                wb.Source = new Uri(ProfileLink);
                            }));
                            wbload = false;
                            while (!wbload)
                            {
                                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                                if ((DateTime.Now - dt).TotalMilliseconds > 15000)
                                {
                                    states.steamAuthState = SteamAuthState.NotAuth;
                                    return;
                                }
                            }
                            wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                doc.LoadHtml(wb.ExecuteJavascriptWithResult("document.documentElement.outerHTML").ToString());
                            }));
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
                while (i < 5)
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
                        Cookie = "sessionid=" + str.Remove(0, str.IndexOf("steamcommunity.comsessionid")).Substring(0, str.Remove(0, str.IndexOf("steamcommunity.comsessionid")).IndexOf("/")).Replace("steamcommunity.comsessionid", "") + ";";
                        Cookie += "steamLogin=" + str.Remove(0, str.IndexOf("steamcommunity.comsteamLogin")).Substring(0, str.Remove(0, str.IndexOf("steamcommunity.comsteamLogin")).IndexOf("/")).Replace("steamcommunity.comsteamLogin", "") + ";";
                        Cookie += "steamLoginSecure=" + str.Remove(0, str.IndexOf("steamcommunity.comsteamLoginSecure")).Substring(0, str.Remove(0, str.IndexOf("steamcommunity.comsteamLoginSecure")).IndexOf("/")).Replace("steamcommunity.comsteamLoginSecure", "") + ";";
                        Cookie += "steamMachineAuth" + str.Remove(0, str.IndexOf("steamcommunity.comsteamMachineAuth")).Substring(0, str.Remove(0, str.IndexOf("steamcommunity.comsteamMachineAuth")).IndexOf("/")).Replace("steamcommunity.comsteamMachineAuth", "").Insert(17, "=") + ";";
                        sessionId = Cookie.Split(';').Where(x => x.Split('=')[0] == "sessionid").First().Split('=')[1];
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

        void KSearch()
        {
            while (states.searchState != SearchState.Off)
            {
                try
                {
                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://steamcommunity.com/market/search/render/?query=&start=0&count=" + GetCount1 + "&search_descriptions=0&sort_column=price&sort_dir=asc&appid=730&category_730_Type%5B%5D=tag_CSGO_Type_Knife&l=russian");
                    //HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://steamcommunity.com/market/search/render/?query=&start=0&count=1&search_descriptions=0&sort_column=price&sort_dir=asc&appid=730&category_730_Weapon%5B%5D=tag_weapon_elite&l=russian");
                    //HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://steamcommunity.com/market/search/render/?query=&start=0&count=1&search_descriptions=0&sort_column=price&sort_dir=asc&appid=730&l=russian");
                    req.Proxy = null;
                    req.Method = "GET";
                    req.Timeout = 2000;
                    req.Headers.Add("Cookie", Cookie);
                    WebResponse resp = req.GetResponse();
                    StreamReader sr = new StreamReader(resp.GetResponseStream());
                    string str = sr.ReadToEnd();
                    sr.Close();
                    dynamic obj = JsonConvert.DeserializeObject(str);
                    string htmlstr = obj.results_html.ToString();
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlstr);
                    List<listing> prices = new List<listing>();
                    foreach (HtmlAgilityPack.HtmlNode q in doc.DocumentNode.Descendants("div").Where(x => x.GetAttributeValue("class", "") == "market_listing_right_cell market_listing_their_price"))
                    {
                        prices.Add(new listing()
                        {
                            sender = q.ParentNode.ParentNode.GetAttributeValue("href", ""),
                            price = Convert.ToDouble(q.InnerText.Replace("\t", "").Replace("\n", "").Replace("\r", "").Split(' ')[0].Replace("От", ""))
                        });
                    }
                    prices = prices.OrderBy(x => x.price).ToList();
                    client.Send((byte)ConnMessType.Log, "SS:" + prices.First().price.ToString());
                    if (LastPrice != prices.First().price)
                    {
                        LastPrice = prices.First().price;
                        Log.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => 
                        {
                            Log.Content = "Search. Current lowest price: " + LastPrice.ToString();
                        }));
                    }
                    if (prices.First().price <= MoneyLimit)
                    {
                        states.searchState = SearchState.GetKnife;
                        Console.WriteLine("Get knife");
                        KGetLot(prices.First().sender);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Search error");
                    client.Send((byte)ConnMessType.Log, "Se");
                }
            }
        }
        void KGetLot(string sender)
        {
            try
            {
                string str = "";
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(sender + "/render?start=0&count=" + GetCount2 + "&currency=5&language=english&format=json");
                req.Proxy = null;
                req.Method = "GET";
                req.Timeout = 2000;
                WebResponse resp = req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                str = sr.ReadToEnd();
                sr.Close();
                Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(str);
                List<listing2> lst = new List<listing2>();
                foreach(Newtonsoft.Json.Linq.JToken jt in obj["listinginfo"])
                {
                    if (jt.First["price"].ToString() != "0")
                    {
                        if (jt.First["converted_price"] != null)
                        {
                            lst.Add(new listing2()
                            {
                                listingid = jt.First["listingid"].ToString(),
                                price = jt.First["converted_price"].ToString(),
                                fee = jt.First["converted_fee"].ToString(),
                                total = (Convert.ToDouble(jt.First["converted_price"].ToString()) + Convert.ToDouble(jt.First["converted_fee"].ToString())).ToString(),
                                BA = false,
                                sender = sender
                            });
                        }
                    }
                }
                lst = lst.OrderBy(x => x.total).ToList();
                List<listing2> ToBuy = new List<listing2>();
                if(lst.Count != 0)
                {
                    foreach(listing2 l in lst)
                    {
                        if(Convert.ToDouble(l.total) / 100 <= MoneyLimit)
                        {
                            ToBuy.Add(l);
                        } 
                    }
                    if(ToBuy.Count != 0)
                    {
                        states.searchState = SearchState.Buying;
                        foreach(listing2 l in ToBuy)
                            new Thread(() => KBuy(l)).Start();
                        client.Send((byte)ConnMessType.Log, "GetLotsSucc: " + ToBuy.Count + "/" + lst.Count);
                        while(ToBuy.Where(x => !x.BA).Count() != 0)
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                        NewKnives(ToBuy);

                    }
                    else
                    {
                        states.searchState = SearchState.Search;
                        Console.WriteLine("All knives corrupt");
                        client.Send((byte)ConnMessType.Log, "All knives corrupt");
                    }
                }
                else
                {
                    states.searchState = SearchState.Search;
                    Console.WriteLine("All sold");
                    client.Send((byte)ConnMessType.Log, "All sold");
                }
            }
            catch(Exception e)
            {
                states.searchState = SearchState.Search;
                Console.WriteLine("GetKnife error: " + e.Message);
                client.Send((byte)ConnMessType.Log, "GetKnife error: " + e.Message);
            }
        }
        void KBuy(listing2 l)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://steamcommunity.com/market/buylisting/" + l.listingid);
                req.Proxy = null;
                req.Method = "POST";
                req.Timeout = 10000;
                req.ContentType = "application/x-www-form-urlencoded;";
                req.Referer = l.sender;
                if (!sub)
                    req.Headers.Add("Cookie", Cookie);
                else
                    req.Headers.Add("Cookie", subc);
                req.ServicePoint.ConnectionLimit = 10;
                byte[] sentData;
                if(!sub)
                    sentData = Encoding.GetEncoding(1251).GetBytes("sessionid=" + sessionId + "&currency=5&subtotal=" + l.price + "&fee=" + l.fee + "&total=" + l.total + "&quantity=1");
                else
                    sentData = Encoding.GetEncoding(1251).GetBytes("sessionid=" + subs + "&currency=5&subtotal=" + l.price + "&fee=" + l.fee + "&total=" + l.total + "&quantity=1");
                req.ContentLength = sentData.Length;
                Stream sendStream = req.GetRequestStream();
                sendStream.Write(sentData, 0, sentData.Length);
                sendStream.Flush();
                sendStream.Close();
                WebResponse resp = req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                string str = sr.ReadToEnd();
                sr.Close();
                Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(str);
                double walb = Convert.ToDouble(obj["wallet_info"]["wallet_balance"].ToString()) / 100;
                if (walb < AccMoney)
                    AccMoney = walb;
                if (AccMoney < MoneyLimit)
                    MoneyLimit = AccMoney;
                //states.searchState = SearchState.Search;
                Console.WriteLine("Buying succ");
                client.Send((byte)ConnMessType.Log, "Buying succ:" + (Convert.ToDouble(l.total) / 100));
                l.BA = true;
                l.succ = true;
            }
            catch (Exception e)
            {
                //states.searchState = SearchState.Search;
                Console.WriteLine("Buying error: " + e.Message);
                client.Send((byte)ConnMessType.Log, "Buying error: " + e.Message);
                l.BA = true;
                l.succ = false;
            }
        }
        void NewKnives(List<listing2> l)
        {
            foreach(listing2 x in l)
            {
                x.price = (Convert.ToDouble(x.price) / 100).ToString();
                KnivesStats.Add(new SKnife()
                {
                    date = DateTime.Now,
                    price = x.price,
                    succ = x.succ,
                    sender = x.sender
                });
            }
            SaveStats();
            states.searchState = SearchState.Search;
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
            //FileStream fs = new FileStream("Access.key", FileMode.Create, FileAccess.Write);
            //StreamWriter sw = new StreamWriter(fs);
            //sw.Write(Crypt.Crypt.Encrypt("GC1=1;GC2=1;Offline=false"));
            ////sw.Write(Crypt.Crypt.Encrypt("nothing"));
            //sw.Close();
            //fs.Close();
            AllocConsole();
            states.clientState = ClientState.Disconnected;
            states.steamAuthState = SteamAuthState.NotAuth;
            states.serverState = ServerState.NotAuth;
            states.searchState = SearchState.Off;
            sett.Margin = new Thickness(0, 0, -sett.Width, 30);
            if (LoadSettings() && LoadStats())
            {
                System.Timers.Timer CheckAllTimer = new System.Timers.Timer(1000);
                CheckAllTimer.Elapsed += (ss, ee) =>
                {
                    new Thread(() => Init()).Start();
                };
                CheckAllTimer.Enabled = true; 
            }

        }
        private void MW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            states.searchState = SearchState.Off;
        }
        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!setts)
            {
                setts = true;
                GetCountSlider.Maximum = 100;
                GetCountSlider.Value = Convert.ToDouble(GetCount1);

                GetCountLSlider.Maximum = 100;
                GetCountLSlider.Value = Convert.ToDouble(GetCount2);

                MoneySlider.Maximum = AccMoney;
                MoneySlider.Value = MoneyLimit;

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
        bool LoadStats()
        {
            if (!File.Exists("Knives.key"))
            {
                ColorGrid.Visibility = System.Windows.Visibility.Hidden;
                return false;
            }
            else
            {
                try
                {
                    KnivesStats.Clear();
                    FileStream fs = new FileStream("Knives.key", FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs);
                    string str = Crypt.Crypt.Decrypt(sr.ReadToEnd());
                    sr.Close();
                    fs.Close();
                    if (str != "nothing")
                    {
                        foreach (string s in str.Split(new string[] { "<::>" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            KnivesStats.Add(new SKnife()
                            {
                                date = Convert.ToDateTime(s.Split(new string[] { "<:>" }, StringSplitOptions.RemoveEmptyEntries)[0]),
                                price = s.Split(new string[] { "<:>" }, StringSplitOptions.RemoveEmptyEntries)[1],
                                succ = Convert.ToBoolean(s.Split(new string[] { "<:>" }, StringSplitOptions.RemoveEmptyEntries)[2]),
                                sender = s.Split(new string[] { "<:>" }, StringSplitOptions.RemoveEmptyEntries)[3]
                            });
                        }
                    }
                    return true;
                }
                catch
                {
                    ColorGrid.Visibility = System.Windows.Visibility.Hidden;
                    return false;
                }
            }
        }
        void SaveStats()
        {
            FileStream fs = new FileStream("Knives.key", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            string str = "";
            foreach (SKnife k in KnivesStats)
                str += k.date.ToString() + "<:>" + k.price + "<:>" + k.succ.ToString() + "<:>" + k.sender + "<::>";
            sw.Write(Crypt.Crypt.Encrypt(str));
            sw.Close();
            fs.Close();
        }
        bool LoadSettings()
        {
            if (!File.Exists("Access.key"))
            {
                ColorGrid.Visibility = System.Windows.Visibility.Hidden;
                return false;
            }
            else
            {
                try
                {
                    FileStream fs = new FileStream("Access.key", FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs);
                    string[] str = Crypt.Crypt.Decrypt(sr.ReadToEnd()).Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);
                    sr.Close();
                    fs.Close();
                    GetCount1 = Convert.ToInt32(str.Where(x => x.Split('=')[0] == "GC1").First().Split('=')[1]);
                    GetCount2 = Convert.ToInt32(str.Where(x => x.Split('=')[0] == "GC2").First().Split('=')[1]);
                    CanOffline = Convert.ToBoolean(str.Where(x => x.Split('=')[0] == "Offline").First().Split('=')[1]);
                    return true;
                }
                catch
                {
                    ColorGrid.Visibility = System.Windows.Visibility.Hidden;
                    return false;
                }
            }
        }
        void SaveSettings()
        {
            FileStream fs = new FileStream("Access.key", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(Crypt.Crypt.Encrypt("GC1=" + GetCount1.ToString() + ";GC2=" + GetCount2.ToString() + ";Offline=" + CanOffline.ToString() + ";"));
            sw.Close();
            fs.Close();
        }
        private void GetCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!setts) return;
            GetCount1 = Convert.ToInt32(GetCountSlider.Value);
            LB_GetCount.Content = "Knife search count: " + GetCount1.ToString();
            SaveSettings();
        }
        private void GetCountLSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!setts) return;
            GetCount2 = Convert.ToInt32(GetCountLSlider.Value);
            LB_GetCountL.Content = "Lots scan count: " + GetCount2.ToString();
            SaveSettings();
        }
        private void MoneySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!setts) return;
            MoneyLimit = MoneySlider.Value;
            LB_Money.Content = "Money limit: " + MoneyLimit.ToString();
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
        private void B_KnivesStat_Click(object sender, RoutedEventArgs e)
        {
            Stats f = new Stats(KnivesStats);
            f.Show();
        }
        public class listing
        {
            public string sender { get; set; }
            public double price { get; set; }
        }
        public class listing2
        {
            public string listingid { get; set; }
            public string price { get; set; }
            public string fee { get; set; }
            public string total { get; set; }
            public bool BA { get; set; }
            public string sender { get; set; }
            public bool succ { get; set; }
        }
        
    }
    public class SKnife
    {
        public DateTime date { get; set; }
        public string price { get; set; }
        public bool succ { get; set; }
        public string sender { get; set; }
    }
    public enum ConnMessType
    {
        Auth,
        Sub,
        Log,
        CAct
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