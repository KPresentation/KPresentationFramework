KPresentationFramework
======================

A framework for the creation of gesture-based applications for big-data presentation and navigation.
Gesture recognition is made by the KinectRecognizer component, using the Microsoft Kinect sensor and the [GestIT] (https://github.com/GestIT/GestIT) library.
The framework is based on the WPF architecture.

Recognized Gestures
===================

KinectRecognizer is able to recognize the following gestures:

- RightHandMove: raised when the user moves the right hand. The recognizer translates the spatial coordinates of the hands in   screen coordinates.
  //Raised when a Pinch and Zoom gesture is beginning.
  public EventHandler<EventArgs> PinchAndZoomStart;
  //Raised when a Pinch and Zoom is in act and the user's hands are moving.
  public EventHandler<PinchAndZoomEventArgs> PinchAndZoomContinue;
  //Raised when a Pinch and Zoom gesture is interrupted.
  public EventHandler<EventArgs> PinchAndZoomInterrupted;
  //Raised when the scale factor goes under the Values.navigateBackThreshold.
  public EventHandler<EventArgs> NavigateBack;
  //Raised when the user closes the right hand.
  public EventHandler<BodyEventArgs> RightHandGrip;
  //Raised when the user opens the right hand.
  public EventHandler<BodyEventArgs> RightHandGripRelease;
  //Raised when the user closes the two hands.
  public EventHandler<EventArgs> ImageGrabStart;
  //Raised when a Image Grab gesture is in act and the user's hands are moving.
  public EventHandler<ImageGrabEventArgs> ImageGrabContinue;
  //Raised when the user open one of the two hands.
  public EventHandler<EventArgs> ImageGrabTerminate;
  //Raised when a Slide Up gesture is recognized.
  public EventHandler<EventArgs> SlideUp;
  //Raised when a Slide Down gesture is recognized.
  public EventHandler<EventArgs> SlideDown;
