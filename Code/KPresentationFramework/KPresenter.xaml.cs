using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media.Animation;
using KRecognizerNS;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;
using System.Windows.Resources;


namespace KPresentationFramework
{
    //This class represents the main control of the framework.
    public partial class KPresenter : UserControl
    {

        ObservableCollection<KNavigable> folderTree;
        
        //This is the root of the navigation tree.
        KFolder root;

        //The index of the current itemsPresenter.
        int activeItemsPresenter;

        List<ItemsPresenter> animItemsPres;
        //This flag indicate if an item is shown
        bool showItem;

        //This flag indicate if the workspace is shown
        bool showWorkspace;

        bool kinectFound;

        //This adorner replace the cursor.
        MyAdorner a;

        Brush backgroundBeforeLoad = Brushes.Black;

        //This property is the folder navigation tree.
        public ObservableCollection<KNavigable> FolderTree
        {
            get { return folderTree; }
        }

        //This property is the background of the frames before that theirs contents are loaded.
        public Brush BackgroundBeforeLoad
        {
            get { return backgroundBeforeLoad; }
            set 
            {
                backgroundBeforeLoad = value;
                itemShow.BackgroundBeforeLoad = backgroundBeforeLoad;
                workspaceShow.BackgroundBeforeLoad = backgroundBeforeLoad;
            }
        }

        //This property contains the URI of the image on the background of the root folder
        public string BackgroundURI
        {
            get { return root.BackgroundURI; }
            set { root.BackgroundURI = value; }
        }

        public KPresenter()
        {
            folderTree = new ObservableCollection<KNavigable>();
            folderTree.CollectionChanged += folderTree_CollectionChanged;

            root = new KFolder();
            root.SetParentFolder(null);
            root.SetRootControl(this);

            showItem = false;
            showWorkspace = false;
            kinectFound = true;

            InitializeComponent();

            noKinect.Background = new RadialGradientBrush(Color.FromArgb(150, 0, 0, 0), Color.FromArgb(0, 0, 0, 0));

            activeItemsPresenter = 0;
            animItemsPres = new List<ItemsPresenter>();
            animItemsPres.Add(items0);
            animItemsPres.Add(items1);

            this.Loaded += NavigationControl_Loaded;
            //this.MouseDown += NavigationControl_MouseDown;

            a = new MyAdorner(this);
            try
            {
                //Tries to initialize the KinectRecognizer.
                KRecognizer k = new KRecognizer();
                k.RightHandMove += Kinect_Move;
                k.RightHandPush += Kinect_Push;
                k.RightHandGrip += Kinect_Grip;
                k.RightHandGripRelease += Kinect_GripRelease;

                k.NavigateBack += Kinect_Back;

                k.PinchAndZoomStart += Kinect_ZoomStart;
                k.PinchAndZoomContinue += Kinect_ZoomContinue;
                k.PinchAndZoomInterrupted += Kinect_ZoomInt;

                k.ImageGrabStart += Kinect_ImageGrabStart;
                k.ImageGrabContinue += Kinect_ImageGrabContinue;
                k.ImageGrabTerminate += Kinect_ImageGrabTerminate;

                k.SlideUp += Kinect_SlideUp;
                k.SlideDown += Kinect_SlideDown;

                k.UserIn += Kinect_UserIn;
                k.UserOut += Kinect_UserOut;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                kinectFound = false;
                noKinect.Visibility = Visibility.Visible;
                noUser.Visibility = Visibility.Collapsed;
            }
        }
        
