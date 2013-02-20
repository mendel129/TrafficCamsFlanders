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
using Microsoft.Phone.Tasks;


namespace TrafficCamsFlanders
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
        }

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MessageBox.Show("will exit app...", "open mendelonline.be?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = (new Uri("http://www.mendelonline.be"));
                webBrowserTask.Show();
            }
        }
        private void TextBlock_Tap_2(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MessageBox.Show("will exit app...", "open verkeercentrum.be?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri=(new Uri("http://www.verkeerscentrum.be"));
                webBrowserTask.Show();
            }
        }
    }
}