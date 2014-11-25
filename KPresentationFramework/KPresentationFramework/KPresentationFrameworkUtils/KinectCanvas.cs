/*
 * Author: Daniele Castellana
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using System.Windows;
using KRecognizerNS;

namespace KPresentationFramework
{
    //This class extends the default Canvas in order to responds to the kinect events
    //raised from the recognizer.
    public class KinectCanvas : Canvas, IKinectInputElement
    {
        public event EventHandler<BodyEventArgs> RightHandPush;
        public event EventHandler<BodyEventArgs> RightHandMove;
        public event EventHandler<BodyEventArgs> RightHandGrip;
        public event EventHandler<BodyEventArgs> RightHandGripRelease;

        //These events are simulated by software on the real events

        //These events are raised at the start and at the end of a grip on element
        public event EventHandler<BodyEventArgs> RightHandGripElement;
        public event EventHandler<BodyEventArgs> RightHandGripReleaseElement;

        bool isGripped = false;
        KinectNavigable grippedElement = null;

        //The containter is gripped if one of its children is gripped
        public bool IsGripped
        {
            get { return isGripped; }
        }

        //The child element gripped
        //IT MUST BE A KINECT NAVIGABLE
        public KinectNavigable GrippedElement
        {
            get { return grippedElement; }
        }

        //Since this element has child elements, it must propagates the event from itself to them.
        //So, each method call the same method on all children: if someone of them have handled the event,
        //the element doesn't call its delegate to handle it. Otherwise the delegate is called.

        public bool RaiseRightHandPush(object sender, BodyEventArgs e)
        {
            bool handled = false;
            foreach (UIElement u in InternalChildren)
            {
                if (u is IKinectInputElement)
                {
                    if ((u as IKinectInputElement).RaiseRightHandPush(sender, e))
                    {
                        handled = true;
                    }
                }
            }
            if (!handled)
            {
                if (RightHandPush != null)
                {
                    RightHandPush(sender, e);
                }
            }
            return true;
        }

        public bool RaiseRightHandGrip(object sender, BodyEventArgs e)
        {
            bool handled = false;
            foreach (UIElement u in InternalChildren)
            {
                if (u is IKinectInputElement)
                {
                    if ((u as IKinectInputElement).RaiseRightHandGrip(sender, e))
                    {
                        if ((u as IKinectInputElement).IsGripped)
                        {
                            //Grip the element if and only if it is a KinectNavigable, ignore otherwise
                            if (u is KinectNavigable)
                            {
                                this.isGripped = true;
                                grippedElement = u as KinectNavigable;
                            }
                        }
                        handled = true;
                    }
                }
            }
            if (grippedElement != null)
            {
                RightHandGripElement(sender, e);
            }
            if (!handled)
            {
                if (RightHandGrip != null)
                {
                    RightHandGrip(sender, e);
                }
            }
            return true;
        }

        public bool RaiseRightHandGripRelease(object sender, BodyEventArgs e)
        {
            bool handled = false;
            if(isGripped)
            {
                RightHandGripReleaseElement(sender, e);
                foreach (UIElement u in InternalChildren)
                {
                    if (u is IKinectInputElement)
                    {
                        if ((u as IKinectInputElement).RaiseRightHandGripRelease(sender, e))
                        {
                            handled = true;
                        }
                    }
                }
                isGripped = false;
                grippedElement = null;
            }
            if (!handled)
            {
                if (RightHandGripRelease != null)
                {
                    RightHandGripRelease(sender, e);
                }
            }
            return true;
        }

        public bool RaiseRightHandMove(object sender, BodyEventArgs e)
        {
            bool handled = false;
            foreach (UIElement u in InternalChildren)
            {
                if (u is IKinectInputElement)
                {
                    if ((u as IKinectInputElement).RaiseRightHandMove(sender, e))
                    {
                        handled = true;
                    }
                }
            }
            if (!handled)
            {
                if (RightHandMove != null)
                {
                    RightHandMove(sender, e);
                }
            }
            return true;
        }

    }
}
