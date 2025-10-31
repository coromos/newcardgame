using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Graceカードをフィールドにドロップした際の処理を管理するクラス
public class DropCard : MonoBehaviour, IDropHandler
{
    // カードがドロップされた時に呼ばれる
    public void OnDrop(PointerEventData eventData)
    {
        // ドロップされたカードを取得
        CardController card = eventData.pointerDrag.GetComponent<CardController>();

        // 使用可能なカードか判定
        if (card != null && card.model.canUse)
        {
            Debug.Log("Use Card");
            UIManager.instance.SetUseGracePanel(false);
            GameManager.instance.UseCardFromHand(card);
        }
    }
}