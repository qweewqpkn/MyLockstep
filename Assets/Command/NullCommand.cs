using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LockStep
{
    [Serializable]
    public class NullCommand : Command
    {
        public NullCommand() : base(CommandType.ENone)
        {

        }

        public override void Process()
        {
            
        }
    }
}
