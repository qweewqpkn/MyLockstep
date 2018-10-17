using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LockStep
{
    interface ICommand
    {
        void Process();

        byte[] Serialize();

        T Deserialize<T>(byte[] bytes);
    }
}
