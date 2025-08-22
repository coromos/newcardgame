using UnityEngine;
using UnityEngine.EventSystems;

// リーダー（プレイヤー本体）が攻撃対象としてドロップされた時の処理を管理するクラス
public class AttackedLeader : MonoBehaviour, IDropHandler
{
    // カードがリーダーにドロップされた時に呼ばれる
    public void OnDrop(PointerEventData eventData)
    {
        // ドロップされたカード（攻撃側）を取得
        CardController devoteCard = eventData.pointerDrag.GetComponent<CardController>();

        // ゲームマネージャーにリーダーへの攻撃処理を依頼
        GameManager.instance.DevoteToLeader(devoteCard);
    }
}