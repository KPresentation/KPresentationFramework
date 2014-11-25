/*
 * Author: Daniele Castellana
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media.Animation;
using System.Windows.Input;
using KRecognizerNS;

namespace KPresentationFramework
{
    //This class represents an internal node of the navigation tree.
    //It contains a set of Navigable element, which are its children on the tree.
    public class KFolder : KNavigable
    {
        UIElement visualPreview;
        ObservableCollection<KNavigable> folderContent;
        String backgroundURI = null;

        //This property is the apparence of the foldere when is closed.
        public UIElement VisualPreview 
        {
            get { return visualPreview; }
            set 
            {
                this.RemoveVisualChild(visualPreview);
                visualPreview = value;
                this.AddVisualChild(visualPreview);
            }
        }

        //This property is the container of the children elements.
        public ObservableCollection<KNavigable> FolderContent 
        {
            get { return folderContent; }
        }

        //This property contains the URI of the image on the background of the folder
        public string BackgroundURI
        {
            get { return backgroundURI; }
            set { backgroundURI = value; }
        }

        public KFolder()
            : base()
        {
            //Initializes the collection of children and register a function
            folderContent = new ObservableCollection<KNavigable>();
            folderContent.CollectionChanged += SetParent;
        }

        //This function is called when a new element is added in the folder
        void SetParent(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                KNavigable newChild = folderContent[e.NewStartingIndex];
                newChild.SetParentFolder(this);
                newChild.SetRootControl(this.RootControl);
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visualPreview;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            visualPreview.Measure(ChildSize);
            return ChildSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            visualPreview.Arrange(new Rect(ChildSize));
            return ChildSize;
        }
    }
}
