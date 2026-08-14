using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Photon.Pun;

public enum CardEffectType
{
    TurnStart,
    TurnEnd,
    Alive,
    Attack,
    Devote,
    Battle,
    Trash,
    Grace,
    AnyAlive,
    AnyTrash,
    AnyAttack,
    AnyBattle,
    AnyDevote,
    AnyGrace,
    AnyExist,
}

// カード効果の処理を管理する汎用クラス
public abstract class CardEffect
{
    public CardController CSource;
    public CardController CTarget;
    public CardController CRef;

    public GameManager gameManager;

    // --- カード絞り込み用プロパティ（共通化） ---
    // デフォルトは既存の挙動を壊さないように安定した値を設定しています。
    // 派生クラスで必要に応じて上書きしてください。
    public bool isPlayerCard = true;
    public string[] cardPlaces = null;
    public CardCategory[] cardCategory = null;
    public CardAttribute[] cardAttribute = null;
    public CardStain[] cardStain = null;
    public int? minCost = null;
    public int? maxCost = null;
    public int[] listCost = null;
    public int? minToughness = null;
    public int? maxToughness = null;
    public int? minPower = null;
    public int? maxPower = null;
    public int? minDevote = null;
    public int? maxDevote = null;
    public int? minDamage = null;
    public int? maxDamage = null;
    public bool? canAttack = null;
    // ---------------------------------------------

    public CardEffect()
    {
        if (GameManager.instance != null)
        {
            gameManager = GameManager.instance;
        }
        else
        {
            Debug.LogError("GameManager instance is not found.");
        }
    }

    // 効果関連カードをセットするメソッド
    public virtual void SetRefCards(CardController source, CardController target, CardController reference)
    {
        CSource = source;
        CTarget = target;
        CRef = reference;
    }

    // カード効果の発動（コルーチン化）
    // 派生クラスは IEnumerator を返すように変更する
    public abstract IEnumerator Activate();

    // カード限定メソッド
    // 引数のカードリストから他の条件でカードを絞り込む
    public List<CardController> PickupCard(
        List<CardController> baseCardList,
        bool isPlayerCard = true,
        string[] cardPlaces = null,
        CardCategory[] cardCategory = null,
        CardAttribute[] cardAttribute = null,
        CardStain[] cardStain = null,
        int? minCost = null,
        int? maxCost = null,
        int[] listCost = null,
        int? minToughness = null,
        int? maxToughness = null,
        int? minPower = null,
        int? maxPower = null,
        int? minDevote = null,
        int? maxDevote = null,
        int? minDamage = null,
        int? maxDamage = null,
        bool? canAttack = null
        )
    {
        List<CardController> selectedCards = new List<CardController>();
        foreach (var cardController in baseCardList)
        {
            if (cardController == null || cardController.model == null)
                continue;
            else if (cardController.model.PlayerCard != isPlayerCard)
                continue;
            else if (cardPlaces != null && System.Array.IndexOf(cardPlaces, cardController.model.fieldPosition) < 0)
                continue;
            else if (cardCategory != null && System.Array.IndexOf(cardCategory, cardController.model.cardCategory) < 0)
                continue;
            else if (cardAttribute != null && System.Array.IndexOf(cardAttribute, cardController.model.cardAttribute) < 0)
                continue;
            else if (minCost.HasValue && cardController.model.cost < minCost.Value)
                continue;
            else if (maxCost.HasValue && cardController.model.cost > maxCost.Value)
                continue;
            else if (listCost != null && System.Array.IndexOf(listCost, cardController.model.cost) < 0)
                continue;
            else if (minToughness.HasValue && cardController.model.toughness < minToughness.Value)
                continue;
            else if (maxToughness.HasValue && cardController.model.toughness > maxToughness.Value)
                continue;
            else if (minPower.HasValue && cardController.model.power < minPower.Value)
                continue;
            else if (maxPower.HasValue && cardController.model.power > maxPower.Value)
                continue;
            else if (minDevote.HasValue && cardController.model.devote < minDevote.Value)
                continue;
            else if (maxDevote.HasValue && cardController.model.devote > maxDevote.Value)
                continue;
            else if (minDamage.HasValue && cardController.model.damage < minDamage.Value)
                continue;
            else if (maxDamage.HasValue && cardController.model.damage > maxDamage.Value)
                continue;
            else if (canAttack.HasValue && cardController.model.canAttack != canAttack.Value)
                continue;
            else if (cardStain != null)
            {
                bool stainMatch = false;
                foreach (var stain in cardStain)
                {
                    if (System.Array.IndexOf(cardController.model.cardStains, stain) >= 0)
                    {
                        stainMatch = true;
                        break;
                    }
                }
                if (!stainMatch)
                    continue;
            }
            selectedCards.Add(cardController);
        }
        return selectedCards;
    }
}

