using UnityEngine;

public class Counter : MonoBehaviour, IInteractable
{
    [Header("櫃檯設定")]
    [Tooltip("物品擺放錨點")]
    public Transform holdingPoint;

    private GameObject placedObject = null;

    public void Interact(PlayerControll player)
    {
        // 情況 A：櫃檯是空的，玩家想把手上的東西放下來
        if (placedObject == null)
        {
            if (player.IsCarrying())
            {
                GameObject carried = player.Drop();
                PlaceObject(carried);
                Debug.Log($"[{player.gameObject.name}] 將 [{carried.name}] 放到了櫃檯 [{gameObject.name}] 上。");
            }
        }
        // 情況 B：櫃檯上有東西
        else
        {
            // B1. 櫃檯上是盤子，玩家手上拿著東西，想要放進盤子
            if (player.IsCarrying())
            {
                GameObject carried = player.GetCarriedObject();
                Plate plate = placedObject.GetComponent<Plate>() ?? placedObject.GetComponentInChildren<Plate>();

                if (plate != null && !plate.isDirty)
                {
                    Plate carriedPlate = carried.GetComponent<Plate>() ?? carried.GetComponentInChildren<Plate>();
                    if (carriedPlate != null)
                    {
                        // 玩家拿著的是盤子 -> 取出盤子裡的食物放進櫃檯的盤子，銷毀玩家手上的空盤
                        GameObject food = carriedPlate.TakeFood();
                        if (food != null)
                        {
                            plate.AddAnyObject(food);
                            player.Drop();
                            Destroy(carried);
                            Debug.Log($"[{player.gameObject.name}] 將手上盤子裡的食物 [{food.name}] 轉移到櫃檯上的盤子中。");
                        }
                    }
                    else
                    {
                        // 玩家拿著的是普通食物/預製物 -> 直接放進櫃檯的盤子
                        player.Drop();
                        plate.AddAnyObject(carried);
                        Debug.Log($"[{player.gameObject.name}] 將手上的 [{carried.name}] 放進了櫃檯上的盤子 [{placedObject.name}]。");
                    }
                }
                // B2. 櫃檯上是任何物品，玩家手上拿著盤子，想要把櫃檯上的物品裝入盤子
                else
                {
                    Plate carriedPlate = carried.GetComponent<Plate>() ?? carried.GetComponentInChildren<Plate>();
                    if (carriedPlate != null && !carriedPlate.isDirty)
                    {
                        GameObject targetObj = placedObject;
                        placedObject = null;
                        carriedPlate.AddAnyObject(targetObj);
                        Debug.Log($"[{player.gameObject.name}] 用手上的盤子裝走了櫃檯上的 [{targetObj.name}]。");
                    }
                }
            }
            // B3. 玩家手是空的，直接把櫃檯上的東西拿走
            else
            {
                GameObject tempObj = placedObject;
                placedObject = null;
                
                // 重新開啟碰撞體，並讓玩家拿走
                player.Carry(tempObj);
                Debug.Log($"[{player.gameObject.name}] 拿走了櫃檯上的 [{tempObj.name}]。");
            }
        }
    }

    private void PlaceObject(GameObject obj)
    {
        placedObject = obj;

        // 綁定 Parent
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

        // 關閉碰撞體避免阻擋移動
        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }
}
