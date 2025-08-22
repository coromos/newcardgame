using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// カードの見た目（UI）を管理するクラス
public class CardView : MonoBehaviour
{
    [SerializeField] Text nameText, costText, toughnessText, powerText, devoteText;
    [SerializeField] Image iconImage;
    [SerializeField] GameObject canAttackPanel, canUsePanel;

    // カードデータをUIに反映
    public void Show(CardModel cardModel)
    {
        nameText.text = cardModel.name;
        costText.text = cardModel.cost.ToString();
        toughnessText.text = (cardModel.toughness - cardModel.damage).ToString();
        powerText.text = cardModel.power.ToString();
        devoteText.text = cardModel.devote.ToString();
        iconImage.sprite = cardModel.icon;
    }

    // 攻撃可能パネルの表示切替
    public void SetCanAttackPanel(bool flag)
    {
        canAttackPanel.SetActive(flag);
    }

    // 使用可能パネルの表示切替
    public void SetCanUsePanel(bool flag)
    {
        canUsePanel.SetActive(flag);
    }
}