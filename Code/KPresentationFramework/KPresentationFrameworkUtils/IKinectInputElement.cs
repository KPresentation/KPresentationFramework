/*
 * Author: Daniele Castellana
 */
using KRecognizerNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KPresentationFramework
{
    //This interface must be implemented by all elements which want to react to kinect events
    public interface IKinectInputElement
    {
        //These events are real: they are raised from the recognizer
        event EventHandler<BodyEventArgs> RightHandPush;
        event EventHandler<BodyEventArgs> RightHandMove;
        event EventHandler<BodyEventArgs> RightHandGrip;
        event EventHandler<BodyEventArgs> RightHandGripRelease;

        bool IsGripped
        {
            get;
        }

        //these methods return true if the event was handled by the object, false otherwise
        bool RaiseRightHandPush(object sender, BodyEventArgs e);
        bool RaiseRightHandMove(object sender, BodyEventArgs e);
        bool RaiseRightHandGrip(object sender, BodyEventArgs e);
        bool RaiseRightHandGripRelease(object sender, BodyEventArgs e);
    }
}
