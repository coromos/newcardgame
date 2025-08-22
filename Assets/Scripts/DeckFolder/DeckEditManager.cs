using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckEditManager : MonoBehaviour
{
    [SerializeField] CardController cardPrefab; // カードプレハブ

    // デッキカードの生成場所
    [SerializeField] Transform deckCardTrans1;

    private void Start()
    {
        // デッキ編成の画面をセットする
        SetDeckEditPanel();
    }

    void SetDeckEditPanel()
    {
        CreateCard(1, deckCardTrans1);
    }

    // カードを生成するメソッド
    void CreateCard(int cardId, Transform trans)
    {
        // cardPrefabをopenedCardTransに生成する
        CardController card = Instantiate(cardPrefab, trans);
        card.Init(cardId, true);
    }
}