using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [System.Serializable]
    public class Order
    {
        public CustomerAI customer;
        public string recipeName;

        public Order(CustomerAI customer, string recipeName)
        {
            this.customer = customer;
            this.recipeName = recipeName;
        }
    }

    private List<Order> activeOrders = new List<Order>();

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

    /// <summary>
    /// 新增一筆新訂單 (當客人點餐時呼叫)
    /// </summary>
    public void AddOrder(CustomerAI customer, string recipeName)
    {
        Order newOrder = new Order(customer, recipeName);
        activeOrders.Add(newOrder);
        Debug.Log($"[訂單系統] 新增訂單！顧客 [{customer.gameObject.name}] 點了 [{recipeName}]。目前未完成訂單數: {activeOrders.Count}");
    }

    /// <summary>
    /// 完成訂單 (當成功送餐時呼叫)
    /// </summary>
    public void CompleteOrder(CustomerAI customer)
    {
        Order orderToRemove = activeOrders.Find(o => o.customer == customer);
        if (orderToRemove != null)
        {
            activeOrders.Remove(orderToRemove);
            Debug.Log($"[訂單系統] 訂單完成！已移除顧客 [{customer.gameObject.name}] 的訂單。目前未完成訂單數: {activeOrders.Count}");
        }
    }

    /// <summary>
    /// 取消訂單 (當顧客生氣離開時呼叫)
    /// </summary>
    public void CancelOrder(CustomerAI customer)
    {
        Order orderToRemove = activeOrders.Find(o => o.customer == customer);
        if (orderToRemove != null)
        {
            activeOrders.Remove(orderToRemove);
            Debug.Log($"[訂單系統] 訂單取消 (顧客生氣離開)。已移除顧客 [{customer.gameObject.name}] 的訂單。目前未完成訂單數: {activeOrders.Count}");
        }
    }

    /// <summary>
    /// 獲取目前所有的未完成訂單
    /// </summary>
    public List<Order> GetActiveOrders()
    {
        return activeOrders;
    }
}
