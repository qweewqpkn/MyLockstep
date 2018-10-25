using Google.Protobuf;
using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
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
                    CommandManager.Instance.SendCommand(mCurTurn);
                    if (mCurTurn >= 0)
                    {
                        if (CommandManager.Instance.ProcessTurn(mPlayerNum, mCurTurn))
                        {
                            mCurTurn++;
                            //Debug.Log("mCurTurn : " + mCurTurn);
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

            for (int i = 0; i < mLockStepList.Count; i++)
            {
                mLockStepList[i].Update();
            }
        }
    }

}