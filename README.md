KPresentationFramework
======================

A framework for the creation of gesture-based applications for big-data presentation and navigation.
Gesture recognition is made by the KRecognizer component, using the Microsoft Kinect sensor and the [GestIT] (https://github.com/GestIT/GestIT) library.
The framework is based on the WPF architecture.

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



- **KPresenter**
- **KFolder**
- **KItem**
- **KPage**
