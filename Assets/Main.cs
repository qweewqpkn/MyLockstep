using Battle;
using DG.Tweening;
using LockStep;
using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    public GameObject mObj;

    // Use this for initialization
    void Start () {
        DOTween.Init();
        SocketClient.Instance.Init();
        BattleManager.Instance.Init(mObj);
    }

    private void Update()
    {
        LockStepManager.Instance.Update();

        if (Input.GetKeyDown(KeyCode.S))
        {
            BattleManager.Instance.C2SEnterRoom();
        }

        if(Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                Vector3 targetPos = hit.point;
                MoveCommand commandData = new MoveCommand(BattleManager.Instance.mPlayerID, targetPos.x, targetPos.y, targetPos.z);
                LockStepManager.Instance.AddCommand(commandData);
            }
        }

        SocketClient.Instance.Update();
    }
}
