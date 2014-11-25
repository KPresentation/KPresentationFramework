/*
 * Author: Vittorio Massaro
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KRecognizerNS
{
    /*
     * This class contains the information about the Pinc And Zoom gesture.
     */
    public class PinchAndZoomEventArgs : System.EventArgs
    {
        public double scaleFact { get; private set; }
        public PinchAndZoomEventArgs(double scale)
        {
            this.scaleFact = scale;
        }
    }
}
