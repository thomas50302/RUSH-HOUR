using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("玩家設定")]
    [Tooltip("1 代表玩家 1 (WASD 控制)，2 代表玩家 2 (方向鍵控制)")]
    [Range(1, 2)]
    public int playerID = 1;

    // 外部腳本讀取的輸入屬性
    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool CyclePressed { get; private set; }

    void Update()
    {
        Horizontal = 0f;
        Vertical = 0f;
        InteractPressed = false;
        CyclePressed = false;

        if (playerID == 1)
        {
            // 玩家 1：使用 W A S D 移動，F 鍵互動，E 鍵切換
            if (Input.GetKey(KeyCode.W)) Vertical = 1f;
            if (Input.GetKey(KeyCode.S)) Vertical = -1f;
            if (Input.GetKey(KeyCode.A)) Horizontal = -1f;
            if (Input.GetKey(KeyCode.D)) Horizontal = 1f;

            if (Input.GetKeyDown(KeyCode.F))
            {
                InteractPressed = true;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                CyclePressed = true;
            }
        }
        else if (playerID == 2)
        {
            // 玩家 2：使用 鍵盤方向鍵 移動，Space (空白鍵) 或 Enter 鍵互動，Slash (/) 或 RightShift 鍵切換
            if (Input.GetKey(KeyCode.UpArrow)) Vertical = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) Vertical = -1f;
            if (Input.GetKey(KeyCode.LeftArrow)) Horizontal = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) Horizontal = 1f;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                InteractPressed = true;
            }
            if (Input.GetKeyDown(KeyCode.Slash) || Input.GetKeyDown(KeyCode.RightShift))
            {
                CyclePressed = true;
            }
        }
    }
}
