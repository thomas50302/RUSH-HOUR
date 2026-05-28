using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("生成設定")]
    [Tooltip("客人的 Prefab")]
    public GameObject customerPrefab;
    [Tooltip("客人出生點")]
    public Transform spawnPoint;
    [Tooltip("客人離開出口")]
    public Transform exitPoint;
    [Tooltip("排隊起點")]
    public Transform queueStartPoint;
    [Tooltip("生成新客人的時間間隔 (秒)")]
    public float spawnInterval = 12f;

    [Header("排隊設定")]
    [Tooltip("排隊時客人間距")]
    public float queueSpacing = 1.0f;
    
    private List<CustomerAI> waitingQueue = new List<CustomerAI>();

    // 追蹤哪個玩家正牽著哪個客人
    private Dictionary<PlayerControll, CustomerAI> activeFollowers = new Dictionary<PlayerControll, CustomerAI>();

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

    private void Start()
    {
        // 遊戲一開始先生成一個客人
        SpawnCustomer();
        StartCoroutine(SpawnRoutine());
    }

    private void Update()
    {
        // 定期重新整理排隊隊伍的位置
        UpdateQueuePositions();
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            // 隊伍上限 5 人，避免客滿塞爆
            if (waitingQueue.Count < 5)
            {
                SpawnCustomer();
            }
        }
    }

    private void SpawnCustomer()
    {
        if (customerPrefab != null && spawnPoint != null)
        {
            GameObject newCustomer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
            CustomerAI ai = newCustomer.GetComponent<CustomerAI>();
            if (ai != null)
            {
                waitingQueue.Add(ai);
                ai.currentState = CustomerAI.CustomerState.WaitingInQueue;
            }
        }
    }

    private void UpdateQueuePositions()
    {
        if (queueStartPoint == null) return;

        for (int i = 0; i < waitingQueue.Count; i++)
        {
            // 如果客人此時沒有玩家在引導，則讓他走到對應的排隊位置
            if (waitingQueue[i] != null && GetPlayerGuidingThisCustomer(waitingQueue[i]) == null)
            {
                // 客人沿著 X 軸排成一列 (水平排隊)
                Vector3 targetPos = queueStartPoint.position + Vector3.right * (i * queueSpacing);
                
                // 平滑移到排隊點
                waitingQueue[i].transform.position = Vector3.MoveTowards(waitingQueue[i].transform.position, targetPos, 4f * Time.deltaTime);
            }
        }
    }

    #region 帶位與跟隨管理 (Seating & Follower Management)

    /// <summary>
    /// 開始讓客人跟隨玩家
    /// </summary>
    public void StartFollowing(PlayerControll player, CustomerAI customer)
    {
        if (activeFollowers.ContainsKey(player))
        {
            StopFollowing(player);
        }

        activeFollowers[player] = customer;
        
        // 從排隊佇列中移出
        if (waitingQueue.Contains(customer))
        {
            waitingQueue.Remove(customer);
        }
    }

    /// <summary>
    /// 停止客人的跟隨
    /// </summary>
    public void StopFollowing(PlayerControll player)
    {
        if (activeFollowers.ContainsKey(player))
        {
            activeFollowers.Remove(player);
        }
    }

    /// <summary>
    /// 獲取目前跟隨該玩家的客人
    /// </summary>
    public CustomerAI GetFollowingCustomer(PlayerControll player)
    {
        if (activeFollowers.ContainsKey(player))
        {
            return activeFollowers[player];
        }
        return null;
    }

    /// <summary>
    /// 檢查某個特定客人目前是否正在被玩家帶路
    /// </summary>
    private PlayerControll GetPlayerGuidingThisCustomer(CustomerAI customer)
    {
        foreach (KeyValuePair<PlayerControll, CustomerAI> pair in activeFollowers)
        {
            if (pair.Value == customer)
            {
                return pair.Key;
            }
        }
        return null;
    }

    #endregion
}
