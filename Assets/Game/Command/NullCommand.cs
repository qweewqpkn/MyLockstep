using Network;
using System;

namespace Game
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
