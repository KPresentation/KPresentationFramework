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
using System.Collections.ObjectModel;

namespace KRecognizerNS.Utils
{
    /*
     * This class contains the information about the tracked user. 
     * This class mustn't be changed.
     */
    class SkeletonEventArgs : System.EventArgs
    {
        public Skeleton skeleton{get; private set;}
        public MyUserInfo olduserinfo { get; private set; }
        public MyUserInfo newuserinfo { get; private set; }
        public long deltaT { get; private set; }

        public SkeletonEventArgs(Skeleton skeleton, UserInfo current, UserInfo previous, long time)
        {
            this.skeleton = skeleton;
            this.olduserinfo = new MyUserInfo(previous);
            this.newuserinfo = new MyUserInfo(current);
            this.deltaT = time;
        }

        
    }
}
