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
    public CardEffectManager cem; // カード効果の管理クラスへの参照
    public int cardInsID;
    public bool canSelect = false;

    // コンポーネントの初期化
    private void Awake()
    {
        view = GetComponent<CardView>();
        movement = GetComponent<CardMovement>();
    }

    // カードの初期化（データと見た目のセットアップ）
    public void Init(int cardID, bool playerCard, int cardIns, string fieldPosition = "Deck")
    {
        cardInsID = cardIns;

        model = new CardModel(cardID, playerCard, fieldPosition);

        System.Type type = System.Type.GetType("CEM" + cardID);
        if (type != null)
        {
            cem = System.Activator.CreateInstance(type) as CardEffectManager;
        }
        else
        {
            cem = new CardEffectManager();
        }

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
        cardInsID = 0;
        Transform setplace = GameManager.instance.GetComponent<Transform>();
        transform.SetParent(setplace, false);
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
        model.fieldPosition = "Field";
        model.canAttack = model.earlier;
        view.SetCanUsePanel(model.canAttack);
    }

    // Graceカードの使用処理
    public void UseGrace()
    {
        DestroyCard();
    }
}