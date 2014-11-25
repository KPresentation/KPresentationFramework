/*
 * Author: Vittorio Massaro
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;

namespace KRecognizerNS.Utils
{
    /*
     * This is an utility class used for creating a clonable version of the Microsoft.Kinect.Toolkit.Interaction.UserInfo.
     */
    class MyUserInfo
    {
        public int skeletonID { get; private set; }
        public InteractionHandEventType leftEventType { get; private set; }
        public InteractionHandEventType rightEventType { get; private set; }
        public bool leftHandTracked { get; private set; }
        public bool rightHandTracked { get; private set; }
        public bool leftHandPressed { get; private set; }
        public bool rightHandPressed { get; private set; }
        public double leftHandX { get; private set; }
        public double rightHandX { get; private set; }
        public double leftHandY { get; private set; }
        public double rightHandY { get; private set; }
        public double leftHandRawX { get; private set; }
        public double rightHandRawX { get; private set; }
        public double leftHandRawY { get; private set; }
        public double rightHandRawY { get; private set; }
        public double leftHandRawZ { get; private set; }
        public double rightHandRawZ { get; private set; }
        public double rightHandPressExtent { get; private set; }
        public double leftHandPressExtent { get; private set; }
        public MyUserInfo(UserInfo toCopy)
        {
            skeletonID = toCopy.SkeletonTrackingId;
            rightHandTracked = false;
            leftHandTracked = false;
            foreach (var hand in toCopy.HandPointers)
            {
                if (hand.HandType == InteractionHandType.Right)
                {
                    rightHandTracked = hand.IsTracked;
                    rightEventType = hand.HandEventType;
                    rightHandPressed = hand.IsPressed;
                    rightHandRawX = hand.RawX;
                    rightHandRawY = hand.RawY;
                    rightHandRawZ = hand.RawZ;
                    rightHandX = hand.X;
                    rightHandY = hand.Y;
                    rightHandPressExtent = hand.PressExtent;
                }
                if (hand.HandType == InteractionHandType.Left)
                {
                    leftHandTracked = hand.IsTracked;
                    leftEventType = hand.HandEventType;
                    leftHandPressed = hand.IsPressed;
                    leftHandRawX = hand.RawX;
                    leftHandRawY = hand.RawY;
                    leftHandRawZ = hand.RawZ;
                    leftHandX = hand.X;
                    leftHandY = hand.Y;
                    leftHandPressExtent = hand.PressExtent;
                }
            }
        }
    }
}
