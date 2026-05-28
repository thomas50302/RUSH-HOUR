using UnityEngine;

public class Container : MonoBehaviour, IInteractable
{
    [Header("食材箱設定")]
    [Tooltip("此箱子提供的食材預製物 (需包含 Ingredient 元件)")]
    public GameObject ingredientPrefab;

    public void Interact(PlayerControll player)
    {
        // 如果玩家雙手空空
        if (!player.IsCarrying())
        {
            if (ingredientPrefab != null)
            {
                // 生成一個食材物件
                GameObject newIngredientObj = Instantiate(ingredientPrefab);
                
                // 讓玩家拿起來
                player.Carry(newIngredientObj);
                
                Ingredient ing = newIngredientObj.GetComponent<Ingredient>();
                string ingName = ing != null ? ing.ingredientName : "未知食材";
                Debug.Log($"[{player.gameObject.name}] 從箱子拿取了 [{ingName}]。");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] 沒有設定食材預製物 (ingredientPrefab)！");
            }
        }
        else
        {
            Debug.Log("手上已經拿著東西了，無法再拿取食材！");
        }
    }
}