        void folderTree_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //Sets the parent folder and the navigation control.
                KNavigable newChild = folderTree[e.NewStartingIndex];
                root.FolderContent.Add(newChild);
            }
        }

        //These methods are handlers of events raised from kinectRecognizer.
        //In each of them is checked if an animation is in progress. If yes, the method does nothing.
        //The events Move,Grip,GripRelease are propagated to the current itemsPresenter control.
        //The eventes ImageGrabStart,ImageGrabContinue,ImageGrabTerminate are propagated to the itemShow control only if it is shown.
        //The other ones are handled here.
        void Kinect_ZoomInt(object sender, EventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating && !showItem && !showWorkspace)
            {
                animItemsPres[activeItemsPresenter].StartBackToOneAnimation();
            }
        }

        void Kinect_ZoomContinue(object sender, PinchAndZoomEventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating && !showItem && !showWorkspace)
            {
                double factScale = e.scaleFact;
                if (e.scaleFact > 1)
                {
                    factScale = 1 + Math.Pow(((factScale) - 1 )/ factScale, 3);
                }
                (animItemsPres[activeItemsPresenter].RenderTransform as ScaleTransform).ScaleX = factScale;
                (animItemsPres[activeItemsPresenter].RenderTransform as ScaleTransform).ScaleY = factScale;
            }
        }

        void Kinect_ZoomStart(object sender, EventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating && !showItem && !showWorkspace)
            {
                //Sets the center for the render transform.
                animItemsPres[activeItemsPresenter].RenderTransformOrigin = new Point(0.5, 0.5);
                //Removes the old animation values.
                (animItemsPres[activeItemsPresenter].RenderTransform as ScaleTransform).BeginAnimation(ScaleTransform.ScaleXProperty, null);
                (animItemsPres[activeItemsPresenter].RenderTransform as ScaleTransform).BeginAnimation(ScaleTransform.ScaleYProperty, null);
                //Sets the start values.
                (animItemsPres[activeItemsPresenter].RenderTransform as ScaleTransform).ScaleX = 1;
                (animItemsPres[activeItemsPresenter].RenderTransform as ScaleTransform).ScaleY = 1;
                if (AdornerLayer.GetAdornerLayer(this).GetAdorners(this) != null)
                {
                    AdornerLayer.GetAdornerLayer(this).Remove(a);
                }
            }
        }

        void Kinect_Back(object sender, EventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating && !showItem && !showWorkspace)
            {
                if (!showItem)
                {
                    if (animItemsPres[activeItemsPresenter].CurrentFolder.ParentFolder != null)
                    {
                        //Load the content of the parent folder.
                        ChangeCurrentFolder(animItemsPres[activeItemsPresenter].CurrentFolder.ParentFolder, true);
                    }
                    else
                    {
                        //The current folder is the root, there isn't a parent folder.
                        animItemsPres[activeItemsPresenter].StartBackToOneAnimation();
                    }
                }
            }
        }

        void Kinect_Push(object sender, BodyEventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating && !showItem && !showWorkspace)
            {
                animItemsPres[activeItemsPresenter].RaiseRightHandPush(sender, e);
            }
        }

        void Kinect_Move(object sender, BodyEventArgs e)
        {
            if (!showItem && !showWorkspace)
            {
                //Gets the new position of the hand.
                Point position = new Point(e.X, e.Y);
                //If there is an adorener, refresh it.
                if (AdornerLayer.GetAdornerLayer(this) != null)
                {
                    if (AdornerLayer.GetAdornerLayer(this).GetAdorners(this) == null)
                    {
                        a = new MyAdorner(this);
                        AdornerLayer.GetAdornerLayer(this).Add(a);
                    }
                    a.setP(position);
                    AdornerLayer.GetAdornerLayer(this).Update();

                }
            }

            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating && !showItem && !showWorkspace)
            {
                    
                    animItemsPres[activeItemsPresenter].RaiseRightHandMove(sender, e);
            }
        }

        void Kinect_Grip(object sender, BodyEventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating && !showItem && !showWorkspace)
            {
                animItemsPres[activeItemsPresenter].RaiseRightHandGrip(sender, e);
                a.IsClosed = true;
                AdornerLayer.GetAdornerLayer(this).Update();
            }
        }

        void Kinect_GripRelease(object sender, BodyEventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating && !showItem && !showWorkspace)
            {
                a.IsClosed = false;
                AdornerLayer.GetAdornerLayer(this).Update();
                if (animItemsPres[activeItemsPresenter].IsGripped)
                {
                    animItemsPres[activeItemsPresenter].RaiseRightHandGripRelease(sender, e);
                }

            }
        }

        void Kinect_ImageGrabStart(object sender, EventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating)
            {
                if (showItem)
                {
                    itemShow.Kinect_ImageGrabStart(sender, e);
                }
            }
        }

        void Kinect_ImageGrabContinue(object sender, ImageGrabEventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating)
            {
                if (showItem)
                {
                    itemShow.Kinect_ImageGrabContinue(sender, e);
                }
            }
        }

        void Kinect_ImageGrabTerminate(object sender, EventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating)
            {
                if (showItem)
                {
                    itemShow.Kinect_ImageGrabTerminate(sender, e);
                }
            }
        }

        void Kinect_SlideUp(object sender, EventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating)
            {
                if (showItem)
                {
                    //If itemShow is shonw, hide it.
                    HideItem();
                    return;
                }
                if(!showWorkspace)
                {
                    //If neither itemShow nor workspace isn't shown, show workspace.
                    ShowWorkspace();
                }
            }
        }

        void Kinect_SlideDown(object sender, EventArgs e)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating)
            {
                if (showWorkspace)
                {
                    //If workspace is shonw, hide it.
                    HideWorkspace();
                }
            }
        }

        void Kinect_UserOut(object sender, EventArgs e)
        {
            noUser.Visibility = Visibility.Visible;
        }

        void Kinect_UserIn(object sender, EventArgs e)
        {
            noUser.Visibility = Visibility.Collapsed;
        }

        //This method is called when the content shown must be updated.
        //The parameter newElement contains the new data.
        //If it is an Item, show its data. 
        //If it is a folder, show its content.
        public void ChangeContent(KNavigable newElement)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating)
            {
                if (newElement is KItem)
                {
                    ShowItem(newElement as KItem);
                }
                if (newElement is KFolder)
                {
                    //mostro contenuto cartella
                    ChangeCurrentFolder(newElement as KFolder, false);
                }
            }
        }

        //This method is called to add an element in the workspace.
        public void AddToWorkspace(KItem i)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating)
            {
                workspaceShow.AddToWorkspace(i.ContentURI);
            }
        }

        //This method is called to remove an element from the workspace.
        public void RemoveFromWorkspace(KItem i)
        {
            if (!animItemsPres[1 - activeItemsPresenter].IsAnimating && !animItemsPres[activeItemsPresenter].IsAnimating &&
                !itemShow.IsAnimating && !workspaceShow.IsAnimating)
            {
                workspaceShow.RemoveFromWorkspace(i.ContentURI);
            }
        }

        //Load the data when the control is completely loaded.
        void NavigationControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (kinectFound)
            {
                items0.StartShowChildContentAnimation(root, new Point(0.5, 0.5));
            }
        }

        void NavigationControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
            {
                if (!showItem)
                {
                    //sto mostrando una cartella
                    if (animItemsPres[activeItemsPresenter].CurrentFolder.ParentFolder != null)
                    {
                        ChangeCurrentFolder(animItemsPres[activeItemsPresenter].CurrentFolder.ParentFolder, true);
                    }
                }
                else
                {
                    //sto mostrando un oggetto, devo mostrare suo genitore
                    HideItem();
                }
            }
            if (e.ChangedButton == System.Windows.Input.MouseButton.Middle)
            {
                ShowWorkspace();
            }
        }

        //Changes the current folder.
        //The second parameter is useful to know if the new folder is the parent of a child of the current one.
        void ChangeCurrentFolder(KFolder newFolder,bool isParent)
        {
            //Sets z-index between ItemsPresenter.
            Panel.SetZIndex(animItemsPres[1 - activeItemsPresenter], 25);
            Panel.SetZIndex(animItemsPres[activeItemsPresenter], 50);

            if (isParent)
            {
                Point centerScale = new Point(0.5, 0.5);

                //Hides the old content.
                animItemsPres[activeItemsPresenter].StartCloseAnimation();
                //Shows the new content.
                animItemsPres[1 - activeItemsPresenter].StartShowParentContentAnimation(newFolder);
            }
            else
            {
                //Calculates the center of the child which is opening in order to perform the animation.
                Size childSize = newFolder.ChildSize;
                Point centerFolder = newFolder.TranslatePoint(new Point(childSize.Width / 2, childSize.Height / 2), animItemsPres[activeItemsPresenter]);
                Point centerScale = new Point(centerFolder.X / animItemsPres[activeItemsPresenter].ActualWidth, centerFolder.Y / animItemsPres[activeItemsPresenter].ActualHeight);

                //Hides the old content.
                animItemsPres[activeItemsPresenter].StartOpenAnimation(centerScale);
                //Shows the new content.
                animItemsPres[1 - activeItemsPresenter].StartShowChildContentAnimation(newFolder, centerScale);
            }
            //Changes the index of the current ItemsPresenter.
            activeItemsPresenter = 1 - activeItemsPresenter;
        }

        void ShowItem(KItem newItem)
        {

            //Sets z-index between ItemsPresenter and itemShow.
            Panel.SetZIndex(itemShow, 25);
            Panel.SetZIndex(animItemsPres[activeItemsPresenter], 50);

            //Calculates the center of the child which is opening in order to perform the animation.
            Size childSize = newItem.ChildSize;
            Point centerFolder = newItem.TranslatePoint(new Point(childSize.Width / 2, childSize.Height / 2), animItemsPres[activeItemsPresenter]);
            Point centerScale = new Point(centerFolder.X / animItemsPres[activeItemsPresenter].ActualWidth, centerFolder.Y / animItemsPres[activeItemsPresenter].ActualHeight);

            showItem = true;

            //Hides the adorner.
            AdornerLayer.GetAdornerLayer(this).Remove(a);
             
            animItemsPres[activeItemsPresenter].StartOpenAnimation(centerScale);
            itemShow.ShowItem(newItem);
        }

        void HideItem()
        {
            //Sets z-index between ItemsPresenter and itemShow
            Panel.SetZIndex(itemShow, 50);
            Panel.SetZIndex(animItemsPres[activeItemsPresenter], 25);

            KFolder parentFolder = itemShow.CurrentItem.ParentFolder;
            Point centerScale = new Point(0.5, 0.5);

            showItem = false;

            itemShow.Close();
            animItemsPres[activeItemsPresenter].StartShowParentContentAnimation(parentFolder);
        }

        void ShowWorkspace() 
        {
            //Sets z-index between ItemsPresenter and workspace.
            Panel.SetZIndex(workspaceShow, 50);
            Panel.SetZIndex(animItemsPres[activeItemsPresenter], 25);

            showWorkspace = true;

            AdornerLayer.GetAdornerLayer(this).Remove(a);

            animItemsPres[activeItemsPresenter].StartCloseToWorkspaceAnimation();
            workspaceShow.StartOpenAnimation();
        }

        void HideWorkspace()
        {
            //Sets z-index between ItemsPresenter and workspace.
            Panel.SetZIndex(workspaceShow, 50);
            Panel.SetZIndex(animItemsPres[activeItemsPresenter], 25);

            showWorkspace = false;
            animItemsPres[activeItemsPresenter].StartOpenFromWorkspaceAnimation();
            workspaceShow.StartCloseAnimation();
        }
    }
    
    //This class represent a cursor.
    class MyAdorner : Adorner
    {

        Point p;
        double w = 50, h = 50;

        public bool IsClosed { get; set; }

        BitmapSource handOpen;
        BitmapSource handClose;

        public MyAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            //Loads the images.
            handOpen = new BitmapImage(new Uri("pack://application:,,,/KPresentationFramework;component/Cursor/hand.png"));
            handClose = new BitmapImage(new Uri("pack://application:,,,/KPresentationFramework;component/Cursor/handclose.png"));
            p = new Point(0, 0);
        }

        public void setP(Point p)
        {
            this.p = p;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {

            if (IsClosed)
            {
                drawingContext.DrawImage(handClose, new Rect(p.X - w / 2, p.Y - h / 2, w, h));
            }
            else
            {
                drawingContext.DrawImage(handOpen, new Rect(p.X - w / 2, p.Y - h / 2, w, h));
            }
        }
    }
}
