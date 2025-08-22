using UnityEngine;

// カードのカテゴリ・属性を定義する列挙型
public enum CardCategory
{
    Anima,      // フィールドに配置できるカード
    Grace,      // 一時的な効果を持つカード
    Ornament    // 装飾・特殊カード
}

public enum CardAttribute
{
    Intellect,
    Wildness,
    Ancient,
    Fantasy,
    Structure,
    Bacillus,
    Nameless
}