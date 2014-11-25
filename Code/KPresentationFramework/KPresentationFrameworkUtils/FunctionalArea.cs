/*
 * Author: Daniele Castellana
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace KPresentationFramework
{
    public class FunctionalArea : KinectFunctionalArea
    {
        static readonly double rappScreenArea = 5;

        //The visual child of the element.
        //It must be animableShape in order to perform an animation.s
        AnimableShape visualShape;

        //The dimension of the area, calculated respect the width of primary screen
        double dimension;

        //These animtaion are used to show/hide the functional area.
        //They operate on the opacity of themselves
        DoubleAnimation showOpac;
        DoubleAnimation hideOpac;

        public AnimableShape VisualShape
        {
            get { return visualShape; }
            set
            {
                this.RemoveVisualChild(visualShape);
                visualShape = value;
                //Set the dimension of visual child equals to the dimension of the whole area
                visualShape.Height = dimension;
                visualShape.Width = dimension;
                this.AddVisualChild(visualShape);
            }
        }

        public FunctionalArea()
        {
            dimension = System.Windows.SystemParameters.PrimaryScreenWidth / rappScreenArea;

            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            //This element is not visible to deafault
            this.Visibility = System.Windows.Visibility.Collapsed;
            
            //This is the start value for animation on the opacity
            this.Opacity = 0;

            showOpac = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(0.25)));

            hideOpac = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.25)));
            hideOpac.Completed += hideOpac_Completed;

            //Start/stop the animation of VisualShape when the right hand of the user enters/leaves
            //from the area.
            this.RightHandEnter += startAnimationExpand;
            this.RightHandLeave += stopAnimationExpand;
        }

        void stopAnimationExpand(object sender, EventArgs e)
        {
            visualShape.stopAnimation();
        }

        void startAnimationExpand(object sender, EventArgs e)
        {
            visualShape.startAnimation();
        }

        public void showArea()
        {
            this.Visibility = System.Windows.Visibility.Visible;

            this.BeginAnimation(FunctionalArea.OpacityProperty, showOpac);
        }

        public void hideArea()
        {
            this.BeginAnimation(FunctionalArea.OpacityProperty, hideOpac);
        }

        void hideOpac_Completed(object sender, EventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        protected override Visual GetVisualChild(int index)
        {
            return visualShape;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            visualShape.Measure(availableSize);
            return visualShape.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            visualShape.Arrange(new Rect(finalSize));
            return finalSize;
        }
    }
}
