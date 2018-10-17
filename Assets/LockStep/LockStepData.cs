using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LockStep
{
    class TurnData
    {
        private int mTurnID;
        private Dictionary<long, PlayerData> mPlayerDataDic;

        public bool IsTurnDataReady(int playerNum)
        {
            if(mPlayerDataDic.Count == playerNum)
            {
                return true;
            }

            return false;
        }

        public void AddCommand(long playerID, ICommand command)
        {
            PlayerData data;
            if(!mPlayerDataDic.TryGetValue(playerID, out data))
            {
                data = new PlayerData(playerID);
                mPlayerDataDic.Add(playerID, data);
            }

            data.AddCommand(command);
        }

        public void AddCommand(long playerID, List<ICommand> commandList)
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
    }

    class PlayerData
    {
        private long mPlayerID;
        private List<ICommand> mCommandList = new List<ICommand>();

        public PlayerData(long playerID)
        {
            mPlayerID = playerID;
        }

        public void AddCommand(ICommand command)
        {
            if(mCommandList != null)
            {
                mCommandList.Add(command);
            }
        }
    }
}
