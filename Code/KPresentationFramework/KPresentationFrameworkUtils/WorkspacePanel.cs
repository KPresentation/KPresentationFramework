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
using System.Windows.Media.Animation;
using System.Windows.Media;
using KRecognizerNS;

namespace KPresentationFramework
{
    //This class represents the workspace.
    class WorkspacePanel : Panel
    {
        //All these variables are utils for the final arrangement of the elements.
        Size sizeForLargh;
        Size sizeForAlt;
        Size sizeForQuad;

        Point topLeftCornerLargh;
        Point topLeftCornerAlt;
        Point topLeftCornerQuad;

        List<double> rapp;

        //List of all URI addedd to the workspace.
        List<String> uriList;

        //Text block to notify the empty workspace.
        TextBlock t;

        double soglia_largh = 1.3;
        double soglia_alt = 0.7;

        double margin = 5;

        //These animations are used to show/hide the workspace.
        Storyboard openWorkspaceAnimation;
        Storyboard closeWorkspaceAnimation;

        bool isAnimating = false;

        Brush backgroundBeforeLoad = Brushes.Black;

        //This property indicates if an animation is on.
        public bool IsAnimating 
        {
            get { return isAnimating; }
        }

        //This property is the background of the frames.
        public Brush BackgroundBeforeLoad
        {
            get { return backgroundBeforeLoad; }
            set { backgroundBeforeLoad = value; }
        }

