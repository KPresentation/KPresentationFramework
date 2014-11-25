How to use KPresentationFramework
=================================

First of all, in order to create a new application, it's strictly necessary to import the KRecognizer and KPresentationFramework assemblies.

**Create a KPresenter control**

In order to create a KPresenter control, you must import the namespace KPresentationFramework.
After that, you can add the KPresenter control as a normal WPF Control. 
```xaml
<Window x:Class="MyMainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:kpf="clr-namespace:KPresentationFramework;assembly=KPresentationFramework">

    <kpf:KPresenter>
        <kpf:KPresenter.FolderTree>
            <!-- Here you must specify the data tree -->
        </kpf:KPresenter.FolderTree>
    </kpf:KPresenter>
</Window>
```

**Specify the data tree**

In order to specify the data tree, you must use KFolder and KItem.
The attributes PercentageX and PercentageY are either in KFolder and in KItem and they allow to specify the position in the screen of the element.
The attribute BakcgroundURI can be used to set an image to show as a background to the folder's content.
The attribute VisualPreview can be used to specify the appearance of the folder when is closed. It is a WPF's UIElement.
The attribute FolderContent allows to specify the data subtree rooted in the folder.
```xaml
<kpf:KFolder BackgroundURI="Img/myBackground.png" PercentageX="50" PercentageY="50">
    <kpf:KFolder.VisualPreview>
        <!-- UIElement -->
    </kpf:KFolder.VisualPreview>
    <kpf:KFolder.FolderContent>
        <!-- Another data tree -->
    </kpf:KFolder.FolderContent>
</kpf:KFolder>
```
The attribute ContentURI must be used to attach a KPage to the KItem.
```xaml
<kpf:KItem PercentageX="20" PercentageY="65" ContentURI="MyKPage.xaml">
    <!-- UIElement that specifies the visual preview -->
</kpf:KItem>
```

**Define a KPage**

In order to handle the manipulation events, you can add the handler function.

```xaml
<kpf:KPage x:Class="MyKPageClass"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
      xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
      xmlns:kpf="clr-namespace:KPresentationFramework;assembly=KPresentationFramework"
      ImageGrabStart="MyKPage_ImageGrabStart"
      ImageGrabContinue="MyKPage_ImageGrabContinue"
      ImageGrabTerminate="MyKPage_ImageGrabTerminate">
   <!-- As usual with a WPF page -->
</kpf:KPage>
```

Code behind:

```c#
void MyKPage_ImageGrabStart(object sender, EventArgs e)
{
    //do something
}

void MyKPage_ImageGrabContinue(object sender, KRecognizerNS.ImageGrabEventArgs e)
{
   //do something
}

void MyKPage_ImageGrabTerminate(object sender, EventArgs e)
{
    //do something
}
```

When a KPage is shown in the workspace, it is loaded with the parameter w set to 1.

```
MyKPage.xaml?w=1
```
