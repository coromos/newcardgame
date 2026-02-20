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

public partial class GameManager : MonoBehaviourPun
{
    // 実行予定の効果呼び出し情報（FIFO 用キュー）
    class EffectCall
    {
        public string MethodName;
        public CardController Source;
        public CardController Target;
        public CardController RefCard;

        public EffectCall(string methodName, CardController source, CardController target, CardController refCard)
        {
            MethodName = methodName;
            Source = source;
            Target = target;
            RefCard = refCard;
        }
    }

    // FIFO キュー（先入れ先出し）
    private Queue<EffectCall> effectCallQueue = new Queue<EffectCall>();

    public void UseCardEffect(CardController mainCard, CardController refCard, CardEffectType type)
    {
        string typeName = type.ToString();

        // Anyが付いている場合の処理
        if (typeName.StartsWith("Any"))
        {
            CardController[] playerFieldCardList = playerField.GetComponentsInChildren<CardController>();
            CardController[] enemyFieldCardList = enemyField.GetComponentsInChildren<CardController>();

            for (int i = 0; i < playerFieldCardList.Length; i++)
            {
                CardController sourceCard = playerFieldCardList[i];
                if (sourceCard != null && sourceCard.gameObject != null)
                {
                    ApplyCardEffect(sourceCard, mainCard, refCard, typeName);
                }
            }

            for (int i = 0; i < enemyFieldCardList.Length; ++i)
            {
                CardController sourceCard = enemyFieldCardList[i];
                if (sourceCard != null && sourceCard.gameObject != null)
                {
                    ApplyCardEffect(sourceCard, mainCard, refCard, typeName);
                }
            }
        }
        else
        {
            ApplyCardEffect(mainCard, mainCard, refCard, typeName);
        }
    }

    // 即時実行は行わず、実行予定を FIFO キューへ追加する
    public void ApplyCardEffect(CardController effectSourceCard, CardController targetCard, CardController refCard, string typeName)
    {
        if (effectSourceCard == null)
            return;

        // カードIDとtypeNameから関数名を生成
        string methodName = typeName + effectSourceCard.model.cardId;

        // メソッドが存在するかを確認してからキューに入れる
        var method = typeof(GameManager).GetMethod(methodName);
        if (method != null)
        {
            effectCallQueue.Enqueue(new EffectCall(methodName, effectSourceCard, targetCard, refCard));
        }
        else
        {
            Debug.LogWarning($"Effect method not found: {methodName}");
        }

        // addEffectList に該当する追加効果もキューに入れる
        if (effectSourceCard.addEffectList != null)
        {
            foreach (var addEffect in effectSourceCard.addEffectList)
            {
                if (addEffect.Contains(typeName))
                {
                    // 追加効果用のメソッド名は同一（既存コードに準拠）
                    var addMethod = typeof(GameManager).GetMethod(methodName);
                    if (addMethod != null)
                    {
                        effectCallQueue.Enqueue(new EffectCall(methodName, effectSourceCard, targetCard, refCard));
                    }
                    else
                    {
                        Debug.LogWarning($"AddEffect method not found: {methodName}");
                    }
                }
            }
        }
    }

    // キューから1件だけ実行（FIFO）
    public void ProcessEffectQueueOne()
    {
        if (effectCallQueue.Count == 0)
            return;

        var call = effectCallQueue.Dequeue();
        var method = typeof(GameManager).GetMethod(call.MethodName);
        if (method != null)
        {
            try
            {
                method.Invoke(this, new object[] { call.Source, call.Target, call.RefCard });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error invoking effect {call.MethodName}: {ex}");
            }
        }
        else
        {
            Debug.LogWarning($"ProcessEffectQueueOne: method not found {call.MethodName}");
        }
    }

    // キュー内をすべて実行（FIFO）
    public void ProcessEffectQueueAll()
    {
        while (effectCallQueue.Count > 0)
        {
            ProcessEffectQueueOne();
        }
    }

    // コルーチンで間隔をあけて処理（必要なら使用）
    public IEnumerator ProcessEffectQueueCoroutine(float delayBetween = 0f)
    {
        while (effectCallQueue.Count > 0)
        {
            ProcessEffectQueueOne();
            if (delayBetween > 0f)
            {
                yield return new WaitForSeconds(delayBetween);
            }
            else
            {
                yield return null;
            }
        }
    }

    // カード選択機能用の変数
    public bool isSelectingCard = false;
    public CardController selectCard;

    //カード選択用リスト取得メソッド
    //引数で指定された場所のカードを取得する
    public List<CardController> GetCardFromPlace(
        bool[] MyCards,
        string[] cardPlaces
        )
    {
        Transform transform;
        List<CardController> baseCardList = new List<CardController>();
        int placeNum = MyCards.Length;
        for (int i = 0; i < placeNum; i++)
        {
            transform = GetPlace(MyCards[i], cardPlaces[i]);
            baseCardList.AddRange(transform.GetComponentsInChildren<CardController>());
        }

        return baseCardList;
    }

    // カード限定メソッド
    // 引数のカードリストから他の条件でカードを絞り込む
    public List<CardController> PickupCardForEffect(
        List<CardController> baseCardList,
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
            if (cardCategory != null && System.Array.IndexOf(cardCategory, cardController.model.cardCategory) < 0)
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

    // カード選択メソッド
    public CardController SelectCard()
    {
        isSelectingCard = true;
        selectCard = null;
        // カードが選択されるまで待機
        while (isSelectingCard)
        {
            if (selectCard != null)
            {
                break;
            }
        }

        return selectCard;
    }

    // カードを外部から選択するためのメソッド
    public void SetSelectedCard(CardController card)
    {
        if (isSelectingCard)
        {
            selectCard = card;
            isSelectingCard = false;
        }
    }
}
