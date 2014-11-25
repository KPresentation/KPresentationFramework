/*
 * Author: Vittorio Massaro
 */
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;
using GestIT;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Control;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Interaction;
using KRecognizerNS.Utils;

namespace KRecognizerNS
{
    public class KRecognizer
    {
        public KinectSensor kinectSensor = null;
        private InteractionStream stream;

        #region Support
        //Support fields
        private Skeleton[] skeletons = new Skeleton[Values.skeletonMaxNumber];
        private UserInfo[] oldUserInfo = new UserInfo[InteractionFrame.UserInfoArrayLength];
        private UserInfo[] userInfo = new UserInfo[InteractionFrame.UserInfoArrayLength];
        private long lastTimeStamp;
        #endregion

        #region InternalState
        private bool userTracking = false;
        //Current skeleton's ID.
        private int trackedId = 0;
        //Is right hand closed?
        private bool rightHandGripped = false;
        //Is left hand closed?
        private bool leftHandGripped = false;
        //Is the user sliding up?
        private bool slidingUp = false;
        //Is the user sliding down?
        private bool slidingDown = false;
        //Is in act a Pinch and Zoom gesture?
        private bool pinchAndZooming = false;
        //If a Pinch and Zoom gesture is in act, this field contains the initial distance between hands.
        private double pinchAndZoomInitialDistance = 0.0;
        //Is in act an Image Grab gesture?
        private bool imageGrabbin = false;
        //If a Pinch and Zoom gesture is in act:
        //This field contains the initial distance between hands.
        private double imageGrabInitialDistance = 0.0;
        //Those fields contain the initial angle.
        private double imageGrabInitalAngleX = 0.0;
        private double imageGrabInitalAngleY = 0.0;
        private double imageGrabInitalAngleZ = 0.0;
        //Those fields contain the initial position of the middle point of hands.
        private double imageGrabInitialX = 0.0;
        private double imageGrabInitialY = 0.0;
        #endregion
        
        //Private Event necessary for using the GestIT library.
        private FSharpEvent<SkeletonEventArgs> InteractionFramesReadyEvent = new FSharpEvent<SkeletonEventArgs>();


        #region PublicEvents
        //Raised when a new user starts being tracked
        public EventHandler<EventArgs> UserIn;
        //Raised when the user is no more tracked
        public EventHandler<EventArgs> UserOut;
        //Raised when the user clicks with the right hand.
        public EventHandler<BodyEventArgs> RightHandPush;
        //Raised when the user moves the right hand.
        public EventHandler<BodyEventArgs> RightHandMove;
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
        #endregion


        public KRecognizer() : this(SkeletonTrackingMode.Default){}

        
        public KRecognizer(SkeletonTrackingMode trackingMode)
        {
            # region FindSensor
            //Search of a kinect sensor.
            foreach (var s in KinectSensor.KinectSensors)
            {
                kinectSensor = s;
                break;
            }

            if (kinectSensor == null)
            {
                throw new Exception("Kinect sensor not found.");
            }
            if (kinectSensor.Status == KinectStatus.NotPowered)
            {
                throw new Exception("Kinect sensor not powered.");
            }

            //init()
            for (int i = 0; i < Values.skeletonMaxNumber; i++)
            {
                skeletons[i] = new Skeleton();
            }
            kinectSensor.SkeletonStream.Enable();
            kinectSensor.SkeletonStream.TrackingMode = trackingMode;
            kinectSensor.ColorStream.Enable();
            kinectSensor.DepthStream.Enable();
            
            kinectSensor.AllFramesReady += _AllFramesReady;
            stream = new InteractionStream(kinectSensor, new InteractionClient());
            stream.InteractionFrameReady += stream_InteractionFrameReady;
            kinectSensor.Start();
            
            //Sensor for the use of the GestIT library.
            SkeletonSensor sensor = new SkeletonSensor();
            sensor.Listen(SkeletonEvents.InteractionFramesReady, InteractionFramesReadyEvent.Publish);
            #endregion

            #region RightHandPush
            GroundTerm<SkeletonEvents, SkeletonEventArgs> rightHandPush = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs,bool>.FromConverter(new Converter<SkeletonEventArgs,bool>(rightHandPushPredicate)));
            rightHandPush.Gesture += rightHandPush_Gesture;
            #endregion

