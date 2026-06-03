using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
    [Header("盤子設定")]
    [Tooltip("這盤料理的名稱 (如果是髒盤子則為空)")]
    public string recipeName = "";

    [Tooltip("是否為髒盤子")]
    public bool isDirty = false;

    [Header("裝載的食材清單")]
    public List<string> addedIngredients = new List<string>();

    // 取得盤子目前盛裝的食物描述 (除錯用)
    public string GetContentsDescription()
    {
        if (isDirty) return "髒盤子";
        if (addedIngredients.Count == 0) return "空盤子";
        return $"{recipeName} (包含: {string.Join(", ", addedIngredients)})";
    }

    /// <summary>
    /// 嘗試加入一個熟的食材到盤中
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
        
        // 每次加入食材時，可以檢查是否達成了某個料理的配方
        CheckRecipe();
        return true;
    }

    /// <summary>
    /// 檢查目前盤中的配方是否符合任何一道印尼名菜
    /// </summary>
    private void CheckRecipe()
    {
        // 這裡做一個簡單的配方配對機制
        // 1. 印尼炒飯 Nasi Goreng: Garlic, Chicken, Rice, Egg
        if (ContainsAll("Garlic", "Chicken", "Rice", "Egg"))
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

    private bool ContainsAll(params string[] ingredients)
    {
        foreach (var ing in ingredients)
        {
            if (!addedIngredients.Contains(ing)) return false;
        }
        return true;
    }
}
