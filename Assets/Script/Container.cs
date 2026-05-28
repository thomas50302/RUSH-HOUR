using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour, IInteractable
{
    [Header("單一食材設定 (相容舊版)")]
    [Tooltip("此箱子提供的單一食材預製物 (如果只需提供一種食材，請放這裡)")]
    public GameObject ingredientPrefab;

    [Header("複數食材設定 (新功能)")]
    [Tooltip("此箱子提供的多種食材預製物清單。如果放了多個，玩家再次互動會直接切換手上的食材")]
    public List<GameObject> ingredientPrefabs = new List<GameObject>();

    [Header("3D 文字提示 (選填)")]
    [Tooltip("可將子物件中的 3D TextMesh 拖入此欄位，會自動顯示目前選中的食材名稱")]
    public TextMesh infoTextMesh;

    private List<GameObject> activeList = new List<GameObject>();
    private int currentCycleIndex = 0; 

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

        // 情況 A：玩家雙手空空 -> 拿取清單中的第一個（或當前選中）的食材
        if (!player.IsCarrying())
        {
            currentCycleIndex = 0; // 重置為第一個
            SpawnIngredientForPlayer(player, activeList[currentCycleIndex]);
            UpdateInfoText();
        }
        // 情況 B：玩家手上拿著東西
        else
        {
            GameObject carried = player.GetCarriedObject();
            Ingredient ing = carried.GetComponent<Ingredient>();

            // 檢查手上的食材是否屬於這個櫃子提供的種類
            bool isFromThisContainer = false;
            int foundIndex = -1;
            
            if (ing != null)
            {
                for (int i = 0; i < activeList.Count; i++)
                {
                    Ingredient listIng = activeList[i].GetComponent<Ingredient>();
                    if (listIng != null && listIng.ingredientName == ing.ingredientName)
                    {
                        isFromThisContainer = true;
                        foundIndex = i;
                        break;
                    }
                }
            }

            // 如果手上的食材是這個櫃子提供的，且櫃子有多個選項 -> 銷毀手上的，直接換成下一個！
            if (isFromThisContainer && activeList.Count > 1)
            {
                // 放下並銷毀舊的
                GameObject oldObj = player.Drop();
                Destroy(oldObj);

                // 計算下一個食材的索引
                currentCycleIndex = (foundIndex + 1) % activeList.Count;
                GameObject nextPrefab = activeList[currentCycleIndex];

                // 生成並讓玩家拿起新的
                SpawnIngredientForPlayer(player, nextPrefab);
                UpdateInfoText();
            }
            else
            {
                Debug.Log("手拿著其他無關的物品 (如盤子或其他箱子的食材)，無法在此切換。");
            }
        }
    }

    private void SpawnIngredientForPlayer(PlayerControll player, GameObject prefab)
    {
        if (prefab == null) return;
        
        GameObject newIngredientObj = Instantiate(prefab);
        player.Carry(newIngredientObj);

        Ingredient ing = newIngredientObj.GetComponent<Ingredient>();
        string ingName = ing != null ? ing.ingredientName : prefab.name;
        Debug.Log($"[{player.gameObject.name}] 拿取了 [{ingName}]。");
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
                // 顯示下一個會被切換出來的食材，給予提示
                int nextIndex = (currentCycleIndex + 1) % activeList.Count;
                Ingredient currentIng = activeList[currentCycleIndex].GetComponent<Ingredient>();
                Ingredient nextIng = activeList[nextIndex].GetComponent<Ingredient>();
                
                string currentName = currentIng != null ? currentIng.ingredientName : activeList[currentCycleIndex].name;
                string nextName = nextIng != null ? nextIng.ingredientName : activeList[nextIndex].name;

                infoTextMesh.text = $"手持: {currentName}\n(再按一次換: {nextName})";
            }
        }
    }
}
