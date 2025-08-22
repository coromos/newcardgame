using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// フィールドにカードを配置するためのドロップ処理を管理するクラス
public class DropPlace : MonoBehaviour, IDropHandler
{
    // カードがフィールドにドロップされた時に呼ばれる
    public void OnDrop(PointerEventData eventData)
    {
        // ドロップされたカードを取得
        CardController card = eventData.pointerDrag.GetComponent<CardController>();

        // 配置可能なカードか判定（Animaカードのみ配置可能）
        if (card != null && card.model.canUse && card.model.cardCategory == CardCategory.Anima)
        {
            // カードの親Transformを更新（フィールドに配置）
            card.movement.cardParent = this.transform;
            // カード効果を発動
            GameManager.instance.UseCardEffect(card, CardEffectType.Alive);
        }
    }
}