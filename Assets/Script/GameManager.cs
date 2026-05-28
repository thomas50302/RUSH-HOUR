using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("遊戲統計數值")]
    [Tooltip("目前金幣數")]
    public int currentGold = 0;
    
    [Tooltip("目前餐廳知名度 (0 到 100)")]
    public int currentPopularity = 50;

    [Tooltip("關卡限時 (秒)")]
    public float levelTime = 120f;

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        // 倒數計時
        levelTime -= Time.deltaTime;
        if (levelTime <= 0f)
        {
            EndLevel();
        }
    }

    /// <summary>
    /// 增加金幣
    /// </summary>
    public void AddGold(int amount)
    {
        if (isGameOver) return;
        currentGold += amount;
        Debug.Log($"[遊戲控制] 獲得金幣 +{amount}！目前總金幣: {currentGold}");
    }

    /// <summary>
    /// 提升餐廳知名度
    /// </summary>
    public void AddPopularity(int amount)
    {
        if (isGameOver) return;
        currentPopularity = Mathf.Clamp(currentPopularity + amount, 0, 100);
        Debug.Log($"[遊戲控制] 知名度提升 +{amount}！目前知名度: {currentPopularity}/100");
    }

    /// <summary>
    /// 降低餐廳知名度 (如果知名度歸 0 則失敗)
    /// </summary>
    public void ReducePopularity(int amount)
    {
        if (isGameOver) return;
        currentPopularity = Mathf.Clamp(currentPopularity - amount, 0, 100);
        Debug.LogWarning($"[遊戲控制] 知名度下降 -{amount}！目前知名度: {currentPopularity}/100");

        if (currentPopularity <= 0)
        {
            FailLevel();
        }
    }

    private void EndLevel()
    {
        isGameOver = true;
        Debug.Log($"<color=green>[遊戲結束] 時間到！關卡勝利！你賺取了 {currentGold} 金幣，知名度維持在 {currentPopularity}。</color>");
    }

    private void FailLevel()
    {
        isGameOver = true;
        Debug.LogError("<color=red>[遊戲結束] 知名度降為 0，餐廳倒閉！關卡失敗！</color>");
    }
}
