using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour, IInteractable
{
    [Header("單一食材設定 (相容舊版)")]
    [Tooltip("此箱子提供的單一食材預製物 (如果只需提供一種食材，請放這裡)")]
    public GameObject ingredientPrefab;

    [Header("複數食材設定 (新功能)")]
    [Tooltip("此箱子提供的多種食材預製物清單。如果放了多個，玩家互動時會開啟選單選擇")]
    public List<GameObject> ingredientPrefabs = new List<GameObject>();

    [Header("3D 文字提示 (選填)")]
    [Tooltip("可將子物件中的 3D TextMesh 拖入此欄位，會自動顯示目前準備拿取的食材名稱")]
    public TextMesh infoTextMesh;

    private List<GameObject> activeList = new List<GameObject>();
    private int currentCycleIndex = 0; // 用於沒有 UI 時的循環切換索引

    void Start()
    {
        // 整理可用食材清單，維持舊版相容性
        activeList.Clear();
        if (ingredientPrefabs != null && ingredientPrefabs.Count > 0)
        {
            activeList.AddRange(ingredientPrefabs);
        }
        else if (ingredientPrefab != null)
        {
            activeList.Add(ingredientPrefab);
        }

        UpdateInfoText();
    }

    public void Interact(PlayerControll player)
    {
        if (activeList.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] 沒有設定任何食材預製物！");
            return;
        }

        // 如果玩家雙手空空
        if (!player.IsCarrying())
        {
            // 情況 A：只有一種食材，直接生成並讓玩家拿著
            if (activeList.Count == 1)
            {
                SpawnIngredientForPlayer(player, activeList[0]);
            }
            // 情況 B：有多種食材
            else
            {
                if (IngredientSelectionMenu.Instance != null)
                {
                    // 如果有建立 UI 選擇選單，開啟選單讓玩家選擇
                    IngredientSelectionMenu.Instance.OpenMenu(player, activeList, "選擇食材", (chosenPrefab) =>
                    {
                        SpawnIngredientForPlayer(player, chosenPrefab);
                    });
                }
                else
                {
                    // Fallback 機制 (無 UI 時)：
                    // 每次玩家點擊會直接拿取目前選中的食材，並自動切換到下一種，達到「循環拿取」的效果
                    GameObject currentPrefab = activeList[currentCycleIndex];
                    SpawnIngredientForPlayer(player, currentPrefab);

                    // 自動輪替到下一個食材
                    currentCycleIndex = (currentCycleIndex + 1) % activeList.Count;
                    UpdateInfoText();
                    
                    Debug.Log($"[食材箱] 未偵測到 UI 選單。已直接生成食材。下次點擊將生成: {activeList[currentCycleIndex].name} (循環切換中)");
                }
            }
        }
        else
        {
            Debug.Log("手上已經拿著東西了，無法再拿取食材！");
        }
    }

    private void SpawnIngredientForPlayer(PlayerControll player, GameObject prefab)
    {
        if (prefab == null) return;
        
        GameObject newIngredientObj = Instantiate(prefab);
        player.Carry(newIngredientObj);

        Ingredient ing = newIngredientObj.GetComponent<Ingredient>();
        string ingName = ing != null ? ing.ingredientName : prefab.name;
        Debug.Log($"[{player.gameObject.name}] 成功拿取了 [{ingName}]。");
    }

    private void UpdateInfoText()
    {
        if (infoTextMesh != null && activeList.Count > 0)
        {
            if (activeList.Count == 1)
            {
                Ingredient ing = activeList[0].GetComponent<Ingredient>();
                infoTextMesh.text = ing != null ? ing.ingredientName : activeList[0].name;
            }
            else
            {
                Ingredient ing = activeList[currentCycleIndex].GetComponent<Ingredient>();
                string nextName = ing != null ? ing.ingredientName : activeList[currentCycleIndex].name;
                infoTextMesh.text = $"{nextName}\n(點擊拿取/切換)";
            }
        }
    }
}
