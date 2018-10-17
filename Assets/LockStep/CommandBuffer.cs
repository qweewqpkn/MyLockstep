using System.Collections.Generic;

namespace LockStep
{


    class CommandBuffer
    {
        //保存回合数据
        private Dictionary<int, TurnData> mTurnDataDic; 

        public CommandBuffer()
        {
            mTurnDataDic = new Dictionary<int, TurnData>();
        }

        void SendCommand()
        {

        }
    }
}
