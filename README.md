KPresentationFramework
======================

A framework for the creation of gesture-based applications for big-data presentation and navigation.
Gesture recognition is made by the KRecognizer component, using the Microsoft Kinect sensor and the [GestIT] (https://github.com/GestIT/GestIT) library.
The framework is based on the WPF architecture.

Click here to read [how to use KPresentationFramework] (https://github.com/KPresentation/KPresentationFramework/blob/master/HOWTO.md) .

Recognized Gestures
===================

KRecognizer is able to recognize the following gestures:

- **Right Hand Move**: raised when the user moves the right hand. The recognizer translates the spatial coordinates of the hands in   screen coordinates.
- **Navigate Back**: raised when the user aligns horizontally its hands and approaches them.  
- **Right Hand Drag&Drop**: raised when the user uses the right hand to drag&drop an object.
- **Image Grab**: raised when the user uses the two hands to grab and manipulate the shown content.
- **Slide Up**: raised when the user fastly moves up the right hand carrying the cursor above the screen.
- **Slide Down**: raised when the user fastly moves down the right hand carrying the cursor under the screen.

Framework Classes
=================

The following classes are the meaningful ones in the framework, because they are strictly necessary to create new applications.

- **KPresenter**: this class is the main control of the framework. Creating a new KPresenter cotrol, the user can access to all features offers. All datas must be specified inside it, as a tree.
- **KFolder**: this class represents a folder. A KFolder is defined by its appearance when is closed and its content, which is a set of KFolders and KItems.
- **KItem**: this class represents a link to the data that the user can manipulate. A KItem is defined by its appearance when is closed and a link to a KPage, which contains the data that are shown when the user opens the KItem.
- **KPage**: this class represents a container of the data that must be shown. It is editable like a WPF page and, what is more, allows to handle the events for the manipulation raised from KRecognizer.

The framework, moreover, offers the possibility to accumulate several KItem and show all the relative Kpages together, introducing a concept of **workspace**.
When the user grabs a KItem, a functional area appears in the lower part of the screen, and permits to select or deselect the KItem.
Opening the workspace, all the KPages attached to the selected KItem are shown together.
