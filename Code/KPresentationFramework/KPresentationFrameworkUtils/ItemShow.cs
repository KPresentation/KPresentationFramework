/*
 * Author: Daniele Castellana
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;

using KRecognizerNS;

//This class represents the control where the data of items are loaded.
namespace KPresentationFramework
{
    public class ItemShow : Border
    {
        //The frame where the page will be loaded.
        Frame contentFrame;

        bool isAnimating;
        Storyboard openShow;
        Storyboard closeShowSlide;
        KItem currentItem;

        Brush backgroundBeforeLoad = Brushes.Black;

        //This property indicates if an animation is on.
        public bool IsAnimating
        {
            get { return isAnimating; }
        }

        //This property contains the item shown.
        public KItem CurrentItem 
        {
            get { return currentItem; }
            set { currentItem = value; }
        }

        //This property is the background of the frame.
        public Brush BackgroundBeforeLoad
        {
            get { return backgroundBeforeLoad; }
            set 
            { 
                backgroundBeforeLoad = value;
                contentFrame.Background = backgroundBeforeLoad;
            }
        }

        public ItemShow()
        {
            contentFrame = new Frame();
            this.Child = contentFrame;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            //Start values for the animation.
            this.Opacity = 0;
            this.Visibility = Visibility.Collapsed;

            isAnimating = false;
            currentItem = null;
            contentFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;

            //Translate transform for animation.
            this.RenderTransform = new TranslateTransform();

            #region openShow

            openShow = new Storyboard();
            openShow.BeginTime = TimeSpan.FromSeconds(0.25);
            openShow.Completed += openShow_Completed;
            //easing function
            CubicEase myEaseOpen = new CubicEase();
            myEaseOpen.EasingMode = EasingMode.EaseIn;

            DoubleAnimation opacOpenShow = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.75)));
            opacOpenShow.EasingFunction = myEaseOpen;
            Storyboard.SetTargetProperty(opacOpenShow, new PropertyPath(ItemShow.OpacityProperty));
            openShow.Children.Add(opacOpenShow);

            #endregion

            #region closeShowSlide
            
            closeShowSlide = new Storyboard();
            closeShowSlide.Completed += closeShowSlide_Completed;
            //elimino lo spostamento dell'animazione
            closeShowSlide.FillBehavior = FillBehavior.Stop;
            //easing function
            CubicEase myEaseClose = new CubicEase();
            myEaseClose.EasingMode = EasingMode.EaseIn;

            DoubleAnimation opacCloseShow = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.75)));
            opacCloseShow.EasingFunction = myEaseClose;
            Storyboard.SetTargetProperty(opacCloseShow, new PropertyPath(ItemShow.OpacityProperty));

            DoubleAnimation offYCloseShow = new DoubleAnimation(0,-System.Windows.SystemParameters.PrimaryScreenHeight, new Duration(TimeSpan.FromSeconds(0.75)));
            offYCloseShow.EasingFunction = myEaseClose;
            DependencyProperty[] chainOffY = 
            {
                ItemsPresenter.RenderTransformProperty,
                TranslateTransform.YProperty
            };
            Storyboard.SetTargetProperty(offYCloseShow, new PropertyPath("(0).(1)", chainOffY));

            closeShowSlide.Children.Add(offYCloseShow);
            closeShowSlide.Children.Add(opacCloseShow);
            #endregion
        }

        void closeShowSlide_Completed(object sender, EventArgs e)
        {
            isAnimating = false;
            //Unloads the content.
            contentFrame.NavigationService.Content = null;
            this.Visibility = Visibility.Collapsed; ;
        }

        void openShow_Completed(object sender, EventArgs e)
        {
            //Navigate to the page where are the data.
            contentFrame.Navigate(new Uri(currentItem.ContentURI, UriKind.Relative));
            isAnimating = false;
        }

        //This method is called in order to show a data of an item. 
        public void ShowItem(KItem newItem)
        {
            isAnimating = true;
            this.CurrentItem = newItem;
            this.Visibility = Visibility.Visible;
            openShow.Begin(this);
        }

        public void Close()
        {
            this.currentItem = null;
            isAnimating = true;

            closeShowSlide.Begin(this);
        }

        //These methods are called in order to propagate the kinect event to the loaded page.
        public void Kinect_ImageGrabTerminate(object sender, EventArgs e)
        {
            (contentFrame.Content as KPage).RaiseImageGrabTerminate(sender, e);
        }

        public void Kinect_ImageGrabContinue(object sender, ImageGrabEventArgs e)
        {
            (contentFrame.Content as KPage).RaiseImageGrabContinue(sender, e);
        }

        public void Kinect_ImageGrabStart(object sender, EventArgs e)
        {
            (contentFrame.Content as KPage).RaiseImageGrabStart(sender, e);
        }
    }
}
