using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

// カードのドラッグ・移動・アニメーションを管理するクラス
public class CardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Transform cardParent; // ドラッグ前の親Transform
    CardController card;         // カードのコントローラー
    bool canDrag = true;         // ドラッグ可能かどうか

    // ドラッグ開始時の処理
    public void OnBeginDrag(PointerEventData eventData)
    {
        card = GetComponent<CardController>();
        canDrag = true;

        // フィールド上のカードは攻撃可能な場合のみドラッグ可能
        if (card.model.onField)
        {
            if (!card.model.canAttack)
            {
                canDrag = false;
            }
        }
        // 手札のカードは使用可能な場合のみドラッグ可能
        else
        {
            if (!card.model.canUse)
            {
                canDrag = false;
            }
        }

        // ドラッグ不可の場合は処理終了
        if (!canDrag)
        {
            return;
        }

        // 親Transformを退避し、Canvas上に移動
        cardParent = transform.parent;
        transform.SetParent(cardParent.parent, false);
        GetComponent<CanvasGroup>().blocksRaycasts = false;

        // Graceカードの場合は専用パネルを表示
        if (card.model.cardCategory == CardCategory.Grace)
        {
            UIManager.instance.SetUseGracePanel(true);
        }
    }

    // ドラッグ中の処理（カードをマウス位置に追従させる）
    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag)
        {
            return;
        }

        transform.position = eventData.position;
    }

    // ドラッグ終了時の処理
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canDrag)
        {
            return;
        }

        // 元の親Transformに戻す
        transform.SetParent(cardParent, false);
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        // Graceカードの場合は専用パネルを非表示
        if (card.model.cardCategory == CardCategory.Grace)
        {
            UIManager.instance.SetUseGracePanel(false);
        }
    }

    // 攻撃アニメーション（カードが攻撃対象に移動して戻る演出）
    public IEnumerator AttackMotion(Transform target)
    {
        Vector3 currentPosition = transform.position;
        cardParent = transform.parent;

        // Canvas上に移動
        transform.SetParent(cardParent.parent);

        // 攻撃対象へ移動
        transform.DOMove(target.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        // 元の位置へ戻る
        transform.DOMove(currentPosition, 0.25f);
        yield return new WaitForSeconds(0.25f);

        // 親Transformを元に戻す
        transform.SetParent(cardParent);
    }

    // マウスオーバー時の拡大表示
    public void OnMouseEnter()
    {
        Debug.Log("In");
        Vector3 enpow = new Vector3(1.2f, 1.2f, 1f);
        transform.DOScale(enpow, 0.1f);
    }

    // マウスアウト時の縮小表示
    public void OnMouseExit()
    {
        Debug.Log("Out");
        Vector3 defaultscale = new Vector3(1f, 1f, 1f);
        transform.DOScale(defaultscale, 0.1f);
    }
}