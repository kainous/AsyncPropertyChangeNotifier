using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel
{
    public sealed class DisposableAction : IDisposable
    {
        private readonly Action _exitLock;

        public DisposableAction(Action exitLockAction)
        { _exitLock = exitLockAction; }

        public void Dispose()
        { _exitLock(); }
    }
}
