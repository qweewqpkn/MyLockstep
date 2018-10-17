using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LockStep
{
    public class LockStepManager : Singleton<LockStepManager>
    {
        private static int mGameFrameTime = 50; 
        private static int mTurnTime = 200;
        private int mGameFrameCount = 0;
        private int mTotalTime = 0;
        private int mCurTurn = 0;
        private int mCommandTurn = 0;
        private int mPlayerNum = 0;
        private long mPlayerID = 0;
        private List<ICommand> mPendingCommandList = new List<ICommand>();
        private Dictionary<int, TurnData> mTurnDataDic = new Dictionary<int, TurnData>();

        public LockStepManager()
        {

        }

        void Update()
        {
            mTotalTime = mTotalTime + (int)(Time.deltaTime * 1000);
            if (mTotalTime > mGameFrameTime)
            {
                if(mGameFrameCount == 0)
                {
                    SendCommand(mCurTurn);
                    if(IsTurnDataReady(mCurTurn))
                    {
                        ProcessTurn();
                    }
                    else
                    {
                        return;
                    }
                }

                mGameFrameCount = mGameFrameCount % (mTurnTime / mGameFrameTime);
                mTotalTime = mTotalTime - mGameFrameTime;
            }
        }

        bool IsTurnDataReady(int turn)
        {
            TurnData data;
            if(mTurnDataDic.TryGetValue(turn, out data))
            {
                if(data.IsTurnDataReady(mPlayerNum))
                {
                    return true;
                }
            }

            return false;
        }

        void SendCommand(int turn)
        {
            //封装消息发送给别人


            //加入本地对应的回合
            TurnData data;
            if (mTurnDataDic.TryGetValue(turn, out data))
            {
                data.AddCommand(mPlayerID, mPendingCommandList);
            }
        }

        void ProcessTurn()
        {

        }

        void NextTurn()
        {

        }
    }

}