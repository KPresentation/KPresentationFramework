KPresentationFramework
======================

A framework for the creation of gesture-based applications for big-data presentation and navigation.
Gesture recognition is made by the KinectRecognizer component, using the Microsoft Kinect sensor and the [GestIT] (https://github.com/GestIT/GestIT) library.
The framework is based on the WPF architecture.

Recognized Gestures
===================

KinectRecognizer is able to recognize the following gestures:

- RightHandMove: raised when the user moves the right hand. The recognizer translates the spatial coordinates of the hands in   screen coordinates.
- PinchAndZoom: raised when a Pinch and Zoom gesture is beginning.
- NavigateBack:
- RightHandGrip: raised when the user closes the right hand.
- RightHandGripRelease: raised when the user opens the right hand.
- ImageGrabStart: raised when the user closes the two hands.
- SlideUp: raised when a Slide Up gesture is recognized.
- SlideDown: raised when a Slide Down gesture is recognized.
