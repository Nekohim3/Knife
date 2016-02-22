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
using System.Diagnostics;
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
            states = new States(this);
            //NClient.Connected += NClient_Connected;
            //NClient.Disconnected += NClient_Disconnected;
            //NClient.Received += NClient_Received;
        }

        States states;
        //string ProfileImgLink = "";
        string accid = "";
        bool CanOffline = false;
        bool sub = false;
        string sessionId = "";

        string subc = "";
        string subs = "";

        bool Alive = true;
        double MoneyLimit = 0;
        double AccMoney = 0;
        double LastPrice = 0;
        bool setts = false;
        //bool wbload = false;
        bool IsInitRunned = false;
        //Point lastwndsize = new Point();
        int GetCount1 = 1;
        int GetCount2 = 1;

        List<SKnife> KnivesStats = new List<SKnife>();
        List<SKnife> KnivesStats1 = new List<SKnife>();
        string linkk = "http://google.com";
        public class States
        {
            public bool newknife = false;
            MainWindow MainW;
            public States( MainWindow mw)
            {
                MainW = mw;
            }

            private SteamAuthState _steamAuthState;
            public SteamAuthState steamAuthState
            {
                get { return _steamAuthState; }
                set
                {
                    _steamAuthState = value;
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
                            Img_ToPanel(MainW.Img_SteamAuth, 0, MainW.L_SteamAuth);
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
                    if (steamAuthState == SteamAuthState.Auth)
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
                            Img_ToPanel(MainW.Img_ServerAuth, 1, MainW.L_ServerAuth);
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
                            if(serverState == ServerState.Auth)
                            MainW.Log.Content = "Off";
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
                            if (!newknife)
                                ca.To = Color.FromArgb(255, 16, 16, 16);
                            else
                                ca.To = Color.FromArgb(127, 16, 255, 16);
                            ca.Duration = TimeSpan.FromMilliseconds(500);
                            ca.EasingFunction = new PowerEase()
                            {
                                EasingMode = EasingMode.EaseOut
                            };
                            MainW.ColorGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
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
                        }));
                    }
                }
            }

            void Img_ToCenter(Image img, Label l)
            {
                ThicknessAnimation ta = new ThicknessAnimation();
                ta.From = img.Margin;
                ta.To = new Thickness(60, 0, 0, 0);
                ta.Duration = TimeSpan.FromMilliseconds(500);
                ta.EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut
                };

                DoubleAnimation da = new DoubleAnimation();
                da.From = img.Width;
                da.To = 360;
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
                ta.To = new Thickness(480, 180*n, 0, 0);
                ta.Duration = TimeSpan.FromMilliseconds(500);
                ta.EasingFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut
                };
                DoubleAnimation da = new DoubleAnimation();
                da.From = img.Width;
                da.To = 160;
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
            NewUser,
            Deny
        }
        public enum SearchState
        {
            Off,
            Search,
            GetKnife, 
            Buying
        }

        void NClient_Received(byte MType, string Mess)
        {
            if (MType == (byte)ConnMessType.Alive)
            {
                Alive = true;
            }
            if (MType == (byte)ConnMessType.Auth)
            {
                states.serverState = (ServerState)Enum.Parse(typeof(ServerState), Mess);
            }
            if (MType == (byte)ConnMessType.Sub)
            {
                if (Mess != "")
                {
                    sub = true;
                    subc = Mess;
                    subs = subc.Split(';').Where(x => x.Split('=')[0] == "sessionid").First().Split('=')[1];
                }
                else
                {
                    sub = false;
                    subc = "";
                    subs = "";
                }
            }
            if(MType == (byte)ConnMessType.FK)
            {
                new Thread(() => { KGetLot(Mess); }).Start();
            }
        }
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
        void NClient_Disconnected()
        {
            states.serverState = ServerState.NotAuth;
            ServerAuth();
        }
        void NClient_Connected(string State)
        {
            if ((ServerState)Enum.Parse(typeof(ServerState), State) == ServerState.Deny)
                states.serverState = ServerState.NotAuth;
            else
                states.serverState = (ServerState)Enum.Parse(typeof(ServerState), State);
        }
        
        void Init()
        {
            t.Interval = 15000;
            t.Tick += t_Tick;
            t.Start();
            if (IsInitRunned)
                return;
            IsInitRunned = true;
            try
            {
                if (states.steamAuthState == SteamAuthState.NotAuth)
                {
                    SteamAuth(false);
                }
                if (states.steamAuthState == SteamAuthState.Auth && states.serverState == ServerState.NotAuth)
                {
                    ServerAuth();
                }
            }
            catch
            {
                states.steamAuthState = SteamAuthState.NotAuth;
                states.serverState = ServerState.NotAuth;
            }
            IsInitRunned = false;
        }
        int conterq = 0;
        void t_Tick(object sender, EventArgs e)
        {
            conterq++;
            if(conterq == 20)
            {
                qwer1.Clear();
            }
        }
        void SteamAuth(bool refresh)
        {
            try {
                states.steamAuthState = SteamAuthState.Authing;
                Tuple<string, string> res = SAuth.SAuth.Auth();
                AccMoney = Convert.ToDouble(res.Item1.Split(' ')[0]);
                if (AccMoney > 1500)
                    MoneyLimit = 1500;
                else
                    MoneyLimit = AccMoney;
                
                if (SAuth.CManager.Get() != "" && AccMoney != -1)
                    states.steamAuthState = SteamAuthState.Auth;
                else
                    states.steamAuthState = SteamAuthState.NotAuth;
            }
            catch
            {
                states.steamAuthState = SteamAuthState.NotAuth;
            }
            //DateTime dt = DateTime.Now;
            //
            //wbload = false;
            //try
            //{
            //    wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            //    {
            //        wb.Source = new Uri("http://google.com");
            //        wb.Source = new Uri("http://steamcommunity.com/market/"); 
            //    }));
            //    while (!wbload)
            //    {
            //        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            //        if ((DateTime.Now - dt).TotalMilliseconds > 15000)
            //        {
            //            states.steamAuthState = SteamAuthState.NotAuth;
            //            return;
            //        }
            //    } 
            //    string qwe = "";
            //    wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            //    {
            //        qwe = wb.Source.AbsoluteUri;
            //    }));
            //    if (qwe == "http://steamcommunity.com/market/" || qwe == "https://steamcommunity.com/market/")
            //    {
            //        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //        wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            //        {
            //            doc.LoadHtml(wb.ExecuteJavascriptWithResult("document.documentElement.outerHTML").ToString());
            //        }));
            //        if (doc.DocumentNode.Descendants("span").Where(x => x.Id == "marketWalletBalanceAmount").Count() != 0)
            //        {
            //            AccMoney = Convert.ToDouble(doc.DocumentNode.Descendants("span").Where(x => x.Id == "marketWalletBalanceAmount").First().InnerText.Split(' ')[0]);
            //            if (AccMoney > 1500)
            //                MoneyLimit = 1500;
            //            else
            //                MoneyLimit = AccMoney;
            //            accid = doc.DocumentNode.Descendants("span").Where(c => c.Id == "account_pulldown").First().InnerText; 
            //            string ProfileLink = doc.DocumentNode.Descendants("img").Where(x => x.Id == "headerUserAvatarIcon").First().ParentNode.GetAttributeValue("href", "");
            //            wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            //            {
            //                wb.Source = new Uri(ProfileLink);
            //            }));
            //            wbload = false;
            //            while (!wbload)
            //            {
            //                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            //                if ((DateTime.Now - dt).TotalMilliseconds > 15000)
            //                {
            //                    states.steamAuthState = SteamAuthState.NotAuth;
            //                    return;
            //                }
            //            }
            //            wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            //            {
            //                doc.LoadHtml(wb.ExecuteJavascriptWithResult("document.documentElement.outerHTML").ToString());
            //            }));
            //            ProfileImgLink = doc.DocumentNode.Descendants("div").Where(x => x.GetAttributeValue("class", "").IndexOf("playerAvatarAutoSizeInner") != -1).First().ChildNodes.Where(x => x.Name == "img").First().GetAttributeValue("src", "");
            //            if (GetCookies())
            //            {
            //                states.steamAuthState = SteamAuthState.Auth;

            //            }
            //            else
            //            {
            //                states.steamAuthState = SteamAuthState.NotAuth;
            //            }
            //        }
            //        else
            //        {
            //            states.steamAuthState = SteamAuthState.NotLogged;
            //        }
            //    }
            //}
            //catch(Exception e)
            //{
            //    states.steamAuthState = SteamAuthState.NotAuth;
            //    MessageBox.Show(e.Message.ToString() +Environment.NewLine + e.ToString());
            //}
        }
        void ServerAuth()
        {
            //NClient.Connect(ServerIp, ServerPort, accid + "<:>" + ProfileImgLink + "<:>" + MoneyLimit.ToString());
            states.serverState = ServerState.Auth;
        }
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
        bool test = false;
        string sstrnt = "http://steamcommunity.com/market/search/render/?query=&start=0&count=1&search_descriptions=0&sort_column=price&sort_dir=asc&appid=730&category_730_Type%5B%5D=tag_CSGO_Type_Knife&l=russian";
        string sstrt = "http://steamcommunity.com/market/search/render/?query=&start=0&count=1&search_descriptions=0&sort_column=price&sort_dir=asc&appid=730&category_730_ItemSet%5B%5D=any&category_730_ProPlayer%5B%5D=any&category_730_TournamentTeam%5B%5D=any&category_730_Weapon%5B%5D=any&category_730_Exterior%5B%5D=tag_WearCategory2&l=russian";
        string errstr = "";
        HttpWebRequest GetNewRequest(string targetUrl)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(targetUrl);
            request.AllowAutoRedirect = false;
            return request;
        }

        private int errcounts = 0;
        private void KSearch()
        {
            while (states.searchState != SearchState.Off)
            {
                try
                {
                    Thread.Sleep(100);
                    string sstr = "";
                    if (!test) sstr = sstrnt;
                    else sstr = sstrt;

                    string str;
                    if (!test)
                        str = SAuth.Http.Get("http://steamcommunity.com/market/search/render/",
                            "query=&start=0&count=" + GetCount1 +
                            "&search_descriptions=0&sort_column=price&sort_dir=asc&appid=730&category_730_Type%5B%5D=tag_CSGO_Type_Knife&l=russian",
                            timeout: 2500);
                    else
                        str = SAuth.Http.Get("http://steamcommunity.com/market/search/render/",
                            "query=&start=0&count=" + GetCount1 +
                            "&search_descriptions=0&sort_column=price&sort_dir=asc&appid=730&category_730_ItemSet%5B%5D=any&category_730_ProPlayer%5B%5D=any&category_730_TournamentTeam%5B%5D=any&category_730_Weapon%5B%5D=any&category_730_Exterior%5B%5D=tag_WearCategory2&l=russian",
                            timeout: 2500);
                    errcounts = 0;
                    dynamic obj = JsonConvert.DeserializeObject(str);
                    string htmlstr = obj.results_html.ToString();
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlstr);
                    List<listing> prices = new List<listing>();
                    errstr = str;
                    var q =
                        doc.DocumentNode.Descendants("div")
                            .Where(
                                x =>
                                    x.GetAttributeValue("class", "") ==
                                    "market_listing_right_cell market_listing_their_price")
                            .ToArray();
                    for (int i = 0; i < q.Count(); i++)
                    {
                        prices.Add(new listing()
                        {
                            sender = doc.GetElementbyId("resultlink_" + i.ToString()).GetAttributeValue("href", ""),
                            price =
                                Convert.ToDouble(
                                    q[i].InnerText.Replace("\t", "").Replace("\n", "").Replace("\r", "").Split(' ')[0]
                                        .Replace("От", ""))
                        });
                    }
                    prices = prices.OrderBy(x => x.price).ToList();
                    //if (prices.First().price <= 792.77 && !mode)
                    //{
                    //    NClient.Send((byte)ConnMessType.FK, prices.First().sender.ToString());
                    //}
                    //NClient.Send((byte)ConnMessType.Log, "SS:" + prices.First().price.ToString());
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
                        if (qwer1.Where(x => x.link == prices.First().sender).Count() != 0 &&
                            qwer1.Where(x => Convert.ToDouble(x.price) == prices.First().price).Count() != 0) continue;
                        DateTime dt = DateTime.Now;
                        states.searchState = SearchState.GetKnife;
                        //Console.WriteLine("Get knife");
                        linkk = prices.First().sender;
                        KGetLot(prices.First().sender);
                        KnivesStats1.Insert(0,
                            new SKnife()
                            {
                                date = DateTime.Now,
                                price = prices.First().price.ToString(),
                                sender = prices.First().sender
                            });
                        //SaveStats1(); Console.WriteLine("711" + (DateTime.Now - dt).TotalMilliseconds);
                    }


                }
                catch (Exception e)
                {
                    Console.WriteLine("[" + sac.ToString() + "/" + DateTime.Now.ToString() + "]:" + "Search error");
                    errcounts++;
                    sac++;
                    if (errcounts > 100)
                        Thread.Sleep(1000 * 60 * 30);
                    //NClient.Send((byte)ConnMessType.Log, "Se");
                }
            }
        }

        bool ba = false;
        int sac = 0;
        bool qwer = false;
        class qweq
        {
            public string link { get; set; }
            public string price { get; set; }
        }
        List<qweq> qwer1 = new List<qweq>();
        void KGetLot(string sender)
        {
            //qwer = true;
            //ba = false;
            //int tcount = 0;
            //System.Timers.Timer t = new System.Timers.Timer(100);
            //t.Elapsed += (ss, ee) =>
            //{
            //    if (tcount > 13 || ba)
            //    {
            //        qwer = false;
            //        t.Stop();
            //        t.Enabled = false;

            //    }
            //    tcount++;
            //};
            //t.Enabled = true;

            //while (qwer)
            //{
            try
            {
                Console.WriteLine("Lot: " + DateTime.Now.ToShortTimeString());
                states.searchState = SearchState.GetKnife;
                //NClient.Send((byte)ConnMessType.CAct, SearchState.GetKnife.ToString());
                string str = SAuth.Http.Get(sender + "/render",
                            "start=0&count=" + GetCount2 + "&currency=5&language=english&format=json",
                            timeout: 2500);
                
                errstr = str;
                Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(str);
                List<listing2> lst = new List<listing2>();
                foreach (Newtonsoft.Json.Linq.JToken jt in obj["listinginfo"])
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
                if (lst.Count != 0)
                {
                    foreach (listing2 l in lst)
                    {
                        if (Convert.ToDouble(l.total) / 100 <= MoneyLimit)
                        {
                            ToBuy.Add(l);
                        }
                    }
                    if (ToBuy.Count != 0)
                    {
                        states.searchState = SearchState.Buying;
                        foreach (listing2 l in ToBuy)
                            new Thread(() => KBuy(l)).Start();
                        //NClient.Send((byte)ConnMessType.Log, "GetLotsSucc: " + ToBuy.Count + "/" + lst.Count);
                        while (ToBuy.Where(x => !x.BA).Count() != 0)
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                        NewKnives(ToBuy);

                    }
                    else
                    {
                        states.searchState = SearchState.Search;
                        qwer1.Add(new qweq() { link = lst.First().sender, price = lst.First().price });
                        conterq = 0;
                        //Console.WriteLine("All knives corrupt");
                        //NClient.Send((byte)ConnMessType.Log, "All knives corrupt");
                    }
                }
                else
                {
                    states.searchState = SearchState.Search;
                    qwer1.Add(new qweq() { link = lst.First().sender, price = lst.First().price });
                    conterq = 0;
                    //Console.WriteLine("All sold");
                    //NClient.Send((byte)ConnMessType.Log, "All sold");
                }
            }
            catch (Exception e)
            {
                bool qwe = false;
                if (e.ToString().IndexOf("не содержит") != -1) qwe = true;
                states.searchState = SearchState.Search;
                Console.WriteLine("GetKnife error: " + e.ToString());
                //NClient.Send((byte)ConnMessType.Log, "GetKnife error: " + e.Message);
            }
            //}
        }
        //bool runn = false;
        void KBuy(listing2 l)
        {
            //HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://steamcommunity.com/market/buylisting/" + l.listingid);
            
            //req.Proxy = null;
            //req.Method = "POST";
            //req.Timeout = 10000;
            //req.ContentType = "application/x-www-form-urlencoded;";
            //req.Referer = l.sender;
            //if (!sub)
            //    req.Headers.Add("Cookie", SAuth.CManager.Get());
            //else
            //    req.Headers.Add("Cookie", subc);
            //req.ServicePoint.ConnectionLimit = 10;
            //byte[] sentData;
            //if (!sub)
            //    sentData = Encoding.GetEncoding(1251).GetBytes("sessionid=" + SAuth.cook.GetS() + "&currency=5&subtotal=" + l.price + "&fee=" + l.fee + "&total=" + l.total + "&quantity=1");
            //else
            //    sentData = Encoding.GetEncoding(1251).GetBytes("sessionid=" + subs + "&currency=5&subtotal=" + l.price + "&fee=" + l.fee + "&total=" + l.total + "&quantity=1");
            //req.ContentLength = sentData.Length;
            //Stream sendStream = req.GetRequestStream();
            //sendStream.Write(sentData, 0, sentData.Length);
            //sendStream.Flush();
            //sendStream.Close();
            //WebResponse resp = req.GetResponse();
            //StreamReader sr = new StreamReader(resp.GetResponseStream());
            //string str = sr.ReadToEnd();
            //sr.Close();
            try
            {
                //NClient.Send((byte)ConnMessType.CAct, SearchState.Buying.ToString());
                SAuth.Params p = new SAuth.Params();
                p.Add("sessionid", SAuth.CManager.GetS());
                p.Add("currency", "5");
                p.Add("subtotal", l.price);
                p.Add("fee", l.fee);
                p.Add("total", l.total);
                p.Add("quantity", "1");

                string str = SAuth.Http.Post("https://steamcommunity.com/market/buylisting/" + l.listingid, p.Get(), 10000, referer: l.sender);
                
                if (!sub)
                {
                    Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(str);
                    double walb = Convert.ToDouble(obj["wallet_info"]["wallet_balance"].ToString()) / 100;
                    if (walb < AccMoney)
                        AccMoney = walb;
                    if (AccMoney < MoneyLimit)
                        MoneyLimit = AccMoney;
                    //states.searchState = SearchState.Search;
                    //Console.WriteLine("Buying succ");
                    //NCclient.Send((byte)ConnMessType.Log, "Buying succ:" + (Convert.ToDouble(l.total) / 100));
                    l.BA = true;
                    l.succ = true;
                    states.newknife = true;
                }
                else
                {
                    l.BA = true;
                    l.succ = false;
                    //NClient.Send((byte)ConnMessType.NSK, "");
                }
                qwer = false;
            }
            catch (Exception e)
            {
                //states.searchState = SearchState.Search;
                Console.WriteLine("Buying error: " + e.Message);
                //NCclient.Send((byte)ConnMessType.Log, "Buying error: " + e.Message);
                l.BA = true;
                l.succ = false;
                qwer = false;
            }
        }
        void NewKnives(List<listing2> l)
        {
            bool nk = false;
            foreach(listing2 x in l)
            {
                x.price = (Convert.ToDouble(x.price) / 100).ToString();
                if (x.succ) nk = true;
                KnivesStats.Add(new SKnife()
                {
                    date = DateTime.Now,
                    price = x.price,
                    succ = x.succ,
                    sender = x.sender
                });
            }
            SaveStats(nk ? true : false);
            if (!mode)
                states.searchState = SearchState.Search;
            else
                states.searchState = SearchState.Off;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //FileStream fs = new FileStream("Knives1.key", FileMode.Create, FileAccess.Write);
            //StreamWriter sw = new StreamWriter(fs);
            //sw.Write(Crypt.Crypt.Encrypt("GC1=1;GC2=1;Offline=false"));
            //sw.Write(Crypt.Crypt.Encrypt("nothing"));
            //sw.Close();
            //fs.Close();
            AllocConsole();
            states.steamAuthState = SteamAuthState.NotAuth;
            states.serverState = ServerState.NotAuth;
            states.searchState = SearchState.Off;
            sett.Margin = new Thickness(0, 0, -sett.Width, 30);
            if (LoadSettings())
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
            //NClient.Shutdown();
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

                if (AccMoney <= 1500)
                    MoneySlider.Maximum = AccMoney;
                else
                    MoneySlider.Maximum = 1500;
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
                    string str = sr.ReadToEnd();
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
        void SaveStats(bool send)
        {
            FileStream fs = new FileStream("Knives.key", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            string str = "";
            foreach (SKnife k in KnivesStats)
                str += k.date.ToString() + "<:>" + k.price + "<:>" + k.succ.ToString() + "<:>" + k.sender + "<::>";
            string estr = str;
            sw.Write(estr);
            sw.Close();
            fs.Close();
            //if(send)
                //NClient.Send((byte)ConnMessType.NK, estr);
        }
        bool LoadStats1()
        {
            if (!File.Exists("Knives1.key"))
            {
                ColorGrid.Visibility = System.Windows.Visibility.Hidden;
                return false;
            }
            else
            {
                try
                {
                    KnivesStats1.Clear();
                    FileStream fs = new FileStream("Knives1.key", FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs);
                    string str = sr.ReadToEnd();
                    sr.Close();
                    fs.Close();
                    if (str != "nothing")
                    {
                        foreach (string s in str.Split(new string[] { "<::>" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            KnivesStats1.Add(new SKnife()
                            {
                                date = Convert.ToDateTime(s.Split(new string[] { "<:>" }, StringSplitOptions.RemoveEmptyEntries)[0]),
                                price = s.Split(new string[] { "<:>" }, StringSplitOptions.RemoveEmptyEntries)[1],
                                sender = s.Split(new string[] { "<:>" }, StringSplitOptions.RemoveEmptyEntries)[2]
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
        void SaveStats1()
        {
            FileStream fs = new FileStream("Knives1.key", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            string str = "";
            foreach (SKnife k in KnivesStats1)
                str += k.date.ToString() + "<:>" + k.price + "<:>" + k.sender + "<::>";
            sw.Write(str);
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
            //if (states.steamAuthState == SteamAuthState.NotLogged)
            //{
            //    lastwndsize = new Point(Width, Height);
            //    wb.Visibility = System.Windows.Visibility.Visible;
            //    Width = 1280;
            //    Height = 720;
            //    wb.Source = new Uri("https://steamcommunity.com/login/home/?goto=market%2F");
            //    Steam_waitforlog = true;
            //}
        }
        private void B_KnivesStat_Click(object sender, RoutedEventArgs e)
        {
            Stats s = new Stats(KnivesStats);
            s.Show();
        }
        private void B_KnivesStat_Click1(object sender, RoutedEventArgs e)
        {
            if (test)
            {
                test = false;
                this.Title = "v2";
            }
            else
            {
                test = true;
                this.Title = "v2t";
            }
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
        bool mode = false;
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            if(states.searchState != SearchState.Off)
            {
                mode = true;   
                states.searchState = SearchState.Off;
            }
            else
            {
                mode = false;
                states.searchState = SearchState.Search;
            }
            //NClient.Send((byte)ConnMessType.CAct, states.searchState.ToString());
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //AccMoney = Convert.ToDouble(SAuth.GetMoney());
            //if(AccMoney == -1)
            //{
            //    states.steamAuthState = SteamAuthState.NotAuth;
            //    MessageBox.Show("Login error. Closing");
            //    Application.Current.Shutdown();
            //}
            //else
            //{
            //    MoneySlider.Maximum = AccMoney;
            //    MoneySlider.Value = MoneyLimit;
            //}
            //wbload = false;
            //wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            //{
            //    wb.Source = new Uri("http://google.com");
            //    wb.Source = new Uri("http://steamcommunity.com/market/");
            //}));
            //while (!wbload)
            //{
            //    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            //}
            //string qwe = "";
            //wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            //{
            //    qwe = wb.Source.AbsoluteUri;
            //}));
            //if (qwe == "http://steamcommunity.com/market/" || qwe == "https://steamcommunity.com/market/")
            //{
            //    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //    wb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            //    {
            //        doc.LoadHtml(wb.ExecuteJavascriptWithResult("document.documentElement.outerHTML").ToString());
            //    }));
            //    if (doc.DocumentNode.Descendants("span").Where(x => x.Id == "marketWalletBalanceAmount").Count() != 0)
            //    {
            //        AccMoney = Convert.ToDouble(doc.DocumentNode.Descendants("span").Where(x => x.Id == "marketWalletBalanceAmount").First().InnerText.Split(' ')[0]);
            //        MoneySlider.Maximum = AccMoney;
            //        MoneySlider.Value = MoneyLimit;
            //    }
            //}
        }

        private void ColorGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (states.newknife)
            {
                if (states.searchState == SearchState.Search || states.searchState == SearchState.Off)
                {
                    ColorAnimation ca = new ColorAnimation();
                    ca.From = ((SolidColorBrush)ColorGrid.Background).Color;
                    ca.To = Color.FromArgb(255, 16, 16, 16);
                    ca.Duration = TimeSpan.FromMilliseconds(500);
                    ca.EasingFunction = new PowerEase()
                    {
                        EasingMode = EasingMode.EaseOut
                    };
                    ColorGrid.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                }
                states.newknife = false;
                Stats s = new Stats(KnivesStats);
                s.Show();
            }
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
        CAct,
        Alive,
        NK,
        upd,
        NSK,
        FK
    }
    //public static class NClient
    //{
    //    public static event ConnectedEventHandler Connected;
    //    public delegate void ConnectedEventHandler(string State);

    //    public static event DisconnectedEventHandler Disconnected;
    //    public delegate void DisconnectedEventHandler();

    //    public static event ReceivedEventHandler Received;
    //    public delegate void ReceivedEventHandler(byte MType, string Mess);

    //    static NetClient s_client;
    //    static NClient()
    //    {
    //        NetPeerConfiguration config = new NetPeerConfiguration("Knife");
    //        config.AutoFlushSendQueue = false;
    //        config.ConnectionTimeout = 10;
    //        s_client = new NetClient(config);
    //        s_client.RegisterReceivedCallback(new SendOrPostCallback(callb));
    //    }
    //    public static void Connect(string host, int port, string AM)
    //    {
    //        s_client.Start();
    //        NetOutgoingMessage hail = s_client.CreateMessage(AM);
    //        s_client.Connect(host, port, hail);
    //    }
    //    public static void Shutdown()
    //    {
    //        s_client.Disconnect("Requested by user");
    //        s_client.Shutdown("");
    //    }
    //    public static void Send(byte MType, string Mess)
    //    {
    //        string ToSend = MType.ToString() + "<:SS:>" + Mess;
    //        NetOutgoingMessage om = s_client.CreateMessage(ToSend);
    //        s_client.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
    //        s_client.FlushSendQueue();
    //    }
    //    static void callb(object peer)
    //    {
    //        NetIncomingMessage im;
    //        while ((im = s_client.ReadMessage()) != null)
    //        {
    //            // handle incoming message
    //            switch (im.MessageType)
    //            {
    //                case NetIncomingMessageType.DebugMessage:
    //                case NetIncomingMessageType.ErrorMessage:
    //                case NetIncomingMessageType.WarningMessage:
    //                case NetIncomingMessageType.VerboseDebugMessage:
    //                    string text = im.ReadString();
    //                    //Output(text);
    //                    break;
    //                case NetIncomingMessageType.StatusChanged:
    //                    NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

    //                    if (status == NetConnectionStatus.Connected)
    //                    {
    //                        ConnectedEventHandler CE = Connected;
    //                        if (CE != null)
    //                            CE(im.SenderConnection.RemoteHailMessage.ReadString());
    //                    }
    //                    if (status == NetConnectionStatus.Disconnected)
    //                    {
    //                        DisconnectedEventHandler DE = Disconnected;
    //                        if (DE != null)
    //                            DE();
    //                    }

    //                    string reason = im.ReadString();
    //                    //Output(status.ToString() + ": " + reason);

    //                    break;
    //                case NetIncomingMessageType.Data:
    //                    string msg = im.ReadString();
    //                    byte MType = Convert.ToByte(msg.Split(new string[]{"<:SS:>"}, StringSplitOptions.None)[0]);
    //                    string Mess = msg.Split(new string[]{"<:SS:>"}, StringSplitOptions.None)[1];
    //                    ReceivedEventHandler RE = Received;
    //                    if (RE != null)
    //                        RE(MType, Mess);
    //                    break;
    //                default:
    //                    //Output("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes");
    //                    break;
    //            }
    //            s_client.Recycle(im);
    //        }
    //    }
    //}
}