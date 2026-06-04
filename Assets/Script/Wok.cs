using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wok : MonoBehaviour, IInteractable
{
    public enum PotState
    {
        Empty,                  // 1. 空鍋，等待加「油」
        HeatingOil,             // 2. 加油了，熱油中 (自動計時)
        OilHot,                 // 3. 油熱了，等待加入「大蒜」
        FryingAromatics,        // 4. 大蒜已加，等待玩家手動互動翻炒 (爆香)
        AromaticsDone,          // 5. 大蒜爆香完成，等待加入「雞肉」
        ChickenAdded,           // 6. 雞肉已加，等待加入「米飯」
        RiceAdded,              // 7. 米飯已加，等待加入「甜醬油」
        SoyAdded,               // 8. 甜醬油已加，等待加入「雞蛋」
        EggAdded,               // 9. 雞蛋已加，食材備齊，等待玩家手動互動翻炒
        Done                    // 10. 烹飪完成！已自動生成成品，等待空手拿取
    }

    [Header("烹飪狀態")]
    public PotState currentState = PotState.Empty;

    [Header("烹飪計時與翻炒次數設定")]
    [Tooltip("熱油需要的時間 (秒)")]
    public float oilHeatTime = 3f;
    [Tooltip("爆香大蒜需要的翻炒次數")]
    public int aromaticsFryClicksNeeded = 5;
    [Tooltip("炒飯最後翻炒需要的次數")]
    public int starchFryClicksNeeded = 5;

    [Header("完成菜品生成設定")]
    [Tooltip("盤子的預製物 (Prefab)")]
    public GameObject platePrefab;
    [Tooltip("成品生成的位置 (若為空則在鍋子上方一點)")]
    public Transform foodSpawnPoint;

    [Header("UI 進度條")]
    [Tooltip("拖入此炒鍋下的 ProgressBar 元件")]
    public ProgressBar progressBar;

    [Header("目前鍋內裝載的食材")]
    public List<string> addedIngredients = new List<string>();

    private float timer = 0f;
    private int currentClicks = 0;
    private GameObject spawnedDishObj = null; // 記錄目前生成的成品盤子

    void Start()
    {
        ResetPot();
    }

    void Update()
    {
        // 熱油自動計時
        if (currentState == PotState.HeatingOil)
        {
            timer += Time.deltaTime;
            if (progressBar != null)
            {
                progressBar.Show();
                progressBar.SetColor(progressBar.cookingColor);
                progressBar.UpdateProgress(timer, oilHeatTime);
            }
            if (timer >= oilHeatTime)
            {
                currentState = PotState.OilHot;
                timer = 0f;
                if (progressBar != null) progressBar.Hide();
                Debug.Log($"【{gameObject.name}】油熱好了！可以放入「大蒜 (Garlic)」進行爆香。");
            }
        }
    }

    /// <summary>
    /// 食材名稱標準化與對照
    /// </summary>
    private string GetNormalizedIngredientName(GameObject carriedObj)
    {
        Ingredient ing = carriedObj.GetComponent<Ingredient>();
        string rawName = "";

        if (ing != null && !string.IsNullOrEmpty(ing.ingredientName))
        {
            rawName = ing.ingredientName;
        }
        else
        {
            rawName = carriedObj.name;
        }

        // 使用正則表達式徹底清洗 Unity 克隆後綴，如 (Clone), (1)
        rawName = System.Text.RegularExpressions.Regex.Replace(rawName, @"\s*\(Clone\)\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        rawName = System.Text.RegularExpressions.Regex.Replace(rawName, @"\s*\(\d+\)\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        rawName = rawName.Trim();

        // 標準化映射，防止拼寫或空格不一致
        if (rawName == "SoySause" || rawName == "SoySauce" || rawName == "SweetSoy")
        {
            return "SweetSoy";
        }
        if (rawName == "Coconut Milk" || rawName == "CoconutMilk")
        {
            return "CoconutMilk";
        }
        if (rawName == "Shrimp Paste" || rawName == "ShrimpPaste")
        {
            return "ShrimpPaste";
        }
        if (rawName == "Rendang Paste" || rawName == "RendangPaste" || rawName == "Rendang")
        {
            return "RendangPaste";
        }

        return rawName;
    }

    public void Interact(PlayerControll player)
    {
        // 如果已經生成好盤子成品，玩家只需要空手就可以直接拿走
        if (spawnedDishObj != null)
        {
            if (!player.IsCarrying())
            {
                GameObject dishToCarry = spawnedDishObj;
                dishToCarry.transform.SetParent(null);
                player.Carry(dishToCarry);
                
                spawnedDishObj = null;
                ResetPot();
                Debug.Log($"[{player.gameObject.name}] 拿取了成品菜餚，Wok 重置！");
            }
            else
            {
                Debug.Log($"【{gameObject.name}】菜已做好！請空手拿取。");
            }
            return;
        }

        // A. 玩家手上拿著物件，嘗試加入鍋中
        if (player.IsCarrying())
        {
            GameObject carried = player.GetCarriedObject();
            string ingName = GetNormalizedIngredientName(carried);

            // 1. 放入「油」
            if (currentState == PotState.Empty)
            {
                if (ingName == "Oil")
                {
                    player.Drop();
                    Destroy(carried);
                    addedIngredients.Add("Oil");
                    currentState = PotState.HeatingOil;
                    timer = 0f;
                    Debug.Log($"【{gameObject.name}】已加入油，熱油中...");
                }
                else
                {
                    Debug.Log($"【{gameObject.name}】必須先加入「油 (Oil)」！目前你拿的是: {ingName}");
                }
                return;
            }

            // 2. 放入「大蒜」
            if (currentState == PotState.OilHot)
            {
                if (ingName == "Garlic")
                {
                    player.Drop();
                    Destroy(carried);
                    addedIngredients.Add("Garlic");
                    currentState = PotState.FryingAromatics;
                    currentClicks = 0;
                    Debug.Log($"【{gameObject.name}】放入了大蒜！請面向鍋子連續按互動鍵進行翻炒爆香！");
                }
                else
                {
                    Debug.Log($"【{gameObject.name}】此時需要放入「大蒜 (Garlic)」！");
                }
                return;
            }

            // 3. 大蒜炒好後，放入「雞肉」
            if (currentState == PotState.AromaticsDone)
            {
                if (ingName == "Chicken")
                {
                    player.Drop();
                    Destroy(carried);
                    addedIngredients.Add("Chicken");
                    currentState = PotState.ChickenAdded;
                    Debug.Log($"【{gameObject.name}】放入雞肉！接下來請放入「米飯 (Rice)」。");
                }
                else
                {
                    Debug.Log($"【{gameObject.name}】此時請放入「雞肉 (Chicken)」！");
                }
                return;
            }

            // 4. 放入「米飯」
            if (currentState == PotState.ChickenAdded)
            {
                if (ingName == "Rice")
                {
                    player.Drop();
                    Destroy(carried);
                    addedIngredients.Add("Rice");
                    currentState = PotState.RiceAdded;
                    Debug.Log($"【{gameObject.name}】放入米飯！接下來請放入「甜醬油 (SweetSoy)」。");
                }
                else
                {
                    Debug.Log($"【{gameObject.name}】此時必須放入「米飯 (Rice)」！");
                }
                return;
            }

            // 5. 放入「甜醬油」
            if (currentState == PotState.RiceAdded)
            {
                if (ingName == "SweetSoy")
                {
                    player.Drop();
                    Destroy(carried);
                    addedIngredients.Add("SweetSoy");
                    currentState = PotState.SoyAdded;
                    Debug.Log($"【{gameObject.name}】放入甜醬油！接下來請放入「雞蛋 (Egg)」。");
                }
                else
                {
                    Debug.Log($"【{gameObject.name}】此時必須放入「甜醬油 (SweetSoy)」！");
                }
                return;
            }

            // 6. 放入「雞蛋」
            if (currentState == PotState.SoyAdded)
            {
                if (ingName == "Egg")
                {
                    player.Drop();
                    Destroy(carried);
                    addedIngredients.Add("Egg");
                    currentState = PotState.EggAdded;
                    currentClicks = 0;
                    Debug.Log($"【{gameObject.name}】放入雞蛋！食材已全數備齊，請連續按互動鍵進行最後翻炒！");
                }
                else
                {
                    Debug.Log($"【{gameObject.name}】此時必須放入「雞蛋 (Egg)」！");
                }
                return;
            }
        }
        else
        {
            // B. 玩家雙手空空 -> 進行互動翻炒
            if (currentState == PotState.FryingAromatics)
            {
                currentClicks++;
                if (progressBar != null)
                {
                    progressBar.Show();
                    progressBar.SetColor(progressBar.cookingColor);
                    progressBar.UpdateProgress(currentClicks, aromaticsFryClicksNeeded);
                }
                Debug.Log($"【{gameObject.name}】翻炒大蒜中... ({currentClicks}/{aromaticsFryClicksNeeded})");
                if (currentClicks >= aromaticsFryClicksNeeded)
                {
                    currentState = PotState.AromaticsDone;
                    if (progressBar != null) progressBar.Hide();
                    Debug.Log($"【{gameObject.name}】大蒜爆香完成！請放入「雞肉」。");
                }
            }
            else if (currentState == PotState.EggAdded)
            {
                currentClicks++;
                if (progressBar != null)
                {
                    progressBar.Show();
                    progressBar.SetColor(progressBar.cookingColor);
                    progressBar.UpdateProgress(currentClicks, starchFryClicksNeeded);
                }
                Debug.Log($"【{gameObject.name}】最後翻炒炒飯中... ({currentClicks}/{starchFryClicksNeeded})");
                if (currentClicks >= starchFryClicksNeeded)
                {
                    currentState = PotState.Done;
                    if (progressBar != null) progressBar.Hide();
                    Debug.Log($"【{gameObject.name}】★★ 印尼炒飯烹飪完成！ ★★ 已自動生成裝盤成品！");
                    SpawnCompletedDish();
                }
            }
        }
    }

    private void SpawnCompletedDish()
    {
        if (spawnedDishObj != null) return;

        if (platePrefab == null)
        {
            Debug.LogWarning($"【{gameObject.name}】未設定 platePrefab，無法自動生成裝盤成品！");
            return;
        }

        Vector3 spawnPos = foodSpawnPoint != null ? foodSpawnPoint.position : transform.position + new Vector3(0f, 0.4f, 0f);
        spawnedDishObj = Instantiate(platePrefab, spawnPos, Quaternion.identity);

        if (foodSpawnPoint != null)
        {
            spawnedDishObj.transform.SetParent(foodSpawnPoint);
            spawnedDishObj.transform.localPosition = Vector3.zero;
        }
        else
        {
            spawnedDishObj.transform.SetParent(transform);
        }

        Plate plateScript = spawnedDishObj.GetComponent<Plate>() ?? spawnedDishObj.GetComponentInChildren<Plate>();
        if (plateScript != null)
        {
            foreach (string ingredient in addedIngredients)
            {
                plateScript.AddIngredient(ingredient);
            }
        }

        Collider2D dishCollider = spawnedDishObj.GetComponent<Collider2D>();
        if (dishCollider != null)
        {
            dishCollider.enabled = false;
        }
    }

    private void ResetPot()
    {
        currentState = PotState.Empty;
        addedIngredients.Clear();
        timer = 0f;
        currentClicks = 0;
        spawnedDishObj = null;
        if (progressBar != null) progressBar.Hide();
    }
}