            #region PinchAndZoom
            /*Pinch and zoom gesture is modeled in the following way:
             * 
             * Start |>> (!*Continue |^| Terminate)
             * 
             */
            GroundTerm<SkeletonEvents, SkeletonEventArgs> pinchAndZoomStart = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(pinchAndZoomStartPredicate)));
            GroundTerm<SkeletonEvents, SkeletonEventArgs> pinchAndZoomContinue = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(pinchAndZoomContinuePredicate)));
            GroundTerm<SkeletonEvents, SkeletonEventArgs> pinchAndZoomTerminate = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(pinchAndZoomTerminatePredicate)));
            Iter<SkeletonEvents,SkeletonEventArgs> pinchAndZoomContinueIter = new Iter<SkeletonEvents,SkeletonEventArgs>(pinchAndZoomContinue);
            GestureExpr<SkeletonEvents, SkeletonEventArgs>[] pinchAndZoomChoiceSubexprs = {pinchAndZoomContinueIter, pinchAndZoomTerminate};
            Choice<SkeletonEvents,SkeletonEventArgs> pinchAndZoomChoice = new Choice<SkeletonEvents,SkeletonEventArgs>(pinchAndZoomChoiceSubexprs);
            GestureExpr<SkeletonEvents, SkeletonEventArgs>[] pinchAndZoomSequenceSubexprs = {pinchAndZoomStart, pinchAndZoomChoice};
            Sequence<SkeletonEvents,SkeletonEventArgs> pinchAndZoomSequence = new Sequence<SkeletonEvents,SkeletonEventArgs>(pinchAndZoomSequenceSubexprs);

            pinchAndZoomStart.Gesture += pinchAndZoomStart_Gesture;
            pinchAndZoomContinue.Gesture += pinchAndZoomContinue_Gesture;
            pinchAndZoomTerminate.Gesture += pinchAndZoomTerminate_Gesture;
            #endregion

            #region RightHandMove
            GroundTerm<SkeletonEvents, SkeletonEventArgs> rightHandMove = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(rightHandMovePredicate)));

            rightHandMove.Gesture += rightHandMove_Gesture;
            #endregion

            #region LeftHandGrip
            /*
             * Grip |>> Release
             */
            GroundTerm<SkeletonEvents, SkeletonEventArgs> leftHandGrip = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(leftHandGripPredicate)));
            GroundTerm<SkeletonEvents, SkeletonEventArgs> leftHandGripRelease = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(leftHandGripReleasePredicate)));
            leftHandGrip.Gesture += leftHandGrip_Gesture;
            leftHandGripRelease.Gesture += leftHandGripRelease_Gesture;
            GestureExpr<SkeletonEvents, SkeletonEventArgs>[] leftHandGripAndReleaseSubexprs = { leftHandGrip, leftHandGripRelease };
            Sequence<SkeletonEvents, SkeletonEventArgs> leftHandGripAndRelease = new Sequence<SkeletonEvents, SkeletonEventArgs>(leftHandGripAndReleaseSubexprs);
            #endregion

            #region RightHandGrip
            /*
             * Grip |>> Release
             */
            GroundTerm<SkeletonEvents, SkeletonEventArgs> rightHandGrip = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(rightHandGripPredicate)));
            GroundTerm<SkeletonEvents, SkeletonEventArgs> rightHandGripRelease = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(rightHandGripReleasePredicate)));
            rightHandGrip.Gesture += rightHandGrip_Gesture;
            rightHandGripRelease.Gesture += rightHandGripRelease_Gesture;
            GestureExpr<SkeletonEvents, SkeletonEventArgs>[] dragAndDropSubexprs = {rightHandGrip, rightHandGripRelease};
            Sequence<SkeletonEvents, SkeletonEventArgs> rightHandGripAndRelease = new Sequence<SkeletonEvents, SkeletonEventArgs>(dragAndDropSubexprs);
            #endregion

            #region GrabImage
            /*
             * Image grab gesture is modeled in the following way:
             * 
             * Grab |>> (!*Continue |^| Terminate )
             * 
             */
            GroundTerm<SkeletonEvents, SkeletonEventArgs> imageGrabStart = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(imageGrabStartPredicate)));
            GroundTerm<SkeletonEvents, SkeletonEventArgs> imageGrabContinue = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(imageGrabContinuePredicate)));
            GroundTerm<SkeletonEvents, SkeletonEventArgs> imageGrabTerminate = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(imageGrabTerminatePredicate)));
            Iter<SkeletonEvents, SkeletonEventArgs> imageGrabContinueIter = new Iter<SkeletonEvents, SkeletonEventArgs>(imageGrabContinue);
            GestureExpr<SkeletonEvents, SkeletonEventArgs>[] imageGrabChoiceSubexprs = { imageGrabContinueIter, imageGrabTerminate };
            Choice<SkeletonEvents, SkeletonEventArgs> imageGrabChoice = new Choice<SkeletonEvents, SkeletonEventArgs>(imageGrabChoiceSubexprs);
            GestureExpr<SkeletonEvents, SkeletonEventArgs>[] imageGrabSequenceSubexprs = { imageGrabStart, imageGrabChoice };
            Sequence<SkeletonEvents, SkeletonEventArgs> imageGrabSequence = new Sequence<SkeletonEvents, SkeletonEventArgs>(imageGrabSequenceSubexprs);

            imageGrabStart.Gesture += imageGrabStart_Gesture;
            imageGrabContinue.Gesture += imageGrabContinue_Gesture;
            imageGrabTerminate.Gesture += imageGrabTerminate_Gesture;
            #endregion

            #region SlideUp
            /*
             * Start |>> ( Convalidate |^| Terminate )
             */
            GroundTerm<SkeletonEvents, SkeletonEventArgs> slideUpStart = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(slideUpStartPredicate)));
            GroundTerm<SkeletonEvents, SkeletonEventArgs> slideUpTerminate = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(slideUpTerminatePredicate)));
            GroundTerm<SkeletonEvents, SkeletonEventArgs> slideUpConvalidate = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(slideUpConvalidatePredicate)));
            slideUpStart.Gesture += slideUpStart_Gesture;
            slideUpTerminate.Gesture += slideUpTerminate_Gesture;
            slideUpConvalidate.Gesture += slideUpConvalidate_Gesture;
            GestureExpr<SkeletonEvents, SkeletonEventArgs>[] slideUpChoiceSubexprs = { slideUpConvalidate, slideUpTerminate };
            Choice<SkeletonEvents, SkeletonEventArgs> slideUpChoice = new Choice<SkeletonEvents, SkeletonEventArgs>(slideUpChoiceSubexprs);
            GestureExpr<SkeletonEvents, SkeletonEventArgs>[] slideUpSubexprs = { slideUpStart, slideUpChoice };
            Sequence<SkeletonEvents, SkeletonEventArgs> slideUpSequence = new Sequence<SkeletonEvents, SkeletonEventArgs>(slideUpSubexprs);
            #endregion

            #region SlideDown
            /*
             * Start |>> ( Convalidate |^| Terminate )
             */
            GroundTerm<SkeletonEvents, SkeletonEventArgs> slideDownStart = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(slideDownStartPredicate)));
            GroundTerm<SkeletonEvents, SkeletonEventArgs> slideDownTerminate = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(slideDownTerminatePredicate)));
            GroundTerm<SkeletonEvents, SkeletonEventArgs> slideDownConvalidate = new GroundTerm<SkeletonEvents, SkeletonEventArgs>(SkeletonEvents.InteractionFramesReady, FSharpFunc<SkeletonEventArgs, bool>.FromConverter(new Converter<SkeletonEventArgs, bool>(slideDownConvalidatePredicate)));
            slideDownStart.Gesture += slideDownStart_Gesture;
            slideDownTerminate.Gesture += slideDownTerminate_Gesture;
            slideDownConvalidate.Gesture += slideDownConvalidate_Gesture; 
            GestureExpr<SkeletonEvents, SkeletonEventArgs>[] slideDownChoiceSubexprs = { slideDownConvalidate, slideDownTerminate };
            Choice<SkeletonEvents, SkeletonEventArgs> slideDownChoice = new Choice<SkeletonEvents, SkeletonEventArgs>(slideDownChoiceSubexprs);
            GestureExpr<SkeletonEvents, SkeletonEventArgs>[] slideDownSubexprs = { slideDownStart, slideDownChoice };
            Sequence<SkeletonEvents, SkeletonEventArgs> slideDownSequence = new Sequence<SkeletonEvents, SkeletonEventArgs>(slideDownSubexprs);
            #endregion

            #region ToGestureNet
            //The recognizer is represented by a net obtatined in this way:
            GestureExpr<SkeletonEvents, SkeletonEventArgs>[] recognizerNetSubexprs = { rightHandGripAndRelease, leftHandGripAndRelease, rightHandPush, rightHandMove, pinchAndZoomSequence, imageGrabSequence, slideDownSequence, slideUpSequence};
            Parallel<SkeletonEvents, SkeletonEventArgs> recognizerNet = new Parallel<SkeletonEvents, SkeletonEventArgs>(recognizerNetSubexprs);
            recognizerNet.ToGestureNet(sensor);
            #endregion

        }


       
        


        #region GestureHandlers
        /*
         * This region contains the handler called to the gestures recognition
         */
        private void rightHandPush_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            //If the user clicks, the right hand push event is raised.
            if (RightHandPush != null)
            {
                double X = args.Item3.newuserinfo.rightHandX;
                double Y = args.Item3.newuserinfo.rightHandY;
                int x = getX(X);
                int y = getY(Y);
                if ( x >= 0 && y >= 0 && x < Screen.PrimaryScreen.Bounds.Width && y < Screen.PrimaryScreen.Bounds.Height )
                    RightHandPush(this, new BodyEventArgs(x,y));
            }
        }
        private void rightHandMove_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            //If the user moves the right hand, a right hand move event is raised.
            double X = args.Item3.newuserinfo.rightHandX;
            double Y = args.Item3.newuserinfo.rightHandY; 
            int x = getX(X);
            int y = getY(Y);
            if (RightHandMove != null)
            {
                 RightHandMove(this, new BodyEventArgs(x, y));
            }
        }
        private void pinchAndZoomStart_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            //The pinch and zoom gesture is starting. The internal state is updated and the 
            //pinch and zoom start event is raised.
            pinchAndZooming = true;
            pinchAndZoomInitialDistance = Math.Abs(args.Item3.skeleton.Joints[JointType.HandLeft].Position.X - args.Item3.skeleton.Joints[JointType.HandRight].Position.X);
            if (PinchAndZoomStart != null)
            {
                PinchAndZoomStart(this, new EventArgs());
            }
        }
        private void pinchAndZoomContinue_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            //When the pinch and zoom gesture is in act, each time the user moves the hands, 
            //the pinch and zoom event is raised.
            double actualDistance = Math.Abs(args.Item3.skeleton.Joints[JointType.HandLeft].Position.X - args.Item3.skeleton.Joints[JointType.HandRight].Position.X);
            
            if (PinchAndZoomContinue != null)
            {
                PinchAndZoomContinue(this, new PinchAndZoomEventArgs(actualDistance / pinchAndZoomInitialDistance));
            }
        }
        private void pinchAndZoomTerminate_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            double newDeltaZ = Math.Abs(args.Item3.skeleton.Joints[JointType.HandLeft].Position.Z - args.Item3.skeleton.Joints[JointType.HandRight].Position.Z);
            double newDeltaY = Math.Abs(args.Item3.skeleton.Joints[JointType.HandLeft].Position.Y - args.Item3.skeleton.Joints[JointType.HandRight].Position.Y);
            
            pinchAndZooming = false;
            if (args.Item3.skeleton.Joints[JointType.HandRight].Position.Y < args.Item3.skeleton.Joints[JointType.Spine].Position.Y)
            {
                //If the pinch and zoom is interrupted, the corrisponding event is raised.
                if (PinchAndZoomInterrupted != null)
                {
                    PinchAndZoomInterrupted(this, new EventArgs());
                }
            }
            else if (args.Item3.skeleton.Joints[JointType.HandLeft].Position.Y < args.Item3.skeleton.Joints[JointType.Spine].Position.Y)
            {
                //If the pinch and zoom is interrupted, the corrisponding event is raised.
                if (PinchAndZoomInterrupted != null)
                {
                    PinchAndZoomInterrupted(this, new EventArgs());
                }
            }
            else if (newDeltaZ > Values.pinchAndZoomThreshold || newDeltaY > Values.pinchAndZoomThreshold )
            {
                //If the pinch and zoom is interrupted, the corrisponding event is raised.
                if (PinchAndZoomInterrupted != null)
                {
                    PinchAndZoomInterrupted(this, new EventArgs());
                }
            }
            else
            {
                //If the scale factor goes below the threshold, a navigate back event is raised.
                if (NavigateBack != null)
                {
                    NavigateBack(this, new EventArgs());
                }
            }
        }  
        private void rightHandGrip_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            rightHandGripped = true;
            //If a pinch and zoom gesture is in act, the recognizer ignores the right hand grip
            if (RightHandGrip != null && !pinchAndZooming)
            {
                //Screen coodinates
                double X = args.Item3.olduserinfo.rightHandX;
                double Y = args.Item3.olduserinfo.rightHandY;
                int x = getX(X);
                int y = getY(Y);
                RightHandGrip(this, new BodyEventArgs(x, y));
            }
        }
        private void rightHandGripRelease_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            double X = args.Item3.olduserinfo.rightHandX;
            double Y = args.Item3.olduserinfo.rightHandY;
            int x = getX(X);
            int y = getY(Y);
            if (RightHandGripRelease != null && !pinchAndZooming)
            {
                RightHandGripRelease(this, new BodyEventArgs(x,y));
            }

            rightHandGripped = false;
        }
        private void leftHandGrip_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            leftHandGripped = true;
        }
        private void leftHandGripRelease_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            leftHandGripped = false;
        }
        private void imageGrabStart_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            //When an image grab gesture starts, the recognizer saves:
            imageGrabbin = true;
            double deltaX = args.Item3.skeleton.Joints[JointType.HandLeft].Position.X - args.Item3.skeleton.Joints[JointType.HandRight].Position.X;
            double deltaY = args.Item3.skeleton.Joints[JointType.HandLeft].Position.Y - args.Item3.skeleton.Joints[JointType.HandRight].Position.Y;
            double deltaZ = args.Item3.skeleton.Joints[JointType.HandLeft].Position.Z - args.Item3.skeleton.Joints[JointType.HandRight].Position.Z;
            //initial distance between hands
            imageGrabInitialDistance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
            //initial angles
            imageGrabInitalAngleX = Math.Atan(deltaY / deltaZ);
            if (deltaZ > 0) imageGrabInitalAngleX += Math.PI;
            imageGrabInitalAngleY = Math.Atan(deltaZ / deltaX);
            if (deltaX > 0) imageGrabInitalAngleY += Math.PI;
            imageGrabInitalAngleZ = Math.Atan(-deltaY / deltaX);
            if (deltaX > 0) imageGrabInitalAngleZ += Math.PI;
            //initial coordinates of the middle point between hands.
            imageGrabInitialX = (args.Item3.skeleton.Joints[JointType.HandLeft].Position.X + args.Item3.skeleton.Joints[JointType.HandRight].Position.X) / 2.0;
            imageGrabInitialY = (args.Item3.skeleton.Joints[JointType.HandLeft].Position.Y + args.Item3.skeleton.Joints[JointType.HandRight].Position.Y) / 2.0;
            if (ImageGrabStart != null)
            {
                ImageGrabStart(this, new EventArgs());
            }
        }
        private void imageGrabContinue_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            double deltaX = args.Item3.skeleton.Joints[JointType.HandLeft].Position.X - args.Item3.skeleton.Joints[JointType.HandRight].Position.X;
            double deltaY = args.Item3.skeleton.Joints[JointType.HandLeft].Position.Y - args.Item3.skeleton.Joints[JointType.HandRight].Position.Y;
            double deltaZ = args.Item3.skeleton.Joints[JointType.HandLeft].Position.Z - args.Item3.skeleton.Joints[JointType.HandRight].Position.Z;
            double distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));

            double angleX = Math.Atan(deltaY / deltaZ);
            if (deltaZ > 0) angleX += Math.PI;
            if (Math.Abs(deltaY) < Values.handsMinDistanceII && Math.Abs(deltaZ) < Values.handsMinDistanceII)
                imageGrabInitalAngleX = angleX;

            double angleY = Math.Atan(deltaZ / deltaX);
            if (deltaX > 0) angleY += Math.PI;
            if (Math.Abs(deltaX) < Values.handsMinDistanceII && Math.Abs(deltaZ) < Values.handsMinDistanceII)
                imageGrabInitalAngleY = angleY;

            double angleZ = Math.Atan(-deltaY / deltaX);
            if (deltaX > 0) angleZ += Math.PI;
            if (Math.Abs(deltaX) < Values.handsMinDistanceII && Math.Abs(deltaY) < Values.handsMinDistanceII)
                imageGrabInitalAngleZ = angleZ;

            double X = (args.Item3.skeleton.Joints[JointType.HandLeft].Position.X + args.Item3.skeleton.Joints[JointType.HandRight].Position.X) / 2.0;
            double Y = (args.Item3.skeleton.Joints[JointType.HandLeft].Position.Y + args.Item3.skeleton.Joints[JointType.HandRight].Position.Y) / 2.0;
            if (ImageGrabContinue != null)
            {
                ImageGrabContinue(this, new ImageGrabEventArgs(distance / imageGrabInitialDistance, angleX - imageGrabInitalAngleX, angleY - imageGrabInitalAngleY, angleZ - imageGrabInitalAngleZ, X - imageGrabInitialX, Y - imageGrabInitialY));
            }
            // State refreshing
            imageGrabInitialDistance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
            imageGrabInitalAngleX = angleX;
            imageGrabInitalAngleY = angleY;
            imageGrabInitalAngleZ = angleZ;
            imageGrabInitialX = (args.Item3.skeleton.Joints[JointType.HandLeft].Position.X + args.Item3.skeleton.Joints[JointType.HandRight].Position.X) / 2.0;
            imageGrabInitialY = (args.Item3.skeleton.Joints[JointType.HandLeft].Position.Y + args.Item3.skeleton.Joints[JointType.HandRight].Position.Y) / 2.0;
        }
        private void imageGrabTerminate_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            imageGrabbin = false;
            if (ImageGrabTerminate != null)
            {
                ImageGrabTerminate(this, new EventArgs());
            }
        }
        private void slideUpStart_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            slidingUp = true;
        }
        private void slideUpConvalidate_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            slidingUp = false;
            if (SlideUp != null)
            {
                SlideUp(this, new EventArgs());
            }
        }
        private void slideUpTerminate_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            slidingUp = false;
        }
        private void slideDownStart_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            slidingDown = true;
        }
        private void slideDownConvalidate_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            slidingDown = false;
            if (SlideDown != null)
            {
                SlideDown(this, new EventArgs());
            }
        }
        private void slideDownTerminate_Gesture(object sender, Tuple<GestureExpr<SkeletonEvents, SkeletonEventArgs>, SkeletonEvents, SkeletonEventArgs> args)
        {
            slidingDown = false;
        }
        #endregion

        #region Predicates
        /*
         * This region contains the predicates for recognizing the gestures.
         */
        private bool rightHandPushPredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton))
                return false;

            if (rightHandGripped)
            {
                return false;
            }
            if (pinchAndZooming)
            {
                return false;
            }
            double deltaX = Math.Abs(e.olduserinfo.rightHandX - e.newuserinfo.rightHandX);
            double deltaY = Math.Abs(e.olduserinfo.rightHandY - e.newuserinfo.rightHandY);
            if (!e.olduserinfo.rightHandPressed && e.newuserinfo.rightHandPressed && deltaX < 0.1 && deltaY < 0.1 && e.newuserinfo.rightHandPressExtent > 1.3)
                return true;

            return false;
        }
        private bool rightHandMovePredicate(SkeletonEventArgs e)
        {
            //During pinch and zooming the right hand moved is ignored.
            if (pinchAndZooming)
            {
                return false;
            }
            return true;
        }
        private bool pinchAndZoomStartPredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton) || !isLeftHandGood(e.skeleton))
                return false;
            if (rightHandGripped)
                return false;
            if (pinchAndZooming)
                return false;

            double newDeltaY = Math.Abs(e.skeleton.Joints[JointType.HandLeft].Position.Y - e.skeleton.Joints[JointType.HandRight].Position.Y);
            double newDeltaZ = Math.Abs(e.skeleton.Joints[JointType.HandLeft].Position.Z - e.skeleton.Joints[JointType.HandRight].Position.Z);
            double newDeltaX = Math.Abs(e.skeleton.Joints[JointType.HandLeft].Position.X - e.skeleton.Joints[JointType.HandRight].Position.X);
            //The hands must be aligned on the Y and Z axis.
            if (newDeltaY > Values.pinchAndZoomThreshold)
                return false;
            if (newDeltaZ > Values.pinchAndZoomThreshold)
                return false;
            if (newDeltaX < Values.pinchAndZoomMinimumStart)
                return false;
            //The two hands must be above the spine's joint.
            if (e.skeleton.Joints[JointType.HandRight].Position.Y < e.skeleton.Joints[JointType.Spine].Position.Y)
                return false;

            if (e.skeleton.Joints[JointType.HandLeft].Position.Y < e.skeleton.Joints[JointType.Spine].Position.Y)
                return false;

            return true;
        }
        private bool pinchAndZoomContinuePredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton) || !isLeftHandGood(e.skeleton))
                return false;

            if (!pinchAndZooming)
                return false;


            double newDeltaY = Math.Abs(e.skeleton.Joints[JointType.HandLeft].Position.Y - e.skeleton.Joints[JointType.HandRight].Position.Y);
            double newDeltaZ = Math.Abs(e.skeleton.Joints[JointType.HandLeft].Position.Z - e.skeleton.Joints[JointType.HandRight].Position.Z);
            double actualDistance = Math.Abs(e.skeleton.Joints[JointType.HandLeft].Position.X - e.skeleton.Joints[JointType.HandRight].Position.X);


            //If the scale factor goes under the threshold, the pinch and zoom terminate because it will be 
            //raised a navigate back event.
            if (actualDistance / pinchAndZoomInitialDistance < Values.navigateBackThreshold)
                return false;

            if (newDeltaY > Values.pinchAndZoomThreshold)
                return false;
            if (newDeltaZ > Values.pinchAndZoomThreshold)
                return false;
            if (e.skeleton.Joints[JointType.HandRight].Position.Y < e.skeleton.Joints[JointType.Spine].Position.Y)
                return false;
            if (e.skeleton.Joints[JointType.HandLeft].Position.Y < e.skeleton.Joints[JointType.Spine].Position.Y)
                return false;

            return true;
        }
        private bool pinchAndZoomTerminatePredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton) || !isLeftHandGood(e.skeleton))
                return false;

            if (!pinchAndZooming)
                return false;


            double newDeltaY = Math.Abs(e.skeleton.Joints[JointType.HandLeft].Position.Y - e.skeleton.Joints[JointType.HandRight].Position.Y);
            double newDeltaZ = Math.Abs(e.skeleton.Joints[JointType.HandLeft].Position.Z - e.skeleton.Joints[JointType.HandRight].Position.Z);
            double actualDistance = Math.Abs(e.skeleton.Joints[JointType.HandLeft].Position.X - e.skeleton.Joints[JointType.HandRight].Position.X);

            if (newDeltaY > Values.pinchAndZoomThreshold)
                return true;
            if (newDeltaZ > Values.pinchAndZoomThreshold)
                return true;
            if (actualDistance / pinchAndZoomInitialDistance < Values.navigateBackThreshold)
                return true;
            if (e.skeleton.Joints[JointType.HandRight].Position.Y < e.skeleton.Joints[JointType.Spine].Position.Y)
                return true;
            if (e.skeleton.Joints[JointType.HandLeft].Position.Y < e.skeleton.Joints[JointType.Spine].Position.Y)
                return true;


            return false;
        }
        private bool leftHandGripPredicate(SkeletonEventArgs e)
        {

            if (!isLeftHandGood(e.skeleton))
                return false;
            if (e.newuserinfo.leftEventType == InteractionHandEventType.Grip)
            {
                return true;
            }
            return false;
        }
        private bool leftHandGripReleasePredicate(SkeletonEventArgs e)
        {
            if (!isLeftHandGood(e.skeleton))
                return false;
            if (e.newuserinfo.leftEventType == InteractionHandEventType.GripRelease)
            {
                return true;
            }
            return false;
        }
        private bool rightHandGripPredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton))
                return false;
            if (rightHandGripped)
            {
                return false;
            }

            if (e.newuserinfo.rightEventType == InteractionHandEventType.Grip)
            {
                return true;
            }

            return false;
        }
        private bool rightHandGripReleasePredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton))
                return false;
            if (e.newuserinfo.rightEventType == InteractionHandEventType.GripRelease)
            {
                return true;
            }

            return false;
        }
        private bool imageGrabStartPredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton) || !isLeftHandGood(e.skeleton))
                return false;
            if (imageGrabbin)
                return false;
            if (e.skeleton.Joints[JointType.HandRight].Position.Y < e.skeleton.Joints[JointType.Spine].Position.Y)
                return false;

            if (e.skeleton.Joints[JointType.HandLeft].Position.Y < e.skeleton.Joints[JointType.Spine].Position.Y)
                return false;

            double distanceX = Math.Abs(e.skeleton.Joints[JointType.HandRight].Position.X - e.skeleton.Joints[JointType.HandLeft].Position.X);
            double distanceY = Math.Abs(e.skeleton.Joints[JointType.HandRight].Position.Y - e.skeleton.Joints[JointType.HandLeft].Position.Y);
            double handsDistance = Math.Sqrt(Math.Pow(distanceX,2)+Math.Pow(distanceY,2));
            //Hands are two close.
            if (handsDistance < Values.handsMinDistance)
                return false;
            //Both hands are closed.
            if (e.newuserinfo.leftEventType == InteractionHandEventType.Grip && e.newuserinfo.rightEventType == InteractionHandEventType.Grip)
                return true;
            if (e.newuserinfo.leftEventType == InteractionHandEventType.Grip && rightHandGripped)
                return true;
            if (leftHandGripped && e.newuserinfo.rightEventType == InteractionHandEventType.Grip)
                return true;
            return false;
        }
        private bool imageGrabContinuePredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton) || !isLeftHandGood(e.skeleton))
                return false;
            if (!imageGrabbin)
                return false;
            double distanceX = Math.Abs(e.skeleton.Joints[JointType.HandRight].Position.X - e.skeleton.Joints[JointType.HandLeft].Position.X);
            double distanceY = Math.Abs(e.skeleton.Joints[JointType.HandRight].Position.Y - e.skeleton.Joints[JointType.HandLeft].Position.Y);
            double distanceZ = Math.Abs(e.skeleton.Joints[JointType.HandRight].Position.Z - e.skeleton.Joints[JointType.HandLeft].Position.Z);
            double handsDistance = Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2) + Math.Pow(distanceZ, 2));

            if (handsDistance < Values.handsMinDistance)
                return false;
            if (e.newuserinfo.leftEventType != InteractionHandEventType.GripRelease && e.newuserinfo.rightEventType != InteractionHandEventType.GripRelease)
                return true;
            return false;
        }
        private bool imageGrabTerminatePredicate(SkeletonEventArgs e)
        {
            if (!imageGrabbin)
                return false;
            //One of the hands is open.
            if (e.newuserinfo.leftEventType == InteractionHandEventType.GripRelease || e.newuserinfo.rightEventType == InteractionHandEventType.GripRelease)
                return true;
            return false;
        }
        private bool slideUpStartPredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton))
                return false;
            if (rightHandGripped || pinchAndZooming || imageGrabbin)
                return false;
            double deltaY = (getY(e.newuserinfo.rightHandY) - getY(e.olduserinfo.rightHandY))/((double)Screen.PrimaryScreen.Bounds.Height);
            double deltaX = (getX(e.newuserinfo.rightHandX) - getX(e.olduserinfo.rightHandX)) / ((double)Screen.PrimaryScreen.Bounds.Width);
            double speedY = deltaY / (double)e.deltaT;
            double speedX = deltaX / (double)e.deltaT;
            double Y = getY(e.olduserinfo.rightHandY);
            if (Y <= Screen.PrimaryScreen.Bounds.Height / 2 )
                return false;

            if (speedY < -Values.slidingThreshold && Math.Abs(speedX) < Values.slidingThreshold / 2 && !slidingUp)
            {
                return true;
            }
            return false;
        }
        private bool slideUpConvalidatePredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton))
                return false;
            if (!slidingUp)
                return false;
            double y = getY(e.newuserinfo.rightHandY);

            if (y <= Screen.PrimaryScreen.Bounds.Height * Values.percentScreen)
            {
                return true;
            }

            return false;
        }
        private bool slideUpTerminatePredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton))
                return false;

            double deltaY = (getY(e.newuserinfo.rightHandY) - getY(e.olduserinfo.rightHandY)) / ((double)Screen.PrimaryScreen.Bounds.Height);
            double speedY = deltaY / (double)e.deltaT;
            if (speedY > -Values.slidingThreshold / 2 && slidingUp)
                return true;
            return false;
        }
        private bool slideDownStartPredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton))
            {
                return false;
            }
            if (rightHandGripped || pinchAndZooming || imageGrabbin)
            {
                return false;
            }
            double deltaY = (getY(e.newuserinfo.rightHandY) - getY(e.olduserinfo.rightHandY)) / ((double)Screen.PrimaryScreen.Bounds.Height);
            double deltaX = (getX(e.newuserinfo.rightHandX) - getX(e.olduserinfo.rightHandX)) / ((double)Screen.PrimaryScreen.Bounds.Width);
            double speedY = deltaY / (double)e.deltaT;
            double speedX = deltaX / (double)e.deltaT;
            double Y = getY(e.olduserinfo.rightHandY);
            if (Y > Screen.PrimaryScreen.Bounds.Height * 2 / 3)
            {
                return false;
            }
            if (speedY > Values.slidingThreshold && Math.Abs(speedX) < Values.slidingThreshold / 2 && !slidingDown)
                return true;
            return false;
        }
        private bool slideDownConvalidatePredicate(SkeletonEventArgs e)
        {
            if (!slidingDown)
                return false;
            if (!isRightHandGood(e.skeleton))
                return false;

            double y = getY(e.newuserinfo.rightHandY);

            if (y > Screen.PrimaryScreen.Bounds.Height )
                return true;

            return false;
        }
        private bool slideDownTerminatePredicate(SkeletonEventArgs e)
        {
            if (!isRightHandGood(e.skeleton))
                return false;
            double deltaY = (getY(e.newuserinfo.rightHandY) - getY(e.olduserinfo.rightHandY)) / ((double)Screen.PrimaryScreen.Bounds.Height);
            double speedY = deltaY / (double)e.deltaT;
            if (speedY < Values.slidingThreshold / 2 && slidingDown)
                return true;
            return false;
        }
        #endregion

        #region Utils
        /*
         * This region contains some util function. It mustn't be changed.
         */
        private void _AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.OpenSkeletonFrame();
            DepthImageFrame dephtFrame = e.OpenDepthImageFrame();

            if (skeletonFrame == null || dephtFrame == null)
                return;

            skeletonFrame.CopySkeletonDataTo(skeletons);

            bool tracking = false;

            foreach (var sk in skeletons)
            {
                //If the tracked user is already present, the recognizer continues to tracking him.
                if (sk.TrackingId == trackedId && sk.TrackingState == SkeletonTrackingState.Tracked)
                {
                    this.userTracking = true;
                    tracking = true;
                    break;
                }
            }
            if (!tracking)
            {
                //Otherwise a new user is selected.
                foreach (var skeleton in skeletons)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        //New skeleton start being tracked.
                        if (UserIn != null)
                        {
                            UserIn(this, new EventArgs());
                        }
                        this.userTracking = true;
                        trackedId = skeleton.TrackingId;
                        tracking = true;
                        break;
                    }
                }
            }
            if (!tracking)
            {
                //No available skeletons.
                if (this.userTracking)
                {
                    this.userTracking = false;
                    if (UserOut != null)
                    {
                        UserOut(this, new EventArgs());
                    }
                }
                skeletonFrame.Dispose();
                dephtFrame.Dispose();
                return;
            }

            var accelerometerReading = kinectSensor.AccelerometerGetCurrentReading();
            stream.ProcessDepth(dephtFrame.GetRawPixelData(), dephtFrame.Timestamp);
            stream.ProcessSkeleton(skeletons, accelerometerReading, skeletonFrame.Timestamp);


            skeletonFrame.Dispose();
            dephtFrame.Dispose();
        }
        private void stream_InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            InteractionFrame frame = e.OpenInteractionFrame();

            if (frame == null)
                return;

            frame.CopyInteractionDataTo(userInfo);

            Skeleton mySkeleton = null;
            bool valid = false;

            foreach (var info in userInfo)
            {
                //Seraching for the info about the tracked user.
                if (info.SkeletonTrackingId == trackedId)
                {
                    UserInfo myUserInfo = info;
                    bool found = false;
                    //Seraching for my tracked skeleton
                    foreach (var skeleton in skeletons)
                    {
                        if (skeleton.TrackingId == trackedId)
                        {
                            mySkeleton = cloneSkeleton(skeleton);
                            found = true;
                            break;
                        }
                    }
                    //Something wrong.
                    if (!found)
                    {
                        lastTimeStamp = frame.Timestamp;
                        frame.Dispose();
                        return;
                    }

                    UserInfo myOldInfo = null;
                    //If the right hand is noising, the frame is rejected.
                    valid = isRightHandGood(mySkeleton);
                    found = false;
                    if (oldUserInfo[0] != null)
                    {
                        //Searching for the old info about the user.
                        foreach (var oldinfo in oldUserInfo)
                        {
                            if (oldinfo.SkeletonTrackingId == trackedId)
                            {
                                myOldInfo = oldinfo;
                                found = true;
                                break;
                            }
                        }
                    }
                    //If there isn't, i reject the frame.
                    if (!found)
                    {
                        lastTimeStamp = frame.Timestamp;
                        frame.CopyInteractionDataTo(oldUserInfo);
                        frame.Dispose();
                        return;
                    }
                    //otherwise the event is generated.
                    InteractionFramesReadyEvent.Trigger(new SkeletonEventArgs(mySkeleton, myUserInfo, myOldInfo,frame.Timestamp - lastTimeStamp));
                    //If the frame wasn't rejected, the timestamp is updated.
                    if ( valid )
                        lastTimeStamp = frame.Timestamp;
                    break;
                }
            }
            if ( valid )
                frame.CopyInteractionDataTo(oldUserInfo);
            frame.Dispose();

        }
        //The methods above are able to convert a point in the space in a point on the screen.
        private int getX(double X)
        {
            double myX = (X + 0.5)/2;
            int width = Screen.PrimaryScreen.Bounds.Width;
            return (int)(myX * width);
        }
        private int getY(double Y)
        {
            double myY = (Y + 0.5) / 2;
            int height = Screen.PrimaryScreen.Bounds.Height;
            return (int)(myY * height);
        }
        private static Skeleton cloneSkeleton(Skeleton skeleton)
        {
            //Cloning a skeleton.
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(ms, skeleton);

            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();

            return obj as Skeleton;
        }
        private bool isRightHandGood(Skeleton skeleton)
        {
            return isRealPosition(skeleton, JointType.HandRight);
        }
        private bool isLeftHandGood(Skeleton skeleton)
        {
            return isRealPosition(skeleton, JointType.HandLeft);
        }
        private bool isRealPosition(Skeleton skeleton, JointType type)
        {

            /*
             * This method is able to decide if a joint in a given skeleton is valid.
             */
            Joint thisJoint = skeleton.Joints[type];
            if (thisJoint.TrackingState == JointTrackingState.Inferred)
                return false;
            if (thisJoint.TrackingState == JointTrackingState.NotTracked)
                return false;

            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.JointType != type)
                {
                    if (thisJoint.Position.X == joint.Position.X)
                        if (thisJoint.Position.Y == joint.Position.Y)
                            if (thisJoint.Position.Z == joint.Position.Z)
                                return false;
                }
            }
            return true;
        }
        #endregion



    }
}
