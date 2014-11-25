/*
 * Author: Daniele Castellana
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using KRecognizerNS;

namespace KPresentationFramework
{
    //This class represents a page where data are shown.
    public class KPage : Page
    {
        //The page receives the event raised from the kinect recognizer.
        public event EventHandler<EventArgs> ImageGrabStart;
        public event EventHandler<ImageGrabEventArgs> ImageGrabContinue;
        public event EventHandler<EventArgs> ImageGrabTerminate;

        //These functions are called in order to raise the events.
        public void RaiseImageGrabStart(object sender, EventArgs e)
        {
            if (ImageGrabStart != null)
            {
                ImageGrabStart(sender, e);
            }
        }

        public void RaiseImageGrabContinue(object sender, ImageGrabEventArgs e)
        {
            if (ImageGrabContinue != null)
            {
                ImageGrabContinue(sender, e);
            }
        }

        public void RaiseImageGrabTerminate(object sender, EventArgs e)
        {
            if (ImageGrabTerminate != null)
            {
                ImageGrabTerminate(sender, e);
            }
        }
    }
}
