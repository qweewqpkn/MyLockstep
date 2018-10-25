using Google.Protobuf;
using Network;
using System.Collections.Generic;


namespace Game
{
    class TurnData
    {
        private int mTurnID;
        private Dictionary<long, PlayerData> mPlayerDataDic = new Dictionary<long, PlayerData>();

        public void AddCommand(long playerID, Command command)
        {
            PlayerData data;
            if (!mPlayerDataDic.TryGetValue(playerID, out data))
            {
                data = new PlayerData();
                data.mPlayerID = playerID;
                mPlayerDataDic.Add(playerID, data);
            }

            data.mCommandList.Add(command);
        }

        public void AddCommand(long playerID, List<Command> commandList)
        {
            if (commandList == null)
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
            if (IsTurnDataReady(playerNum))
            {
                foreach (var item in mPlayerDataDic)
                {
                    for (int i = 0; i < item.Value.mCommandList.Count; i++)
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

    class CommandManager : Singleton<CommandManager>
    {
        private List<Command> mPendingCommandList = new List<Command>();
        private Dictionary<int, TurnData> mTurnDataDic = new Dictionary<int, TurnData>();
        private int mCommandTurn = 0;

        public void SendCommand(int curTurn)
        {
            if (mCommandTurn == curTurn)
            {
                return;
            }
            mCommandTurn = curTurn + 2;

            //封装消息发送给别人
            C2SBattleCommand packageData = new C2SBattleCommand();
            packageData.TurnId = mCommandTurn;
            packageData.PlayerId = BattleManager.Instance.mPlayerID;
            if (mPendingCommandList.Count == 0)
            {
                NullCommand commandData = new NullCommand();
                mPendingCommandList.Add(commandData);
            }

            for (int i = 0; i < mPendingCommandList.Count; i++)
            {
                BattleCommand commandData = new BattleCommand();
                commandData.Type = mPendingCommandList[i].mCommandType;
                commandData.Data = ByteString.CopyFrom(Command.Serialize(mPendingCommandList[i]));
                packageData.Commands.Add(commandData);
            }
            NetworkManager.Instance.SendData(ServiceNo.C2SbattleCommand, packageData);

            //加入本地对应的回合
            TurnData data;
            if (mTurnDataDic.TryGetValue(mCommandTurn, out data))
            {
                data.AddCommand(BattleManager.Instance.mPlayerID, mPendingCommandList);
            }

            mPendingCommandList.Clear();
        }

        public void AddCommand(Command command)
        {
            if (mPendingCommandList != null)
            {
                mPendingCommandList.Add(command);
            }
        }

        public void AddCommand(S2CBattleCommand msg)
        {
            TurnData turnData;
            if (!mTurnDataDic.TryGetValue(msg.TurnId, out turnData))
            {
                turnData = new TurnData();
                mTurnDataDic[msg.TurnId] = turnData;
            }

            List<Command> commandList = new List<Command>();
            for (int i = 0; i < msg.Commands.Count; i++)
            {
                Command command = null;
                switch (msg.Commands[i].Type)
                {
                    case CommandType.EMove:
                        {
                            command = Command.Deserialize<MoveCommand>(msg.Commands[i].Data.ToByteArray());
                        }
                        break;
                    case CommandType.ENone:
                        {
                            command = Command.Deserialize<NullCommand>(msg.Commands[i].Data.ToByteArray());
                        }
                        break;
                }

                commandList.Add(command);
            }

            turnData.AddCommand(msg.PlayerId, commandList);
        }

        public bool ProcessTurn(int playerNum, int curTurn)
        {
            TurnData data;
            if (mTurnDataDic.TryGetValue(curTurn, out data))
            {
                return data.ProcessTurnData(playerNum);
            }
            return false;
        }
    }
}