        public WorkspacePanel()
        {
            //trnaslate trasfrom for the animations.
            this.RenderTransform = new TranslateTransform();

            sizeForLargh = new Size();
            sizeForAlt = new Size();
            sizeForQuad = new Size();

            topLeftCornerLargh = new Point(margin, margin);
            topLeftCornerAlt = new Point(margin, margin);
            topLeftCornerQuad = new Point(margin, margin);

            this.Visibility = System.Windows.Visibility.Collapsed;
            this.Opacity = 0;

            t = new TextBlock();
            t.Text = "EMPTY WORKSPACE";
            t.Foreground = Brushes.White;
            t.FontSize = 60;
            t.FontWeight = FontWeights.Bold;
            //this.Background = new RadialGradientBrush(Color.FromArgb(150, 0, 0, 0), Color.FromArgb(0, 0, 0, 0));
            t.Padding = new Thickness(25,0,25,5);
            Border b = new Border();
            b.CornerRadius = new CornerRadius(20);
            b.Background = Brushes.Black;
            b.Child = t;
            this.InternalChildren.Add(b);

            uriList = new List<String>();

            #region openWorkspaceAnimation

            openWorkspaceAnimation = new Storyboard();
            openWorkspaceAnimation.BeginTime = TimeSpan.FromSeconds(0.25);
            //easing function
            CubicEase myEaseOpen= new CubicEase();
            myEaseOpen.EasingMode = EasingMode.EaseOut;

            DoubleAnimation opacOpenWork = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.75)));
            opacOpenWork.EasingFunction = myEaseOpen;
            Storyboard.SetTargetProperty(opacOpenWork, new PropertyPath(ItemsPresenter.OpacityProperty));

            DoubleAnimation offYOpenWork = new DoubleAnimation(System.Windows.SystemParameters.PrimaryScreenHeight,0, new Duration(TimeSpan.FromSeconds(0.75)));
            offYOpenWork.EasingFunction = myEaseOpen;
            DependencyProperty[] chainOffY = 
            {
                ItemsPresenter.RenderTransformProperty,
                TranslateTransform.YProperty
            };
            Storyboard.SetTargetProperty(offYOpenWork, new PropertyPath("(0).(1)", chainOffY));

            openWorkspaceAnimation.Children.Add(offYOpenWork);
            openWorkspaceAnimation.Children.Add(opacOpenWork);

            openWorkspaceAnimation.Completed += openWorkspaceAnimation_Completed;

            #endregion

            #region closeWorkspaceAnimation

            closeWorkspaceAnimation = new Storyboard();

            //easing function
            CubicEase myEaseClose = new CubicEase();
            myEaseClose.EasingMode = EasingMode.EaseIn;

            DoubleAnimation opacCloseWork = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.75)));
            opacCloseWork.EasingFunction = myEaseClose;
            Storyboard.SetTargetProperty(opacCloseWork, new PropertyPath(ItemsPresenter.OpacityProperty));

            DoubleAnimation offYCloseWork = new DoubleAnimation(0,System.Windows.SystemParameters.PrimaryScreenHeight, new Duration(TimeSpan.FromSeconds(0.75)));
            offYCloseWork.EasingFunction = myEaseClose;
            Storyboard.SetTargetProperty(offYCloseWork, new PropertyPath("(0).(1)", chainOffY));

            closeWorkspaceAnimation.Children.Add(offYCloseWork);
            closeWorkspaceAnimation.Children.Add(opacCloseWork);

            closeWorkspaceAnimation.Completed += closeWorkspaceAnimation_Completed;

            #endregion
        }

        void closeWorkspaceAnimation_Completed(object sender, EventArgs e)
        {
            isAnimating = false;
            for (int i=1; i < InternalChildren.Count; i++)
            {
                //Disposes all content loaded.
                (InternalChildren[i] as Frame).NavigationService.Content = null;
            }
            this.Visibility = Visibility.Collapsed;
        }

        void openWorkspaceAnimation_Completed(object sender, EventArgs e)
        {
            isAnimating = false;
            for (int i = 0; i < uriList.Count; i++) 
            {
                //Loads all content.
                String s = uriList[i];
                String newS = "";
                //Adds a parameter which indicates if the page is shown in the workspace
                if (s.Contains('?')) 
                {
                    newS = String.Concat(s, "&w=1");
                }
                else
                {
                    newS = String.Concat(s, "?w=1");
                }
                (this.InternalChildren[i+1] as Frame).Navigate(new Uri(newS, UriKind.Relative));
            }
        }

        public void StartOpenAnimation()
        {
            isAnimating = true;
            this.Visibility = Visibility.Visible;
            openWorkspaceAnimation.Begin(this);
        }

        public void StartCloseAnimation()
        {
            isAnimating = true;
            closeWorkspaceAnimation.Begin(this);
        }

        public void AddToWorkspace(String s)
        {
            if (!uriList.Contains(s))
            {
                if (uriList.Count == 0)
                {
                    t.Visibility = System.Windows.Visibility.Collapsed;
                }

                uriList.Add(s);
                Frame f = new Frame();
                f.Background = backgroundBeforeLoad;
                f.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
                this.InternalChildren.Add(f);
            }
        }

        public void RemoveFromWorkspace(String s)
        {
            if (uriList.Contains(s))
            {
                if (uriList.Count == 1)
                {
                    t.Visibility = System.Windows.Visibility.Visible;
                }

                uriList.Remove(s);
                this.InternalChildren.RemoveAt(this.InternalChildren.Count - 1);
            }
        }
        
        protected override Size MeasureOverride(Size availableSize)
        {
            int largh = 0, alt = 0, quad = 0;
            Size avaibleAlt = new Size();
            Size avaibleLargh = new Size();
            Size avaibleQuad = new Size();
            Size infinity = new Size(double.PositiveInfinity, double.PositiveInfinity);

            rapp = new List<double>();

            if (Double.IsInfinity(availableSize.Width))
            {
                //Sets the size of the screen
                availableSize.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            }

            if (Double.IsInfinity(availableSize.Height))
            {
                //Sets the size of the screen
                availableSize.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            }

            if (InternalChildren.Count == 1)
            {
                InternalChildren[0].Measure(availableSize);
                return availableSize;
            }

            //Call onMeasure on each child with infinity space.
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                //Controls if the text block is visible
                if (InternalChildren[i].Visibility == System.Windows.Visibility.Collapsed)
                {
                    rapp.Insert(i, -1);
                    continue;
                }
                InternalChildren[i].Measure(infinity);
                rapp.Insert(i, InternalChildren[i].DesiredSize.Width / InternalChildren[i].DesiredSize.Height);
                if (rapp[i] > soglia_largh)
                {
                    //Element with width greater than height
                    largh++;
                }
                else
                {
                    if (rapp[i] < soglia_alt)
                    {
                        ///Element with height greater than width
                        alt++;
                    }
                    else
                    {
                        //Element with width equal than height
                        quad++;
                    }
                }
            }

            if (alt != 0)
            {
                if (alt == 1)
                {
                    avaibleAlt.Height = availableSize.Height;
                    avaibleAlt.Width = availableSize.Width / 3;

                    topLeftCornerAlt.Y = margin;
                    topLeftCornerAlt.X = 2 * availableSize.Width / 3;
                }
                else
                {
                    avaibleAlt.Height = availableSize.Height;
                    avaibleAlt.Width = availableSize.Width / 2;

                    topLeftCornerAlt.Y = margin;
                    topLeftCornerAlt.X = availableSize.Width / 2;
                }
                if (largh == 0 && quad == 0)
                {
                    avaibleAlt.Width = availableSize.Width;
                    topLeftCornerAlt.X = margin;
                }
            }

            if (largh != 0)
            {
                avaibleLargh.Width = availableSize.Width - avaibleAlt.Width;
                avaibleLargh.Height = availableSize.Height / 2;
                if (largh >= 2)
                {
                    avaibleLargh.Height = (2 * availableSize.Height) / 3;
                }

                topLeftCornerLargh.X = margin;
                topLeftCornerLargh.Y = margin;

                if (quad == 0)
                {
                    avaibleLargh.Height = availableSize.Height;
                }
            }

            if (quad != 0)
            {
                avaibleQuad.Width = availableSize.Width - avaibleAlt.Width;
                avaibleQuad.Height = availableSize.Height - avaibleLargh.Height;

                topLeftCornerQuad.X = margin;
                topLeftCornerQuad.Y = avaibleLargh.Height;
            }

            sizeForLargh.Width = avaibleLargh.Width;
            sizeForLargh.Height = avaibleLargh.Height / largh;
            if (sizeForLargh.Width / sizeForLargh.Height < soglia_largh)
            {
                if (sizeForLargh.Width / soglia_largh < sizeForLargh.Height)
                {
                    sizeForLargh.Height = sizeForLargh.Width / soglia_largh;
                }
                else
                {
                    sizeForLargh.Width = sizeForLargh.Height * soglia_largh;
                }
            }

            sizeForAlt.Width = avaibleAlt.Width / alt;
            sizeForAlt.Height = avaibleAlt.Height;
            if (sizeForAlt.Width / sizeForAlt.Height > soglia_alt)
            {
                if (sizeForAlt.Width / soglia_alt < sizeForAlt.Height)
                {
                    sizeForAlt.Height = sizeForAlt.Width / soglia_alt;
                }
                else
                {
                    sizeForAlt.Width = sizeForAlt.Height * soglia_alt;
                }
            }

            sizeForQuad.Width = avaibleQuad.Width / quad;
            sizeForQuad.Height = avaibleQuad.Height;

            if (sizeForQuad.Width / sizeForQuad.Height > soglia_largh || sizeForQuad.Width / sizeForQuad.Height < soglia_alt)
            {
                double min = Math.Min(sizeForQuad.Width, sizeForQuad.Height);
                sizeForQuad.Width = min;
                sizeForQuad.Height = min;
            }

            topLeftCornerAlt.X += ((avaibleAlt.Width - (sizeForAlt.Width * alt)) / 2);

            topLeftCornerAlt.Y += ((avaibleAlt.Height - sizeForAlt.Height) / 2);

            topLeftCornerLargh.X += ((avaibleLargh.Width - sizeForLargh.Width) / 2);
            topLeftCornerLargh.Y += ((avaibleLargh.Height - (sizeForLargh.Height * largh)) / 2);

            topLeftCornerQuad.X += ((avaibleQuad.Width - (sizeForQuad.Width * quad)) / 2);

            topLeftCornerQuad.Y += ((avaibleQuad.Height - sizeForQuad.Height) / 2);


            if (sizeForLargh.Width > 10 && sizeForLargh.Height > 10)
            {
                sizeForLargh.Width -= margin * 2;
                sizeForLargh.Height -= margin * 2;
            }

            if (sizeForAlt.Width > 10 && sizeForAlt.Height > 10)
            {
                sizeForAlt.Width -= margin * 2;
                sizeForAlt.Height -= margin * 2;
            }

            if (sizeForQuad.Width > 10 && sizeForQuad.Height > 10)
            {
                sizeForQuad.Width -= margin * 2;
                sizeForQuad.Height -= margin * 2;
            }

            //Call onMeasure on each child with calculated space.
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                if (rapp[i] == -1)
                {
                    continue;
                }
                if (rapp[i] > soglia_largh)
                {
                    InternalChildren[i].Measure(sizeForLargh);
                }
                else
                {
                    if (rapp[i] < soglia_alt)
                    {
                        InternalChildren[i].Measure(sizeForAlt);
                    }
                    else
                    {
                        InternalChildren[i].Measure(sizeForQuad);
                    }
                }
            }

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Point ausLargh = new Point(topLeftCornerLargh.X, topLeftCornerLargh.Y);
            Point ausAlt = new Point(topLeftCornerAlt.X, topLeftCornerAlt.Y);
            Point ausQuad = new Point(topLeftCornerQuad.X, topLeftCornerQuad.Y);

            if (InternalChildren.Count == 1)
            {
                //There is only the text block. Calculates its position.
                double w = InternalChildren[0].DesiredSize.Width;
                double h = InternalChildren[0].DesiredSize.Height;
                double sx = (finalSize.Width - w) / 2;
                double sy = (finalSize.Height - h) / 2;

                InternalChildren[0].Arrange(new Rect(sx,sy,w,h));

                return finalSize;
            }

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                if (InternalChildren[i].Visibility != System.Windows.Visibility.Visible || rapp[i]==-1)
                {
                    continue;
                }
                if (rapp[i] > soglia_largh)
                {
                    InternalChildren[i].Arrange(new Rect(ausLargh, sizeForLargh));
                    ausLargh.Y += sizeForLargh.Height + margin;
                }
                else
                {
                    if (rapp[i] < soglia_alt)
                    {
                        InternalChildren[i].Arrange(new Rect(ausAlt, sizeForAlt));
                        ausAlt.X += sizeForAlt.Width + margin;
                    }
                    else
                    {
                        InternalChildren[i].Arrange(new Rect(ausQuad, sizeForQuad));
                        ausQuad.X += sizeForQuad.Width + margin;
                    }
                }
            }

            return finalSize;
        }
    }
}