using System.Collections;
using UnityEngine;

public class CustomerAI : MonoBehaviour, IInteractable
{
    public enum CustomerState
    {
        WaitingInQueue, // 在等候區排隊
        WalkingToTable, // 正在走向座位
        Seated_WaitOrder,  // 坐下等待點餐
        WaitingForFood,    // 等待送餐
        Eating,         // 正在用餐
        Leaving         // 正在離開
    }

    [Header("狀態設定")]
    public CustomerState currentState = CustomerState.WaitingInQueue;
    public string orderRecipeName = "";
    
    [Tooltip("等待餐點的耐心時間")]
    public float patienceTime = 45f;
    public float currentPatience;

    [Header("移動設定")]
    public float moveSpeed = 3f;

    [Header("髒盤子預製物")]
    public GameObject dirtyPlatePrefab;

    private Table assignedTable;
    private Vector3 targetDestination;
    private PlayerControll followingPlayer = null;
    private bool isMoving = false;

    void Start()
    {
        currentPatience = patienceTime;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetDestination, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetDestination) < 0.1f)
            {
                isMoving = false;
                OnReachDestination();
            }
        }

        // 客人跟隨玩家移動
        if (currentState == CustomerState.WaitingInQueue && followingPlayer != null)
        {
            // 跟隨在玩家後方
            Vector3 followPos = followingPlayer.transform.position - (followingPlayer.transform.up * 0.8f);
            transform.position = Vector3.MoveTowards(transform.position, followPos, moveSpeed * Time.deltaTime);
        }

        // 計時耐心值
        if (currentState == CustomerState.WaitingForFood)
        {
            currentPatience -= Time.deltaTime;
            if (currentPatience <= 0)
            {
                LeaveAngry();
            }
        }
    }

    public void Interact(PlayerControll player)
    {
        // 1. 如果客人在排隊且沒人在帶位
        if (currentState == CustomerState.WaitingInQueue)
        {
            if (followingPlayer == null)
            {
                followingPlayer = player;
                CustomerManager.Instance.StartFollowing(player, this);
                Debug.Log("客人開始跟隨服務生，請帶位到空桌子。");
            }
            else if (followingPlayer == player)
            {
                followingPlayer = null;
                CustomerManager.Instance.StopFollowing(player);
                Debug.Log("客人停止跟隨。");
            }
        }
        // 2. 如果坐下了，等待點餐
        else if (currentState == CustomerState.Seated_WaitOrder)
        {
            TakeOrder();
        }
    }

    public void OnSeated(Table table)
    {
        assignedTable = table;
        followingPlayer = null;
        currentState = CustomerState.WalkingToTable;
        targetDestination = table.seatPoint != null ? table.seatPoint.position : table.transform.position;
        isMoving = true;
    }

    private void OnReachDestination()
    {
        if (currentState == CustomerState.WalkingToTable)
        {
            currentState = CustomerState.Seated_WaitOrder;
            Debug.Log("客人已入座，點擊進行點餐。");
        }
        else if (currentState == CustomerState.Leaving)
        {
            Destroy(gameObject);
        }
    }

    private void TakeOrder()
    {
        // 隨機選一道印尼名菜點餐
        string[] recipes = { "Nasi Goreng", "Rendang", "Ayam Bakar Madu", "Tempe Goreng" };
        orderRecipeName = recipes[Random.Range(0, recipes.Length)];
        currentState = CustomerState.WaitingForFood;
        currentPatience = patienceTime;

        // 登錄訂單到訂單管理器
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.AddOrder(this, orderRecipeName);
        }

        Debug.Log($"點餐完成！客人點了 {orderRecipeName}，已將訂單送往廚房。");
    }

    public void ServeFood()
    {
        if (currentState == CustomerState.WaitingForFood)
        {
            StartCoroutine(EatRoutine());
        }
    }

    private IEnumerator EatRoutine()
    {
        currentState = CustomerState.Eating;
        Debug.Log("客人開始吃東西...");

        // 通知訂單系統完成此訂單
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.CompleteOrder(this);
        }

        yield return new WaitForSeconds(5f); // 吃 5 秒

        // 吃完了，給予金幣與知名度
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(100);
            GameManager.Instance.AddPopularity(10);
        }

        // 通知桌子客人用完餐，轉為髒桌子並生成髒盤子
        if (assignedTable != null)
        {
            assignedTable.OnCustomerFinished(dirtyPlatePrefab);
        }

        LeaveNormal();
    }

    private void LeaveNormal()
    {
        currentState = CustomerState.Leaving;
        targetDestination = CustomerManager.Instance.exitPoint != null ? CustomerManager.Instance.exitPoint.position : transform.position + Vector3.down * 10f;
        isMoving = true;
        Debug.Log("客人用餐完畢，開心地離開了。");
    }

    private void LeaveAngry()
    {
        currentState = CustomerState.Leaving;
        if (assignedTable != null)
        {
            assignedTable.seatedCustomer = null;
            assignedTable.currentState = Table.TableState.Empty;
        }

        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.CancelOrder(this);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReducePopularity(15); // 耐心耗盡生氣扣除知名度
        }

        targetDestination = CustomerManager.Instance.exitPoint != null ? CustomerManager.Instance.exitPoint.position : transform.position + Vector3.down * 10f;
        isMoving = true;
        Debug.Log("等候太久，客人氣鼓鼓地離開了！");
    }
}
