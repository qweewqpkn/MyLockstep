using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LockStep
{
    public class LockStepManager : MonoBehaviour
    {

        private static int mGameFrameTime = 50;
        private int mTotalTime = 0;
        private int mCurTurnID = 0;


        void Start()
        {

        }

        void Update()
        {
            mTotalTime = mTotalTime + (int)(Time.deltaTime * 1000);
            if (mTotalTime > mGameFrameTime)
            {

                mTotalTime = mTotalTime - mGameFrameTime;
            }
        }

        void SendCommand()
        {

        }

        void ProcessTurn()
        {

        }

        void NextTurn()
        {

        }


    }

}