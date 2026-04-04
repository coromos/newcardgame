using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Photon.Pun;

public enum CardEffectType
{
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
}

// カード効果の処理を管理する汎用クラス
public abstract class CardEffect
{
    public CardController CSource;
    public CardController CTarget;
    public CardController CRef;

    public CardEffect()
    {
    }

    // 効果関連カードをセットするメソッド
    public void SetRefCards(CardController source, CardController target, CardController reference)
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
            GameManager.instance.CallDrawCard(CSource.model.PlayerCard);
        }
    }
}

public abstract class CEDamage : CardEffect
{
    public int damage;
    public int cardAmount;
    public CEDamage() : base() { }
    public override IEnumerator Activate()
    {
        yield return GameManager.instance.StartCoroutine(Damage(damage, cardAmount));
    }

    protected IEnumerator Damage(int dmg, int camt)
    {
        CardController[] allCards = UnityEngine.Object.FindObjectsByType<CardController>(FindObjectsSortMode.None);
        List<CardController> selectableCards = PickupCard(allCards.ToList(), cardPlaces: new string[] { "Field" }, isPlayerCard: false);

        // StartCardSelection は IEnumerator に変更済み -> 起動して完了を待つ
        yield return GameManager.instance.StartCoroutine(GameManager.instance.StartCardSelection(selectableCards, camt));
        List<CardController> targetCards = GameManager.instance.SelectionResults;

        if (targetCards != null)
        {
            for (int i = 0; i < targetCards.Count; i++)
            {
                GameManager.instance.CallDamageCard(targetCards[i], dmg);
            }
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
    public CEBuff() : base() { }
    public override IEnumerator Activate()
    {
        yield return GameManager.instance.StartCoroutine(Buff(buffth, buffpw, buffdv, cardAmount));
    }

    protected IEnumerator Buff(int buffth, int buffpw, int buffdv, int camt)
    {
        CardController[] allCards = UnityEngine.Object.FindObjectsByType<CardController>(FindObjectsSortMode.None);
        List<CardController> selectableCards = PickupCard(allCards.ToList(), cardPlaces: new string[] { "Field" }, isPlayerCard: true);
        yield return GameManager.instance.StartCoroutine(GameManager.instance.StartCardSelection(selectableCards, camt));
        List<CardController> targetCards = GameManager.instance.SelectionResults;
        if (targetCards != null)
        {
            for (int i = 0; i < targetCards.Count; i++)
            {
                GameManager.instance.CallBuffCard(targetCards[i], buffth, buffpw, buffdv);
            }
        }
        yield break;
    }
}