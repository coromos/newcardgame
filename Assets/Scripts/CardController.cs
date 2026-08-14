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
    public GameManager gameManager; // ゲームマネージャーへの参照
    public int cardInsID;
    public bool canSelect = false;

    // 破壊処理の完了待ちを示すフラグ（破壊通知後、最終化されるまでは true）
    public bool pendingDestroy = false;

    // コンポーネントの初期化
    private void Awake()
    {
        view = GetComponent<CardView>();
        movement = GetComponent<CardMovement>();
        if (GameManager.instance != null)
        {
            gameManager = GameManager.instance;
        }
        else
        {
            Debug.LogError("GameManager instance is not found.");
        }
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
    // - ここでは「論理的破壊」（fieldPosition の更新、破壊通知、視覚的移動等）を行う。
    // - cardInsID のクリア（最終化）は GameManager 側のバッチ最終化に委ねる（pendingDestroy フラグで管理）。
    public void DestroyCard(CardController card)
    {
        // まずフィールド位置を Trash にし、他の効果が参照可能な状態にする
        model.fieldPosition = PlaceList.Trash.ToString();

        // 破壊が進行中であることを示すフラグを立てる
        pendingDestroy = true;

        // 破壊時効果を GameManager に通知して効果キューへ登録する
        gameManager.NotifyCardDestroyed(this, card);

        // オブジェクトを待機場所へ移動（視覚的な移動はローカルで行う）
        Transform setplace = gameManager != null ? gameManager.GetComponent<Transform>() : this.transform.root;
        transform.SetParent(setplace, false);

        gameManager.UseCardEffect(this, this, CardEffectType.AnyExist);

        // cardInsID はここではクリアしない（効果実行が完了してから GameManager 側でクリアする）
    }

    // ダメージが耐久値を超えた場合にカードを破棄する
    public void DamageDestroy(CardController card)
    {
        if (model.damage >= model.toughness)
        {
            DestroyCard(card);
        }
    }

    // カードをフィールドに配置した時の処理
    public void DropField()
    {
        model.fieldPosition = "Field";
        model.canAttack = model.earlier;
        model.canITF = model.interference;
        view.SetCanUsePanel(model.canAttack);
    }

    // Graceカードの使用処理
    public void UseGrace()
    {
        DestroyCard(null);
    }
}