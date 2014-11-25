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
    public abstract class AnimableShape : Shape
    {
        //This is the property that will be animated.
        //The classes which extend this one, can use its value on onRendere to perform an animation
        public static readonly DependencyProperty CurrentStepProperty =
             DependencyProperty.Register("CurrentStep", typeof(double),
             typeof(AnimableShape), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double CurrentStep
        {
            get { return (double)GetValue(CurrentStepProperty); }
            set { SetValue(CurrentStepProperty, value); }
        }

        //Animation on currentStep
        Storyboard animate;

        public AnimableShape() : base()
        {
            animate = new Storyboard();

            //create the animation
            DoubleAnimation doubleAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.5)));
            doubleAnimation.AutoReverse = true;
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            CubicEase cubicErase = new CubicEase();
            cubicErase.EasingMode = EasingMode.EaseInOut;
            doubleAnimation.EasingFunction = cubicErase;
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(CurrentStepProperty));

            animate.Children.Add(doubleAnimation);
        }

        //Extends the default behavior in order to personalize the appearance
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            internalDraw(drawingContext);
        }

        //This method must be implemented to realize a different appearance
        protected virtual void internalDraw(DrawingContext drawingContext){}

        //This method start the animation
        public void startAnimation()
        {
            (animate.Children[0] as DoubleAnimation).To = this.Width / 7;
            animate.Begin(this, true);
        }

        //This method stop the animation
        public void stopAnimation()
        {
            animate.Stop(this);
        }
    }
}
