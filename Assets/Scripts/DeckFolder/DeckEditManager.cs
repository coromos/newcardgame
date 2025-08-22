using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckEditManager : MonoBehaviour
{
    [SerializeField] CardController cardPrefab; // �J�[�h�v���n�u

    // �f�b�L�J�[�h�̐����ꏊ
    [SerializeField] Transform deckCardTrans1;

    private void Start()
    {
        // �f�b�L�Ґ��̉�ʂ��Z�b�g����
        SetDeckEditPanel();
    }

    void SetDeckEditPanel()
    {
        CreateCard(1, deckCardTrans1);
    }

    // �J�[�h�𐶐����郁�\�b�h
    void CreateCard(int cardId, Transform trans)
    {
        // cardPrefab��openedCardTrans�ɐ�������
        CardController card = Instantiate(cardPrefab, trans);
        card.Init(cardId, true);
    }
}