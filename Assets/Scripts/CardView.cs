using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// カードの見た目（UI）を管理するクラス
public class CardView : MonoBehaviour
{
    [SerializeField] Text costText, toughnessText, powerText, devoteText;
    [SerializeField] Image iconImage;
    [SerializeField] GameObject canAttackPanel, canUsePanel, SelectedPanel;

    // カードデータをUIに反映
    public void Show(CardModel cardModel)
    {
        costText.text = cardModel.GetCost().ToString();
        toughnessText.text = cardModel.GetToughness().ToString();
        powerText.text = cardModel.GetPower().ToString();
        devoteText.text = cardModel.GetDevote().ToString();
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

    // 選択可能パネルの表示切替
    public void SetCanSelectPanel(bool flag)
    {
        // ここではcanUsePanelを流用して選択可能状態を表現する
        canUsePanel.SetActive(flag);
    }
}