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
using KPresentationFramework;
using System.Windows.Media.Animation;
using KRecognizerNS;

namespace Drugs.Pages
{
    /// <summary>
    /// Logica di interazione per web.xaml
    /// </summary>
    public partial class web : KPage
    {
        String mol = "";

        public web()
        {
            InitializeComponent();

            myWebBrowser.LoadCompleted += myWebBrowser_LoadCompleted;
            this.Loaded += web_Loaded;
        }

        void web_Loaded(object sender, RoutedEventArgs e)
        {
            mol = this.NavigationService.CurrentSource.OriginalString.Substring(19);
            myWebBrowser.Navigate("http://molexample.altervista.org/index.html");
        }

        void myWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            myWebBrowser.Visibility = System.Windows.Visibility.Visible;
            loadingIcon.Visibility = System.Windows.Visibility.Collapsed;
            if(!mol.Contains('&'))
            {
                   myWebBrowser.InvokeScript("create", new object[] { mol });
            }
            else
            {
                mol = mol.Substring(0, mol.IndexOf('&'));
                myWebBrowser.InvokeScript("create_workspace", new object[] { mol });
            }
        }

        void web_ImageGrabTerminate(object sender, EventArgs e)
        {
            myWebBrowser.InvokeScript("show_description");
        }

        void web_ImageGrabContinue(object sender, KRecognizerNS.ImageGrabEventArgs e)
        {
            double myScaleFact = -(e.scaleFact - 1) * 25;
            myWebBrowser.InvokeScript("rotoTrasloScale", new object[] { e.rotateAngleX, e.rotateAngleY,e.rotateAngleZ,e.traslateX,e.traslateY,myScaleFact });
        }

        void web_ImageGrabStart(object sender, EventArgs e)
        {
            myWebBrowser.InvokeScript("hide_description");
        }
    }
}
