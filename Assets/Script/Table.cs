using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour, IInteractable
{
    public enum TableState
    {
        Empty,  // 空桌
        Seated, // 客人已入座
        Dirty   // 髒桌子 (有髒盤子)
    }

    [Header("桌子狀態")]
    public TableState currentState = TableState.Empty;
    public CustomerAI seatedCustomer = null;

    [Header("位置錨點")]
    [Tooltip("客人坐的位置")]
    public Transform seatPoint;
    [Tooltip("盤子放的位置")]
    public Transform platePoint;

    private GameObject dirtyPlateObj = null;

    public void Interact(PlayerControll player)
    {
        // 情況 1：桌子是空的
        if (currentState == TableState.Empty)
        {
            // 檢查玩家是否正帶著客人
            if (CustomerManager.Instance != null)
            {
                CustomerAI followingCustomer = CustomerManager.Instance.GetFollowingCustomer(player);
                if (followingCustomer != null)
                {
                    // 讓客人入座
                    SeatCustomer(followingCustomer);
                    CustomerManager.Instance.StopFollowing(player);
                    Debug.Log($"[{player.gameObject.name}] 成功帶位客人到桌子 [{gameObject.name}]。");
                }
                else
                {
                    Debug.Log("這是一張空桌子。");
                }
            }
        }
        // 情況 2：客人坐著，等待送餐
        else if (currentState == TableState.Seated)
        {
            if (player.IsCarrying())
            {
                GameObject carried = player.GetCarriedObject();
                Plate plate = carried.GetComponent<Plate>();
                
                // 檢查手上是不是盛載食物的乾淨盤子
                if (plate != null && !plate.isDirty)
                {
                    if (seatedCustomer != null && seatedCustomer.currentState == CustomerAI.CustomerState.WaitingForFood)
                    {
                        // 驗證食物是否與客人點的一致
                        if (seatedCustomer.orderRecipeName == plate.recipeName)
                        {
                            // 移除玩家手上的盤子並銷毀
                            player.Drop();
                            Destroy(carried);
                            
                            // 開始用餐
                            seatedCustomer.ServeFood();
                            Debug.Log($"[{player.gameObject.name}] 成功送上 {plate.recipeName}！客人開始享用。");
                        }
                        else
                        {
                            Debug.Log($"送錯食物了！客人點的是 {seatedCustomer.orderRecipeName}，你手上拿的是 {plate.recipeName}");
                        }
                    }
                }
            }
            else
            {
                // 手上沒東西，如果客人還沒點餐，可以透過互動來幫他點餐
                if (seatedCustomer != null && seatedCustomer.currentState == CustomerAI.CustomerState.Seated_WaitOrder)
                {
                    seatedCustomer.Interact(player); // 觸發點餐
                }
            }
        }
        // 情況 3：髒桌子，需要清理
        else if (currentState == TableState.Dirty)
        {
            // 如果玩家雙手空空，可以把髒盤子收走
            if (!player.IsCarrying() && dirtyPlateObj != null)
            {
                player.Carry(dirtyPlateObj);
                dirtyPlateObj = null;
                currentState = TableState.Empty;
                Debug.Log($"[{player.gameObject.name}] 收走了髒盤子，桌子已清理乾淨。");
            }
        }
    }

    /// <summary>
    /// 客人入座設定
    /// </summary>
    public void SeatCustomer(CustomerAI customer)
    {
        seatedCustomer = customer;
        currentState = TableState.Seated;
        customer.OnSeated(this);
    }

    /// <summary>
    /// 當客人用餐完畢，產生髒盤子
    /// </summary>
    public void OnCustomerFinished(GameObject dirtyPlatePrefab)
    {
        seatedCustomer = null;
        currentState = TableState.Dirty;

        if (dirtyPlatePrefab != null)
        {
            Vector3 spawnPos = platePoint != null ? platePoint.position : transform.position;
            dirtyPlateObj = Instantiate(dirtyPlatePrefab, spawnPos, Quaternion.identity);
            
            Plate plateScript = dirtyPlateObj.GetComponent<Plate>();
            if (plateScript != null)
            {
                plateScript.isDirty = true;
            }
        }
    }
}
