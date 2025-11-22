using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// カードのデータ構造を定義するクラス
public class CardModel
{
    // 基本情報
    public int cardId;
    public string name;
    public CardCategory cardCategory;
    public CardAttribute cardAttribute;
    public CardStain[] cardStains;
    public int cost;
    public int toughness;
    public int power;
    public int devote;
    public Sprite icon;

    public string CardType;
    public int SelfTypeSet;
    public int AnyTypeSet;

    // 状態管理
    public bool canUse = false;
    public bool onField = false;
    public bool canAttack = true;
    public bool PlayerCard = true;

    // 初期能力
    public bool earlier = false;

    // ScriptableObject 側で定義されている初期能力（CardEntity からコピー）
    public bool interference = false;
    public bool stealth = false;
    public bool strong = false;
    public bool flying = false;
    public bool homeostasis = false;
    public bool alone = false;
    public bool noaction = false;

    // フィールド効果・状態異常
    public int flnCost = 0;
    public int flnToughness = 0;
    public int flnPower = 0;
    public int flnDevote = 0;
    public int damage = 0;
    public int stayturn = 0;
    public int Curse = -1;
    public int Poison = 0;

    // コンストラクタ：ScriptableObjectからデータを取得
    public CardModel(int cardID, bool playerCard)
    {
        CardEntity cardEntity = Resources.Load<CardEntity>("CardEntityList/Card" + cardID);

        if (cardEntity == null)
        {
            Debug.LogError($"CardEntity not found: Card{cardID}");
            return;
        }

        cardId = cardEntity.cardId;

        cardCategory = cardEntity.cardCategory;
        cardAttribute = cardEntity.cardAttribute;
        cardStains = cardEntity.cardStains;
        name = cardEntity.name;
        cost = cardEntity.cost;
        toughness = cardEntity.toughness;
        power = cardEntity.power;
        devote = cardEntity.devote;

        icon = cardEntity.icon;

        // ScriptableObject 側の初期能力をモデルへコピー
        earlier = cardEntity.earlier;
        interference = cardEntity.interference;
        stealth = cardEntity.stealth;
        strong = cardEntity.strong;
        flying = cardEntity.flying;
        homeostasis = cardEntity.homeostasis;
        alone = cardEntity.alone;
        noaction = cardEntity.noaction;

        PlayerCard = playerCard;
    }
}