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
    public int cost;
    public int toughness;
    public int power;
    public int devote;
    public CardEffectType cardEffectType;
    public Sprite icon;

    public string CardType;

    // 状態管理
    public bool canUse = false;
    public bool onField = false;
    public bool canAttack = false;
    public bool PlayerCard = true;

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

        cardId = cardEntity.cardId;
        cardCategory = cardEntity.cardCategory;
        cardAttribute = cardEntity.cardAttribute;
        name = cardEntity.name;
        cost = cardEntity.cost;
        toughness = cardEntity.toughness;
        power = cardEntity.power;
        devote = cardEntity.devote;

        icon = cardEntity.icon;

        PlayerCard = playerCard;
    }
}