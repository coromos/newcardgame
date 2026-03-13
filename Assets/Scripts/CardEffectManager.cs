using System;
using System.Collections.Generic;
using UnityEngine;

// シンプルな管理クラス。カードごとの効果を管理。
public class CardEffectManager
{
    private List<CardEffect>[] buckets;

    public CardEffectManager()
    {
        int enumCount = Enum.GetValues(typeof(CardEffectType)).Length;
        buckets = new List<CardEffect>[enumCount];
        for (int i = 0; i < enumCount; i++)
            buckets[i] = new List<CardEffect>();
    }

    // 効果を登録（登録時に type を読んで該当バケツへ追加）
    public void RegisterEffect(CardEffect effect, CardController card, CardEffectType type)
    {
        if (effect == null) return;

        var list = buckets[(int)type];
        list.Add(effect);
    }

    public void UnregisterEffect(CardEffect effect)
    {
        if (effect == null) return;
        for (int i = 0; i < buckets.Length; i++)
        {
            var list = buckets[i];
            if (list.Contains(effect))
                list.Remove(effect);
        }
    }

    public List<CardEffect> GetEffects(CardEffectType type)
    {
        return buckets[(int)type];
    }
}
