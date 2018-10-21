using Battle;
using Google.Protobuf;
using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LockStep
{
    public class LockStepManager : Singleton<LockStepManager>
    {
        public static int mGameFrameTime = 50; 
        private static int mTurnTime = 200;
        private int mGameFrameCount = 0;
        private int mTotalTime = 0;
        private int mCurTurn = -2;
        private int mCommandTurn = 0;
        private int mPlayerNum = 2;
        private List<Command> mPendingCommandList = new List<Command>();
        private Dictionary<int, TurnData> mTurnDataDic = new Dictionary<int, TurnData>();
        private List<ILockStep> mLockStepList = new List<ILockStep>();
        public bool mIsStartLockStep = false;

        public LockStepManager()
        {
            mLockStepList.Add(BattleManager.Instance);
        }

        public void Update()
        {
            if(!mIsStartLockStep)
            {
                return;
            }

            mTotalTime = mTotalTime + (int)(Time.deltaTime * 1000);
            if (mTotalTime > mGameFrameTime)
            {
                if(mGameFrameCount == 0)
                {
                    SendCommand();
                    if (mCurTurn >= 0)
                    {
                        if (ProcessTurn(mPlayerNum))
                        {
                            mCurTurn++;
                            Debug.Log("mCurTurn : " + mCurTurn);
                        }
                    }
                    else
                    {
                        mCurTurn++;
                    }
                }

                for(int i = 0; i < mLockStepList.Count; i++)
                {
                    mLockStepList[i].UpdateFixed(mGameFrameTime);
                }

                mGameFrameCount = mGameFrameCount % (mTurnTime / mGameFrameTime);
                mTotalTime = mTotalTime - mGameFrameTime;
            }
        }

        void SendCommand()
        {
            if(mCommandTurn == mCurTurn)
            {
                return;
            }
            mCommandTurn = mCurTurn + 2;

            //封装消息发送给别人
            C2SBattleCommand packageData = new C2SBattleCommand();
            packageData.TurnId = mCommandTurn;
            packageData.PlayerId = BattleManager.Instance.mPlayerID;
            if(mPendingCommandList.Count == 0)
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
            SocketClient.Instance.SendData(ServiceNo.C2SbattleCommand, packageData);

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
            if(mPendingCommandList != null)
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

        bool ProcessTurn(int playerNum)
        {
            TurnData data;
            if(mTurnDataDic.TryGetValue(mCurTurn, out data))
            {
                return data.ProcessTurnData(playerNum);
            }
            return false;
        }
    }

}