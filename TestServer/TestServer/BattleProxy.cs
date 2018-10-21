using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    class BattleProxy : Singleton<BattleProxy>, IProxy
    {
        private long id;
        private List<long> mReadyBattlePlayerList = new List<long>();
        private List<long> mPlayerIDList = new List<long>();

        public BattleProxy()
        {
            SocketServer.Instance.RegisterMessage<C2SBattleCommand>(ServiceNo.C2SbattleCommand, OnC2SbattleCommand);
            SocketServer.Instance.RegisterMessage<C2SBattleCommand>(ServiceNo.C2SreadyBattle, OnC2SreadyBattle);
            SocketServer.Instance.RegisterMessage<C2SBattleCommand>(ServiceNo.C2SenterRoom, OnC2SenterRoom);
        }

        private void OnC2SbattleCommand(Socket socket, C2SBattleCommand msg)
        {
            S2CBattleCommand data = new S2CBattleCommand();
            data.TurnId = msg.TurnId;
            data.PlayerId = msg.PlayerId;
            data.Commands.AddRange(msg.Commands);
            Console.WriteLine(msg.PlayerId);
            SocketServer.Instance.BroadcastMessage(ServiceNo.S2CbattleCommand, data);
        }

        private void OnC2SreadyBattle(Socket socket, C2SBattleCommand msg)
        {
            mReadyBattlePlayerList.Add(msg.PlayerId);
            if(mReadyBattlePlayerList.Count == 2)
            {
                S2CStartBattle data = new S2CStartBattle();
                data.PlayerIdList.AddRange(mPlayerIDList.ToArray());
                SocketServer.Instance.BroadcastMessage(ServiceNo.S2CstartBattle, data);
            }
        }

        private void OnC2SenterRoom(Socket socket, C2SBattleCommand msg)
        {
            id++;
            mPlayerIDList.Add(id);

            S2CEnterRoom data = new S2CEnterRoom();
            data.PlayerId = id;
            SocketServer.Instance.SendMessage(ServiceNo.S2CenterRoom, data, socket);
        }
    }
}
