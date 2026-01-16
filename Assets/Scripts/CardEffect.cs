using Photon.Pun;
using UnityEngine;

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
    public void ApplyCardEffect(CardController effectSourceCard, CardController targetCard, CardController refCard, string typeName)
    {
        if (effectSourceCard == null)
            return;

        // カードIDとtypeNameから関数名を生成
        string methodName = typeName + effectSourceCard.model.cardId;
        // リフレクションでGameManagerのメソッドを呼び出す
        var method = typeof(GameManager).GetMethod(methodName);
        if (method != null)
        {
            method.Invoke(this, new object[] { effectSourceCard, targetCard, refCard });
        }

        foreach (var addEffect in effectSourceCard.addEffectList)
        {
            if (addEffect.Contains(typeName))
            {
                typeof(GameManager).GetMethod(methodName).Invoke(this, new object[] {effectSourceCard, targetCard, refCard });
            }
        }
    }

    // カード選択機能用の変数
    public bool isSelectingCard = false;
    public CardController selectCard;

    // カード指定メソッド
    // 引数に選択対象の条件を指定
    public void PickupCardForEffect(
        string[] fieldPositions = null,
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
        bool? canAttack = null,
        bool? playerCard = null,
        bool highLight = false
        )
    {
        bool isSelectable = false;
        CardController[] cardControllers = FindObjectsByType<CardController>(FindObjectsSortMode.None);
        foreach (var cardController in cardControllers)
        {
            isSelectable = true;
            if (fieldPositions != null && System.Array.IndexOf(fieldPositions, cardController.model.fieldPosition) < 0)
                isSelectable = false;
            else if (cardCategory != null && System.Array.IndexOf(cardCategory, cardController.model.cardCategory) < 0)
                isSelectable = false;
            else if (cardAttribute != null && System.Array.IndexOf(cardAttribute, cardController.model.cardAttribute) < 0)
                isSelectable = false;
            else if (minCost.HasValue && cardController.model.cost < minCost.Value)
                isSelectable = false;
            else if (maxCost.HasValue && cardController.model.cost > maxCost.Value)
                isSelectable = false;
            else if (listCost != null && System.Array.IndexOf(listCost, cardController.model.cost) < 0)
                isSelectable = false;
            else if (minToughness.HasValue && cardController.model.toughness < minToughness.Value)
                isSelectable = false;
            else if (maxToughness.HasValue && cardController.model.toughness > maxToughness.Value)
                isSelectable = false;
            else if (minPower.HasValue && cardController.model.power < minPower.Value)
                isSelectable = false;
            else if (maxPower.HasValue && cardController.model.power > maxPower.Value)
                isSelectable = false;
            else if (minDevote.HasValue && cardController.model.devote < minDevote.Value)
                isSelectable = false;
            else if (maxDevote.HasValue && cardController.model.devote > maxDevote.Value)
                isSelectable = false;
            else if (minDamage.HasValue && cardController.model.damage < minDamage.Value)
                isSelectable = false;
            else if (maxDamage.HasValue && cardController.model.damage > maxDamage.Value)
                isSelectable = false;
            else if (canAttack.HasValue && cardController.model.canAttack != canAttack.Value)
                isSelectable = false;
            else if (playerCard.HasValue && cardController.model.PlayerCard != playerCard.Value)
                isSelectable = false;
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
                    isSelectable = false;
            }
            cardController.model.canUse = isSelectable;
            if (highLight)
                cardController.view.SetCanUsePanel(cardController.model.canUse);
        }
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
