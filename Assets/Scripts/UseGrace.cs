using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Graceカードをフィールドにドロップした際の処理を管理するクラス
public class UseGrace : MonoBehaviour, IDropHandler
{
    // カードがドロップされた時に呼ばれる
    public void OnDrop(PointerEventData eventData)
    {
        // ドロップされたカードを取得
        CardController card = eventData.pointerDrag.GetComponent<CardController>();

        // 使用可能なカードか判定
        if (card != null && card.model.canUse)
        {
            Debug.Log("Use Grace");
            // Graceカードの効果を発動
            card.UseGrace();
            // カードを破棄
            card.DestroyCard();
            // UIパネルを非表示
            UIManager.instance.SetUseGracePanel(false);
        }
    }
}