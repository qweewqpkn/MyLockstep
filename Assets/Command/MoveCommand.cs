using Battle;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LockStep
{
    [Serializable]
    public class MoveCommand : Command
    {
        private long mTargetID;
        private float mX;
        private float mY;
        private float mZ;

        public MoveCommand(long targetID, float x, float y, float z) : base(Network.CommandType.EMove)
        {
            mTargetID = targetID;
            mX = x;
            mY = y;
            mZ = z;
        }

        public override void Process()
        {
            BattlePlayer player = BattleManager.Instance.GetPlayer(mTargetID);
            if(player != null)
            {
                player.TargetPos = new Vector3(mX, mY, mZ);
            }
        }

    }
}
