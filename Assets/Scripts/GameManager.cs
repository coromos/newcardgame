using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum PlaceList
{
    Field,
    Hand,
    Deck,
    Trash
}


public partial class GameManager : MonoBehaviourPun
{
    [SerializeField] UIManager uIManager;
    [SerializeField] CardController cardPrefab;
    [SerializeField] Transform playerHand, enemyHand, playerField, enemyField, targetField, playerDeck, enemyDeck;
    [SerializeField] Text playerLeaderHPText, enemyLeaderHPText;
    [SerializeField] Text playerSeedsText, enemySeedsText;
    [SerializeField] Text playerTreeText, enemyTreeText;
    [SerializeField] GameObject NoCard;

    public bool isPlayerTurn = false; //
    //List<int> deck = new List<int>() { 4, 4, 4, 22, 22, 22, 29, 29, 29, 28, 28, 28, 26, 26, 26, 104, 104, 104, 21, 21, 21, 52, 52, 52, 38, 38, 38, 27, 27, 27 };  //
                                                                                                                                                                  //
    List<int> deck;

    public int playerLeaderHP;
    public int playerSeeds;
    public int playerTree;

    public int enemyLeaderHP;
    public int enemySeeds;
    public int enemyTree;
    public int cardInsID = 0;


    public static GameManager instance;
    // シングルトンインスタンスの初期化
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    IEnumerator waitFew(float time)
    {
        yield return new WaitForSeconds(time);
    }

    // シーンへ移動してきた後、他プレイヤーのシーン読み込み完了を待ってからゲーム開始する
    bool gameStarted = false;
    const string LoadedScenePropKey = "LoadedGameScene";

    void Start()
    {
        StartCoroutine(WaitForAllPlayersAndStart());
    }

    IEnumerator WaitForAllPlayersAndStart()
    {
        // まずルームに入るのを待つ
        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }

        // 自分がこのゲームシーンへ来たことをプロパティで知らせる
        Hashtable myProps = new Hashtable();
        myProps[LoadedScenePropKey] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(myProps);

