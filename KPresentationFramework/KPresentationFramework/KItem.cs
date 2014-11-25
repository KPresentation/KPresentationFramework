/*
 * Author: Daniele Castellana
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Markup;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using KRecognizerNS;
using System.Windows.Documents;
using System.Windows.Shapes;

namespace KPresentationFramework
{
    //This class represents an item which contains data.
    [ContentProperty("Content")]
    public class KItem : KNavigable
    {
        string source = "";
        UIElement content;
        //This variable indicates if the item is or not in the workspace.
        bool inWorkspace = false;

        CheckAdorner checkAdorner;
        Border border;

        //This property represents the visual content of the item.
        public UIElement Content 
        {
            get { return content; }
            set
            {
                content = value;
                (border.Child as Grid).Children.Add(content);
            }
        }

        //This property contains the URI of the page where the data are shown.
        public string ContentURI
        {
            get { return source; }
            set { source = value; }
        }

        public KItem() : base()
        {
            //Initialize the adorner used to mark selected items.
            checkAdorner = new CheckAdorner(ChildSize.Width,ChildSize.Height);
            Panel.SetZIndex(checkAdorner, 20);
            checkAdorner.Visibility = Visibility.Collapsed;


            border = new Border();
            border.Child = new Grid();
            (border.Child as Grid).Children.Add(checkAdorner);

            this.AddVisualChild(border);
        }

        protected override Visual GetVisualChild(int index)
        {
            return border;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            border.Measure(ChildSize);
            return ChildSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            border.Arrange(new Rect(ChildSize));
            return ChildSize;
        }

        //This method is called when the item is released in the workspace area.
        //It sets the right value of inWorkspace and calls the right method of ControlNavigation to add/remove the item.
        public void InWorkspace()
        {
            if (RootControl != null)
            {
                if (!inWorkspace)
                {
                    inWorkspace = true;
                    checkAdorner.Visibility = Visibility.Visible;
                    RootControl.AddToWorkspace(this);
                }
                else
                {
                    inWorkspace = false;
                    checkAdorner.Visibility = Visibility.Collapsed;
                    RootControl.RemoveFromWorkspace(this);
                }
                this.RestorePosition();
            }
        }
    }

    //This class represents a tick.
    //It is used to mark selected items.
    class CheckAdorner : Shape
    {

        double w, h,off;
        SolidColorBrush fill;
        Pen stroke;
        Point p1, p2, p3,p4;

        public CheckAdorner(double width, double height)
            : base()
        {
            fill = Brushes.Black;
            stroke = new Pen(Brushes.White, 4);

            w = width / 5;
            h = height / 5;
            off = w / 30;
            stroke.Thickness = w / 7;

            p1 = new Point(w / 8.0 , h / 2.0);
            p2 = new Point((3.0 * w) / 8.0, (3.0 * h) / 4.0);
            p3 = new Point((3.0 * w) / 8.0 - stroke.Thickness / Math.Sqrt(8), (3.0 * h) / 4.0 + stroke.Thickness / Math.Sqrt(8));
            p4 = new Point((7.0 * w) / 8.0, h / 4.0);

        }


        protected override Geometry DefiningGeometry
        {
            get { return new RectangleGeometry(new Rect(0,0,w,h)); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(fill, null, new Rect(0,0, w, h));
            drawingContext.DrawLine(stroke, p1, p2);
            drawingContext.DrawLine(stroke, p3, p4);
        }


    }
}
