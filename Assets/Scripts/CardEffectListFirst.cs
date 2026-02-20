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
    public void Alive1(CardController SourceCard, CardController targetCard, CardController refCard)
    {
        CallDrawCard(true);
    }

    public void Grace4(CardController SourceCard, CardController targetCard, CardController refCard)
    {
        //カードを2枚引く
        for (int i = 0; i < 2; i++)
        {
            CallDrawCard(true);
        }
        Debug.Log("Use Crace4!");
    }

    public void Devote5(CardController SourceCard, CardController targetCard, CardController refCard)
    {
        //カードを1枚引く
        CallDrawCard(true);
    }

    public void Grace7(CardController SourceCard, CardController targetCard, CardController refCard)
    {
        //自分の場のアニマ全てを全回復し、0/+7/+7する
        CardController[] playerFieldCardList = playerField.GetComponentsInChildren<CardController>();
        for (int i = 0; i < playerFieldCardList.Length; i++)
        {
            CardController target = playerFieldCardList[i];
            if (target != null && target.gameObject != null)
            {
                //ダメージを0にする
                target.model.damage = 0;
                //+7/+7する
                target.model.flnToughness += 7;
                target.model.flnPower += 7;
                //ステータス更新
            }
        }
        Debug.Log("Use Crace7!");
    }

}
