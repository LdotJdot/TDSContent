﻿using System.Threading;

namespace TDSAot.State
{
    internal class RunningState
    {
        internal SemaphoreSlim gOs = new SemaphoreSlim(1,1);
        internal bool Threadrunning = false;
        internal bool Threadrest = false;

    }
}