public abstract class CEExist : CardEffect
{
    public CEExist() : base() { }

    public override void SetRefCards(CardController source, CardController target, CardController reference)
    {
        base.SetRefCards(source, target, reference);
    }

    public override IEnumerator Activate()
    {
        // ターゲットがこのカードで場にあるとき効果適応
        if (CSource == CRef)
        {
            yield return GameManager.instance.StartCoroutine(Destroyed(CSource));
        }
        else if (CSource == CTarget)
        {
            yield return GameManager.instance.StartCoroutine(IntoField(CSource));
        }
        else
        {
            yield return GameManager.instance.StartCoroutine(AffectOther(CTarget));
        }
    }

    // ソースカードが場に出たときの処理
    public virtual IEnumerator IntoField(CardController destroyedCard)
    {
        yield break;
    }

    // ソースカードが破壊されたときの処理
    public virtual IEnumerator Destroyed(CardController destroyedCard)
    {
        yield break;
    }

    // 他のカードに影響する処理
    public virtual IEnumerator AffectOther(CardController affectedCard)
    {
        yield break;
    }
}

public abstract class CEDraw : CardEffect
{
    public int drawCount;

    public CEDraw() : base() { }
    public override IEnumerator Activate()
    {
        DrawCard(drawCount);
        yield break;
    }

    protected void DrawCard(int count)
    {
        // ドロー処理を実装
        for (int i = 0; i < count; i++)
        {
            // プレイヤーのデッキからカードを引いて手札に加える処理を実装
            gameManager.CallDrawCard(CSource.model.PlayerCard);
        }
    }
}

public abstract class CEDamage : CardEffect
{
    public int damage;
    public int cardAmount;

    public CEDamage() : base()
    {
        // 既存の CEDamage の既定動作を維持（相手のフィールドを対象）
        isPlayerCard = false;
        cardPlaces = new string[] { "Field" };
    }
    public override IEnumerator Activate()
    {
        yield return GameManager.instance.StartCoroutine(Damage(damage, cardAmount));
    }

    protected IEnumerator Damage(int dmg, int camt)
    {
        CardController[] allCards = UnityEngine.Object.FindObjectsByType<CardController>(FindObjectsSortMode.None);
        List<CardController> selectableCards = PickupCard(
            allCards.ToList(),
            isPlayerCard: isPlayerCard,
            cardPlaces: cardPlaces,
            cardCategory: cardCategory,
            cardAttribute: cardAttribute,
            cardStain: cardStain,
            minCost: minCost,
            maxCost: maxCost,
            listCost: listCost,
            minToughness: minToughness,
            maxToughness: maxToughness,
            minPower: minPower,
            maxPower: maxPower,
            minDevote: minDevote,
            maxDevote: maxDevote,
            minDamage: minDamage,
            maxDamage: maxDamage,
            canAttack: canAttack
        );

        // StartCardSelection は IEnumerator に変更済み -> 起動して完了を待つ
        yield return gameManager.StartCoroutine(gameManager.StartCardSelection(selectableCards, camt));
        List<CardController> targetCards = gameManager.SelectionResults;

        if (targetCards != null)
        {
            for (int i = 0; i < targetCards.Count; i++)
            {
                gameManager.CallDamageCard(targetCards[i], dmg);
            }
        }
        yield break;
    }
}

