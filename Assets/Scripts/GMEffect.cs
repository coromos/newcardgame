using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Photon.Pun;

// ゲーム管理のうち、カード効果に関連する部分
public partial class GameManager
{
    // FIFO キュー（先入れ先出し）
    private Queue<CardEffect> effectCallQueue = new Queue<CardEffect>();

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
                    ApplyCardEffect(sourceCard, mainCard, refCard, type);
                }
            }

            for (int i = 0; i < enemyFieldCardList.Length; ++i)
            {
                CardController sourceCard = enemyFieldCardList[i];
                if (sourceCard != null && sourceCard.gameObject != null)
                {
                    ApplyCardEffect(sourceCard, mainCard, refCard, type);
                }
            }
        }
        else
        {
            ApplyCardEffect(mainCard, mainCard, refCard, type);
        }
    }

    // 即時実行は行わず、実行予定を FIFO キューへ追加する
    public void ApplyCardEffect(CardController effectSourceCard, CardController targetCard, CardController refCard, CardEffectType type)
    {
        if (effectSourceCard == null)
            return;

        List<CardEffect> effects = effectSourceCard.cem.GetEffects(type);
        for (int i = 0; i < effects.Count; i++)
        {
            effects[i].SetRefCards(effectSourceCard, targetCard, refCard);
            effectCallQueue.Enqueue(effects[i]);
        }
    }

    // キューから1件だけ実行（FIFO） — コルーチン化
    public IEnumerator ProcessEffectQueueOne()
    {
        if (effectCallQueue.Count == 0)
            yield break;

        var call = effectCallQueue.Dequeue();
        // call.Activate は IEnumerator に変更済み
        yield return StartCoroutine(call.Activate());
    }

    // キュー内をすべて実行（FIFO） — コルーチン化
    public IEnumerator ProcessEffectQueueAll()
    {
        while (effectCallQueue.Count > 0)
        {
            yield return StartCoroutine(ProcessEffectQueueOne());
        }
    }

    // コルーチンで間隔をあけて処理（必要なら使用）
    public IEnumerator ProcessEffectQueueCoroutine(float delayBetween = 0f)
    {
        while (effectCallQueue.Count > 0)
        {
            yield return StartCoroutine(ProcessEffectQueueOne());
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

    public bool isSelectingCard = false;
    public CardController SelectedCard;

    // コルーチン用に選択結果を格納するフィールドを追加（複数選択対応）
    public List<CardController> SelectionResults;

    // カード選択処理（クリックで SetSelectedCard を呼ぶ既存フローを利用）
    public void SetSelectedCard(CardController card)
    {
        if (isSelectingCard)
        {
            SelectedCard = card;
            // isSelectingCard の制御は StartCardSelection 内で行う
        }
    }

    public IEnumerator StartCardSelection(List<CardController> selectableCards, int selectCount = 1)
    {
        isSelectingCard = true;
        SelectionResults = new List<CardController>();

        CardController NC = NoCard.GetComponent<CardController>();
        // 選択可能なカードに選択UIを表示
        foreach (CardController card in selectableCards)
        {
            card.canSelect = true;
            card.view.SetCanUsePanel(false);
            card.view.SetCanAttackPanel(false);
            card.view.SetCanSelectPanel(true);
        }
        NC.canSelect = true;
        NC.view.SetCanSelectPanel(true);
        NoCard.SetActive(true);
        // カードが選択されるのを待つ（非ブロッキング）
        while (isSelectingCard)
        {
            // 選択されたカードをリストに追加
            if (SelectedCard != null)
            {
                SelectionResults.Add(SelectedCard);
                SelectedCard.view.SetCanSelectPanel(false);
                SelectedCard = null;
                if (SelectionResults.Count >= Math.Min(selectableCards.Count, selectCount))
                {
                    isSelectingCard = false;
                }
            }
            yield return null;
        }
        // 選択UIを非表示にする
        foreach (CardController card in selectableCards)
        {
            card.canSelect = false;
            card.view.SetCanSelectPanel(false);
            card.view.SetCanAttackPanel(card.model.canAttack);
            //card.view.SetCanUsePanel(card.model.canUse);
        }
        NC.canSelect = false;
        NC.view.SetCanSelectPanel(false);
        NoCard.SetActive(false);
        yield break;
    }

    // 指定のカードにダメージを与えるメソッド
    // --- ここから RPC で共有するダメージ処理メソッド群 ---


    // ローカルから呼んで、全クライアントへダメージ適用を通知する（カードインスタンスIDを使用）
    public void CallDamageCard(CardController card, int damage)
    {
        if (card == null)
            return;

        // cardInsID を全クライアントで一致させているのでそれを渡す
        photonView.RPC("RPC_ApplyDamageToCard", RpcTarget.All, card.cardInsID, damage);
    }

    // RPC ハンドラ：受け取ったカードID に対してダメージをローカル適用する
    [PunRPC]
    void RPC_ApplyDamageToCard(int targetCardInsID, int damage, PhotonMessageInfo info)
    {
        var target = FindCardByInstanceID(targetCardInsID);
        if (target == null)
        {
            Debug.LogWarning($"RPC_ApplyDamageToCard: cardInsID {targetCardInsID} が見つかりませんでした。");
            return;
        }

        ApplyDamageLocal(target, damage);
    }

    // ローカルでダメージを適用し、ビュー更新・破壊判定を実行する
    private void ApplyDamageLocal(CardController card, int damage)
    {
        if (card == null)
            return;

        card.GrantDamage(damage);
        card.DamageDestroy();
    }

    // --- RPC ダメージ処理ここまで ---

    // カードにバフを付与するメソッド（例：耐久力アップ）
    public void CallBuffCard(CardController card, int toughnessBuff, int powerBuff, int devoteBuff)
    {
        if (card == null)
            return;
        photonView.RPC("RPC_ApplyBuffToCard", RpcTarget.All, card.cardInsID, toughnessBuff, powerBuff, devoteBuff);
    }

    [PunRPC]
    void RPC_ApplyBuffToCard(int targetCardInsID, int toughnessBuff, int powerBuff, int devoteBuff, PhotonMessageInfo info)
    {
        var target = FindCardByInstanceID(targetCardInsID);
        if (target == null)
        {
            Debug.LogWarning($"RPC_ApplyBuffToCard: cardInsID {targetCardInsID} が見つかりませんでした。");
            return;
        }
        ApplyBuffLocal(target, toughnessBuff, powerBuff, devoteBuff);
    }

    void ApplyBuffLocal(CardController card, int toughnessBuff, int powerBuff, int devoteBuff)
    {
        if (card == null)
            return;
        card.model.flnToughness += toughnessBuff;
        card.model.flnPower += powerBuff;
        card.model.flnDevote += devoteBuff;
        card.view.Show(card.model);
    }
}
