/*
 * Author: Vittorio Massaro
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace KRecognizerNS
{
    /*
     * This class contains the information about the Image Grab gesture.
     */
    public class ImageGrabEventArgs : System.EventArgs
    {
        //Scale factor.
        public double scaleFact { get; private set; }
        //Rotation angles relative to the three axis.
        public double rotateAngleZ { get; private set; }
        public double rotateAngleX { get; private set; }
        public double rotateAngleY { get; private set; }
        //The traslational offsets.
        public double traslateX { get; private set; }
        public double traslateY { get; private set; }
        public ImageGrabEventArgs(double scale, double angleX, double angleY, double angleZ, double traslateX, double traslateY)
        {
            this.traslateX = traslateX * Screen.PrimaryScreen.Bounds.Width * 2.5;
            this.traslateY = - traslateY * Screen.PrimaryScreen.Bounds.Height * 2.5;
            this.scaleFact = scale;
            this.rotateAngleX = angleX;
            this.rotateAngleY = angleY;
            this.rotateAngleZ = angleZ;
        }
    }
}