public abstract class CERandomDamage : CEDamage
{
    public CERandomDamage() : base() { }

    public override IEnumerator Activate()
    {
        yield return gameManager.StartCoroutine(RandomDamage(damage, cardAmount));
    }

    // 引数にはダメージ量と対象数のみ。フィルタはクラスプロパティから PickupCard に渡す。
    protected IEnumerator RandomDamage(int dmg, int camt)
    {
        if (camt <= 0)
            yield break;

        CardController[] allCards = UnityEngine.Object.FindObjectsByType<CardController>(FindObjectsSortMode.None);
        List<CardController> selectable = PickupCard(
            allCards.ToList(),
            isPlayerCard: isPlayerCard,
            cardPlaces: cardPlaces,
            cardCategory: cardCategory,
            cardAttribute: cardAttribute,
            cardStain: cardStain,
            minCost: minCost,
            maxCost: maxCost,
            listCost: listCost,
            minToughness: minToughness,
            maxToughness: maxToughness,
            minPower: minPower,
            maxPower: maxPower,
            minDevote: minDevote,
            maxDevote: maxDevote,
            minDamage: minDamage,
            maxDamage: maxDamage,
            canAttack: canAttack
        );

        if (selectable == null || selectable.Count == 0)
            yield break;

        int toSelect = Mathf.Min(camt, selectable.Count);
        for (int i = 0; i < toSelect; i++)
        {
            int idx = UnityEngine.Random.Range(0, selectable.Count);
            var target = selectable[idx];
            gameManager.CallDamageCard(target, dmg);
            // 重複選択を避けるため除去
            selectable.RemoveAt(idx);
            // アニメーション等の余裕を持たせるため1フレーム待機
            yield return null;
        }

        yield break;
    }
}

public abstract class CEBuff : CardEffect
{
    public int buffth;
    public int buffpw;
    public int buffdv;
    public int cardAmount;

    public CEBuff() : base()
    {
        // 既存の CEBuff の既定動作を維持（自分のフィールドを対象）
        isPlayerCard = true;
        cardPlaces = new string[] { "Field" };
    }

    public override IEnumerator Activate()
    {
        yield return gameManager.StartCoroutine(Buff(buffth, buffpw, buffdv, cardAmount));
    }

    protected IEnumerator Buff(int buffth, int buffpw, int buffdv, int camt)
    {
        CardController[] allCards = UnityEngine.Object.FindObjectsByType<CardController>(FindObjectsSortMode.None);
        List<CardController> selectableCards = PickupCard(
            allCards.ToList(),
            isPlayerCard: isPlayerCard,
            cardPlaces: cardPlaces,
            cardCategory: cardCategory,
            cardAttribute: cardAttribute,
            cardStain: cardStain,
            minCost: minCost,
            maxCost: maxCost,
            listCost: listCost,
            minToughness: minToughness,
            maxToughness: maxToughness,
            minPower: minPower,
            maxPower: maxPower,
            minDevote: minDevote,
            maxDevote: maxDevote,
            minDamage: minDamage,
            maxDamage: maxDamage,
            canAttack: canAttack
        );

        yield return gameManager.StartCoroutine(gameManager.StartCardSelection(selectableCards, camt));
        List<CardController> targetCards = gameManager.SelectionResults;
        if (targetCards != null)
        {
            for (int i = 0; i < targetCards.Count; i++)
            {
                gameManager.CallBuffCard(targetCards[i], buffth, buffpw, buffdv);
            }
        }
        yield break;
    }
}