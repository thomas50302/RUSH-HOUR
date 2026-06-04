using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public enum IngredientState
    {
        Raw,       // 生的
        Processed, // 經處理的 (例如切好的、醃漬好的)
        Cooked,    // 煮熟的
        Burnt      // 烤焦/炒焦的
    }

    [Header("食材屬性")]
    [Tooltip("食材名稱，例如: Garlic, Chicken, Rice, Beef, Egg, Honey, SweetSoy, CoconutMilk, Tempeh, Flour")]
    public string ingredientName = "";
    
    [Tooltip("目前食材狀態")]
    public IngredientState currentState = IngredientState.Raw;

    [Header("除錯顏色 (無貼圖時的視覺回饋)")]
    public Color rawColor = Color.white;
    public Color processedColor = new Color(0.5f, 1f, 0.5f); // 淺綠色
    public Color cookedColor = new Color(1f, 0.8f, 0.2f);    // 偏黃橘色
    public Color burntColor = Color.black;                  // 黑色

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        if (string.IsNullOrEmpty(ingredientName))
        {
            string cleanedName = gameObject.name;
            cleanedName = System.Text.RegularExpressions.Regex.Replace(cleanedName, @"\s*\(Clone\)\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cleanedName = System.Text.RegularExpressions.Regex.Replace(cleanedName, @"\s*\(\d+\)\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            ingredientName = cleanedName.Trim();
        }
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    /// <summary>
    /// 更新食材的狀態，並同步改變視覺外觀
    /// </summary>
    public void SetState(IngredientState newState)
    {
        currentState = newState;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            switch (currentState)
            {
                case IngredientState.Raw:
                    spriteRenderer.color = rawColor;
                    break;
                case IngredientState.Processed:
                    spriteRenderer.color = processedColor;
                    break;
                case IngredientState.Cooked:
                    spriteRenderer.color = cookedColor;
                    break;
                case IngredientState.Burnt:
                    spriteRenderer.color = burntColor;
                    break;
            }
        }
    }

    public string GetStateName()
    {
        switch (currentState)
        {
            case IngredientState.Raw: return "生的";
            case IngredientState.Processed: return "處理過";
            case IngredientState.Cooked: return "熟的";
            case IngredientState.Burnt: return "燒焦的";
            default: return "";
        }
    }
}
