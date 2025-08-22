using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Photon.Pun;


public partial class GameManager : MonoBehaviourPun
{
    [SerializeField] UIManager uIManager;
    [SerializeField] CardController cardPrefab;
    [SerializeField] Transform playerHand, playerField, enemyField, targetField;
    [SerializeField] Text playerLeaderHPText, enemyLeaderHPText;
    [SerializeField] Text playerManaPointText;
    [SerializeField] Text playerDefaultManaPointText;

    public bool isPlayerTurn = true; //
    List<int> deck = new List<int>() { 1, 1, 1, 1, 1, 4, 4, 4, 4, 4, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3 };  //
    List<int> enemy_deck = new List<int>() { 1,1,1,1,1,1,1,1, 2,2,2,2,2,2,2,3,3,3,3,3 };  //

    public int playerLeaderHP;
    public int enemyLeaderHP;
    public int playerManaPoint;
    public int playerDefaultManaPoint;


    public static GameManager instance;
    // シングルトンインスタンスの初期化
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // ゲーム開始時の初期処理
    void Start()
    {
        StartGame();
    }

    // ゲームの初期化処理
    void StartGame()
    {
        enemyLeaderHP = 0;
        playerLeaderHP = 0;

        // デッキをシャッフル
        deck = deck.OrderBy(x => Guid.NewGuid()).ToList();
        enemy_deck = enemy_deck.OrderBy(x => Guid.NewGuid()).ToList();

        playerManaPoint = 0;
        playerDefaultManaPoint = 4;
        ShowManaPoint();

        // 初期手札の配布
        SetStartHand();

        // ターン処理開始
        StartCoroutine(TurnCalc());
    }

    // 指定した場所にカードを生成する
    void CreateCard(int cardID, Transform place)
    {
        CardController card = Instantiate(cardPrefab, place);
        
        // 敵フィールドの場合は敵カードとして初期化
        if (place == enemyField)
        {
            card.Init(cardID, false);
        }
        else
        {
            card.Init(cardID, true);
        }
    }

    // 手札にカードを1枚引く
    void DrawCard(Transform hand)
    {
        // デッキが空なら何もしない
        if (deck.Count == 0)
        {
            return;
        }

        CardController[] playerHandCardList = playerHand.GetComponentsInChildren<CardController>();

        // 手札が9枚未満ならカードを追加
        int r_index = UnityEngine.Random.Range(0, deck.Count);
        int cardID = deck[0];
        deck.RemoveAt(0);
        if (playerHandCardList.Length < 9)
        {
            CreateCard(cardID, hand);
        }

        // 使用可能パネルの更新
        SetCanUsePanelHand();
    }

    // ゲーム開始時に手札を3枚配る
    void SetStartHand()
    {
        for (int i = 0; i < 3; i++)
        {
            DrawCard(playerHand);
        }
    }

    // ターンの管理（プレイヤー・敵のターンを切り替える）
    IEnumerator TurnCalc()
    {
        yield return StartCoroutine(uIManager.ShowChangeTurnPanel());
        if (isPlayerTurn)
        {
            PlayerTurn();
        }
        else
        {
            StartCoroutine(EnemyTurn());
        }
    }

