using LockStep;
using Network;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    class BattleManager : Singleton<BattleManager>, ILockStep
    {
        public long mPlayerID;
        private GameObject mObj;
        private Dictionary<long, BattlePlayer> mPlayerDic = new Dictionary<long, BattlePlayer>();

        public BattleManager()
        {
            SocketClient.Instance.RegisterMessage<S2CEnterRoom>(ServiceNo.S2CenterRoom, S2CEnterRoom);
            SocketClient.Instance.RegisterMessage<S2CStartBattle>(ServiceNo.S2CstartBattle, S2CStartBattle);
            SocketClient.Instance.RegisterMessage<S2CBattleCommand>(ServiceNo.S2CbattleCommand, S2CBattleCommand);
        }

        public void Init(GameObject obj)
        {
            mObj = obj;
        }

        public void CreatePlayer(long id)
        {
            BattlePlayer player;
            if (!mPlayerDic.TryGetValue(id, out player))
            {
                GameObject newObj = GameObject.Instantiate(mObj);
                player = newObj.AddComponent<BattlePlayer>();
                mPlayerDic[id] = player;
            }
            player.Init();
        }

        public BattlePlayer GetPlayer(long id)
        {
            BattlePlayer player;
            if(mPlayerDic.TryGetValue(id, out player))
            {
                return player;
            }

            return null;
        }

        public void UpdateFixed(int deltaTime)
        {
            foreach(var player in mPlayerDic)
            {
                player.Value.UpdateFixed(deltaTime);
            }
        }

        public void C2SEnterRoom()
        {
            C2SEnterRoom data = new C2SEnterRoom();
            SocketClient.Instance.SendData(ServiceNo.C2SenterRoom, data);
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
            SocketClient.Instance.SendData(ServiceNo.C2SreadyBattle, data);
        }

        public void S2CStartBattle(S2CStartBattle msg)
        {
            for(int i = 0; i < msg.PlayerIdList.Count; i++)
            {
                CreatePlayer(msg.PlayerIdList[i]);
            }

            LockStepManager.Instance.mIsStartLockStep = true;
        }

        public void S2CBattleCommand(S2CBattleCommand msg)
        {
            LockStepManager.Instance.AddCommand(msg);
            //Debug.Log("S2CBattleCommand : " + msg.PlayerId);
        }
    }
}
