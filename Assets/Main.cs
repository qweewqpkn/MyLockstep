using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    public Text text;
    public Button button;

	// Use this for initialization
	void Start () {
        SocketClient.Instance.Init();
        SocketClient.Instance.RegisterMessage<S2CBattleCommand>(ServiceNo.S2CbattleCommand, (o) =>
        {
            text.text = o.Result;
        });

        button.onClick.AddListener(() =>
        {
            C2SBattleCommand data = new C2SBattleCommand();
            data.TurnId = 100;
            for(int i = 0; i < 5; i++)
            {
                BattleCommand command = new BattleCommand();
                command.Type = CommandType.EPos;
                data.Commands.Add(command);
            }
            SocketClient.Instance.SendData(ServiceNo.C2SbattleCommand, data);
        });
	}

    private void Update()
    {
        SocketClient.Instance.Update();
    }
}
