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
    public void Alive1(CardController card, CardController targetCard, PhotonMessageInfo info)
    {
        DrawCard(playerHand);
    }

}
