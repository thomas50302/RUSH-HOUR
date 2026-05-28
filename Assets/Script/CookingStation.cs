using UnityEngine;

public class CookingStation : MonoBehaviour, IInteractable
{
    public enum StationType
    {
        CuttingBoard, // 砧板 (用來處理生食材 Raw -> Processed)
        Stove,        // 炒鍋 (用來炒熟食材 Processed -> Cooked)
        Oven          // 烤盤/烤箱 (用來烘烤/醃漬食材 -> Cooked)
    }

    [Header("工作台屬性")]
    public StationType stationType = StationType.CuttingBoard;
    
    [Tooltip("工作處理或烹飪所需時間 (秒)")]
    public float processTime = 4f;
    
    [Tooltip("煮熟後放置多久會燒焦 (秒，僅適用於爐子/烤箱)")]
    public float burnTime = 5f;

    [Header("物品擺放錨點")]
    public Transform holdingPoint;

    private GameObject placedObject = null;
    private Ingredient placedIngredient = null;
    
    private float timer = 0f;
    private bool isWorking = false;
    private bool isBurntTimerActive = false;

    void Update()
    {
        // 處理食材的烹飪與烤焦邏輯
        if (isWorking && placedIngredient != null)
        {
            timer += Time.deltaTime;

            // 狀況 1：正在處理生食材（切菜或下鍋炒）
            if (placedIngredient.currentState == Ingredient.IngredientState.Raw || 
                (stationType == StationType.Stove && placedIngredient.currentState == Ingredient.IngredientState.Processed))
            {
                if (timer >= processTime)
                {
                    if (stationType == StationType.CuttingBoard)
                    {
                        // 砧板：生 -> 處理過
                        placedIngredient.SetState(Ingredient.IngredientState.Processed);
                        Debug.Log($"[{gameObject.name}] 食材切好了！");
                        isWorking = false;
                    }
                    else
                    {
                        // 爐火/烤盤：處理過 -> 熟
                        placedIngredient.SetState(Ingredient.IngredientState.Cooked);
                        Debug.Log($"[{gameObject.name}] 食材煮熟了！");
                        
                        // 重設計時器，轉為準備燒焦階段
                        timer = 0f;
                        isBurntTimerActive = true;
                    }
                }
            }
            // 狀況 2：食物已經熟了，在爐火上會開始計時燒焦
            else if (isBurntTimerActive && placedIngredient.currentState == Ingredient.IngredientState.Cooked)
            {
                if (stationType == StationType.Stove || stationType == StationType.Oven)
                {
                    if (timer >= burnTime)
                    {
                        placedIngredient.SetState(Ingredient.IngredientState.Burnt);
                        Debug.LogWarning($"[{gameObject.name}] 糟糕！食物在爐火上燒焦了！");
                        isWorking = false;
                        isBurntTimerActive = false;
                    }
                }
            }
        }
    }

    public void Interact(PlayerControll player)
    {
        // A. 如果工作台上是空的
        if (placedObject == null)
        {
            if (player.IsCarrying())
            {
                GameObject carried = player.GetCarriedObject();
                Ingredient ing = carried.GetComponent<Ingredient>();

                if (ing != null)
                {
                    bool canPlace = false;

                    // 檢查食材狀態是否符合當前工作台
                    if (stationType == StationType.CuttingBoard && ing.currentState == Ingredient.IngredientState.Raw)
                    {
                        canPlace = true;
                    }
                    else if ((stationType == StationType.Stove || stationType == StationType.Oven) && 
                             (ing.currentState == Ingredient.IngredientState.Processed || ing.currentState == Ingredient.IngredientState.Raw))
                    {
                        canPlace = true;
                    }

                    if (canPlace)
                    {
                        // 玩家放下物品，物品放置到工作台上
                        player.Drop();
                        PlaceObject(carried, ing);
                    }
                    else
                    {
                        Debug.Log("這個食材不適合放在此工作台，或者它已經不需要再處理了。");
                    }
                }
            }
        }
        // B. 如果工作台上有東西
        else
        {
            // B1. 玩家手上有拿盤子，想要直接把煮好或切好的食材裝進盤子裡
            if (player.IsCarrying())
            {
                GameObject carried = player.GetCarriedObject();
                Plate plate = carried.GetComponent<Plate>();

                if (plate != null && !plate.isDirty)
                {
                    // 只有「熟的」或是不需要再加熱的食材可以裝盤
                    if (placedIngredient != null && 
                        (placedIngredient.currentState == Ingredient.IngredientState.Cooked || 
                         placedIngredient.currentState == Ingredient.IngredientState.Processed))
                    {
                        if (plate.AddIngredient(placedIngredient.ingredientName))
                        {
                            // 銷毀工作台上的食材物件，清理工作台
                            Destroy(placedObject);
                            ClearStation();
                            Debug.Log($"[{player.gameObject.name}] 將工作台上的食物裝入盤中！");
                        }
                    }
                }
            }
            // B2. 玩家手是空的，直接把工作台上的東西拿起來
            else
            {
                GameObject tempObj = placedObject;
                ClearStation();
                player.Carry(tempObj);
                Debug.Log($"[{player.gameObject.name}] 拿走了工作台上的 [{tempObj.name}]。");
            }
        }
    }

    private void PlaceObject(GameObject obj, Ingredient ing)
    {
        placedObject = obj;
        placedIngredient = ing;

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

        // 開始計時工作
        timer = 0f;
        isWorking = true;
        isBurntTimerActive = false;
    }

    private void ClearStation()
    {
        placedObject = null;
        placedIngredient = null;
        isWorking = false;
        isBurntTimerActive = false;
        timer = 0f;
    }
}
