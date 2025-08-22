using Photon.Pun;
using UnityEngine;

public enum CardEffectType
{
    Alive,
    Trash,
    Attack,
    Battle,
    Devote,
    AnyAlive,
    AnyTrash,
    AnyAttack,
    AnyBattle,
    AnyDevote,
    AnyGrace
}

public partial class GameManager : MonoBehaviourPun
{

    public void UseCardEffect(CardController card, CardEffectType type)
    {
        string typeName = type.ToString();

        // Anyが付いている場合の処理
        if (typeName.StartsWith("Any"))
        {
            CardController[] playerFieldCardList = playerField.GetComponentsInChildren<CardController>();
            CardController[] enemyFieldCardList = enemyField.GetComponentsInChildren<CardController>();

            int targetInstanceId = card.GetInstanceID();

            for (int i = 0; i < playerFieldCardList.Length; i++)
            {
                CardController sourceCard = playerFieldCardList[i];
                if (sourceCard != null && sourceCard.gameObject != null)
                {
                    int effectSourceInstanceId = sourceCard.GetInstanceID();
                    photonView.RPC("ApplyCardEffectRPC", RpcTarget.AllViaServer, effectSourceInstanceId, targetInstanceId, typeName);
                }
            }

            for (int i = 0; i < enemyFieldCardList.Length; ++i)
            {
                CardController sourceCard = enemyFieldCardList[i];
                if (sourceCard != null && sourceCard.gameObject != null)
                {
                    int effectSourceInstanceId = sourceCard.GetInstanceID();
                    photonView.RPC("ApplyCardEffectRPC", RpcTarget.AllViaServer, effectSourceInstanceId, targetInstanceId, typeName);
                }
            }
        }
        // Anyが付いていない場合の処理は未実装
        else
        {
            photonView.RPC("ApplyCardEffectRPC", RpcTarget.AllViaServer, card.GetInstanceID(), 0, typeName);
        }
    }

    [PunRPC]
    public void ApplyCardEffectRPC(int effectSourceInstanceId, int targetInstanceId, string typeName, PhotonMessageInfo info)
    {
        CardController effectSourceCard = FindCardByInstanceID(effectSourceInstanceId);
        CardController targetCard = FindCardByInstanceID(targetInstanceId);

        if (effectSourceCard == null)
            return;

        // カードIDとtypeNameから関数名を生成
        string methodName = typeName + effectSourceCard.model.cardId;

        // リフレクションでGameManagerのメソッドを呼び出す
        var method = typeof(GameManager).GetMethod(methodName);
        if (method != null)
        {
            method.Invoke(this, new object[] { effectSourceCard, targetCard, info });
        }
        else
        {
            // メソッドが見つからない場合はスルー
        }
    }

    // インスタンスIDからCardControllerを検索
    public CardController FindCardByInstanceID(int instanceId)
    {
        CardController[] allCards = FindObjectsByType<CardController>(FindObjectsSortMode.None);
        foreach (CardController card in allCards)
        {
            if (card.GetInstanceID() == instanceId)
            {
                return card;
            }
        }
        return null;
    }
}
