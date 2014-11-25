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
    //This class implements the appearance of the functional area add/remove element from the workspace.
    //It is made up of an ellipse with a folder and an arrow.
    class WorkspaceShape : AnimableShape
    {
        //Pen to draw
        Pen strokeContent;

        public WorkspaceShape() : base()
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
            drawBack(drawingContext);
            drawArrow(drawingContext);
            drawFolder(drawingContext);
        }

        private void drawBack(DrawingContext drawingContext)
        {
            double maxAngle = 20;
            double folderHeigt = this.Height / 5;
            double folderWidth = this.Width / 2.5;
            double labelHeight = folderHeigt / 4;
            double distanceFromBase = (5.0 / 7.0);
            double angle = - (CurrentStep * 7 / this.Width) * maxAngle;
            drawingContext.PushTransform(new SkewTransform(angle, 0, (this.Width- folderWidth) / 2, distanceFromBase * this.Height));
            drawingContext.DrawRectangle(null, strokeContent, new Rect(new Point((this.Width- folderWidth) / 2, distanceFromBase * this.Height), new Point((this.Width + folderWidth) / 2, distanceFromBase * this.Height- Math.Cos((angle * Math.PI) / 90.0) * (folderHeigt + labelHeight / 2))));
            drawingContext.Pop();
        }

        private void drawFolder(DrawingContext drawingContext)
        {
            double folderHeight = this.Height / 5;
            double folderWidth = this.Width / 2.5;
            double labelHeight = folderHeight / 4;
            double distanceFromBase = (5.0 / 7.0);
            Point p1 = new Point((this.Width- folderWidth) / 2, distanceFromBase * this.Height);
            Point p2 = new Point((this.Width+ folderWidth) / 2, distanceFromBase * this.Height);
            Point p3 = new Point((this.Width+ folderWidth) / 2, distanceFromBase * this.Height- folderHeight);
            Point p4 = new Point((this.Width) / 2, distanceFromBase * this.Height- folderHeight);
            Point p5 = new Point((this.Width) / 2 - labelHeight, distanceFromBase * this.Height- (folderHeight + labelHeight));
            Point p6 = new Point((this.Width- folderWidth) / 2, distanceFromBase * this.Height- (labelHeight + folderHeight));

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
                                             };
                geometryContext.PolyLineTo(points, true, true);
            }
            drawingContext.DrawGeometry(null, strokeContent, streamGeometry);
        }

        private void drawArrow(DrawingContext drawingContext)
        {
            double arrowHeigt = this.Height / 8;
            double arrowWidth = this.Width / 16;
            double arrowHeadHeight = arrowHeigt / 2;
            double distanceFromBase = 3 * this.Height/ 7;
            Point p1 = new Point((this.Width- arrowWidth) / 2, distanceFromBase - (arrowHeigt + arrowHeadHeight) + CurrentStep);
            Point p2 = new Point((this.Width+ arrowWidth) / 2, distanceFromBase - (arrowHeigt + arrowHeadHeight) + CurrentStep);
            Point p3 = new Point((this.Width+ arrowWidth) / 2, distanceFromBase - (arrowHeigt + arrowHeadHeight) + CurrentStep + arrowHeigt);
            Point p4 = new Point(this.Width/ 2 + arrowWidth, distanceFromBase - (arrowHeigt + arrowHeadHeight) + CurrentStep + arrowHeigt);
            Point p5 = new Point(this.Width/ 2, distanceFromBase + CurrentStep);
            Point p6 = new Point((this.Width- arrowWidth) / 2 - arrowWidth / 2, distanceFromBase - (arrowHeigt + arrowHeadHeight) + CurrentStep + arrowHeigt);
            Point p7 = new Point((this.Width- arrowWidth) / 2, distanceFromBase - (arrowHeigt + arrowHeadHeight) + CurrentStep + arrowHeigt);


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
            drawingContext.DrawGeometry(null, strokeContent, streamGeometry);
        }
    }
}

