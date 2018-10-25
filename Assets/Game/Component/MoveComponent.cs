using UnityEngine;

namespace Game
{
    class MoveComponent : Component
    {
        public float mSpeed = 5.0f;
        private Vector3 mNextPos = Vector3.zero;
        private float mTotalTime;
        private Vector3 mDirection;
        private float mTotalDeltaTime1;
        private float mTotalDeltaTime2;

        private Vector3 mTargetPos;
        public Vector3 TargetPos
        {
            get
            {
                return mTargetPos;
            }

            set
            {
                mTargetPos = value;
                mDirection = (transform.position - mTargetPos).normalized;
                mTotalTime = (transform.position - mTargetPos).magnitude / mSpeed;
                mTotalDeltaTime1 = 0.0f;
            }
        }

        public MoveComponent()
        {
        }

        public override void UpdateFixed(int deltaTime)
        {
            if (transform.position != mTargetPos)
            {
                mTotalDeltaTime1 += deltaTime / 1000.0f;
                mNextPos = Vector3.Lerp(transform.position, mTargetPos, mTotalDeltaTime1 / mTotalTime);
            }
            else
            {
                mTotalDeltaTime1 = 0.0f;
            }
        }

        public override void Update()
        {
            if (transform.position != mNextPos)
            {
                mTotalDeltaTime2 += Time.deltaTime;
                Vector3 newPos = Vector3.Lerp(transform.position, mNextPos, mTotalDeltaTime2 / (LockStepManager.mGameFrameTime / 1000.0f));
                transform.position = newPos;
            }
            else
            {
                mTotalDeltaTime2 = 0.0f;
            }
        }
    }
}
