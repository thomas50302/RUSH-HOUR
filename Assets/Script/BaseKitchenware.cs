using UnityEngine;

public abstract class BaseKitchenware : MonoBehaviour, IInteractable
{
    [Header("基礎擺放設定")]
    [Tooltip("物品擺放錨點 (若無設定會放在此物件中心上方一點)")]
    public Transform holdingPoint;

    protected GameObject placedObject = null;
    protected Ingredient placedIngredient = null;

    /// <summary>
    /// 核心互動邏輯，子類別通常不需修改此邏輯
    /// </summary>
    public virtual void Interact(PlayerControll player)
    {
        // A. 廚具上是空的，玩家想放東西上去
        if (placedObject == null)
        {
            if (player.IsCarrying())
            {
                GameObject carried = player.GetCarriedObject();
                Ingredient ing = carried.GetComponent<Ingredient>();

                if (ing != null)
                {
                    // 呼叫子類別的條件檢查
                    if (CanPlace(ing))
                    {
                        player.Drop();
                        PlaceObject(carried, ing);
                    }
                    else
                    {
                        Debug.Log($"【{gameObject.name}】這個食材狀態 ({ing.GetStateName()}) 不適合放在這裡。");
                    }
                }
            }
        }
        // B. 廚具上有東西，玩家想拿走或裝盤
        else
        {
            // B1. 玩家手上拿著盤子，且上面的食材已經處理或煮好，直接裝盤
            if (player.IsCarrying())
            {
                GameObject carried = player.GetCarriedObject();
                Plate plate = carried.GetComponent<Plate>() ?? carried.GetComponentInChildren<Plate>();

                if (plate != null && !plate.isDirty)
                {
                    if (placedIngredient != null && 
                        (placedIngredient.currentState == Ingredient.IngredientState.Cooked || 
                         placedIngredient.currentState == Ingredient.IngredientState.Processed))
                    {
                        GameObject food = placedObject;
                        ClearStation();
                        plate.AddAnyObject(food);
                        Debug.Log($"[{player.gameObject.name}] 將【{gameObject.name}】上的食物 [{food.name}] 裝入盤中！");
                    }
                }
            }
            // B2. 玩家手是空的，直接把東西拿走
            else
            {
                GameObject tempObj = placedObject;
                ClearStation();
                player.Carry(tempObj);
                Debug.Log($"[{player.gameObject.name}] 拿走了【{gameObject.name}】上的 [{tempObj.name}]。");
            }
        }
    }

    /// <summary>
    /// 檢查食材是否可以放在此廚具上 (子類別必須實作此條件)
    /// </summary>
    protected abstract bool CanPlace(Ingredient ing);

    /// <summary>
    /// 放下物件時的物理與狀態設定
    /// </summary>
    protected virtual void PlaceObject(GameObject obj, Ingredient ing)
    {
        placedObject = obj;
        placedIngredient = ing;

        // 物理位置綁定
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

        // 關閉碰撞體避免阻礙玩家
        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    /// <summary>
    /// 清除廚具的佔用狀態
    /// </summary>
    protected virtual void ClearStation()
    {
        placedObject = null;
        placedIngredient = null;
    }
}
