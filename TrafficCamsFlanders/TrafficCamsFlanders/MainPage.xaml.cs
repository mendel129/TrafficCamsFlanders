using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;

namespace TrafficCamsFlanders
{
    public partial class MainPage : PhoneApplicationPage
    {
        public List<cams> camlist_Antwerp = new List<cams>();
        public List<cams> camlist_Brussel = new List<cams>();
        public List<cams> camlist_Gent = new List<cams>();
        public List<cams> camlist_Lummen = new List<cams>();
        public List<cams> camlist_Favorites = new List<cams>();
        public List<favorite> favlist_Favorites = new List<favorite>();

        private List<string> placelistantwerp = new List<string>();
        private List<string> placelistbrussel = new List<string>();
        private List<string> placelistgent = new List<string>();
        private List<string> placelistlummen = new List<string>();

        bool working = false;
        bool check = true;//used for recognition from live tile

        private ProgressBar bar = new ProgressBar();
        bool addfavs = false;
        private int[] checklist = new int[4] {0, 0, 0, 0};
        //private int[] errors = new int[4] { 0, 0, 0, 0 };
        bool createlivetile = false;

        IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
        bool firstrunindicator = true;
        bool needsupdate = true;
      
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            createagent();

           // this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            //check if it is the first run of the app
            try
            {
                firstrunindicator = Convert.ToBoolean((string)appSettings["firstrunindicator"]);
                //quickexit = Convert.ToBoolean((string)appSettings["quickexit"]);
            }
            catch (Exception e) { }

            //check if update required
            try
            {
                needsupdate = Convert.ToBoolean((string)appSettings["updateindicator11"]);
            }
            catch(Exception ee){}

            //try
            //{
            //    updateto12done = Convert.ToBoolean((string)appSettings["updateto12done"]);
            //}
            //catch (Exception e) { }

            if (firstrunindicator)// || updateto12done)
            {
                firstrun();
            }


            try
            {
                needsupdate = Convert.ToBoolean((string)appSettings["updateindicator11"]);
            }
            catch (Exception ee) { }

