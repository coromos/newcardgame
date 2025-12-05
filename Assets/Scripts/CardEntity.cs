using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// カードデータのScriptableObject定義
[CreateAssetMenu(fileName = "CardEntity", menuName = "Create CardEntity")]
public class CardEntity : ScriptableObject
{
    public int cardId;
    public new string name;
    public CardCategory cardCategory;
    public CardAttribute cardAttribute;
    public CardStain[] cardStains;
    public CardRarity cardRarity;
    public int cost;
    public int toughness;
    public int power;
    public int devote;
    public Sprite icon;

    // 初期能力
    public bool earlier = false;//早成
    public bool interference = false;//妨害
    public bool stealth = false;//ステルス
    public bool strong = false;//強靭
    public bool flying = false;//妨害の効果を受けない
    public bool homeostasis = false;//状態異常無効
    public bool alone = false;//孤立
    public bool noaction = false;//攻撃・捧身不可
}