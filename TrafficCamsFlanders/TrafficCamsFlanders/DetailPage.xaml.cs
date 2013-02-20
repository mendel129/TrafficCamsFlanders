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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Tasks;

namespace TrafficCamsFlanders
{
    public partial class DetailPage : PhoneApplicationPage
    {
        public DetailPage()
        {
            InitializeComponent();
        }

        // TraficCamsEvolved_Flanders.DetailsPage
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string text = "";
            string text2 = "";
            if (base.NavigationContext.QueryString.TryGetValue("selectedItem", out text))
            {
                BitmapImage source = new BitmapImage(new Uri(text));
                this.image1.Source = source;
            }
            if (base.NavigationContext.QueryString.TryGetValue("selectedName", out text2))
            {
                this.ListTitle.Text=text2;
            }
        }

        private void textBlock1_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MessageBox.Show("will exit app...", "open m.filebeeld.be?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = (new Uri("http://m.filebeeld.be"));
                webBrowserTask.Show();
            }
        }

        private void image1_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MessageBox.Show("will exit app...", "open m.filebeeld.be?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = (new Uri("http://m.filebeeld.be"));
                webBrowserTask.Show();
            }
        }
    }
}