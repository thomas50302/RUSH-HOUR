using UnityEngine;

public class CuttingBoard : BaseKitchenware
{
    [Header("砧板設定")]
    [Tooltip("切菜需要時間 (秒)")]
    public float processTime = 4f;

    private float timer = 0f;
    private bool isWorking = false;

    protected override bool CanPlace(Ingredient ing)
    {
        // 砧板只接受 生 (Raw) 的食材
        return ing.currentState == Ingredient.IngredientState.Raw;
    }

    protected override void PlaceObject(GameObject obj, Ingredient ing)
    {
        base.PlaceObject(obj, ing);
        timer = 0f;
        isWorking = true;
    }

    protected override void ClearStation()
    {
        base.ClearStation();
        timer = 0f;
        isWorking = false;
    }

    void Update()
    {
        if (isWorking && placedIngredient != null)
        {
            timer += Time.deltaTime;
            if (timer >= processTime)
            {
                placedIngredient.SetState(Ingredient.IngredientState.Processed);
                Debug.Log($"【{gameObject.name}】食材處理完成 (切好了)！");
                isWorking = false;
            }
        }
    }
}
