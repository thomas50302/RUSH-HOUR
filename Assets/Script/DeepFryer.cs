using UnityEngine;

public class DeepFryer : BaseHeatingKitchenware
{
    protected override bool CanPlace(Ingredient ing)
    {
        // 接受生 (Raw) 或 處理過 (Processed) 的食材
        return ing.currentState == Ingredient.IngredientState.Raw || ing.currentState == Ingredient.IngredientState.Processed;
    }
}
