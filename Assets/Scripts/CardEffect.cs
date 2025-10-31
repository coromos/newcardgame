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
}
