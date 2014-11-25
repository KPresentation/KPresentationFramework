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
    //This class extends the default FrameworkElement in order to responds to the kinect events
    //raised from the recognizer.
    //It represents an item the user can use to navigate the tree (folder or item).
    public class KinectNavigable : FrameworkElement , IKinectInputElement
    {
        public event EventHandler<BodyEventArgs> RightHandPush;
        public event EventHandler<BodyEventArgs> RightHandMove;
        public event EventHandler<BodyEventArgs> RightHandGrip;
        public event EventHandler<BodyEventArgs> RightHandGripRelease;

        //These events are simulated by software on the real events

        //This event is raised when the user move the right hand during a grip
        public event EventHandler<BodyEventArgs> RightHandGripMove;

        public bool IsGripped
        {
            get { return isGripped; }
        }

        bool isGripped = false;

        //This method returns true if the point of interaction is inside the element itself
        bool IsInside(Point positionAbsolute)
        {
            return (this.InputHitTest(this.PointFromScreen(positionAbsolute)) != null);
        }

        public bool RaiseRightHandPush(object sender, BodyEventArgs e)
        {
            if (IsInside(new Point(e.X, e.Y)))
            {
                if (RightHandPush != null)
                {
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
            bool handled = false;
            if (isGripped)
            {
                //The element is gripped, raise the grip move event
                if (RightHandGripMove != null)
                {
                    RightHandGripMove(sender, e);
                }
                handled = true;
            }
            if (IsInside(new Point(e.X, e.Y)))
            {
                //The element is NOT gripped, raise the move event
                if (RightHandMove != null)
                {
                    RightHandMove(sender, e);
                }
                handled = true;
            }
            return handled;
        }
    }
}
