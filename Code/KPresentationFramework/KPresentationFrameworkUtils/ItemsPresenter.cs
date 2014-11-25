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
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace KPresentationFramework
{

    //This class represents the control where the folder's content are shown.
    public class ItemsPresenter : KinectCanvas
    {
        //Scale factor for animtion.
        static readonly double factScaleAnim = 20;

        //Current folder.
        KFolder currentFolder = null;

        bool isAnimating = false;

        //All animations performed.
        Storyboard openAnimation;
        Storyboard showChildContentAnimation;
        Storyboard closeAnimation;
        Storyboard showParentContentAnimation;
        Storyboard backToOneAnimation;
        Storyboard closeOpacAnimation;
        Storyboard openOpacAnimation;

        //Functional area used to open elements.
        FunctionalArea oa;

        //Functional area used to add elements in workspace.
        FunctionalArea wa;

        //Z-INDEX valuse
        //  0 - element
        //  5 - functional area
        // 10 - gripped element

        //This property indicates if an animation is on.
        public bool IsAnimating
        {
            set { isAnimating = value; }
            get { return isAnimating; }
        }

        //This property contains the current folder.
        public KFolder CurrentFolder
        {
            get { return currentFolder; }
        }

        public ItemsPresenter()
        {
            //Scale transform to perform animation.
            RenderTransform = new ScaleTransform();

            this.Visibility = System.Windows.Visibility.Collapsed;
            this.Opacity = 0;

            //Adds the functional areas.
            oa = new FunctionalArea();
            oa.VisualShape = new OpenShape();
            Canvas.SetZIndex(oa, 5);
            this.Children.Add(oa);

            wa = new FunctionalArea();
            wa.VisualShape = new WorkspaceShape();
            Canvas.SetZIndex(wa, 5);
            Canvas.SetBottom(wa, 0);
            this.Children.Add(wa);

            this.RightHandGripElement += showAreas;
            this.RightHandGripReleaseElement += hideAreas;

            //This animation is used to "enter" in a child element.
            #region openAnimation

            //setto animazione quando viene cliccata la cartella
            openAnimation = new Storyboard();
            //ristabilisco valore inizio animazione a fine animazione
            //cambio contenuto a fine animazione
            openAnimation.Completed += endAnimationCollapseControl;

            //easing function
            CubicEase myEaseIn = new CubicEase();
            myEaseIn.EasingMode = EasingMode.EaseIn;

            //TODO: 20 deve essere calcolato
            DoubleAnimation scaleXIn = new DoubleAnimation(factScaleAnim, new Duration(TimeSpan.FromSeconds(0.75)));
            scaleXIn.EasingFunction = myEaseIn;
            DependencyProperty[] chainScaleX = 
            {
                ItemsPresenter.RenderTransformProperty,
                ScaleTransform.ScaleXProperty
            };
            Storyboard.SetTargetProperty(scaleXIn, new PropertyPath("(0).(1)", chainScaleX));
            openAnimation.Children.Add(scaleXIn);

            //TODO: 20 deve essere calcolato
            DoubleAnimation scaleYIn = new DoubleAnimation(factScaleAnim, new Duration(TimeSpan.FromSeconds(0.75)));
            scaleYIn.EasingFunction = myEaseIn;
            DependencyProperty[] chainScaleY = 
            {
                ItemsPresenter.RenderTransformProperty,
                ScaleTransform.ScaleYProperty
            };
            Storyboard.SetTargetProperty(scaleYIn, new PropertyPath("(0).(1)", chainScaleY));
            openAnimation.Children.Add(scaleYIn);

            DoubleAnimation opacIn = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.75)));
            opacIn.EasingFunction = myEaseIn;
            Storyboard.SetTargetProperty(opacIn, new PropertyPath(ItemsPresenter.OpacityProperty));
            openAnimation.Children.Add(opacIn);

            #endregion

            //This animation is used to open the new folder. The new contents seem to arrive from behind the old ones. 
            #region showChildContentAnimation

            showChildContentAnimation = new Storyboard();
            showChildContentAnimation.BeginTime = TimeSpan.FromSeconds(0.25);
            showChildContentAnimation.Completed += endAnimation;
            //easing function
            CubicEase myEaseChild = new CubicEase();
            myEaseChild.EasingMode = EasingMode.EaseOut;

            DoubleAnimation scaleXChild = new DoubleAnimation(0,1, new Duration(TimeSpan.FromSeconds(0.75)));
            scaleXChild.EasingFunction = myEaseChild;
            Storyboard.SetTargetProperty(scaleXChild, new PropertyPath("(0).(1)", chainScaleX));
            showChildContentAnimation.Children.Add(scaleXChild);

            DoubleAnimation scaleYChild = new DoubleAnimation(0,1, new Duration(TimeSpan.FromSeconds(0.75)));
            scaleYChild.EasingFunction = myEaseChild;
            Storyboard.SetTargetProperty(scaleYChild, new PropertyPath("(0).(1)", chainScaleY));
            showChildContentAnimation.Children.Add(scaleYChild);

            DoubleAnimation opacChild = new DoubleAnimation(0,1, new Duration(TimeSpan.FromSeconds(0.75)));
            opacChild.EasingFunction = myEaseChild;
            Storyboard.SetTargetProperty(opacChild, new PropertyPath(ItemsPresenter.OpacityProperty));
            showChildContentAnimation.Children.Add(opacChild);

            #endregion

            //This animation is used to close the current folder making smaller its content.
            #region closeAnimation

            //setto animazione quando viene cliccata la cartella
            closeAnimation = new Storyboard();

            closeAnimation.Completed += endAnimationCollapseControl;

            //easing function
            CubicEase myEaseOut = new CubicEase();
            myEaseOut.EasingMode = EasingMode.EaseIn;

            DoubleAnimation scaleXOut = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.75)));
            scaleXOut.EasingFunction = myEaseOut;
            Storyboard.SetTargetProperty(scaleXOut, new PropertyPath("(0).(1)", chainScaleX));
            closeAnimation.Children.Add(scaleXOut);

            DoubleAnimation scaleYOut = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.75)));
            scaleYOut.EasingFunction = myEaseOut;
            Storyboard.SetTargetProperty(scaleYOut, new PropertyPath("(0).(1)", chainScaleY));
            closeAnimation.Children.Add(scaleYOut);

            DoubleAnimation opacOut = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.75)));
            opacOut.EasingFunction = myEaseOut;
            Storyboard.SetTargetProperty(opacOut, new PropertyPath(ItemsPresenter.OpacityProperty));
            closeAnimation.Children.Add(opacOut);

            #endregion

            //This animation is used to open the parent folder. The new contents seem to arrive from ahead the old ones.
            #region showParentContentAnimation

            showParentContentAnimation = new Storyboard();
            showParentContentAnimation.BeginTime = TimeSpan.FromSeconds(0.25);
            showParentContentAnimation.Completed += endAnimation;
            //easing function
            CubicEase myEaseParent = new CubicEase();
            myEaseParent.EasingMode = EasingMode.EaseOut;

            DoubleAnimation scaleXParent = new DoubleAnimation(factScaleAnim, 1, new Duration(TimeSpan.FromSeconds(0.75)));
            scaleXParent.EasingFunction = myEaseParent;
            Storyboard.SetTargetProperty(scaleXParent, new PropertyPath("(0).(1)", chainScaleX));
            showParentContentAnimation.Children.Add(scaleXParent);

            DoubleAnimation scaleYParent = new DoubleAnimation(factScaleAnim, 1, new Duration(TimeSpan.FromSeconds(0.75)));
            scaleYParent.EasingFunction = myEaseParent;
            Storyboard.SetTargetProperty(scaleYParent, new PropertyPath("(0).(1)", chainScaleY));
            showParentContentAnimation.Children.Add(scaleYParent);

            DoubleAnimation opacParent = new DoubleAnimation(0,1, new Duration(TimeSpan.FromSeconds(0.75)));
            opacParent.EasingFunction = myEaseParent;
            Storyboard.SetTargetProperty(opacParent, new PropertyPath(ItemsPresenter.OpacityProperty));
            showParentContentAnimation.Children.Add(opacParent);

            #endregion

            //This animation is used to reset the scale factore to one.
            #region backToOneAnimation

            backToOneAnimation = new Storyboard();
            backToOneAnimation.Completed += endAnimation;
            //easing function
            CubicEase myEaseBack = new CubicEase();
            myEaseBack.EasingMode = EasingMode.EaseOut;

            DoubleAnimation scaleXBack = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(0.75)));
            scaleXBack.EasingFunction = myEaseBack;
            Storyboard.SetTargetProperty(scaleXBack, new PropertyPath("(0).(1)", chainScaleX));
            backToOneAnimation.Children.Add(scaleXBack);

            DoubleAnimation scaleYBack = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(0.75)));
            scaleYBack.EasingFunction = myEaseBack;
            Storyboard.SetTargetProperty(scaleYBack, new PropertyPath("(0).(1)", chainScaleY));
            backToOneAnimation.Children.Add(scaleYBack);

            #endregion

            //This animation is used to hide the control animating its opacity.
            #region closeOpacAnimation

            closeOpacAnimation = new Storyboard();
            DoubleAnimation opacClose = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.75)));
            CubicEase opacCloseEase = new CubicEase();
            opacCloseEase.EasingMode = EasingMode.EaseIn;
            Storyboard.SetTargetProperty(opacClose, new PropertyPath(ItemsPresenter.OpacityProperty));
            closeOpacAnimation.Children.Add(opacClose);
            closeOpacAnimation.Completed += endAnimationCollapseControl;

            #endregion

            //This animation is used to show the control animating its opacity.
            #region openOpacAnimation

            openOpacAnimation = new Storyboard();
            openOpacAnimation.BeginTime = TimeSpan.FromSeconds(0.25);
            DoubleAnimation opacOpen = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(0.75)));
            CubicEase opacOpenEase = new CubicEase();
            opacCloseEase.EasingMode = EasingMode.EaseOut;
            Storyboard.SetTargetProperty(opacOpen, new PropertyPath(ItemsPresenter.OpacityProperty));
            openOpacAnimation.Children.Add(opacOpen);
            openOpacAnimation.Completed += endAnimation;   

            #endregion
        }


        void hideAreas(object sender, BodyEventArgs e)
        {
            oa.hideArea();
            wa.hideArea();
            if (oa.IsHandInside)
            {
                if (GrippedElement != null)
                {
                    //apro contenuto
                    (GrippedElement as KNavigable).OpenContent();
                }
                return;
            }
            if (wa.IsHandInside)
            {
                if (GrippedElement != null)
                {
                    //aggiungi elementeo al workspace
                    if (GrippedElement is KItem)
                    {
                        (GrippedElement as KItem).InWorkspace();
                    }
                }
                return;
            }
            if (GrippedElement != null)
            {
                //ristabilisco posizione
                (GrippedElement as KNavigable).RestorePosition();
            }
        }

        void showAreas(object sender, BodyEventArgs e)
        {
            oa.Width = this.ActualWidth;
            oa.showArea();

            if (GrippedElement is KItem)
            {
                //mostro WorkspaceArea solo se è strato grippato un oggetto e non una cartella
                wa.Width = this.ActualWidth;
                wa.showArea();
            }
        }

        void endAnimation(object sender, EventArgs e) 
        {
            isAnimating = false;
        }

        void endAnimationCollapseControl(object sender, EventArgs e)
        {
            isAnimating = false;
            this.Visibility = Visibility.Collapsed;
        }

        public void StartCloseAnimation()
        {
            isAnimating = true;
            this.RenderTransformOrigin = new Point(0.5,0.5);

            closeAnimation.Begin(this);
        }

        public void StartShowParentContentAnimation(KFolder newFolder)
        {
            isAnimating = true;
            this.RenderTransformOrigin = new Point(0.5,0.5);

            //Loads new content.
            ChangeContent(newFolder);
            
            this.Visibility = Visibility.Visible;
            showParentContentAnimation.Begin(this);
        }

        public void StartOpenAnimation(Point centerScale)
        {
            isAnimating = true;
            this.RenderTransformOrigin = centerScale;

            openAnimation.Begin(this);
        }

        public void StartShowChildContentAnimation(KFolder newFolder,Point centerScale)
        {
            isAnimating = true;
            //Sets the origin of the transforms.
            this.RenderTransformOrigin = centerScale;

            //Loads new content.
            ChangeContent(newFolder);

            this.Visibility = Visibility.Visible;
            showChildContentAnimation.Begin(this);
        }

        public void StartBackToOneAnimation()
        {
            isAnimating = true;
            backToOneAnimation.Begin(this);
        }

        public void StartCloseToWorkspaceAnimation()
        {
            isAnimating = true;
            closeOpacAnimation.Begin(this);
        }

        public void StartOpenFromWorkspaceAnimation()
        {
            isAnimating = true;
            this.Visibility = Visibility.Visible;
            openOpacAnimation.Begin(this);
        }

        void ChangeContent(KFolder newFolder)
        {
            currentFolder = newFolder;
            //If the new folder has a background, sets it.
            if (newFolder.BackgroundURI != null)
            {
                this.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/" + newFolder.BackgroundURI)));
            }
            else
            {
                this.Background = null;
            }

            //Deletes all content except the functional areas.
            this.InternalChildren.RemoveRange(2, this.InternalChildren.Count - 2);
            
            //Loads new content.
            foreach (UIElement element in newFolder.FolderContent)
            {
                this.InternalChildren.Add(element);
                //Sets the position of each element.
                Canvas.SetLeft(element, (element as KNavigable).X);
                Canvas.SetTop(element, (element as KNavigable).Y);
            }
        }
    }
}
