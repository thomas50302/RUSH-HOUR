public interface IInteractable
{
    /// <summary>
    /// 當玩家按下互動鍵 (F 或 Space) 且面對此物件時觸發
    /// </summary>
    /// <param name="player">發起互動的玩家控制器</param>
    void Interact(PlayerControll player);
}
