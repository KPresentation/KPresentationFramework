/*
 * Author: Daniele Castellana
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Animation;

namespace KPresentationFramework
{
    //This class implements the appearance of the functional area to open element
    //It is made up of an ellipse and four arrows.
    class OpenShape : AnimableShape
    {
    
        //Pen to draw
        Pen strokeContent;

        public OpenShape() : base()
        {
            this.Fill = new RadialGradientBrush(Color.FromArgb(150, 0, 0, 0), Color.FromArgb(0, 0, 0, 0));
            strokeContent = new Pen(Brushes.White, 10);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                return new EllipseGeometry(new Rect(0, 0, this.Width, this.Height));
            }
        }

        //Custom drawing to personalize the appearance
        protected override void internalDraw(DrawingContext drawingContext)
        {
            for (int i = 0; i < 4; i++)
            {
                drawingContext.PushTransform(new RotateTransform(45, this.Width / 2, this.Height / 2));
                drawArrow(drawingContext);
                drawingContext.PushTransform(new RotateTransform(45,this.Width/2,this.Height/2));
            }
        }

        //The use of CurrentStep permits the animation
        private void drawArrow(DrawingContext drawingContext)
        {
            double arrowHeight = this.Height / 5;
            double arrowWidth = this.Width / 10;
            double arrowHeadHeight = arrowHeight/2;
            Point p1 = new Point((this.Width- arrowWidth) / 2, this.Height/ 2 - (arrowWidth + CurrentStep));
            Point p2 = new Point((this.Width+ arrowWidth) / 2, this.Height/ 2 - (arrowWidth + CurrentStep));
            Point p3 = new Point((this.Width+ arrowWidth) / 2, (this.Height- arrowWidth) / 2 - (CurrentStep + arrowHeight));
            Point p4 = new Point((this.Width+ arrowWidth) / 2 + arrowWidth/2, (this.Height- arrowWidth) / 2 - (CurrentStep + arrowHeight));
            Point p5 = new Point(this.Width/ 2, (this.Height- arrowWidth) / 2 - (CurrentStep + arrowHeight + arrowHeadHeight));
            Point p6 = new Point(this.Width/ 2 - arrowWidth, (this.Height- arrowWidth) / 2 - (CurrentStep + arrowHeight));
            Point p7 = new Point((this.Width- arrowWidth) / 2, (this.Height- arrowWidth) / 2 - (CurrentStep + arrowHeight));
            
            
            StreamGeometry streamGeometry = new StreamGeometry();

            using (StreamGeometryContext geometryContext = streamGeometry.Open())
            {
                geometryContext.BeginFigure(p1, true, true);
                PointCollection points = new PointCollection
                                             {
                                                 p2,
                                                 p3,
                                                 p4,
                                                 p5,
                                                 p6,
                                                 p7
                                             };
                geometryContext.PolyLineTo(points, true, true);
            }
            drawingContext.DrawGeometry(null, strokeContent , streamGeometry);
        }
    }
}
