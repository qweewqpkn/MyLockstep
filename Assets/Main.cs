using DG.Tweening;
using Game;
using UnityEngine;

public class Main : MonoBehaviour {

    public GameObject mObj;

    // Use this for initialization
    void Start () {
        DOTween.Init();
        NetworkManager.Instance.Init();
        BattleManager.Instance.Init(mObj);
    }

    private void Update()
    {
        LockStepManager.Instance.Update();
        InputManager.Instance.Update();
        NetworkManager.Instance.Update();
    }
}
