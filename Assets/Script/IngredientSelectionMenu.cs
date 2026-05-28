using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientSelectionMenu : MonoBehaviour
{
    public static IngredientSelectionMenu Instance { get; private set; }

    [Header("UI 面板組件")]
    [Tooltip("選單的主面板 GameObject")]
    public GameObject menuPanel;

    [Tooltip("選單標題 Text")]
    public Text titleText;

    [Tooltip("放置各個選項按鈕/文字的容器 (例如具有 Vertical/Horizontal Layout Group 的物件)")]
    public Transform optionsContainer;

    [Tooltip("單一選項的 Prefab (底下需包含 Text 組件)")]
    public GameObject optionPrefab;

    private PlayerControll activePlayer;
    private List<GameObject> availablePrefabs;
    private Action<GameObject> onSelectedCallback;

    private int selectedIndex = 0;
    private List<Text> optionTexts = new List<Text>();
    private float inputCooldown = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 開啟食材選擇選單
    /// </summary>
    public void OpenMenu(PlayerControll player, List<GameObject> prefabs, string title, Action<GameObject> callback)
    {
        activePlayer = player;
        availablePrefabs = prefabs;
        onSelectedCallback = callback;
        selectedIndex = 0;

        // 鎖定玩家的輸入與移動
        player.isInputLocked = true;

        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            if (titleText != null) titleText.text = title;

            // 清空舊的選項項目
            foreach (Transform child in optionsContainer)
            {
                Destroy(child.gameObject);
            }
            optionTexts.Clear();

            // 建立新的選項項目
            for (int i = 0; i < prefabs.Count; i++)
            {
                GameObject optObj = Instantiate(optionPrefab, optionsContainer);
                Text txt = optObj.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    Ingredient ing = prefabs[i].GetComponent<Ingredient>();
                    txt.text = ing != null ? ing.ingredientName : prefabs[i].name;
                    optionTexts.Add(txt);
                }
            }

            // 更新選取項目的視覺回饋
            UpdateSelectionVisuals();
        }
        else
        {
            // Fallback：若沒有設定 UI，直接印出警告並自動選擇第一個
            Debug.LogWarning("[選單系統] 未設定 UI 面板！自動選取第一個食材。");
            ConfirmSelection(prefabs[0]);
        }
    }

    private void Update()
    {
        // 只有在選單開啟且有玩家在互動時才運作
        if (activePlayer == null || menuPanel == null || !menuPanel.activeSelf) return;

        // 處理輸入冷卻時間
        if (inputCooldown > 0f) inputCooldown -= Time.deltaTime;

        PlayerInput playerInput = activePlayer.GetComponent<PlayerInput>();
        if (playerInput == null) return;

        float horizontalInput = playerInput.Horizontal;

        // 透過左右移動來切換選單選項
        if (inputCooldown <= 0f)
        {
            if (horizontalInput > 0.5f)
            {
                selectedIndex = (selectedIndex + 1) % availablePrefabs.Count;
                UpdateSelectionVisuals();
                inputCooldown = 0.2f; // 設定 0.2 秒冷卻
            }
            else if (horizontalInput < -0.5f)
            {
                selectedIndex = (selectedIndex - 1 + availablePrefabs.Count) % availablePrefabs.Count;
                UpdateSelectionVisuals();
                inputCooldown = 0.2f;
            }
        }

        // 按下互動鍵確認選擇
        if (playerInput.InteractPressed)
        {
            ConfirmSelection(availablePrefabs[selectedIndex]);
        }
    }

    private void UpdateSelectionVisuals()
    {
        for (int i = 0; i < optionTexts.Count; i++)
        {
            if (optionTexts[i] != null)
            {
                if (i == selectedIndex)
                {
                    // 被選取的選項：黃色、大字體
                    optionTexts[i].color = Color.yellow;
                    optionTexts[i].fontStyle = FontStyle.Bold;
                    optionTexts[i].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                }
                else
                {
                    // 未選取的選項：白色、一般字體
                    optionTexts[i].color = Color.white;
                    optionTexts[i].fontStyle = FontStyle.Normal;
                    optionTexts[i].transform.localScale = Vector3.one;
                }
            }
        }
    }

    private void ConfirmSelection(GameObject chosenPrefab)
    {
        // 解鎖玩家輸入
        activePlayer.isInputLocked = false;
        activePlayer = null;

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        // 執行回呼函式，將選擇的食材交給玩家
        onSelectedCallback?.Invoke(chosenPrefab);
    }
}
