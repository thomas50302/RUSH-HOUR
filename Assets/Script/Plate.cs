using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public struct RecipeSpriteMapping
    {
        [Tooltip("菜色名稱，需與程式中的配方名稱一致 (例如: Nasi Goreng, Rendang, Ayam Bakar Madu, Tempe Goreng)")]
        public string recipeName;
        [Tooltip("對應顯示的菜色圖片")]
        public Sprite recipeSprite;
    }

    [Header("盤子設定")]
    [Tooltip("這盤料理的名稱 (如果是髒盤子則為空)")]
    public string recipeName = "";

    [Tooltip("是否為髒盤子")]
    public bool isDirty = false;

    [Header("食物圖片顯示設定")]
    [Tooltip("用來顯示完成菜品圖片的 Sprite Renderer (建議建立一個子物件並掛載 Sprite Renderer，再拖入此欄位)")]
    public SpriteRenderer foodSpriteRenderer;

    [Tooltip("設定不同菜色對應的圖片對照表")]
    public List<RecipeSpriteMapping> recipeSprites = new List<RecipeSpriteMapping>();

    [Header("裝載的食材清單")]
    public List<string> addedIngredients = new List<string>();

    void Start()
    {
        UpdateFoodVisuals();
    }

    // 取得盤子目前盛裝的食物描述 (除錯用)
    public string GetContentsDescription()
    {
        if (isDirty) return "髒盤子";
        if (addedIngredients.Count == 0) return "空盤子";
        return $"{recipeName} (包含: {string.Join(", ", addedIngredients)})";
    }

    /// <summary>
    /// 玩家直接與盤子進行互動
    /// </summary>
    public void Interact(PlayerControll player)
    {
        // 情況 A：玩家雙手空空，只端起盤子裡的食物，盤子保留在原地
        if (!player.IsCarrying())
        {
            if (addedIngredients.Count > 0)
            {
                GameObject food = TakeFood();
                if (food != null)
                {
                    player.Carry(food);
                    Debug.Log($"[{player.gameObject.name}] 從盤子中拿走了食物 [{food.name}]，盤子保留在原地。");
                }
            }
            else
            {
                // 只有在空盤子掉在地上且無父級容器時，才允許直接端起空盤子
                if (transform.parent == null)
                {
                    player.Carry(gameObject);
                    Debug.Log($"[{player.gameObject.name}] 端起了地上的空盤子 [{gameObject.name}]。");
                }
                else
                {
                    Debug.Log("盤子目前是空的，且已固定在設備上，不需端起。");
                }
            }
        }
        // 情況 B：玩家手上拿著東西，放進盤子
        else
        {
            GameObject carriedObj = player.GetCarriedObject();
            if (carriedObj != gameObject && !isDirty)
            {
                player.Drop();
                AddAnyObject(carriedObj);
                Debug.Log($"[{player.gameObject.name}] 將 [{carriedObj.name}] 放進了盤子 [{gameObject.name}]。");
            }
        }
    }

    /// <summary>
    /// 從盤子中取出盛裝的實體食物物件，並重置盤子資料
    /// </summary>
    public GameObject TakeFood()
    {
        if (addedIngredients.Count == 0) return null;

        GameObject foodObj = null;

        // 搜尋子物件中的實體食物（排除用於顯示UI菜品圖片的 foodSpriteRenderer）
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (foodSpriteRenderer != null && child == foodSpriteRenderer.transform)
                continue;

            foodObj = child.gameObject;
            break;
        }

        // 若找到實體食物物件，將其與盤子分離
        if (foodObj != null)
        {
            foodObj.transform.SetParent(null);

            // 重新開啟其碰撞體
            Collider2D col = foodObj.GetComponent<Collider2D>() ?? foodObj.GetComponentInChildren<Collider2D>();
            if (col != null)
            {
                col.enabled = true;
            }

            // 清空盤子的食材紀錄與食譜狀態
            addedIngredients.Clear();
            recipeName = "";
            UpdateFoodVisuals();

            return foodObj;
        }

        return null;
    }

    /// <summary>
    /// 將任何實體物件放入盤中，並進行視覺堆疊與名稱判定
    /// </summary>
    public bool AddAnyObject(GameObject obj)
    {
        if (isDirty) return false;

        // 獲取物件的食材名稱 (用於食譜判定)
        string ingName = obj.name;
        Ingredient ing = obj.GetComponent<Ingredient>() ?? obj.GetComponentInChildren<Ingredient>();
        if (ing != null)
        {
            ingName = ing.ingredientName;
        }
        
        // 清洗名稱
        ingName = System.Text.RegularExpressions.Regex.Replace(ingName, @"\s*\(Clone\)\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        ingName = System.Text.RegularExpressions.Regex.Replace(ingName, @"\s*\(\d+\)\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        ingName = ingName.Trim();

        // 標準化名稱映射
        if (ingName == "SoySause" || ingName == "SoySauce" || ingName == "SweetSoy")
        {
            ingName = "SweetSoy";
        }
        else if (ingName == "Coconut Milk" || ingName == "CoconutMilk")
        {
            ingName = "CoconutMilk";
        }
        else if (ingName == "Shrimp Paste" || ingName == "ShrimpPaste")
        {
            ingName = "ShrimpPaste";
        }

        addedIngredients.Add(ingName);

        // 將該物件實體變為盤子的子物件，並做視覺排版 (向上堆疊)
        obj.transform.SetParent(this.transform);
        float offsetOffset = (addedIngredients.Count - 1) * 0.08f;
        obj.transform.localPosition = new Vector3(0f, offsetOffset, -0.05f * addedIngredients.Count);
        obj.transform.localRotation = Quaternion.identity;

        // 關閉該物件的碰撞體，防止干擾
        Collider2D col = obj.GetComponent<Collider2D>() ?? obj.GetComponentInChildren<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Debug.Log($"盤子收納了實體物件: {obj.name} (判定名稱: {ingName})。目前內容: {GetContentsDescription()}");

        // 檢查食譜與更新盤子圖片 (如果是特定套餐)
        CheckRecipe();
        UpdateFoodVisuals();

        return true;
    }

    /// <summary>
    /// 嘗試加入一個熟的食材到盤中 (保留原方法以向下相容)
    /// </summary>
    public bool AddIngredient(string ingredientName)
    {
        if (isDirty) return false;

        // 標準化名稱，避免拼寫或空白鍵造成食譜配對失敗
        if (ingredientName == "SoySause" || ingredientName == "SoySauce" || ingredientName == "SweetSoy")
        {
            ingredientName = "SweetSoy";
        }
        else if (ingredientName == "Coconut Milk" || ingredientName == "CoconutMilk")
        {
            ingredientName = "CoconutMilk";
        }
        else if (ingredientName == "Shrimp Paste" || ingredientName == "ShrimpPaste")
        {
            ingredientName = "ShrimpPaste";
        }
        
        addedIngredients.Add(ingredientName);
        Debug.Log($"盤子加入了食材: {ingredientName}。目前內容: {GetContentsDescription()}");
        
        // 每次加入食材時，會檢查是否達成了某個料理的配方
        CheckRecipe();
        UpdateFoodVisuals();
        
        return true;
    }

    /// <summary>
    /// 檢查目前盤中的配方是否符合任何一道印尼名菜
    /// </summary>
    private void CheckRecipe()
    {
        // 這裡做一個簡單的配方配對機制
        // 1. 印尼炒飯 Nasi Goreng: Oil, Garlic, Chicken, Rice, SweetSoy (SoySause), Egg
        if (ContainsAll("Oil", "Garlic", "Chicken", "Rice", "SweetSoy", "Egg"))
        {
            recipeName = "Nasi Goreng";
        }
        // 2. 巴東燴牛肉 Rendang: Garlic, Beef, CoconutMilk
        else if (ContainsAll("Garlic", "Beef", "CoconutMilk"))
        {
            recipeName = "Rendang";
        }
        // 3. 蜂蜜烤雞 Ayam Bakar Madu: Garlic, Honey, SweetSoy, Chicken
        else if (ContainsAll("Garlic", "Honey", "SweetSoy", "Chicken"))
        {
            recipeName = "Ayam Bakar Madu";
        }
        // 4. 炸天貝 Tempe Goreng: Garlic, SweetSoy, Tempeh, Flour
        else if (ContainsAll("Garlic", "SweetSoy", "Tempeh", "Flour"))
        {
            recipeName = "Tempe Goreng";
        }
        else
        {
            recipeName = "Custom Dish"; // 未完成的混合料理
        }
    }

    /// <summary>
    /// 更新盤子上的食物圖片顯示
    /// </summary>
    public void UpdateFoodVisuals()
    {
        if (foodSpriteRenderer == null) return;

        // 如果是髒盤子或空盤子，隱藏食物圖片
        if (isDirty || addedIngredients.Count == 0 || string.IsNullOrEmpty(recipeName) || recipeName == "Custom Dish")
        {
            foodSpriteRenderer.sprite = null;
            return;
        }

        // 搜尋配方對照表中的圖片
        Sprite matchedSprite = null;
        foreach (var mapping in recipeSprites)
        {
            if (mapping.recipeName == recipeName)
            {
                matchedSprite = mapping.recipeSprite;
                break;
            }
        }

        // 套用圖片
        foodSpriteRenderer.sprite = matchedSprite;
    }

    private bool ContainsAll(params string[] ingredients)
    {
        foreach (var ing in ingredients)
        {
            if (!addedIngredients.Contains(ing)) return false;
        }
        return true;
    }
}
