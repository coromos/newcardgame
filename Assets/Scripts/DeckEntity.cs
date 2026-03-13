using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// カードデータのScriptableObject定義
[CreateAssetMenu(fileName = "DeckEntity", menuName = "Create DeckEntity")]
public class DeckEntity : ScriptableObject
{
    public bool useDeck = false; // デッキとして使用するかどうか
    public string deckName; // デッキの名前
    public int[] cardIds;
}