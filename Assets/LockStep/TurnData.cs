using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LockStep
{
    class TurnData
    {
        private long mPlayerID;
        private int mTurnID;
        private List<ICommand> mCommands;
    }
}
