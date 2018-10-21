using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LockStep
{
    interface ILockStep
    {
        void UpdateFixed(int deltaTime);
    }
}
