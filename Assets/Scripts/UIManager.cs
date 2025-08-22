using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject changeTurnPanel, UseGracePanel;
    [SerializeField] Text changeTurnText;

    public static UIManager instance;
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public IEnumerator ShowChangeTurnPanel()
    {
        changeTurnPanel.SetActive(true);

        if (GameManager.instance.isPlayerTurn == true)
        {
            changeTurnText.text = "Your Turn";
        }
        else
        {
            changeTurnText.text = "Enemy Turn";
        }

        yield return new WaitForSeconds(1f);

        changeTurnPanel.SetActive(false);
    }

    public void SetUseGracePanel(bool useGracePanel)
    {
        UseGracePanel.SetActive(useGracePanel);
    }
}