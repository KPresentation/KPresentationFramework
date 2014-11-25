/*
 * Author: Vittorio Massaro
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KRecognizerNS.Utils
{
    /*
     * This class contains the constant values used for the recognition of the gestures. By changing them
     * is possible to modify the recognizer's behavior.
     * The user shouldn't change them.
     */
    static class Values
    {
        //Maximum skeletons number
        public static int skeletonMaxNumber { get{return 6;} }
        //When, during a Pinch And Zoom gesture, the scale factor goes below "navigateBackThreshold", the Navigate Back Event is raised.
        public static double navigateBackThreshold { get { return 0.5; } }
        //Maximum distance between the hands on the X and Z axis during the Pinch And Zoom gesture.
        public static double pinchAndZoomThreshold { get { return 0.2; } }
        //Minimum distance between the hands on the X axis on the beginning of the Pinch And Zoom gesture.
        public static double pinchAndZoomMinimumStart { get { return 0.25;  } }
        //Mimimum speed of the hand during a Slide gesture.
        public static double slidingThreshold { get { return 0.005; } }
        //Percentage of the screen to be reached with the hand so that the Slide Up gesture is recognized.
        public static double percentScreen { get { return 0.1; } }
        //Minimum distance between the hands during the Image Grab.
        public static double handsMinDistance { get { return 0.1;  } }
        //Minimum distance between the hands on two axis so that is enabled the rotation around the third one.
        public static double handsMinDistanceII { get { return 0.05;  } }
    }
}
