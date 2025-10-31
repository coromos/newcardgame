using Photon.Pun;
using UnityEngine;
/*
public enum CardEffectType
{
    Alive,
    Trash,
    Attack,
    Battle,
    Devote,
    Grace,
    AnyAlive,
    AnyTrash,
    AnyAttack,
    AnyBattle,
    AnyDevote,
    AnyGrace
}
 */
public partial class GameManager : MonoBehaviourPun
{
    [PunRPC]
    public void ApplyDamage(int targetActor, int amount)
    {
    }

    [PunRPC]
    public void ApplyHeal(int targetActor, int amount)
    {
    }

    [PunRPC]
    public void Alive1(CardController SourceCard, CardController targetCard, CardController refCard)
    {
        DrawCard(playerHand);
    }

    public void Grace4(CardController SourceCard, CardController targetCard, CardController refCard)
    {
        DrawCard(playerHand);
        Debug.Log("Use Crace4!");
    }

}
