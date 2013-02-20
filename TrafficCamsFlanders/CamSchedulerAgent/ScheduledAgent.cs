using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using System.Threading;
//using System.Windows.Threading;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Serialization;





namespace CamSchedulerAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        public List<cams> camlist_Antwerp = new List<cams>();
        public List<cams> camlist_Brussel = new List<cams>();
        public List<cams> camlist_Gent = new List<cams>();
        public List<cams> camlist_Lummen = new List<cams>();
        //public List<cams> camlist_Favorites = new List<cams>();
        //public List<favorite> favlist_Favorites = new List<favorite>();

        private List<string> placelistantwerp = new List<string>();
        private List<string> placelistbrussel = new List<string>();
        private List<string> placelistgent = new List<string>();
        private List<string> placelistlummen = new List<string>();

        private int[] checklist = new int[4] { 0, 0, 0, 0 };

        bool working = false;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            if (task is PeriodicTask)
            {
                //ShellToast toast = new ShellToast();
                //toast.Title = "TrafficCamsFlanders";
                //toast.Content = "running update";
                //toast.Show();
                
                try
                {
                    refresh();
                }
                catch (Exception e) 
                {
                   // ShellToast toaste = new ShellToast();
                   // toaste.Title = "Error in TrafficCamsFlanders: ";
                  //  toaste.Content = e.ToString();
                  //  toaste.Show();
                }
                //ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("id"));

                //string all = TileToFind.NavigationUri.ToString();
                //string regio = "";
                //regio=all.Substring(all.IndexOf("regio")+6);
                //int tile;
                //string temp = "";
                //int tempone = all.IndexOf("id");
                //int temptwo = all.IndexOf("&regio");
                //int length = temptwo - tempone - 3;
                //temp = all.Substring(tempone + 3, length);
                //tile=Convert.ToInt32( temp);
                //string imageloc="";
                //switch (regio)
                //{
                //    case "Antwerpen": imageloc = camlist_Antwerp[tile].getImage; break;
                //    case "Brussel": imageloc = camlist_Brussel[tile].getImage; break;
                //    case "Lummen": imageloc = camlist_Lummen[tile].getImage; break;
                //    case "Gent": imageloc = camlist_Gent[tile].getImage; break;
                //}

                //if (TileToFind != null)
                //{
                //    StandardTileData NewTileData = new StandardTileData
                //    {
                //        BackgroundImage = new Uri(imageloc, UriKind.RelativeOrAbsolute),
                //        //Title = "updated by scheduled task",
                //        Count = System.DateTime.Now.Second
                //    };
                //    TileToFind.Update(NewTileData);
                //}
            }
            else
            {
                // Execute resource-intensive task actions here.
            }

            //NotifyComplete();
        }

        #region everything
        private void refresh()
        {
            if (working == false && connected() == true)
            {
                placelistlummen.Clear();
                placelistgent.Clear();
                placelistbrussel.Clear();
                placelistantwerp.Clear();
                //favlist_Favorites.Clear();
                //camlist_Favorites.Clear();
                camlist_Lummen.Clear();
                camlist_Gent.Clear();
                camlist_Brussel.Clear();
                camlist_Antwerp.Clear();

                checklist = new int[4] { 0, 0, 0, 0 };

                working = true;
                try
                {
                   // this.bar.IsIndeterminate = true;
                    //this.LayoutRoot.Children.Add(this.bar);
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
                    //MessageBox.Show("something went wrong, not connected to the internet?");
                  //  ShellToast toast = new ShellToast();
                 //   toast.Title = "Error in TrafficCamsFlanders: ";
                 //   toast.Content = ee.ToString();
                  //  toast.Show();
                }
            }
            //already working
            if (connected() == false)
            {
                //MessageBox.Show("please connect to the internet");
               // ShellToast toast = new ShellToast();
               // toast.Title = "Error in TrafficCamsFlanders: ";
               // toast.Content = "no internet";
               // toast.Show();
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
                loaddata_antwerp(returnValue);
                //Action<string> act = new Action<string>(this.loaddata_antwerp);
                //this.Dispatcher.BeginInvoke(act, returnValue);
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
            //this.Antwerpenlbx.ItemsSource = this.camlist_Antwerp;
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
                loaddata_gent(returnValue);
                //Action<string> act = new Action<string>(this.loaddata_gent);
                //this.Dispatcher.BeginInvoke(act, returnValue);
            }
            catch (Exception ee)
            {
                //MessageBox.Show("something went wrong, not connected to the internet?");
            }
        }
        private void loaddata_gent(string msg)
        {
            this.camlist_Gent = this.loader(msg);
            //this.Gentlbx.ItemsSource = this.camlist_Gent;
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
                loaddata_lummen(returnValue);
                //Action<string> act = new Action<string>(this.loaddata_lummen);
               // this.Dispatcher.BeginInvoke(act, returnValue);
            }
            catch (Exception ee)
            {
                //MessageBox.Show("something went wrong, not connected to the internet?");
            }
        }
        private void loaddata_lummen(string msg)
        {
            this.camlist_Lummen = this.loader(msg);
            //this.Lummenlbx.ItemsSource = this.camlist_Lummen;
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
                loaddata_Brussel(returnValue);
               // Action<string> act = new Action<string>(this.loaddata_Brussel);
               // this.Dispatcher.BeginInvoke(act, returnValue);
            }
            catch (Exception ee)
            {
                //MessageBox.Show("something went wrong, not connected to the internet?");
            }
        }
        private void loaddata_Brussel(string msg)//brussels is the last asynchronous webcall, so do some more loading in here!
        {
            this.camlist_Brussel = this.loader(msg);
            //this.LayoutRoot.Children.Remove(this.bar);
            //this.Brussellbx.ItemsSource = this.camlist_Brussel;
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
                    foreach (ShellTile TileToFind in ShellTile.ActiveTiles)
                    {
                        string regio = "";
                        int tile=0;
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

                working = false;


                //ShellToast toast = new ShellToast();
                //toast.Title = "TrafficCamsFlanders";
                //toast.Content = "done";
                //toast.Show();
                //NotifyComplete();
            }
        }
        private bool connected()
        {
            bool thereturn = false;
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
        #endregion 
    }

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
            get { return name; }
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