using UnityEngine;

public class BaseHeatingKitchenware : BaseKitchenware
{
    [Header("加熱烹飪設定")]
    [Tooltip("烹飪熟成需要的時間 (秒)")]
    public float processTime = 4f;

    [Tooltip("煮熟後放置多久會燒焦 (秒)")]
    public float burnTime = 5f;

    [Header("UI 進度條")]
    [Tooltip("拖入此廚具下的 ProgressBar 元件")]
    public ProgressBar progressBar;

    protected float timer = 0f;
    protected bool isWorking = false;
    protected bool isBurntTimerActive = false;

    protected override void PlaceObject(GameObject obj, Ingredient ing)
    {
        base.PlaceObject(obj, ing);
        timer = 0f;
        isWorking = true;
        isBurntTimerActive = false;
    }

    protected override void ClearStation()
    {
        base.ClearStation();
        timer = 0f;
        isWorking = false;
        isBurntTimerActive = false;
        if (progressBar != null) progressBar.Hide();
    }

    protected override bool CanPlace(Ingredient ing)
    {
        // 預設加熱廚具接受 生 (Raw) 或是 處理過 (Processed) 的食材
        return ing.currentState == Ingredient.IngredientState.Raw || ing.currentState == Ingredient.IngredientState.Processed;
    }

    protected virtual void Update()
    {
        if (isWorking && placedIngredient != null)
        {
            timer += Time.deltaTime;

            // 狀況 1：烹飪中 (Raw/Processed ➔ Cooked)
            if (placedIngredient.currentState == Ingredient.IngredientState.Raw || 
                placedIngredient.currentState == Ingredient.IngredientState.Processed)
            {
                if (progressBar != null)
                {
                    progressBar.Show();
                    progressBar.SetColor(progressBar.cookingColor);
                    progressBar.UpdateProgress(timer, processTime);
                }

                if (timer >= processTime)
                {
                    placedIngredient.SetState(Ingredient.IngredientState.Cooked);
                    Debug.Log($"【{gameObject.name}】食材烹飪完成 (煮熟了)！");
                    timer = 0f;
                    isBurntTimerActive = true;
                }
            }
            // 狀況 2：食物熟了，如果不拿走會開始計時燒焦 (Cooked ➔ Burnt)
            else if (isBurntTimerActive && placedIngredient.currentState == Ingredient.IngredientState.Cooked)
            {
                if (progressBar != null)
                {
                    progressBar.Show();
                    progressBar.SetColor(progressBar.burningColor);
                    progressBar.UpdateProgress(timer, burnTime);
                }

                if (timer >= burnTime)
                {
                    placedIngredient.SetState(Ingredient.IngredientState.Burnt);
                    Debug.LogWarning($"【{gameObject.name}】糟糕！食物燒焦了！");
                    isWorking = false;
                    isBurntTimerActive = false;
                    if (progressBar != null) progressBar.Hide();
                }
            }
        }
    }
}
