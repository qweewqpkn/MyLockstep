using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LockStep
{
    class TurnData
    {
        private int mTurnID;
        private Dictionary<long, PlayerData> mPlayerDataDic = new Dictionary<long, PlayerData>();

        public void AddCommand(long playerID, Command command)
        {
            PlayerData data;
            if(!mPlayerDataDic.TryGetValue(playerID, out data))
            {
                data = new PlayerData();
                data.mPlayerID = playerID;
                mPlayerDataDic.Add(playerID, data);
            }

            data.mCommandList.Add(command);
        }

        public void AddCommand(long playerID, List<Command> commandList)
        {
            if(commandList == null)
            {
                return;
            }

            for (int i = 0; i < commandList.Count; i++)
            {
                AddCommand(playerID, commandList[i]);
            }
        }

        private bool IsTurnDataReady(int playerNum)
        {
            if (mPlayerDataDic.Count == playerNum)
            {
                return true;
            }

            return false;
        }

        public bool ProcessTurnData(int playerNum)
        {
            if(IsTurnDataReady(playerNum))
            {
                foreach(var item in mPlayerDataDic)
                {
                    for(int i = 0; i < item.Value.mCommandList.Count; i++)
                    {
                        item.Value.mCommandList[i].Process();
                    }
                }

                return true;
            }

            return false;
        }
    }

    class PlayerData
    {
        public long mPlayerID;
        public List<ICommand> mCommandList = new List<ICommand>();
    }
}
