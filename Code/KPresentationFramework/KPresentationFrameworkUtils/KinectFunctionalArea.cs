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
using System.Windows.Controls;

namespace KPresentationFramework
{
    //This class extends the default FrameworkElement in order to responds to the kinect events
    //raised from the recognizer.
    //It represents an area where the user can activate a functionality of the system.
    public class KinectFunctionalArea : FrameworkElement, IKinectInputElement
    {        
        public event EventHandler<BodyEventArgs> RightHandMove;
        public event EventHandler<BodyEventArgs> RightHandPush;
        public event EventHandler<BodyEventArgs> RightHandGrip;
        public event EventHandler<BodyEventArgs> RightHandGripRelease;

        //These events are simulated by software on the real events

        //This event is raised when when the right hand of the user enters in the element
        public event EventHandler<EventArgs> RightHandEnter;
        //This event is raised when when the right hand of the user leaves the element
        public event EventHandler<EventArgs> RightHandLeave;

        bool isGripped = false;
        bool isHandInside = false;

        public bool IsGripped
        {
            get { return isGripped; }
        }

        public bool IsHandInside
        {
            get { return isHandInside; }
        }

        //This method returns true if the point of interaction is inside the element itself
        bool IsInside(Point positionAbslute) 
        {
            return (this.InputHitTest(this.PointFromScreen(positionAbslute)) != null);
        }
        
        public bool RaiseRightHandPush(object sender, BodyEventArgs e)
        {
            if (IsInside(new Point(e.X, e.Y)))
            {
                if (RightHandPush != null)
                {
                    //Calls the delegate who someone else has registered
                    RightHandPush(sender, e);
                }
                return true;
            }
            return false;
        }

        public bool RaiseRightHandGrip(object sender, BodyEventArgs e)
        {
            if (IsInside(new Point(e.X, e.Y)))
            {
                if (RightHandGrip != null)
                {
                    RightHandGrip(sender, e);
                }
                isGripped = true;
                return true;
            }
            return false;
        }

        public bool RaiseRightHandGripRelease(object sender, BodyEventArgs e)
        {
            bool handled = false;
            if (IsGripped)
            {
                if (RightHandGripRelease != null)
                {
                    RightHandGripRelease(sender, e);
                }
                handled = true;
            }
            isGripped = false;
            return handled;
        }

        public bool RaiseRightHandMove(object sender, BodyEventArgs e)
        {
            if (IsInside(new Point(e.X, e.Y)))
            {
                if (!isHandInside)
                {
                    //This is the first time the hand enters on element, raises the enter event
                    isHandInside = true;
                    RightHandEnter(sender, new EventArgs());
                }
                else
                {
                    //The hand is already in, raises only the move event
                    if (RightHandMove != null)
                    {
                        RightHandMove(sender, e);
                    }
                }
                return true;
            }
            else
            {
                if (isHandInside)
                {
                    //This is the first time the hand exits from element, raises the leave event
                    isHandInside = false;
                    RightHandLeave(sender, new EventArgs());
                    return true;
                }
                return false;
            }
        }
    }
}
