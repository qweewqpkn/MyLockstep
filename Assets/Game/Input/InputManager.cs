using UnityEngine;

namespace Game
{
    class InputManager : Singleton<InputManager>
    {
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                BattleManager.Instance.C2SEnterRoom();
            }

            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 targetPos = hit.point;
                    MoveCommand commandData = new MoveCommand(BattleManager.Instance.mPlayerID, targetPos.x, targetPos.y, targetPos.z);
                    CommandManager.Instance.AddCommand(commandData);
                }
            }
        }
    }
}
