using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// カードのゲーム内挙動を管理するクラス
public class CardController : MonoBehaviourPun
{
    public CardView view; // カードの見た目を管理
    public CardModel model; // カードのデータを管理
    public CardMovement movement;  // カードの移動・ドラッグ操作を管理

    // コンポーネントの初期化
    private void Awake()
    {
        view = GetComponent<CardView>();
        movement = GetComponent<CardMovement>();
    }

    // カードの初期化（データと見た目のセットアップ）
    public void Init(int cardID, bool playerCard) 
    {
        model = new CardModel(cardID, playerCard);
        view.Show(model);
    }

    // カードにダメージを与える
    public void GrantDamage(int damage)
    {
        model.damage += damage;
        view.Show(model);
    }

    // カードを破棄する
    public void DestroyCard()
    {
        Destroy(this.gameObject);
    }

    // ダメージが耐久値を超えた場合にカードを破棄する
    public void DamageDestroy()
    {
        if (model.damage >= model.toughness)
        {
            DestroyCard();
        }
    }

    // カードをフィールドに配置した時の処理
    public void DropField()
    {
        GameManager.instance.ReduceManaPoint(model.cost);
        model.onField = true;
        model.canUse = false;
        view.SetCanUsePanel(model.canUse);
    }

    // Graceカードの使用処理
    public void UseGrace()
    {
        GameManager.instance.ReduceManaPoint(model.cost);
    }
}