using UnityEngine;
using UnityEngine.EventSystems;

// フィールド上のカードが攻撃対象としてドロップされた時の処理を管理するクラス
public class AttackedCard : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    // 攻撃カードがこのカードにドロップされた時に呼ばれる
    public void OnDrop(PointerEventData eventData)
    {
        // 攻撃側カード（ドラッグしてきたカード）を取得
        CardController attackCard = eventData.pointerDrag.GetComponent<CardController>();

        // 防御側カード（このカード自身）を取得
        CardController defenceCard = GetComponent<CardController>();

        // ゲームマネージャーにバトル処理を依頼
        GameManager.instance.CardBattle(attackCard, defenceCard);
    }

    //　カード選択モード時にクリックされた場合の処理
    public void OnPointerClick(PointerEventData eventData)
    {
        CardController card = GetComponent<CardController>();

        if (card.model.canSelect)
        {
            GameManager.instance.SetSelectedCard(card);
        }
    }
}