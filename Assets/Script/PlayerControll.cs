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
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        // 自動進行物理設定，防止滑動與旋轉
        rb2d.gravityScale = 0f;
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.rotation = Quaternion.identity; // 確保角色角度重置為預設 (無旋轉)
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


        // 偵測互動
        if (playerInput.InteractPressed)
        {
            TryInteract();
        }

        // 偵測切換與選擇食材 (E / Slash / RightShift)
        if (playerInput.CyclePressed)
        {
            TryCycleInteract();
        }
    }

    /// <summary>
    /// 嘗試與面前的物件進行互動
    /// </summary>
    void TryInteract()
    {
        // 清除已被銷毀物件的 Null 參照，防止報錯
        nearbyInteractables.RemoveAll(item => item == null || (item is MonoBehaviour mb && mb == null));

        IInteractable target = null;

        // 1. 優先偵測前方半徑 0.4 的圓形範圍內的碰撞體 (面向優先)
        float interactDistance = 1.0f; // 互動探測距離
        Vector2 checkPos = (Vector2)transform.position + faceDirection * interactDistance;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.4f);
        if (hit != null && hit.gameObject != gameObject)
        {
            target = hit.GetComponent<IInteractable>() ?? hit.GetComponentInParent<IInteractable>();
        }

        // 2. 備用：若前方無偵測到物件，則從身體碰觸的清單中選擇距離最近的
        if (target == null && nearbyInteractables.Count > 0)
        {
            float minDistance = float.MaxValue;
            foreach (var item in nearbyInteractables)
            {
                if (item != null && item is MonoBehaviour mb)
                {
                    float dist = Vector3.Distance(transform.position, mb.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        target = item;
                    }
                }
            }
        }

        // 3. 執行互動
        if (target != null)
        {
            Debug.Log($"[{gameObject.name}] 與 [{((MonoBehaviour)target).gameObject.name}] 進行按鍵互動！");
            target.Interact(this);
        }
    }

    /// <summary>
    /// 嘗試切換/選擇食材互動 (E 鍵)
    /// </summary>
    void TryCycleInteract()
    {
        // 排除已被銷毀物件的 Null 參照
        nearbyInteractables.RemoveAll(item => item == null || (item is MonoBehaviour mb && mb == null));

        Container targetContainer = null;

        // 1. 優先偵測前方範圍內的 Container
        float interactDistance = 1.0f;
        Vector2 checkPos = (Vector2)transform.position + faceDirection * interactDistance;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.4f);
        if (hit != null && hit.gameObject != gameObject)
        {
            targetContainer = hit.GetComponent<Container>() ?? hit.GetComponentInParent<Container>();
        }

        // 2. 備用：從碰觸到的清單中選擇 Container
        if (targetContainer == null && nearbyInteractables.Count > 0)
        {
            foreach (var item in nearbyInteractables)
            {
                if (item is Container c)
                {
                    targetContainer = c;
                    break;
                }
            }
        }

        // 3. 執行切換食材選擇
        if (targetContainer != null)
        {
            targetContainer.CycleIngredient(this);
        }
    }

    #region 身體碰撞與觸發區域登錄

    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>() ?? other.GetComponentInParent<IInteractable>();
        if (interactable != null && !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>() ?? other.GetComponentInParent<IInteractable>();
        if (interactable != null && nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Remove(interactable);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        IInteractable interactable = collision.gameObject.GetComponent<IInteractable>() ?? collision.gameObject.GetComponentInParent<IInteractable>();
        if (interactable != null && !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        IInteractable interactable = collision.gameObject.GetComponent<IInteractable>() ?? collision.gameObject.GetComponentInParent<IInteractable>();
        if (interactable != null && nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Remove(interactable);
        }
    }

    #endregion

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

        // 關閉被拿取物品的碰撞體，以免阻擋玩家移動 (支援子碰撞體)
        Collider2D objCollider = obj.GetComponent<Collider2D>() ?? obj.GetComponentInChildren<Collider2D>();
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

        // 重新開啟物品的碰撞體 (支援子碰撞體)
        Collider2D objCollider = droppedObj.GetComponent<Collider2D>() ?? droppedObj.GetComponentInChildren<Collider2D>();
        if (objCollider != null)
        {
            objCollider.enabled = true;
        }

        return droppedObj;
    }

    #endregion
}