    // ターンを切り替える
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn; // ターンフラグを反転
        StartCoroutine (TurnCalc());  // ターン処理を再開
    }

    // プレイヤーのターン開始処理
    void PlayerTurn()
    {
        Debug.Log("Playerのターン");

        CardController[] playerFieldCardList = playerField.GetComponentsInChildren<CardController>();
        SetAttackableFieldCard(playerFieldCardList, true);

        // マナポイントを加算
        playerManaPoint += playerDefaultManaPoint;
        ShowManaPoint();

        // 手札を1枚引く
        DrawCard(playerHand);
    }

    // 敵のターン処理
    IEnumerator EnemyTurn()
    {
        Debug.Log("Enemyのターン");

        CardController[] enemyFieldCardList = enemyField.GetComponentsInChildren<CardController>();

        yield return new WaitForSeconds(0.5f);

        SetAttackableFieldCard(enemyFieldCardList, true);

        yield return new WaitForSeconds(0.5f);

        // 敵デッキからカードを引いてフィールドに配置
        if (enemy_deck.Count != 0)
        {
            int cardID = enemy_deck[0];
            enemy_deck.RemoveAt(0);
            if (enemyFieldCardList.Length < 5)
            {
                CreateCard(cardID, enemyField);
            }
        }

        yield return new WaitForSeconds(0.5f);

        int index = 0;

        // 攻撃可能な敵カードがある限り攻撃処理を繰り返す
        while (Array.Exists(enemyFieldCardList, card => card.model.canAttack))
        {
            CardController[] enemyCanAttackCardList = Array.FindAll(enemyFieldCardList, card => card.model.canAttack);
            CardController attackCard = enemyCanAttackCardList[0];

            CardController[] playerFieldCardList = playerField.GetComponentsInChildren<CardController>();

            if(playerFieldCardList.Length > 0) // プレイヤーの場にカードがある場合
            {
                // ランダムなプレイヤーカードを攻撃
                index = UnityEngine.Random.Range(0, playerFieldCardList.Length);
                CardController defenceCard = playerFieldCardList[index];
                yield return StartCoroutine(attackCard.movement.AttackMotion(defenceCard.transform));
                CardBattle(attackCard, defenceCard);
            }
            else // プレイヤーの場にカードがない場合はリーダーを攻撃
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(targetField));
                DevoteToLeader(attackCard);
            }

            yield return new WaitForSeconds(0.5f);

            enemyFieldCardList = enemyField.GetComponentsInChildren<CardController>();
        }

        // ターン終了
        ChangeTurn();
    }

    // カード同士のバトル処理
    public void CardBattle(CardController attackCard, CardController defenceCard)
    {
        if (attackCard.model.canAttack == true
            && attackCard.model.PlayerCard != defenceCard.model.PlayerCard)
        {
            GameManager.instance.UseCardEffect(attackCard, CardEffectType.Attack);
            // ダメージ計算
            defenceCard.GrantDamage(attackCard.model.power);
            attackCard.GrantDamage(defenceCard.model.power);

            // ダメージが耐久値を超えた場合は破壊
            attackCard.DamageDestroy();
            defenceCard.DamageDestroy();

            // 攻撃パネルを非表示にし、攻撃不可にする
            attackCard.view.SetCanAttackPanel(false);
            attackCard.model.canAttack = false;
        }
    }

    // フィールド上のカードの攻撃可能状態を設定
    void SetAttackableFieldCard(CardController[] cardList, bool canAttack)
    {
        foreach (CardController card in cardList)
        {
            card.model.canAttack = canAttack;
            card.view.SetCanAttackPanel(canAttack);
        }
    }

    // リーダーへの攻撃処理
    public void DevoteToLeader(CardController attackCard)
    {
        if (attackCard.model.canAttack == false)
        {
            return;
        }

        if (attackCard.model.PlayerCard == true) // プレイヤーカードの場合
        {
            Debug.Log(attackCard.model.name + "がリーダーに奉納");
            CreateThrift(attackCard.model.devote, true);
        }
        else // 敵カードの場合
        {
            Debug.Log(attackCard.model.name + "が敵に奉納");
            CreateThrift(attackCard.model.devote, false);
        }

        attackCard.model.canAttack = false;
        attackCard.view.SetCanAttackPanel(false);

        // 奉納分のダメージを与えて破壊判定
        attackCard.GrantDamage(attackCard.model.devote);
        attackCard.DamageDestroy();
        ShowLeaderHP();
    }

    // リーダーのHPを加算
    public void CreateThrift(int devote, bool Myleader)
    {
        if (Myleader)
        {
            playerLeaderHP += devote;
            Debug.Log("自分のHPは" + playerLeaderHP);
        }
        else
        {
            enemyLeaderHP += devote;
            Debug.Log("敵のHPは" + enemyLeaderHP);
        }
    }

    // リーダーのHPをUIに反映
    public void ShowLeaderHP()
    {
        if (playerLeaderHP <= 0)
        {
            playerLeaderHP = 0;
        }
        if (enemyLeaderHP <= 0)
        {
            enemyLeaderHP = 0;
        }

        playerLeaderHPText.text = playerLeaderHP.ToString();
        enemyLeaderHPText.text = enemyLeaderHP.ToString();
    }

    // マナポイントのUI表示を更新
    void ShowManaPoint()
    {
        playerManaPointText.text = playerManaPoint.ToString();
        playerDefaultManaPointText.text = playerDefaultManaPoint.ToString();
    }

    // マナポイントを消費する
    public void ReduceManaPoint(int cost)
    {
        playerManaPoint -= cost;
        ShowManaPoint();

        SetCanUsePanelHand();
    }

    // 手札のカードの使用可能状態を更新
    void SetCanUsePanelHand()
    {
        CardController[] playerHandCardList = playerHand.GetComponentsInChildren<CardController>();
        foreach (CardController card in playerHandCardList)
        {
            if (card.model.cost <= playerManaPoint)
            {
                card.model.canUse = true;
                card.view.SetCanUsePanel(card.model.canUse);
            }
            else
            {
                card.model.canUse = false;
                card.view.SetCanUsePanel(card.model.canUse);
            }
        }
    }
}
