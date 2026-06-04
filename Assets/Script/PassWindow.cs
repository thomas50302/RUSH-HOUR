using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassWindow : MonoBehaviour, IInteractable
{
    [Header("傳菜口對照設定")]
    [Tooltip("對應的另一側傳菜口 (例如：內場傳菜點對應外場，外場對應內場)")]
    public PassWindow linkedPassWindow;

    [Header("擺放錨點")]
    [Tooltip("物品擺放的位置 (建議設置一個空子物件拖入此處)")]
    public Transform holdingPoint;

    [Header("目前擺放物件 (唯讀)")]
    [SerializeField]
    private GameObject placedObject = null;

    private void Start()
    {
        // 為了防止漏設 Inspector，自動雙向尋找並連結傳菜口
        if (linkedPassWindow == null)
        {
            PassWindow[] windows = FindObjectsOfType<PassWindow>();
            foreach (var win in windows)
            {
                if (win != this)
                {
                    linkedPassWindow = win;
                    if (win.linkedPassWindow == null)
                    {
                        win.linkedPassWindow = this;
                    }
                    Debug.Log($"【{gameObject.name}】自動與【{win.gameObject.name}】傳菜口完成了雙向綁定！");
                    break;
                }
            }
        }
    }

    private PassWindow GetLinkedWindow()
    {
        // 支援動態與雙向綁定，防範場景非同步加載時 Start 漏綁問題
        if (linkedPassWindow == null)
        {
            PassWindow[] windows = FindObjectsOfType<PassWindow>();
            foreach (var win in windows)
            {
                if (win != this)
                {
                    linkedPassWindow = win;
                    if (win.linkedPassWindow == null)
                    {
                        win.linkedPassWindow = this;
                    }
                    Debug.Log($"【{gameObject.name}】動態與【{win.gameObject.name}】傳菜口完成了雙向綁定！");
                    break;
                }
            }
        }
        return linkedPassWindow;
    }

    public void Interact(PlayerControll player)
    {
        PassWindow linked = GetLinkedWindow();

        // 情況 A：傳菜口目前是空的，玩家嘗試放食物/物件上去
        if (placedObject == null)
        {
            if (player.IsCarrying())
            {
                GameObject carried = player.GetCarriedObject();
                Plate carriedPlate = carried.GetComponent<Plate>() ?? carried.GetComponentInChildren<Plate>();

                player.Drop();

                // 如果放的是盤子，直接做為傳菜口的盤子載體並同步
                if (carriedPlate != null)
                {
                    PlaceObject(carried);
                    if (linked != null)
                    {
                        linked.ReceiveObject(carried);
                    }
                    Debug.Log($"[{player.gameObject.name}] 將盤子 [{carried.name}] 放上了傳菜口。");
                }
                else
                {
                    // 如果放的是普通食物，直接放到傳菜口並同步
                    PlaceObject(carried);
                    if (linked != null)
                    {
                        linked.ReceiveObject(carried);
                    }
                    Debug.Log($"[{player.gameObject.name}] 將食物 [{carried.name}] 放上了傳菜口。");
                }
            }
        }
        // 情況 B：傳菜口上有東西 (可能是食物，也可能是空盤子)
        else
        {
            Plate plate = placedObject.GetComponent<Plate>() ?? placedObject.GetComponentInChildren<Plate>();

            // B1. 如果玩家手上有拿東西，且傳菜口上是一個空盤子 -> 將食物裝入盤子並傳送！
            if (player.IsCarrying() && plate != null && plate.addedIngredients.Count == 0 && !plate.isDirty)
            {
                GameObject carried = player.GetCarriedObject();
                Plate carriedPlate = carried.GetComponent<Plate>() ?? carried.GetComponentInChildren<Plate>();

                if (carriedPlate != null)
                {
                    // 玩家拿著的是盤子 -> 取出食物裝入傳菜點的盤子，銷毀玩家手上的空盤
                    GameObject food = carriedPlate.TakeFood();
                    if (food != null)
                    {
                        plate.AddAnyObject(food);
                        player.Drop();
                        Destroy(carried);
                        Debug.Log($"[{player.gameObject.name}] 將手上盤子裡的食物 [{food.name}] 轉移到傳菜口的盤子中。");
                    }
                }
                else
                {
                    // 玩家拿著的是普通食物 -> 直接放進傳菜口的盤子
                    player.Drop();
                    plate.AddAnyObject(carried);
                    Debug.Log($"[{player.gameObject.name}] 將手上的食物 [{carried.name}] 放進了傳菜口的盤子。");
                }
                
                // 同步機制：如果對應端（外場）也有靜態盤子，僅把食物實體移至對方的盤子裡，兩個盤子本身不動！
                if (linked != null && linked.placedObject != null)
                {
                    Plate linkedPlate = linked.placedObject.GetComponent<Plate>() ?? linked.placedObject.GetComponentInChildren<Plate>();
                    if (linkedPlate != null)
                    {
                        GameObject food = plate.TakeFood();
                        if (food != null)
                        {
                            linkedPlate.AddAnyObject(food);
                            Debug.Log($"傳菜口已同步將食物 [{food.name}] 從內場盤子 [{plate.name}] 移至外場盤子 [{linkedPlate.name}] 中，盤子本體不移動。");
                        }
                    }
                }
                // 相容備用：若對應端沒有盤子，則將整盤食物傳送過去
                else if (linked != null)
                {
                    GameObject filledPlate = placedObject;
                    ClearObject();
                    linked.ReceiveObject(filledPlate);
                }
            }
            // B2. 玩家手是空的，嘗試從傳菜口拿取食物
            else if (!player.IsCarrying())
            {
                if (plate != null)
                {
                    // 傳菜口上是盤子 -> 拿走盤子裡的食物，盤子保留在傳菜口！
                    if (plate.addedIngredients.Count > 0)
                    {
                        GameObject food = plate.TakeFood();
                        if (food != null)
                        {
                            player.Carry(food);
                            Debug.Log($"[{player.gameObject.name}] 從傳菜口盤子拿走了食物 [{food.name}]，盤子保留在傳菜口。");

                            // 只有在對應端沒有盤子（亦即盤子是傳送過去的）時，才需要把空盤子自動送回內場
                            if (linked != null && linked.placedObject == null)
                            {
                                GameObject emptyPlate = placedObject;
                                ClearObject(); // 清除本側的盤子參照
                                linked.ReceiveObject(emptyPlate); // 內場接收空盤子
                                Debug.Log($"傳菜口已自動將空盤子 [{emptyPlate.name}] 送回內場。");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("傳菜口上的盤子目前是空的！");
                    }
                }
                else
                {
                    // 傳菜口上是普通物品（不是盤子），空手直接拿走，並同步清空對應端
                    GameObject item = placedObject;
                    ClearObject();

                    if (linked != null)
                    {
                        linked.ClearObject();
                    }

                    player.Carry(item);
                    Debug.Log($"[{player.gameObject.name}] 從傳菜口拿走了 [{item.name}]！");
                }
            }
            else
            {
                Debug.Log("手上已經有東西，或是傳菜口已被佔用，無法互動！");
            }
        }
    }

    /// <summary>
    /// 本地端擺放物件
    /// </summary>
    public void PlaceObject(GameObject obj)
    {
        placedObject = obj;

        // 設定父子級關係與坐標歸零
        if (holdingPoint != null)
        {
            obj.transform.SetParent(holdingPoint);
            obj.transform.localPosition = Vector3.zero;
        }
        else
        {
            obj.transform.SetParent(transform);
            obj.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        }
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one; // 重設縮放以防變形

        // 關閉盤子碰撞體，防止玩家阻擋
        Collider2D collider = obj.GetComponent<Collider2D>() ?? obj.GetComponentInChildren<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    /// <summary>
    /// 當另一側擺放食物時，此側同步接收該物件的視覺參照並移入此側錨點
    /// </summary>
    public void ReceiveObject(GameObject obj)
    {
        placedObject = obj;

        if (holdingPoint != null)
        {
            obj.transform.SetParent(holdingPoint);
            obj.transform.localPosition = Vector3.zero;
        }
        else
        {
            obj.transform.SetParent(transform);
            obj.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        }
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one; // 重設縮放以防變形

        // 接收端也必須關閉碰撞體，防止阻擋玩家
        Collider2D collider = obj.GetComponent<Collider2D>() ?? obj.GetComponentInChildren<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    /// <summary>
    /// 清除此側的物件佔用參照
    /// </summary>
    public void ClearObject()
    {
        placedObject = null;
    }
}
