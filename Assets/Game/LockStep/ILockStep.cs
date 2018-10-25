using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    interface ILockStep
    {
        void Update();
        void UpdateFixed(int deltaTime);
    }
}
