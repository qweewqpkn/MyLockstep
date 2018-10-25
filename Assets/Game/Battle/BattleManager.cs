using Network;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    class BattleManager : Singleton<BattleManager>, ILockStep
    {
        public long mPlayerID;
        private GameObject mObj;
        private Dictionary<long, BattleEntity> mEntityDic = new Dictionary<long, BattleEntity>();

        public BattleManager()
        {
            NetworkManager.Instance.RegisterMessage<S2CEnterRoom>(ServiceNo.S2CenterRoom, S2CEnterRoom);
            NetworkManager.Instance.RegisterMessage<S2CStartBattle>(ServiceNo.S2CstartBattle, S2CStartBattle);
            NetworkManager.Instance.RegisterMessage<S2CBattleCommand>(ServiceNo.S2CbattleCommand, S2CBattleCommand);
        }

        public void Init(GameObject obj)
        {
            mObj = obj;
        }

        public BattleEntity CreateEntity(long id)
        {
            BattleEntity player;
            if (!mEntityDic.TryGetValue(id, out player))
            {
                player  = new BattleEntity(mObj);
                mEntityDic[id] = player;
            }

            return player;
        }

        public BattleEntity GetEntity(long id)
        {
            BattleEntity entity;
            if(mEntityDic.TryGetValue(id, out entity))
            {
                return entity;
            }

            return null;
        }

        public void UpdateFixed(int deltaTime)
        {
            foreach(var player in mEntityDic)
            {
                player.Value.UpdateFixed(deltaTime);
            }
        }

        public void Update()
        {
            foreach (var player in mEntityDic)
            {
                player.Value.Update();
            }
        }

        public void C2SEnterRoom()
        {
            C2SEnterRoom data = new C2SEnterRoom();
            NetworkManager.Instance.SendData(ServiceNo.C2SenterRoom, data);
        }

        public void S2CEnterRoom(S2CEnterRoom msg)
        {
            mPlayerID = msg.PlayerId;
            C2SReadyBattle();
        }

        public void C2SReadyBattle()
        {
            C2SReadyBattle data = new C2SReadyBattle();
            data.PlayerId = mPlayerID;
            NetworkManager.Instance.SendData(ServiceNo.C2SreadyBattle, data);
        }

        public void S2CStartBattle(S2CStartBattle msg)
        {
            for(int i = 0; i < msg.PlayerIdList.Count; i++)
            {
                BattleEntity entity = CreateEntity(msg.PlayerIdList[i]);
                entity.AddComponent<MoveComponent>();
            }

            LockStepManager.Instance.mIsStartLockStep = true;
        }

        public void S2CBattleCommand(S2CBattleCommand msg)
        {
            CommandManager.Instance.AddCommand(msg);
            Debug.Log(msg.PlayerId);
        }
    }
}
