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
    public int cost;
    public int toughness;
    public int power;
    public int devote;
    public Sprite icon;
}