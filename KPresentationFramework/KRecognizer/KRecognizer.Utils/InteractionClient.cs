/*
 * Author: Vittorio Massaro
 */
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.Kinect;
using GestIT;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Control;
using System.Windows.Forms;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Interaction;

namespace KRecognizerNS.Utils
{
    class InteractionClient : IInteractionClient
    {
        /*
         * This class is necessary for enabling the InteractionStream.
         * This class mustn't be changed.
         */
        public InteractionInfo GetInteractionInfoAtLocation(int skeletonTrackingId, InteractionHandType handType, double x, double y)
        {
               return new InteractionInfo
                {
                    IsPressTarget = true,
                    IsGripTarget = true,
                };
        }
    }
}
