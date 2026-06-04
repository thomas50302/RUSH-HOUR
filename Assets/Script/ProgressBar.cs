using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("UI 元件")]
    [Tooltip("用於顯示進度的 Slider (如果使用 Slider)")]
    public Slider slider;
    [Tooltip("用於顯示進度的 Fill Image (如果使用 Filled 圖片)")]
    public Image fillImage;

    [Header("顏色預設值")]
    [Tooltip("一般做菜/進步時的進度條顏色")]
    public Color cookingColor = Color.green;
    [Tooltip("警告/燒焦警告時的進度條顏色")]
    public Color burningColor = Color.red;

    void Start()
    {
        Hide();
    }

    /// <summary>
    /// 更新進度條比例值 (current / max) 並限縮在 0~1 之間
    /// </summary>
    public void UpdateProgress(float current, float max)
    {
        float ratio = 0f;
        if (max > 0.001f)
        {
            ratio = Mathf.Clamp01(current / max);
        }

        if (slider != null)
        {
            slider.value = ratio;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = ratio;
        }
    }

    /// <summary>
    /// 設定進度條填充顏色
    /// </summary>
    public void SetColor(Color color)
    {
        if (slider != null && slider.fillRect != null)
        {
            Image fill = slider.fillRect.GetComponent<Image>();
            if (fill != null)
            {
                fill.color = color;
            }
        }

        if (fillImage != null)
        {
            fillImage.color = color;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
