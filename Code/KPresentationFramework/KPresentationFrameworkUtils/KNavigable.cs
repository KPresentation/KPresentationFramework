/*
 * Author: Daniele Castellana
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using KRecognizerNS;
using System.Windows.Controls;
using System.Windows.Data;

namespace KPresentationFramework
{
    //This abstract class represents an item of the navigation tree.
    //It will be a folder if it is an internal node, an item if it is a leaf.
    public abstract class KNavigable : KinectNavigable
    {
        //Ratio between the width of the screen and the width of the element
        static readonly double rappScreenChild = 8.6;

        KFolder parentFolder;

        Size childSize;

        double percX = 0;
        double percY = 0;

        double x=0;
        double y=0;

        //This is a reference to the root control where this element is defined.
        KPresenter rootControl;
        
        //This animation is performed when the user open the element.
        Storyboard clickAnim;

        //This animation is performed when the user release the element in another position.
        Storyboard restorePostion;

        //These properties are useful to set the position of the item.
        //Their value are a percentage respect the width and height of the container.
        public double PercentageX
        {
            get { return percX; }
            set 
            {
                percX = value;
                x = (System.Windows.SystemParameters.PrimaryScreenWidth * percX) / 100 - childSize.Width / 2;
            }
        }

        public double PercentageY
        {
            get { return percY; }
            set
            { 
                percY = value;
                y = (System.Windows.SystemParameters.PrimaryScreenHeight * percY) / 100 - childSize.Height / 2;
            }
        }

        //These properties are the position of the element in its container.
        public double X
        {
            get { return x; }
        }

        public double Y
        {
            get { return y; }
        }

        //This property is the size of the element.
        public Size ChildSize
        {
            get { return childSize; }
        }

        //This property is a reference to the parent folder of this element.
        public KFolder ParentFolder
        {
            get { return parentFolder; }
        }

        //This property is a reference to the NavigationControl where the item is definied
        public KPresenter RootControl
        {
            get { return rootControl; }
        }


        public KNavigable()
        {
            //Calculates the dimesion of the element.
            double childDim = (System.Windows.SystemParameters.PrimaryScreenWidth / rappScreenChild);
            childSize = new Size(childDim, childDim);

            this.RenderTransformOrigin = new Point(0.5, 0.5);
            this.RenderTransform = new ScaleTransform();

            //this.MouseDown += MyMouseDown;

            this.RightHandGrip += Kinect_Grip;
            this.RightHandPush += Kinect_Push;
            this.RightHandGripMove += Kinect_GripMove;

            //The deafult z-index is 0.
            Panel.SetZIndex(this, 0);

            #region clickAnimation

            clickAnim = new Storyboard();
            clickAnim.Completed += ClickAnim_Completed;
            //easing function
            CubicEase myEaseIn = new CubicEase();
            myEaseIn.EasingMode = EasingMode.EaseIn;
            clickAnim.AutoReverse = true;

            DoubleAnimation scaleXIn = new DoubleAnimation(1, 0.8, new Duration(TimeSpan.FromSeconds(0.2)));
            scaleXIn.EasingFunction = myEaseIn;
            DependencyProperty[] chainScaleX = 
            {
                ItemsPresenter.RenderTransformProperty,
                ScaleTransform.ScaleXProperty
            };
            Storyboard.SetTargetProperty(scaleXIn, new PropertyPath("(0).(1)", chainScaleX));
            clickAnim.Children.Add(scaleXIn);

            DoubleAnimation scaleYIn = new DoubleAnimation(1, 0.8, new Duration(TimeSpan.FromSeconds(0.2)));
            scaleYIn.EasingFunction = myEaseIn;
            DependencyProperty[] chainScaleY = 
            {
                ItemsPresenter.RenderTransformProperty,
                ScaleTransform.ScaleYProperty
            };
            Storyboard.SetTargetProperty(scaleYIn, new PropertyPath("(0).(1)", chainScaleY));
            clickAnim.Children.Add(scaleYIn);

            #endregion

            #region restorePosition

            restorePostion = new Storyboard();
            restorePostion.Completed += restorePostion_Completed;
            //easing function
            CubicEase myEaseRestore = new CubicEase();
            myEaseRestore.EasingMode = EasingMode.EaseInOut;

            DoubleAnimation traslXBack = new DoubleAnimation(this.X, new Duration(TimeSpan.FromSeconds(0.75)));
            traslXBack.EasingFunction = myEaseRestore;
            Storyboard.SetTargetProperty(traslXBack, new PropertyPath(Canvas.LeftProperty));
            restorePostion.Children.Add(traslXBack);

            DoubleAnimation traslYBack = new DoubleAnimation(this.Y, new Duration(TimeSpan.FromSeconds(0.75)));
            traslYBack.EasingFunction = myEaseRestore;
            Storyboard.SetTargetProperty(traslYBack, new PropertyPath(Canvas.TopProperty));
            restorePostion.Children.Add(traslYBack);

            #endregion
        }

        void ClickAnim_Completed(object sender, EventArgs e)
        {
            if (this.Parent is ItemsPresenter)
            {
                (this.Parent as ItemsPresenter).IsAnimating = false;
            }

            if (RootControl != null)
            {
                //After the animtion, notify to the control root to change the content
                RootControl.ChangeContent(this);
            }
        }

        void restorePostion_Completed(object sender, EventArgs e)
        {
            //Sets the lower z-index value.
            Panel.SetZIndex(this, 0);

            if (this.Parent is ItemsPresenter)
            {
                (this.Parent as ItemsPresenter).IsAnimating = false;
            }
        }

        void Kinect_Push(object sender, BodyEventArgs e)
        {
            if (this.Parent is ItemsPresenter)
            {
                (this.Parent as ItemsPresenter).IsAnimating = true;
            }

            clickAnim.Begin(this);
        }

        void Kinect_Grip(object sender, BodyEventArgs e)
        {
            //Removes animation from the property.
            this.BeginAnimation(Canvas.LeftProperty, null);
            this.BeginAnimation(Canvas.TopProperty, null);

            //Increses z-index in order to put on top the selected item.
            Panel.SetZIndex(this, 10);
        }

        //Moves the element during the kinectMove event.
        void Kinect_GripMove(object sender, BodyEventArgs e)
        {
            Canvas.SetLeft(this, e.X - this.ActualWidth / 2);
            Canvas.SetTop(this, e.Y - this.ActualHeight / 2);
        }

        //Permits to navigate in the application using mouse.
        //NOW IS NEVER CALLED
        void MyMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                clickAnim.Begin(this);
            }
        }

        //Start the animation to restore the default position.
        public void RestorePosition()
        {
            if (this.Parent is ItemsPresenter)
            {
                (this.Parent as ItemsPresenter).IsAnimating = true;
            }

            (restorePostion.Children[0] as DoubleAnimation).To = this.X;
            (restorePostion.Children[1] as DoubleAnimation).To = this.Y;
            restorePostion.Begin(this);
        }

        //Starts the animation to open the content.
        public void OpenContent()
        {
            if (this.Parent is ItemsPresenter)
            {
                (this.Parent as ItemsPresenter).IsAnimating = true;
            }

            clickAnim.Begin(this);
        }

        public void SetParentFolder(KFolder parent) 
        {
            parentFolder = parent;
        }

        public void SetRootControl(KPresenter root) 
        {
            rootControl = root;
        }
    }
}
