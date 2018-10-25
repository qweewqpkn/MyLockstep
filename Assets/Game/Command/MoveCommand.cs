using System;
using UnityEngine;

namespace Game
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
            BattleEntity player = BattleManager.Instance.GetEntity(mTargetID);
            if(player != null)
            {
                MoveComponent moveCmp = player.GetComponent<MoveComponent>();
                moveCmp.TargetPos = new Vector3(mX, mY, mZ);
            }
        }

    }
}
