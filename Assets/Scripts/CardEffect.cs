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

    // カード効果の発動
    // ここではmethod名のみを指定して、実際の処理は継承クラスで実装する
    public abstract void Activate();

    // カード限定メソッド
    // 引数のカードリストから他の条件でカードを絞り込む
    public List<CardController> PickupCard(
        List<CardController> baseCardList,
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
            if (cardPlaces != null && System.Array.IndexOf(cardPlaces, cardController.model.fieldPosition) < 0)
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
    public override void Activate()
    {
        DrawCard(drawCount);
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
    public override void Activate()
    {
        Damage(damage, cardAmount);
    }
    protected void Damage(int dmg, int camt)
    {
        CardController[] allCards = UnityEngine.Object.FindObjectsByType<CardController>(FindObjectsSortMode.None);
        List<CardController> selectableCards = PickupCard(allCards.ToList());
        List<CardController> targetCards = GameManager.instance.StartCardSelection(selectableCards, camt);

        for (int i = 0; i < targetCards.Count; i++)
        {
            GameManager.instance.CallDamageCard(targetCards[i], dmg);
        }
    }
}

public abstract class CEBuff : CardEffect
{
    public int[] buff;
    public int cardAmount;
    public CEBuff() : base() { }
    public override void Activate()
    {
        Buff(buff, cardAmount);
    }

    protected void Buff(int[] buff, int camt)
    {
        CardController[] allCards = UnityEngine.Object.FindObjectsByType<CardController>(FindObjectsSortMode.None);
        List<CardController> selectableCards = PickupCard(allCards.ToList());
        List<CardController> targetCards = GameManager.instance.StartCardSelection(selectableCards, camt);
        for (int i = 0; i < targetCards.Count; i++)
        {
            GameManager.instance.CallBuffCard(targetCards[i], buff[0], buff[1], buff[2]);
        }
    }
}