            //version 1.1 needs new live tiles
            if(needsupdate){
                MessageBox.Show("because of an error in the previous version, all live tiles have to be recreated, my bad! (sorry :-) )");
                deletealltiles();
                appSettings.Add("updateindicator11", "false");
             }

        }
        //on navigated to
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs ee) 
        {
            try
            {
                refresh();
            }
            catch (Exception e) { MessageBox.Show("something went wrong"); }
        }
        //private void MainPage_Loaded(object sender, RoutedEventArgs e)
        //{
        //   // refresh();
        //}

        //load favorites
        public void loadfavs() 
        {
            camlist_Favorites.Clear();
            try
            {
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile("favcams.xml", FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<favorite>));
                        favlist_Favorites = (List<favorite>)serializer.Deserialize(stream);
                    }
                }
            }
            catch
            {
                //add some code here
            }

            findfavs();
            
            this.Favoriteslbx.ItemsSource = null;
            this.Favoriteslbx.ItemsSource = this.camlist_Favorites;
        }
        //save the favorites
        public void savefavs() 
        { 
            // Write to the Isolated Storage
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;

            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile("favcams.xml", FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<favorite>));
                    using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                    {
                        serializer.Serialize(xmlWriter, favlist_Favorites);
                    } 
                }
            }
        }
        //function to find favorites from html
        private void findfavs() 
        { 
            foreach(favorite tempfav in favlist_Favorites)
            {
                switch (tempfav.getRegion)
                {
                    case "Antwerpen": camlist_Favorites.Add(new cams() { getImage = camlist_Antwerp[tempfav.getCameranumber].getImage, getName = camlist_Antwerp[tempfav.getCameranumber].getName }); break;
                    case "Brussel": camlist_Favorites.Add(new cams() { getImage = camlist_Brussel[tempfav.getCameranumber].getImage, getName = camlist_Brussel[tempfav.getCameranumber].getName }); break;
                    case "Lummen": camlist_Favorites.Add(new cams() { getImage = camlist_Lummen[tempfav.getCameranumber].getImage, getName = camlist_Lummen[tempfav.getCameranumber].getName }); break;
                    case "Gent": camlist_Favorites.Add(new cams() { getImage = camlist_Gent[tempfav.getCameranumber].getImage, getName = camlist_Gent[tempfav.getCameranumber].getName }); break;
                }
            }
        }

        private void addfav(string region, int cameranumber, string name)
        {

            if (MessageBox.Show(" ", "add " + name + " to favs?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //camlist_Favorites.Add(new cams() { getImage = imagelocation, getName = cameranumber });
                favlist_Favorites.Add(new favorite() { getRegion = region, getCameranumber = cameranumber });
                savefavs();
                loadfavs();
            }

            Favoriteslbx.SelectedIndex = -1;
            addfavs = false;
            disablebuttons("addfavs", false);
        }


        #region loadalldata
        private void refresh()
        {
            if (working == false && connected() == true)
            {
                placelistlummen.Clear();
                placelistgent.Clear();
                placelistbrussel.Clear();
                placelistantwerp.Clear();
                favlist_Favorites.Clear();
                camlist_Favorites.Clear(); 
                camlist_Lummen.Clear(); 
                camlist_Gent.Clear(); 
                camlist_Brussel.Clear(); 
                camlist_Antwerp.Clear();

                this.Favoriteslbx.ItemsSource = null;
                this.Antwerpenlbx.ItemsSource = null;
                this.Brussellbx.ItemsSource = null;
                this.Gentlbx.ItemsSource = null;
                this.Lummenlbx.ItemsSource = null;

                checklist = new int[4] {0, 0, 0, 0}; 
                
                working = true;
                try
                {
                    this.bar.IsIndeterminate = true;
                    this.LayoutRoot.Children.Add(this.bar);
                    string text = "http://www.verkeerscentrum.be/verkeersinfo/camerabeelden/antwerpen";
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(text);
                    IAsyncResult asyncResult = httpWebRequest.BeginGetResponse(new AsyncCallback(this.AntwerpGetInfoCallBack), httpWebRequest);
                    string text2 = "http://www.verkeerscentrum.be/verkeersinfo/camerabeelden/gent";
                    HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create(text2);
                    IAsyncResult asyncResult2 = httpWebRequest2.BeginGetResponse(new AsyncCallback(this.GentGetInfoCallBack), httpWebRequest2);
                    string text3 = "http://www.verkeerscentrum.be/verkeersinfo/camerabeelden/lummen";
                    HttpWebRequest httpWebRequest3 = (HttpWebRequest)WebRequest.Create(text3);
                    IAsyncResult asyncResult3 = httpWebRequest3.BeginGetResponse(new AsyncCallback(this.LummenGetInfoCallBack), httpWebRequest3);
                    string text4 = "http://www.verkeerscentrum.be/verkeersinfo/camerabeelden/brussel";
                    HttpWebRequest httpWebRequest4 = (HttpWebRequest)WebRequest.Create(text4);
                    IAsyncResult asyncResult4 = httpWebRequest4.BeginGetResponse(new AsyncCallback(this.BrusselGetInfoCallBack), httpWebRequest4);
                }
                catch (Exception ee) 
                {
                    MessageBox.Show("something went wrong, not connected to the internet?");
                }
            }
            //already working
            if (connected() == false)
            {
                MessageBox.Show("please connect to the internet");
            }
        }
        private void AntwerpGetInfoCallBack(IAsyncResult result)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)result.AsyncState;
                WebResponse webResponse = httpWebRequest.EndGetResponse(result);
                string returnValue;
                using (Stream responseStream = webResponse.GetResponseStream())
                {
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        returnValue = streamReader.ReadToEnd();
                    }
                }
                webResponse.Close();
                Action<string> act = new Action<string>(this.loaddata_antwerp);
                this.Dispatcher.BeginInvoke(act, returnValue);
            }
            catch (Exception ee)
            {
                //errors[0] = 1;
                //throwerror();
                //MessageBox.Show("something went wrong, not connected to the internet?");
            }
        }
        private void loaddata_antwerp(string msg)
        {
            this.camlist_Antwerp = this.loader(msg);
            this.Antwerpenlbx.ItemsSource=this.camlist_Antwerp;
            checklist[0] = 1;
            trybindfavorites();
        }
        private void GentGetInfoCallBack(IAsyncResult result)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)result.AsyncState;
                WebResponse webResponse = httpWebRequest.EndGetResponse(result);
                string returnValue;
                using (Stream responseStream = webResponse.GetResponseStream())
                {
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        returnValue = streamReader.ReadToEnd();
                    }
                }
                webResponse.Close();
                Action<string> act = new Action<string>(this.loaddata_gent);
                this.Dispatcher.BeginInvoke(act, returnValue);
            }
            catch (Exception ee)
            {
                //MessageBox.Show("something went wrong, not connected to the internet?");
            }
        }
        private void loaddata_gent(string msg)
        {
            this.camlist_Gent = this.loader(msg);
            this.Gentlbx.ItemsSource = this.camlist_Gent;
            checklist[1] = 1;
            trybindfavorites();
        }
        private void LummenGetInfoCallBack(IAsyncResult result)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)result.AsyncState;
                WebResponse webResponse = httpWebRequest.EndGetResponse(result);
                string returnValue;
                using (Stream responseStream = webResponse.GetResponseStream())
                {
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        returnValue = streamReader.ReadToEnd();
                    }
                }
                webResponse.Close();
                Action<string> act = new Action<string>(this.loaddata_lummen);
                this.Dispatcher.BeginInvoke(act, returnValue);
            }
            catch (Exception ee)
            {
                //MessageBox.Show("something went wrong, not connected to the internet?");
            }
        }
        private void loaddata_lummen(string msg)
        {
            this.camlist_Lummen = this.loader(msg);
            this.Lummenlbx.ItemsSource=this.camlist_Lummen;
            checklist[2] = 1;
            trybindfavorites();
        }
        private void BrusselGetInfoCallBack(IAsyncResult result)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)result.AsyncState;
                WebResponse webResponse = httpWebRequest.EndGetResponse(result);
                string returnValue;
                using (Stream responseStream = webResponse.GetResponseStream())
                {
                    using (StreamReader streamReader = new StreamReader(responseStream))
                    {
                        returnValue = streamReader.ReadToEnd();
                    }
                }
                webResponse.Close();
                Action<string> act = new Action<string>(this.loaddata_Brussel);
                this.Dispatcher.BeginInvoke(act, returnValue);
            }
            catch (Exception ee)
            {
                //MessageBox.Show("something went wrong, not connected to the internet?");
            }
        }
        private void loaddata_Brussel(string msg)//brussels is the last asynchronous webcall, so do some more loading in here!
        {
            this.camlist_Brussel = this.loader(msg);
            this.LayoutRoot.Children.Remove(this.bar);
            this.Brussellbx.ItemsSource = this.camlist_Brussel;
            checklist[3] = 1;
            trybindfavorites();

        }
        private List<cams> loader(string msg)
        {
            string name = "";
            List<string> list = new List<string>();
            int num = 0;
            string image = "";
            List<cams> list2 = new List<cams>();
            string text = msg;
            string text2 = msg;
            int num2 = text.IndexOf("class=\"Titel_bericht\">Regio ");
            int num3 = text.IndexOf("<", num2);
            string text3 = text.Substring(num2 + 28, num3 - num2 - 28);
            text = text.Substring(num3);
            text2 = text2.Substring(num3);
            bool flag = true;
            while (flag)
            {
                try
                {
                    int num4 = 0;
                    num = 0;
                    num4 = text2.IndexOf("src=\"/camera-images/", num4);
                    num = text2.IndexOf(" border=\"0\"></td></tr>", num4);
                    image = "http://www.verkeerscentrum.be/camera-images/" + text2.Substring(num4 + 20, num - num4 - 21);
                    text2 = text2.Substring(num);
                }
                catch (Exception var_12_CF)
                {
                    flag = false;
                    break;
                }
                try
                {
                    num2 = 0;
                    num2 = text.IndexOf("<img id=\"", num2);
                    num3 = text.IndexOf("pos", num2);
                    if (num2 < num)
                    {
                        name = text.Substring(num2 + 9, num3 - num2 - 11);
                        text = text.Substring(num3);
                    }
                    else
                    {
                        name = text3;
                        text = text.Substring(num);
                    }
                }
                catch (Exception var_12_CF)
                {
                    name = text3;
                }
                //temp.Add(new computerset() { Name = "PC-DeSwaef", Mac = "00:24:1D:86:70:AD" });
                list2.Add(new cams() { getImage = image, getName = name });
            }
            return list2;
        }
        private void trybindfavorites() 
        {
            if (checklist[0] == 1 && checklist[1] == 1 && checklist[2] == 1 && checklist[3] == 1)
            {
                disablebuttons("edit", false);

                try//when coming from live tile
                {
                    string id = "";
                    string regio = "";

                    string text = "";
                    string text2 = "";
                    if (base.NavigationContext.QueryString.TryGetValue("id", out text))
                    {
                        id = text;
                    }
                    if (base.NavigationContext.QueryString.TryGetValue("regio", out text2))
                    {
                        regio = text2;
                    }

                    int idint = Convert.ToInt32(id);
                    string imageloc = "";
                    string selectedname = "";

                    switch (regio)
                    {
                        case "Antwerpen": imageloc = camlist_Antwerp[idint].getImage; selectedname = camlist_Antwerp[idint].getName; break;
                        case "Brussel": imageloc = camlist_Brussel[idint].getImage; selectedname = camlist_Brussel[idint].getName; break;
                        case "Lummen": imageloc = camlist_Lummen[idint].getImage; selectedname = camlist_Lummen[idint].getName; break;
                        case "Gent": imageloc = camlist_Gent[idint].getImage; selectedname = camlist_Gent[idint].getName; break;
                    }

                    if (check)
                    { 
                        base.NavigationService.Navigate(new Uri("/DetailPage.xaml?selectedItem=" + imageloc + "&selectedName=" + selectedname, UriKind.Relative)); 
                    }
                    check = false;
                    //this.Lummenlbx.SelectedIndex = -1;
                }
                catch (Exception e) { }

                try
                {
                    updatetiles();
                    loadfavs();
                }
                catch (Exception ee) { }//cause it can can be empty

                working = false;
            }
        }
        #endregion

        //on selected camera in antwerp listbox
        private void Antwerpenlbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (addfavs) 
            {
                addfav("Antwerpen", this.Antwerpenlbx.SelectedIndex, this.camlist_Antwerp[this.Antwerpenlbx.SelectedIndex].getName);
            }
            else if (createlivetile)
            {
                createtile(Antwerpenlbx.SelectedIndex, "Antwerpen");
            }
            else
            {
                if (this.Antwerpenlbx.SelectedIndex != -1)
                {
                    base.NavigationService.Navigate(new Uri("/DetailPage.xaml?selectedItem=" + this.camlist_Antwerp[this.Antwerpenlbx.SelectedIndex].getImage + "&selectedName=" + this.camlist_Antwerp[this.Antwerpenlbx.SelectedIndex].getName, UriKind.Relative));
                }
            }

            addfavs = false;
            createlivetile = false;
            this.Antwerpenlbx.SelectedIndex = -1;
        }
        //on selected camera in brussel listbox
        private void Brussellbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (addfavs)
            {
                addfav("Brussel", this.Brussellbx.SelectedIndex, this.camlist_Brussel[this.Brussellbx.SelectedIndex].getName);
            }
            else if (createlivetile)
            {
                createtile(Brussellbx.SelectedIndex, "Brussel");
            }
            else
            {
                if (this.Brussellbx.SelectedIndex != -1)
                {
                    base.NavigationService.Navigate(new Uri("/DetailPage.xaml?selectedItem=" + this.camlist_Brussel[this.Brussellbx.SelectedIndex].getImage + "&selectedName=" + this.camlist_Brussel[this.Brussellbx.SelectedIndex].getName, UriKind.Relative));
                }
            }

            addfavs = false;
            createlivetile = false;
            this.Brussellbx.SelectedIndex = -1;
        }
        //on selected camera in gent listbox
        private void Gentlbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (addfavs) 
            {
                addfav("Gent", this.Gentlbx.SelectedIndex, this.camlist_Gent[this.Gentlbx.SelectedIndex].getName);
            }
            else if (createlivetile)
            {
                createtile(Gentlbx.SelectedIndex, "Gent");
            }
            else
            {
                if (this.Gentlbx.SelectedIndex != -1)
                {
                    base.NavigationService.Navigate(new Uri("/DetailPage.xaml?selectedItem=" + this.camlist_Gent[this.Gentlbx.SelectedIndex].getImage + "&selectedName=" + this.camlist_Gent[this.Gentlbx.SelectedIndex].getName, UriKind.Relative));  
                }
            }

            addfavs = false;
            createlivetile = false;
            this.Gentlbx.SelectedIndex = -1;
        }
        //on selected camera in lummen listbox
        private void Lummenlbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (addfavs) 
            {
                addfav("Lummen", this.Lummenlbx.SelectedIndex, this.camlist_Lummen[this.Lummenlbx.SelectedIndex].getName);
            }
            else if (createlivetile)
            {
                createtile(Lummenlbx.SelectedIndex, "Lummen");
            }
            else
            {
                if (this.Lummenlbx.SelectedIndex != -1)
                {
                    base.NavigationService.Navigate(new Uri("/DetailPage.xaml?selectedItem=" + this.camlist_Lummen[this.Lummenlbx.SelectedIndex].getImage + "&selectedName=" + this.camlist_Lummen[this.Lummenlbx.SelectedIndex].getName, UriKind.Relative));  
                }
            }

            addfavs = false;
            createlivetile = false;
            this.Lummenlbx.SelectedIndex = -1;
        }

        //refresh
        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (connected())
            {
                this.refresh();
                disablebuttons("refresh", true);
            }
            else
                MessageBox.Show("please connect to the internet first");
        }
        //about
        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            base.NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        //on selected camera in favorites listbox
        private void Favoriteslbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Favoriteslbx.SelectedIndex != -1)
            {
                if (addfavs)
                {
                    MessageBox.Show("operation not supported on favorites");
                }
                else if (createlivetile)
                {
                    string region = favlist_Favorites[this.Favoriteslbx.SelectedIndex].getRegion;
                    int index = favlist_Favorites[this.Favoriteslbx.SelectedIndex].getCameranumber;
                    createtile(index, region); ;
                }
                else
                {

                    if (this.Favoriteslbx.SelectedIndex != -1)
                    {
                        base.NavigationService.Navigate(new Uri("/DetailPage.xaml?selectedItem=" + this.camlist_Favorites[this.Favoriteslbx.SelectedIndex].getImage + "&selectedName=" + this.camlist_Favorites[this.Favoriteslbx.SelectedIndex].getName, UriKind.Relative));
                        this.Favoriteslbx.SelectedIndex = -1;
                    }
                }

                    this.Favoriteslbx.SelectedIndex = -1;
            }
        }

        //add to favorite
        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)//add to favorite
        {
            if (addfavs)
            { 
                addfavs = false;
                disablebuttons("addfavs", false);

            }
            else
            {
                addfavs = true; 
                disablebuttons("addfavs", true);

            }
        }

        //open filebeeld.be
        private void textBlock1_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MessageBox.Show("will exit app...", "open filebeeld.be?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = (new Uri("http://m.filebeeld.be"));
                webBrowserTask.Show();
            }
        }

        //edit favorites
        private void ApplicationBarMenuItem_Click_1(object sender, EventArgs e)
        {
            if (connected())
                NavigationService.Navigate(new Uri("/EditFavs.xaml", UriKind.Relative));
            else
                MessageBox.Show("please connect to the internet first");
        }

        private void disablebuttons(string function, bool on)
        {
            if (on)
            {
                if (function == "refresh")
                {
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
                    //((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = false;
                }
                if (function == "addfavs")
                {
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                    //((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
                    //((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = false;
                }
                if (function == "addtile")
                {
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                    //((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
                //    ((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = false;
                }
                //if (function == "save") 
                //{
                //    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                //    ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                //    ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
                //    //((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = false;
                //}
            }
            else
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
                ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = true;
                //((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = true;
            }

        }

        private void createtile(int tile, string regio) 
        {
            try
            {
                //refresh();
                string imageloc = "";
                string name = "";
                switch (regio)
                {
                    case "Antwerpen": imageloc = camlist_Antwerp[tile].getImage; name = camlist_Antwerp[tile].getName; break;
                    case "Brussel": imageloc = camlist_Brussel[tile].getImage; name = camlist_Brussel[tile].getName; break;
                    case "Lummen": imageloc = camlist_Lummen[tile].getImage; name = camlist_Lummen[tile].getName; break;
                    case "Gent": imageloc = camlist_Gent[tile].getImage; name = camlist_Gent[tile].getName; break;
                }

                //addfav("Brussel", this.Brussellbx.SelectedIndex, this.camlist_Brussel[this.Brussellbx.SelectedIndex].getName);
                //createlivetile
                StandardTileData NewTileData = new StandardTileData
                {
                    //BackgroundImage = new Uri(imageloc, UriKind.RelativeOrAbsolute),
                    BackgroundImage = new Uri("/cams.png", UriKind.RelativeOrAbsolute),
                    Title = name,
                    //BackTitle = name,
                    //BackContent = "",
                    //BackBackgroundImage = new Uri("Blue.jpg", UriKind.Relative) 
                };
                ShellTile.Create(new Uri("/MainPage.xaml?id=" + tile + "&regio=" + regio, UriKind.Relative), NewTileData); //exits application ?action=" + listBox1.SelectedIndex
                createlivetile = false;
                updatetiles();

                foreach (favorite tempfav in favlist_Favorites)
                {

                }
            }
            catch (Exception e) { MessageBox.Show("Tile already exist"); }
            
        }

        void updatetiles()
        {
            refresh();
            try
            {
                foreach (ShellTile TileToFind in ShellTile.ActiveTiles)
                {
                    string regio = "";
                    int tile = 0;
                    string imageloc = "";
                    try
                    {
                        //ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("id"));

                        string all = TileToFind.NavigationUri.ToString();

                        regio = all.Substring(all.IndexOf("regio") + 6);

                        string temp = "";
                        int tempone = all.IndexOf("id");
                        int temptwo = all.IndexOf("&regio");
                        int length = temptwo - tempone - 3;
                        temp = all.Substring(tempone + 3, length);
                        tile = Convert.ToInt32(temp);


                        switch (regio)
                        {
                            case "Antwerpen": imageloc = camlist_Antwerp[tile].getImage; break;
                            case "Brussel": imageloc = camlist_Brussel[tile].getImage; break;
                            case "Lummen": imageloc = camlist_Lummen[tile].getImage; break;
                            case "Gent": imageloc = camlist_Gent[tile].getImage; break;
                        }

                        if (TileToFind != null)
                        {
                            StandardTileData NewTileData = new StandardTileData
                            {
                                BackgroundImage = new Uri(imageloc, UriKind.RelativeOrAbsolute),
                                //Title = "updated by scheduled task",
                                //Count = System.DateTime.Now.Second
                            };
                            TileToFind.Update(NewTileData);
                        }
                    }
                    catch (Exception e) { }
                }
            }
            catch (Exception ee) { }//cause it can can be empty
        }

        void createagent() 
        {
            try
            {

                PeriodicTask periodicTask = new PeriodicTask("PeriodicAgent");


                periodicTask.Description = "Updates traffic camera's for TrafficCamsFlanders";
                //periodicTask.ExpirationTime = System.DateTime.Now.AddDays(1);

                // If the agent is already registered with the system,
                if (ScheduledActionService.Find(periodicTask.Name) != null)
                {
                    ScheduledActionService.Remove("PeriodicAgent");
                }


                //not supported in current version
                //periodicTask.BeginTime = DateTime.Now.AddSeconds(10);


                //only can be called when application is running in foreground
                ScheduledActionService.Add(periodicTask);


                ScheduledActionService.LaunchForTest("PeriodicAgent", System.TimeSpan.FromSeconds(5));
            }
            catch (Exception e) { 
               // MessageBox.Show("Failed to create background agent, check settings to enable"); 
            }
            
        }

        private void StartPeriodicAgent()
        {
//            // Obtain a reference to the period task, if one exists
//            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

//            // If the task already exists and background agents are enabled for the
//            // application, you must remove the task and then add it again to update
//            // the schedule
//            if (periodicTask != null)
//            {
//                //RemoveAgent(periodicTaskName);
//            }

//            periodicTask = new PeriodicTask(periodicTaskName);

//            // The description is required for periodic agents. This is the string that the user
//            // will see in the background services Settings page on the device.
//            periodicTask.Description = "This demonstrates a periodic task.";

//            // Place the call to Add in a try block in case the user has disabled agents.
//            try
//            {
//                ScheduledActionService.Add(periodicTask);
//                PeriodicStackPanel.DataContext = periodicTask;

//                // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
//#if(DEBUG_AGENT)
//            ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(60));
//#endif
//            }
//            catch (InvalidOperationException exception)
//            {
//                if (exception.Message.Contains("BNS Error: The action is disabled"))
//                {
//                    MessageBox.Show("Background agents for this application have been disabled by the user.");
//                }
//            }
        }

        private void deletetile(int tile)
        {
            string tempstring = tile.ToString();//id of the tile
            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(tempstring));

            // If the Tile was found, then delete it.
            if (TileToFind != null)
            {
                TileToFind.Delete();
            }

        }

        //create livetile
        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            if (createlivetile)
            {
                createlivetile = false;
                disablebuttons("addtile", false);
                
            }
            else 
            {
                createlivetile = true;
                disablebuttons("addtile", true);
            }
        }

        private bool connected()
        {
            bool thereturn=false;
            string type = Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType.ToString();
            if (type == "None")
            {
                thereturn = false;
            }
            else 
            {
                thereturn = true;
            }
            return thereturn;
        }

        private void firstrun() 
        {
            if (firstrunindicator)// || updateto12done)
            {
                try
                {
                    appSettings.Remove("firstrunindicator");
                    appSettings.Remove("quickexit");
                    appSettings.Remove("updateindicator11");
                }
                catch (Exception e) { }
                if (MessageBox.Show("This app uses data. For live tiles it uses background agents which could lower battery life", "agree?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    //MessageBox.Show("");
                    appSettings.Add("firstrunindicator", "false");
                    appSettings.Add("updateindicator11", "false");
                }
                else
                {
                    NavigationService.GoBack();
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }

            //if (updateto12done) //actions for an update 
            //{

            //}
            
            
           
        }

        void deletealltiles() 
        {
            try
            {
                ShellTile tile = ShellTile.ActiveTiles.Last();
                tile.Delete();
                deletealltiles();
            }
            catch (Exception e) { }
        }

        //help page
        private void ApplicationBarMenuItem_Click_2(object sender, EventArgs e)
        {
            if (connected())
                NavigationService.Navigate(new Uri("/HelpPage.xaml", UriKind.Relative));
            else
                MessageBox.Show("please connect to the internet first");
        }





    }//the end







    public class cams
    {
        private string image;
        private string name;

        public string getImage
        {
            get { return image; }
            set { image = value; }
        }
        public string getName
        {
            get {  return name; }
            set { name = value; }
        }

    }

    public class favorite
    {
        private string region;
        private int cameranumber;

        public string getRegion
        {
            get { return region; }
            set { region = value; }
        }
        public int getCameranumber
        {
            get { return cameranumber; }
            set { cameranumber = value; }
        }
    }
}