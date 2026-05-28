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
            // B1. 櫃檯上是盤子，玩家手上拿著熟食材，想要直接裝盤
            if (player.IsCarrying())
            {
                GameObject carried = player.GetCarriedObject();
                Ingredient ing = carried.GetComponent<Ingredient>();
                Plate plate = placedObject.GetComponent<Plate>();

                if (plate != null && !plate.isDirty && ing != null && 
                    (ing.currentState == Ingredient.IngredientState.Cooked || ing.currentState == Ingredient.IngredientState.Processed))
                {
                    if (plate.AddIngredient(ing.ingredientName))
                    {
                        player.Drop();
                        Destroy(carried);
                        Debug.Log($"[{player.gameObject.name}] 直接將手上的 {ing.ingredientName} 裝入櫃檯上的盤子。");
                    }
                }
                // B2. 櫃檯上是熟食材，玩家手上拿著盤子，想要把食材裝盤
                else
                {
                    Plate carriedPlate = carried.GetComponent<Plate>();
                    Ingredient placedIng = placedObject.GetComponent<Ingredient>();

                    if (carriedPlate != null && !carriedPlate.isDirty && placedIng != null && 
                        (placedIng.currentState == Ingredient.IngredientState.Cooked || placedIng.currentState == Ingredient.IngredientState.Processed))
                    {
                        if (carriedPlate.AddIngredient(placedIng.ingredientName))
                        {
                            Destroy(placedObject);
                            placedObject = null;
                            Debug.Log($"[{player.gameObject.name}] 用手上的盤子裝走了櫃檯上的 {placedIng.ingredientName}。");
                        }
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
