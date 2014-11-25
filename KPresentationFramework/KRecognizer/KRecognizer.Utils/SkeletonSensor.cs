/*
 * Author: Vittorio Massaro
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestIT;
using Microsoft.FSharp.Control;

namespace KRecognizerNS.Utils
{
    /*
     * This class is necessary for using the GestIT library.
     * This class mustn't be changed.
     */
    class SkeletonSensor : ISensor<SkeletonEvents,SkeletonEventArgs>
    {
        private Dictionary<SkeletonEvents, IEvent<FSharpHandler<SkeletonEventArgs>, SkeletonEventArgs>> item = new Dictionary<SkeletonEvents, IEvent<FSharpHandler<SkeletonEventArgs>, SkeletonEventArgs>>();

        public void Listen ( SkeletonEvents ev, IEvent<FSharpHandler<SkeletonEventArgs>,SkeletonEventArgs> ie )
        {
            item.Add(ev, ie);
        }

        IEvent<FSharpHandler<SkeletonEventArgs>, SkeletonEventArgs> ISensor<SkeletonEvents, SkeletonEventArgs>.this[SkeletonEvents value]
        {
            get { return item[value]; }
        }
    }
}
