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
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Phone.Shell;

namespace TrafficCamsFlanders
{
    public partial class EditFavs : PhoneApplicationPage
    {
        public List<cams> camlist_Antwerp = new List<cams>();
        public List<cams> camlist_Brussel = new List<cams>();
        public List<cams> camlist_Gent = new List<cams>();
        public List<cams> camlist_Lummen = new List<cams>();
        public List<cams> camlist_Favorites = new List<cams>();
        public List<favorite> favlist_Favorites = new List<favorite>();

        private int[] checklist = new int[4] { 0, 0, 0, 0 };
        bool working = false;


        bool delete = false;
        bool up = false;
        bool down = false;
        bool saved = true;

        private ProgressBar bar = new ProgressBar();

        public EditFavs()
        {
            InitializeComponent();
            try
            {
                refresh();
            }
            catch (Exception e) { }
        }

        public void loadfavs()
        {
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

        private void findfavs()
        {
            foreach (favorite tempfav in favlist_Favorites)
            {
                switch (tempfav.getRegion)
                {
                    case "Antwerpen": camlist_Favorites.Add(new cams() { getImage = camlist_Antwerp[tempfav.getCameranumber].getImage, getName = camlist_Antwerp[tempfav.getCameranumber].getName }); break;
                    case "Brussel": camlist_Favorites.Add(new cams() { getImage = camlist_Brussel[tempfav.getCameranumber].getImage, getName = camlist_Brussel[tempfav.getCameranumber].getName }); break;
                    case "Lummen": camlist_Favorites.Add(new cams() { getImage = camlist_Lummen[tempfav.getCameranumber].getImage, getName = camlist_Lummen[tempfav.getCameranumber].getName }); break;
                    case "Gent": camlist_Favorites.Add(new cams() { getImage = camlist_Gent[tempfav.getCameranumber].getImage, getName = camlist_Gent[tempfav.getCameranumber].getName }); break;
                }
            }

             this.LayoutRoot.Children.Remove(this.bar);
        }

        //main actions
        private void Favoriteslbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //delete fav
            if (delete == true && up == false && down == false) {
                camlist_Favorites.RemoveAt(Favoriteslbx.SelectedIndex);
                favlist_Favorites.RemoveAt(Favoriteslbx.SelectedIndex);
            }
            //move fav up
            if (delete == false && up == true && down == false) {
                try
                {
                    camlist_Favorites.Insert(Favoriteslbx.SelectedIndex - 1, camlist_Favorites[Favoriteslbx.SelectedIndex]);
                    camlist_Favorites.RemoveAt(Favoriteslbx.SelectedIndex+1);

                    favlist_Favorites.Insert(Favoriteslbx.SelectedIndex - 1, favlist_Favorites[Favoriteslbx.SelectedIndex]);
                    favlist_Favorites.RemoveAt(Favoriteslbx.SelectedIndex + 1);
                }
                catch (Exception ee) { }
            }
            //move fav down
            if (delete == false && up == false && down == true) {
                try
                {
                    camlist_Favorites.Insert(Favoriteslbx.SelectedIndex + 2, camlist_Favorites[Favoriteslbx.SelectedIndex]);
                    camlist_Favorites.RemoveAt(Favoriteslbx.SelectedIndex);

                    favlist_Favorites.Insert(Favoriteslbx.SelectedIndex + 2, favlist_Favorites[Favoriteslbx.SelectedIndex]);
                    favlist_Favorites.RemoveAt(Favoriteslbx.SelectedIndex);
                }
                catch (Exception ee) { }
            }

            delete = false;
            up = false;
            down = false;

            disablebuttons("edit", false);


            this.Favoriteslbx.ItemsSource = null;
            this.Favoriteslbx.ItemsSource = this.camlist_Favorites;

            saved = false;
        }

        //delete
        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (delete)
            {
                delete = false;
                disablebuttons("delete", false);

            }
            else
            {
                delete = true;
                disablebuttons("delete", true);

            }
        }

        //move up
        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            if (up)
            { 
                up = false;
                disablebuttons("up", false);
            }
            else
            {
                up = true; 
                disablebuttons("up", true);
            }
        }

        //down
        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            if (down)
            {
                down = false; 
                disablebuttons("down", false);
            }
            else
            {
                down = true; 
                disablebuttons("down", true);
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
           
        }

        //save on unload
        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (saved == false)
            {
                if (MessageBox.Show("yes or no", "save changes?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    savefavs();
                }
            }
        }

        //save the stuff!
        private void ApplicationBarIconButton_Click_3(object sender, EventArgs e)
        {
            savefavs();
            saved = true;
        }



        #region loadalldata
        private void refresh()
        {
            if (working == false)
            {
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
                catch (Exception e) { }
            }
            //already working

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
                //errors[0] = 1;
                //throwerror();
                //MessageBox.Show("something went wrong, not connected to the internet?");
            }
        }
        private void loaddata_gent(string msg)
        {
            this.camlist_Gent = this.loader(msg);
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
                //errors[0] = 1;
                //throwerror();
                //MessageBox.Show("something went wrong, not connected to the internet?");
            }
        }
        private void loaddata_lummen(string msg)
        {
            this.camlist_Lummen = this.loader(msg);
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
                //errors[0] = 1;
                //throwerror();
                //MessageBox.Show("something went wrong, not connected to the internet?");
            }
        }
        private void loaddata_Brussel(string msg)//brussels is the last asynchronous webcall, so do some more loading in here!
        {
            this.camlist_Brussel = this.loader(msg);
            this.LayoutRoot.Children.Remove(this.bar);
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
                try
                {
                    loadfavs();
                }
                catch (Exception ee) { }//cause it can can be empty

                working = false;
            }
        }
        #endregion
    
        private void trytoupdate(){}

        private void disablebuttons(string function, bool on)
        {
            if (on)
            {
                if (function == "delete")
                {
                    //((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = false;
                }
                if (function == "up")
                {
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                    //((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = false;
                }
                if (function == "down")
                {
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                    //((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = false;
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
                ((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = true;
            }

        }



    }
}