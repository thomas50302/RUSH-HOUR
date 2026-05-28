using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NasiGorengPot : MonoBehaviour, IInteractable
{
    public enum PotState
    {
        Empty,              // 1. 空鍋，等待加「油」
        HeatingOil,         // 2. 加油了，熱油中 (自動計時進度條)
        OilHot,             // 3. 油熱了，等待加入「切好的蒜」與「切好的雞肉」
        FryingChickenGarlic,// 4. 蒜跟雞肉加進去了，等待玩家翻炒 (需點擊/互動增加翻炒進度)
        FryingChickenDone,  // 5. 雞肉翻炒熟了，等待加入「甜醬油」與「蝦醬」
        AddingSauces,       // 6. 醬料加進去了，等待加入「米飯」
        FryingRice,         // 7. 米飯加進去了，等待第二次翻炒 (需點擊/互動增加進度)
        Done                // 8. 炒飯完成！等待盤子裝盤
    }

    [Header("炒鍋狀態")]
    public PotState currentState = PotState.Empty;

    [Header("時間與進度參數")]
    [Tooltip("熱油需要的時間 (秒)")]
    public float oilHeatTime = 3f;
    [Tooltip("翻炒雞肉需要的總點擊/互動次數")]
    public int chickenFryClicksNeeded = 5;
    [Tooltip("翻炒米飯需要的總點擊/互動次數")]
    public int riceFryClicksNeeded = 5;

    [Header("裝載的食材與調味狀態")]
    public bool hasOil = false;
    public bool hasGarlic = false;
    public bool hasChicken = false;
    public bool hasSweetSoy = false;
    public bool hasShrimpPaste = false;
    public bool hasRice = false;

    private float timer = 0f;
    private int currentClicks = 0;

    void Update()
    {
        // 狀態 2：自動進行熱油計時
        if (currentState == PotState.HeatingOil)
        {
            timer += Time.deltaTime;
            if (timer >= oilHeatTime)
            {
                currentState = PotState.OilHot;
                timer = 0f;
                Debug.Log("【炒鍋】油鍋熱好了！現在可以加入 蒜(Garlic) 與 雞肉(Chicken) 了。");
            }
        }
    }

    public void Interact(PlayerControll player)
    {
        // 玩家手上有拿東西
        if (player.IsCarrying())
        {
            GameObject carried = player.GetCarriedObject();
            Ingredient ing = carried.GetComponent<Ingredient>();
            Plate plate = carried.GetComponent<Plate>();

            // 1. 空鍋狀態：加入「油」
            if (currentState == PotState.Empty)
            {
                if (ing != null && ing.ingredientName == "Oil")
                {
                    player.Drop();
                    Destroy(carried);
                    
                    hasOil = true;
                    currentState = PotState.HeatingOil;
                    timer = 0f;
                    Debug.Log("【炒鍋】加入了油，開始熱油中...");
                }
                else
                {
                    Debug.Log("【炒鍋】此時需要先加入「油 (Oil)」來開鍋！");
                }
                return;
            }

            // 2. 油熱了狀態：加入「切好的蒜」與「切好的雞肉」
            if (currentState == PotState.OilHot)
            {
                if (ing != null)
                {
                    if (ing.ingredientName == "Garlic" && ing.currentState == Ingredient.IngredientState.Processed && !hasGarlic)
                    {
                        player.Drop();
                        Destroy(carried);
                        hasGarlic = true;
                        Debug.Log("【炒鍋】加入了切好的蒜。");
                    }
                    else if (ing.ingredientName == "Chicken" && ing.currentState == Ingredient.IngredientState.Processed && !hasChicken)
                    {
                        player.Drop();
                        Destroy(carried);
                        hasChicken = true;
                        Debug.Log("【炒鍋】加入了切好的雞肉。");
                    }

                    // 檢查是否兩者都加進去了
                    if (hasGarlic && hasChicken)
                    {
                        currentState = PotState.FryingChickenGarlic;
                        currentClicks = 0;
                        Debug.Log("【炒鍋】蒜與雞肉都加進去了！請連續點擊鍋子進行翻炒！");
                    }
                }
                return;
            }

            // 3. 雞肉炒熟了狀態：加入「甜醬油」與「蝦醬」
            if (currentState == PotState.FryingChickenDone)
            {
                if (ing != null)
                {
                    if (ing.ingredientName == "SweetSoy" && !hasSweetSoy)
                    {
                        player.Drop();
                        Destroy(carried);
                        hasSweetSoy = true;
                        Debug.Log("【炒鍋】加入了甜醬油。");
                    }
                    else if (ing.ingredientName == "ShrimpPaste" && !hasShrimpPaste)
                    {
                        player.Drop();
                        Destroy(carried);
                        hasShrimpPaste = true;
                        Debug.Log("【炒鍋】加入了蝦醬。");
                    }

                    // 檢查是否兩者都加進去了
                    if (hasSweetSoy && hasShrimpPaste)
                    {
                        currentState = PotState.AddingSauces;
                        Debug.Log("【炒鍋】調味料添加完畢！接下來請加入「米飯 (Rice)」。");
                    }
                }
                return;
            }

            // 4. 調料加好了狀態：加入「米飯」
            if (currentState == PotState.AddingSauces)
            {
                if (ing != null && ing.ingredientName == "Rice")
                {
                    player.Drop();
                    Destroy(carried);
                    hasRice = true;
                    currentState = PotState.FryingRice;
                    currentClicks = 0;
                    Debug.Log("【炒鍋】加入了米飯！請連續點擊鍋子進行最後翻炒！");
                }
                return;
            }

            // 5. 炒飯完成狀態：用盤子裝盤
            if (currentState == PotState.Done)
            {
                // 玩家拿著一個空盤子或是尚未完成的盤子
                if (plate != null && !plate.isDirty)
                {
                    // 將炒飯放入盤中 (這裡我們一次加入炒飯的配料成分)
                    plate.AddIngredient("Oil");
                    plate.AddIngredient("Garlic");
                    plate.AddIngredient("Chicken");
                    plate.AddIngredient("SweetSoy");
                    plate.AddIngredient("ShrimpPaste");
                    plate.AddIngredient("Rice");

                    // 重新初始化鍋子狀態以利下次烹飪
                    ResetPot();
                    Debug.Log("【炒鍋】炒飯製作完成並成功裝盤！(此盤子此時還差一個太陽蛋才能成為完整的 Nasi Goreng)。");
                }
                return;
            }
        }
        else
        {
            // 玩家雙手空空，進行點擊翻炒互動
            if (currentState == PotState.FryingChickenGarlic)
            {
                currentClicks++;
                Debug.Log($"【炒鍋】翻炒雞肉中... 點擊數: {currentClicks}/{chickenFryClicksNeeded}");
                if (currentClicks >= chickenFryClicksNeeded)
                {
                    currentState = PotState.FryingChickenDone;
                    Debug.Log("【炒鍋】雞肉炒熟了！現在請加入 甜醬油 (SweetSoy) 與 蝦醬 (ShrimpPaste)。");
                }
            }
            else if (currentState == PotState.FryingRice)
            {
                currentClicks++;
                Debug.Log($"【炒鍋】翻炒米飯中... 點擊數: {currentClicks}/{riceFryClicksNeeded}");
                if (currentClicks >= riceFryClicksNeeded)
                {
                    currentState = PotState.Done;
                    Debug.Log("【炒鍋】★★ 印尼炒飯翻炒完成！ ★★ 請用盤子 (Plate) 來裝盤！");
                }
            }
        }
    }

    private void ResetPot()
    {
        currentState = PotState.Empty;
        hasOil = false;
        hasGarlic = false;
        hasChicken = false;
        hasSweetSoy = false;
        hasShrimpPaste = false;
        hasRice = false;
        timer = 0f;
        currentClicks = 0;
    }
}
