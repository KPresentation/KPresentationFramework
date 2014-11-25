/*
 * Author: Vittorio Massaro
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace KRecognizerNS
{
    /*
     * This class contains the information about the gestures in which is necessary to know only the screen coordinates.
     */
    public class BodyEventArgs : System.EventArgs
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public BodyEventArgs(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