        // マスタークライアントは全員のプロパティを監視し、全員が読み込み完了したらゲーム開始を通知する
        if (PhotonNetwork.IsMasterClient)
        {
            // 全員が読み込み済みになるまで待機
            while (!AllPlayersLoaded())
            {
                yield return new WaitForSeconds(0.1f);
            }

            // すべて揃ったので全員に StartGame を呼ばせる
            photonView.RPC("RPC_StartGame", RpcTarget.All);
        }
        else
        {
            // 非マスターはマスターが送る RPC_StartGame を待つだけ
            // （OnPlayerPropertiesUpdate によりマスターの判断が早まる）
        }
    }

    // 全プレイヤーがシーン読み込み完了フラグを持っているか
    bool AllPlayersLoaded()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.ContainsKey(LoadedScenePropKey))
            {
                return false;
            }
            object val = p.CustomProperties[LoadedScenePropKey];
            if (!(val is bool) || !(bool)val)
            {
                return false;
            }
        }
        return true;
    }

    // マスターが全員揃ったと判断したら呼ぶ RPC
    [PunRPC]
    void RPC_StartGame(PhotonMessageInfo info)
    {
        if (gameStarted) return;
        gameStarted = true;
        StartGame();
    }

    void Update()
    {
        //10ミリ秒ごとに更新
        StartCoroutine(waitFew(0.01f));
        if (gameStarted)
        {
            ShowSeed();
            if (isPlayerTurn && !isSelectingCard)
            {
                SetCanUsePanelHand();
            }
        }
    }

    // ゲームの初期化処理
    void StartGame()
    {
        //オーナーなら、cardInsIDを0から、オーナー以外ならintの最低値から始める
        if (PhotonNetwork.IsMasterClient)
        {
            isPlayerTurn = true;
            cardInsID = 1;
        }
        else
        {
            cardInsID = System.Int32.MinValue;
        }
        enemyLeaderHP = 0;
        playerLeaderHP = 0;

        // デッキを初期化
        DeckEntity[] deckEntities = Resources.LoadAll<DeckEntity>("Decks");
        for(int i = 0; i < deckEntities.Length; i++)
        {
            if (deckEntities[i].useDeck)
            {
                deck = new List<int>(deckEntities[i].cardIds);
                break;
            }
        }
        // デッキをシャッフル
        deck = deck.OrderBy(x => Guid.NewGuid()).ToList();
        for (int i = 0; i < deck.Count; i++)
        {
            CreateCard(deck[i], true, PlaceList.Deck);
        }

        playerSeeds = 0;
        playerTree = 4;

        enemySeeds = 0;
        enemyTree = 4;

        // 初期手札の配布
        SetStartHand();

        // ターン処理開始
        StartCoroutine(TurnCalc());
    }

    void CreateCard(int cardID, bool myCard, PlaceList place)
    {
        photonView.RPC("CreateCardRPC", RpcTarget.All, cardID, myCard, PhotonNetwork.LocalPlayer.ActorNumber, cardInsID, place.ToString());

        cardInsID += 1;
    }
    // 指定した場所にカードを生成する
    [PunRPC]
    void CreateCardRPC(int cardID, bool tmpmyCard, int plNum, int cardIns, string placeName)
    {
        bool myCard = (plNum != PhotonNetwork.LocalPlayer.ActorNumber) ^ tmpmyCard;
        Transform parentTransform = GetPlace(myCard, placeName);

        // 待機中カードがあればそれを使用する
        CardController[] setCardList = GetComponentsInChildren<CardController>();
        if (setCardList != null && setCardList.Length > 0)
        {
            setCardList[0].transform.SetParent(parentTransform, false);
            setCardList[0].Init(cardID, myCard, cardIns, placeName);
        }
        else
        {
            // カードを生成して親Transformの子に設定
            CardController newCard = Instantiate(cardPrefab, parentTransform);
            newCard.Init(cardID, myCard, cardIns, placeName);
        }
        
    }

    // DrawCardを全員に通知
    public void CallDrawCard(bool mine)
    {
        photonView.RPC("CallDrawCardRPC", RpcTarget.All, mine);
    }

    [PunRPC]
    void CallDrawCardRPC(bool tmpmine, PhotonMessageInfo info)
    {
        bool mine = (PhotonNetwork.LocalPlayer.ActorNumber != info.Sender.ActorNumber) ^ tmpmine;
        DrawCard(mine);
    }

    // 手札にカードを1枚引く
    void DrawCard(bool mine)
    {
        CardController[] handCardList;
        CardController[] deckCardList;
        if (mine)
        {
            handCardList = playerHand.GetComponentsInChildren<CardController>();
            
            deckCardList = playerDeck.GetComponentsInChildren<CardController>();
        }
        else
        {
            handCardList = enemyHand.GetComponentsInChildren<CardController>();

            deckCardList = enemyDeck.GetComponentsInChildren<CardController>();
        }
        if (deckCardList != null)
        {
            PlaceList place = PlaceList.Trash;
            // 手札が9枚未満ならカードを追加
            if (handCardList.Length < 9)
            {
                place = PlaceList.Hand;
            }

            deckCardList[0].transform.SetParent(GetPlace(mine, place.ToString()), false);
        }
    }

    // ゲーム開始時に手札を3枚配る
    void SetStartHand()
    {
        for (int i = 0; i < 3; i++)
        {
            CallDrawCard(playerHand);
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
            //StartCoroutine(EnemyTurn());
        }
    }

    // ターンを切り替える
    public void ChangeTurn()
    {
        //turn end script Want!!!!
        if (!isPlayerTurn)
        {
            return;
        }
        TurnEnd();
        photonView.RPC("ChangeTurnRPC", RpcTarget.All, true);
    }

    [PunRPC]
    public void ChangeTurnRPC(bool isturn, PhotonMessageInfo info)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != info.Sender.ActorNumber)
        {
            isPlayerTurn = true;
            StartCoroutine(TurnCalc());  // ターン処理を再開
        }
        else
        {
            isPlayerTurn = false;
            CardController[] playerCardList = playerHand.GetComponentsInChildren<CardController>();
            foreach (CardController card in playerCardList)
            {
                card.model.canUse = false;
                card.view.SetCanUsePanel(card.model.canUse);
            }
            playerCardList = playerField.GetComponentsInChildren<CardController>();
            foreach (CardController card in playerCardList)
            {
                card.model.canUse = false;
                card.view.SetCanUsePanel(card.model.canUse);
            }
        }
    }

    // プレイヤーのターン開始処理
    void PlayerTurn()
    {
        Debug.Log("Playerのターン");

        //CardController[] playerFieldCardList = playerField.GetComponentsInChildren<CardController>();
        //SetAttackableFieldCard(playerFieldCardList, true);
        CallSetAttackableFieldCard(true);

        // シードを加算
        CallAddSeeds(playerTree, true);

        //ボタンのフラグリセット
        drawBottonFlag = 0;
        growTreeFlag = 0;
    }

    //// 敵のターン処理
    //IEnumerator EnemyTurn()
    //{
    //    Debug.Log("Enemyのターン");

    //    CardController[] enemyFieldCardList = enemyField.GetComponentsInChildren<CardController>();

    //    yield return new WaitForSeconds(0.5f);

    //    SetAttackableFieldCard(enemyFieldCardList, true);

    //    yield return new WaitForSeconds(0.5f);

    //    // 敵デッキからカードを引いてフィールドに配置
    //    if (enemy_deck.Count != 0)
    //    {
    //        int cardID = enemy_deck[0];
    //        enemy_deck.RemoveAt(0);
    //        if (enemyFieldCardList.Length < 5)
    //        {
    //            CreateCard(cardID, false, PlaceList.Field);
    //        }
    //    }

    //    yield return new WaitForSeconds(0.5f);

    //    int index = 0;

    //    // 攻撃可能な敵カードがある限り攻撃処理を繰り返す
    //    while (Array.Exists(enemyFieldCardList, card => card.model.canAttack))
    //    {
    //        CardController[] enemyCanAttackCardList = Array.FindAll(enemyFieldCardList, card => card.model.canAttack);
    //        CardController attackCard = enemyCanAttackCardList[0];

    //        CardController[] playerFieldCardList = playerField.GetComponentsInChildren<CardController>();

    //        if(playerFieldCardList.Length > 0) // プレイヤーの場にカードがある場合
    //        {
    //            // ランダムなプレイヤーカードを攻撃
    //            index = UnityEngine.Random.Range(0, playerFieldCardList.Length);
    //            CardController defenceCard = playerFieldCardList[index];
    //            yield return StartCoroutine(attackCard.movement.AttackMotion(defenceCard.transform));
    //            CardBattle(attackCard, defenceCard);
    //        }
    //        else // プレイヤーの場にカードがない場合はリーダーを攻撃
    //        {
    //            yield return StartCoroutine(attackCard.movement.AttackMotion(targetField));
    //            Devote(attackCard);
    //        }

    //        yield return new WaitForSeconds(0.5f);

    //        enemyFieldCardList = enemyField.GetComponentsInChildren<CardController>();
    //    }


        // ターン終了
    //    ChangeTurn();
    //}

    public int drawBottonFlag = 0;

    public void DrawBotton()
    {
        int drawBottonCost = 0;
        if (drawBottonFlag == 0 && playerSeeds >= 1)
        {
            drawBottonCost = 1;
            drawBottonFlag++;
        }
        else if (drawBottonFlag == 1 && playerSeeds >= 2)
        {
            drawBottonCost = 2;
            drawBottonFlag++;
        }
        else
        {
            return;
        }
        photonView.RPC("DrawBottonRPC", RpcTarget.All, drawBottonCost);
    }

    [PunRPC]
    public void DrawBottonRPC(int cost, PhotonMessageInfo info)
    {
        bool mine = (PhotonNetwork.LocalPlayer.ActorNumber == info.Sender.ActorNumber);

        ReduceSeeds(cost, mine);
        DrawCard(mine);
    }

    public int growTreeFlag = 0;

    public void GrowTree()
    {
        if (growTreeFlag == 0 && playerSeeds >= 2)
        {
            // 初回
        }
        else
        {
            return;
        }
        growTreeFlag++;
        photonView.RPC("GrowTreeRPC", RpcTarget.All);
    }

    [PunRPC]
    public void GrowTreeRPC(PhotonMessageInfo info)
    {
        bool mine = (PhotonNetwork.LocalPlayer.ActorNumber == info.Sender.ActorNumber);
        ReduceSeeds(2, mine);
        if (mine)
        {
            playerTree += 1;
            growTreeFlag++;
        }
        else
        {
            enemyTree += 1;
            growTreeFlag++;
        }
    }

    //Seedsを増やすことを全員に通知
    void CallAddSeeds(int amount, bool mine)
    {
        photonView.RPC("CallAddSeedsRPC", RpcTarget.All, amount, mine);
    }

    [PunRPC]
    void CallAddSeedsRPC(int amount, bool tmpmine, PhotonMessageInfo info)
    {
        bool mine = (PhotonNetwork.LocalPlayer.ActorNumber != info.Sender.ActorNumber) ^ tmpmine;
        if (mine)
        {
            playerSeeds += amount;
        }
        else
        {
            enemySeeds += amount;
        }

    }

    // カード同士のバトル処理
    public void CardBattle(CardController attackCard, CardController defenceCard)
    {
        if (attackCard.model.canAttack == true && attackCard.model.noaction == false
            && attackCard.model.PlayerCard != defenceCard.model.PlayerCard)
        {
            photonView.RPC("SelectInterferenceRPC", RpcTarget.All);

            if (selectedInterferenceCardID != 0)
            {
                photonView.RPC("CardBattleRPC", RpcTarget.All, attackCard.cardInsID, selectedInterferenceCardID);
            }
            else
            {
                photonView.RPC("CardBattleRPC", RpcTarget.All, attackCard.cardInsID, defenceCard.cardInsID);
            }


        }
    }

    [PunRPC]
    public void CardBattleRPC(int attackCardID, int defenceCardID)
    {
        CardController attackCard = FindCardByInstanceID(attackCardID);
        CardController defenceCard = FindCardByInstanceID(defenceCardID);

        if (attackCard != null && defenceCard != null)
        {
            GameManager.instance.UseCardEffect(attackCard, defenceCard, CardEffectType.Attack);
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

    // 【妨害】機能用のメソッド
    // 妨害可能なカードを選択するためのRPC
    [PunRPC]
    public void SelectInterferenceRPC(PhotonMessageInfo info)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == info.Sender.ActorNumber)
        {
            List<CardController> selectableCards = new List<CardController>(playerField.GetComponentsInChildren<CardController>());
            selectableCards = selectableCards.Where(card => card.model.interference).ToList();

            if (selectableCards.Count > 0)
            {
                List<CardController> selectedCards = StartCardSelection(selectableCards, 1);
                selectedInterferenceCardID = selectedCards[0].cardInsID; 
            }
            else
            {
                selectedInterferenceCardID = 0; // 妨害可能なカードがない場合は0をセット
            }

            photonView.RPC("InterferenceRPC", RpcTarget.All, selectedInterferenceCardID);
            selectedInterferenceCardID = -1;
        }
        else
        {
            // 他プレイヤーは選択されたカードIDを待つ
            while (selectedInterferenceCardID == -1)
            {
                // 待機
            }
        }
    }

    // 選択されたカードIDを共有する変数
    int selectedInterferenceCardID = 0;
    // 選択された【妨害】カードを通知するためのRPC
    [PunRPC]
    public void InterferenceRPC(int cardInsID, PhotonMessageInfo info)
    {
        selectedInterferenceCardID = cardInsID;
    }

    void CallSetAttackableFieldCard(bool canAttack)
    {
        photonView.RPC("SetAttackableFieldCardRPC", RpcTarget.All, canAttack);
    }

    [PunRPC]
    void SetAttackableFieldCardRPC(bool canAttack, PhotonMessageInfo info)
    {
        bool mine = (PhotonNetwork.LocalPlayer.ActorNumber == info.Sender.ActorNumber);
        CardController[] fieldCardList;
        if (mine)
        {
            fieldCardList = playerField.GetComponentsInChildren<CardController>();
        }
        else
        {
            fieldCardList = enemyField.GetComponentsInChildren<CardController>();
        }
        SetAttackableFieldCard(fieldCardList, canAttack);
    }

    // フィールド上のカードの攻撃可能状態を設定
    void SetAttackableFieldCard(CardController[] cardList, bool canAttack)
    {
        foreach (CardController card in cardList)
        {
            card.model.canAttack = canAttack;
            card.view.SetCanAttackPanel(!card.model.noaction && canAttack);
        }
    }

    public void CallDevote(CardController attackCard)
    {
        if (attackCard.model.canAttack == false)
        {
            return;
        }

        photonView.RPC("DevoteRPC", RpcTarget.All, attackCard.cardInsID);
    }

    //リーダーへの攻撃処理のRPC
    [PunRPC]
    public void DevoteRPC(int attackCardID)
    {
        CardController attackCard = FindCardByInstanceID(attackCardID);
        if (attackCard != null)
        {
            Devote(attackCard);
        }
    }

    // リーダーへの攻撃処理
    public void Devote(CardController attackCard)
    {if (attackCard.model.PlayerCard == true) // プレイヤーカードの場合
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
            Debug.Log("自分の繁栄は" + playerLeaderHP);
        }
        else
        {
            enemyLeaderHP += devote;
            Debug.Log("敵の繁栄は" + enemyLeaderHP);
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

    // シードのUI表示を更新
    void ShowSeed()
    {
        playerTreeText.text = playerTree.ToString();
        playerSeedsText.text = playerSeeds.ToString();

        enemyTreeText.text = enemyTree.ToString();
        enemySeedsText.text = enemySeeds.ToString();
    }

    //ReduceSeedsを全員に通知
    void CallReduceSeeds(int cost, bool mine)
    {
        photonView.RPC("CallReduceSeedsRPC", RpcTarget.All, cost, mine);
    }

    [PunRPC]
    void CallReduceSeedsRPC(int cost, bool tmpmine, PhotonMessageInfo info)
    {
        bool mine = (PhotonNetwork.LocalPlayer.ActorNumber != info.Sender.ActorNumber) ^ tmpmine;
        ReduceSeeds(cost, mine);
    }

    // シードを消費する
    public void ReduceSeeds(int cost, bool mine)
    {
        if (mine)
        {
            playerSeeds -= cost;
        }
        else
        {
            enemySeeds -= cost;
        }
    }

    // 手札のカードの使用可能状態を更新
    void SetCanUsePanelHand()
    {
        if (isPlayerTurn)
        {
            CardController[] playerHandCardList = playerHand.GetComponentsInChildren<CardController>();
            foreach (CardController card in playerHandCardList)
            {
                card.model.canUse = (card.model.cost <= playerSeeds);
                card.view.SetCanUsePanel(card.model.canUse);
            }
        }
    }

    // 手札からカードを使用
    public void UseCardFromHand(CardController card)
    {
        if (!isPlayerTurn )
        {
            return;
        }

        UIManager.instance.SetUseGracePanel(false);

        if (card.model.cardCategory == CardCategory.Anima)
        {
            CallReduceSeeds(card.model.cost, true);
            // カード効果を発動
            UseCardEffect(card, card, CardEffectType.Alive);
            ProcessEffectQueueOne();

            SetPlace(card, PlaceList.Field);
            card.movement.cardParent = playerField;
            card.DropField();
        }
        else if (card.model.cardCategory == CardCategory.Grace)
        {
            CallReduceSeeds(card.model.cost, true);
            UseCardEffect(card, card, CardEffectType.Grace);
            ProcessEffectQueueOne();
            card.UseGrace();
        }
    }

    public void SetPlace(CardController card, PlaceList place)
    {
        photonView.RPC("SetPlaceRPC", RpcTarget.All, card.cardInsID, place.ToString());
    }

    [PunRPC]
    public void SetPlaceRPC(int cardIns, string placeName)
    {
        CardController card = FindCardByInstanceID(cardIns);
        
        if (card != null)
        {
            Transform parentTransform = GetPlace(card.model.PlayerCard, placeName);
            card.transform.SetParent(parentTransform, false);
            card.model.fieldPosition = placeName;
        }
    }

    Transform GetPlace(bool myCard, string placeName)
    {
        Transform transform;

        // 自分のカードか敵のカードかと、配置場所に応じて親Transformを決定
        if (placeName == PlaceList.Hand.ToString())
        {
            transform = myCard ? playerHand : enemyHand;
        }
        else if (placeName == PlaceList.Field.ToString())
        {
            transform = myCard ? playerField : enemyField;
        }
        else if (placeName == PlaceList.Deck.ToString())
        {
            transform = myCard ? playerDeck : enemyDeck;
        }
        else
        {
            transform = this.transform;
        }
        return transform;
    }

    // インスタンスIDからCardControllerを検索
    public CardController FindCardByInstanceID(int instanceId)
    {
        CardController[] allCards = FindObjectsByType<CardController>(FindObjectsSortMode.None);
        for(int i = 0; i < allCards.Length; i++)
        {
            if (allCards[i].cardInsID == instanceId)
            {
                return allCards[i];
            }
        }
        return null;
    }

    // ターン開始時処理
    // 


    // ターン終了時処理
    // 毒処理
    // カードのターン終了時処理
    // 一時バフのリセット
    void TurnEnd()
    {

    }
}
