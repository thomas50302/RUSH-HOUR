using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerControll : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("移動速度")]
    public float moveSpeed = 5f;

    [Header("持有物錨點")]
    [Tooltip("角色拿取物品時，物品要放置的位置")]
    public Transform holdPoint;

    [HideInInspector]
    public bool isInputLocked = false; // 用來鎖定玩家輸入 (例如在開啟選單時)

    private Rigidbody2D rb2d;
    private PlayerInput playerInput;
    private Vector2 faceDirection = Vector2.down; // 預設面向下方
    private GameObject carriedObject = null;      // 目前手上拿著的物件

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        // 自動進行物理設定，防止滑動與旋轉
        rb2d.gravityScale = 0f;
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // Update is called once per frame
    void Update()
    {
        // 如果輸入被鎖定，停止移動並跳過更新
        if (isInputLocked)
        {
            if (rb2d != null) rb2d.velocity = Vector2.zero;
            return;
        }

        // 取得該玩家的輸入
        float horizontal = playerInput.Horizontal;
        float vertical = playerInput.Vertical;

        // 計算移動向量
        Vector2 movement = new Vector2(horizontal, vertical);
        if (movement.magnitude > 0.1f)
        {
            movement.Normalize();
            faceDirection = movement; // 更新角色面朝的方向
        }

        // 使用物理速度移動
        rb2d.velocity = movement * moveSpeed;

        // 簡單旋轉角色 Sprite 來表示方向 (在 2D 俯視角中常用)
        if (movement.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(faceDirection.y, faceDirection.x) * Mathf.Rad2Deg - 90f; // -90 是因為預設頭朝上
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 偵測互動
        if (playerInput.InteractPressed)
        {
            TryInteract();
        }
    }

    /// <summary>
    /// 嘗試與面前的物件進行互動
    /// </summary>
    void TryInteract()
    {
        float interactDistance = 1.0f; // 互動探測距離
        Vector2 checkPos = (Vector2)transform.position + faceDirection * interactDistance;

        // 畫出綠色射線以便在 Scene 視窗中偵測與除錯 (如果需要除錯，可以取消註解這行)
        // Debug.DrawRay(transform.position, faceDirection * interactDistance, Color.green, 1f);

        // 偵測前方半徑 0.4 的圓形範圍內的碰撞體
        Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.4f);
        if (hit != null && hit.gameObject != gameObject)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log($"[{gameObject.name}] 與 [{hit.gameObject.name}] 進行互動！");
                interactable.Interact(this);
            }
        }
    }

    #region 手持物品系統 (Carrying System)

    /// <summary>
    /// 檢查玩家手上是否有拿東西
    /// </summary>
    public bool IsCarrying()
    {
        return carriedObject != null;
    }

    /// <summary>
    /// 取得目前手上拿著的物件
    /// </summary>
    public GameObject GetCarriedObject()
    {
        return carriedObject;
    }

    /// <summary>
    /// 讓玩家拿起一個物件
    /// </summary>
    public void Carry(GameObject obj)
    {
        if (carriedObject != null)
        {
            Debug.LogWarning("手上已經有東西了，無法再拿取！");
            return;
        }

        carriedObject = obj;

        // 將物品的 Parent 設為 holdPoint (如果沒有設定就設為玩家自己)
        if (holdPoint != null)
        {
            obj.transform.SetParent(holdPoint);
            obj.transform.localPosition = Vector3.zero;
        }
        else
        {
            obj.transform.SetParent(transform);
            obj.transform.localPosition = new Vector3(0f, 0.5f, 0f); // 預設放在前方一點點
        }
        obj.transform.localRotation = Quaternion.identity;

        // 關閉被拿取物品的碰撞體，以免阻擋玩家移動
        Collider2D objCollider = obj.GetComponent<Collider2D>();
        if (objCollider != null)
        {
            objCollider.enabled = false;
        }
    }

    /// <summary>
    /// 丟下/放下手上的物品
    /// </summary>
    /// <returns>回傳放下的物品物件</returns>
    public GameObject Drop()
    {
        if (carriedObject == null) return null;

        GameObject droppedObj = carriedObject;
        carriedObject = null;

        // 解除 Parent 綁定
        droppedObj.transform.SetParent(null);

        // 重新開啟物品的碰撞體
        Collider2D objCollider = droppedObj.GetComponent<Collider2D>();
        if (objCollider != null)
        {
            objCollider.enabled = true;
        }

        return droppedObj;
    }

    #endregion
}
