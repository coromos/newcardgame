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

    public bool isPlayerTurn = false;
    List<int> deck;

    public int playerLeaderHP;
    public int playerSeeds;
    public int playerTree;

    public int enemyLeaderHP;
    public int enemySeeds;
    public int enemyTree;
    public int cardInsID = 0;


    public static GameManager instance;
    
    // 選択関連の共有フラグと定数
    const int INTERFERENCE_UNDECIDED = -1;
    const int INTERFERENCE_NONE = 0;
    // 選択された妨害カードID（-1: 未決定 / 0: 妨害なし / >0: カードID）
    int selectedInterferenceCardID = INTERFERENCE_NONE;

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

    public int drawBottonFlag = 0;

    public void DrawBotton()
    {
        // 選択待ちモード中はボタン操作を受け付けない
        if (isSelectingCard)
        {
            return;
        }

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
        // 選択待ちモード中はボタン操作を受け付けない
        if (isSelectingCard)
        {
            return;
        }

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

    // カード同士のバトル処理（非同期化された妨害選択を行う）
    public void CardBattle(CardController attackCard, CardController defenceCard)
    {
        // 選択待ちモード中は他のアクションをブロックする
        if (isSelectingCard)
        {
            return;
        }

        if (attackCard.model.canAttack == true && attackCard.model.noaction == false
            && attackCard.model.PlayerCard != defenceCard.model.PlayerCard)
        {
            // 選択要求を全員へ送信し、その後コルーチンで結果を待つ
            photonView.RPC("SelectInterferenceRPC", RpcTarget.All, defenceCard.cardInsID);
            StartCoroutine(CardBattleCoroutine(attackCard, defenceCard));
        }
    }

    // 妨害選択の結果を待ってから攻撃処理へ進むコルーチン
    IEnumerator CardBattleCoroutine(CardController attackCard, CardController defenceCard)
    {
        // 待機状態を示す値にリセット
        selectedInterferenceCardID = INTERFERENCE_UNDECIDED;

        // 選択結果到着を待つ（ブロッキングしない）
        yield return new WaitUntil(() => selectedInterferenceCardID != INTERFERENCE_UNDECIDED);

        int chosenID = selectedInterferenceCardID;

        // 妨害カードが選択されていればそのIDで攻撃処理を呼ぶ。未選択（0）の場合は防御カードを対象にする
        if (chosenID != INTERFERENCE_NONE)
        {
            photonView.RPC("CardBattleRPC", RpcTarget.All, attackCard.cardInsID, chosenID, true);
        }
        else
        {
            photonView.RPC("CardBattleRPC", RpcTarget.All, attackCard.cardInsID, defenceCard.cardInsID, false);
        }

        // 終了後は既定値に戻す
        selectedInterferenceCardID = INTERFERENCE_NONE;
    }

    [PunRPC]
    public void CardBattleRPC(int attackCardID, int defenceCardID, bool isitf)
    {
        CardController attackCard = FindCardByInstanceID(attackCardID);
        CardController defenceCard = FindCardByInstanceID(defenceCardID);

        if (attackCard != null && defenceCard != null)
        {
            UseCardEffect(attackCard, defenceCard, CardEffectType.Attack);
            // ダメージ計算
            defenceCard.GrantDamage(attackCard.model.power);
            attackCard.GrantDamage(defenceCard.model.power);

            // ダメージが耐久値を超えた場合は破壊
            attackCard.DamageDestroy();
            defenceCard.DamageDestroy();

            // 攻撃パネルを非表示にし、攻撃不可にする
            attackCard.view.SetCanAttackPanel(false);
            attackCard.model.canAttack = false;
            attackCard.model.canITF = false;

            if (isitf)
            {
                // 妨害カードがあった場合は攻撃カードの攻撃効果も発動させる
                defenceCard.view.SetCanAttackPanel(false);
                defenceCard.model.canITF = false;
            }
        }
    }

    // 妨害選択要求を受け取る RPC（選択処理は選択権を持つクライアントがローカルコルーチンで実行し、
    // 結果を別 RPC で全員に通知する設計）
    [PunRPC]
    public void SelectInterferenceRPC(int targetID, PhotonMessageInfo info)
    {
        // 初期未決定状態に設定
        selectedInterferenceCardID = INTERFERENCE_UNDECIDED;

        // RPC 発行元が相手であれば、選択 UI を表示してローカルコルーチンで選択を行い結果を送信する
        if (PhotonNetwork.LocalPlayer.ActorNumber != info.Sender.ActorNumber)
        {
            StartCoroutine(HandleLocalInterferenceSelection(targetID));
        }
        // それ以外のクライアントは結果通知 RPC を受け取るまで待機する（コルーチン側で WaitUntil する）
    }

    // 選択結果を全員に通知する RPC
    [PunRPC]
    public void SelectInterferenceResultRPC(int cardInsID, PhotonMessageInfo info)
    {
        selectedInterferenceCardID = cardInsID;
    }

    // 選択権を持つローカルクライアントが選択 UI を表示して結果を RPC で送るコルーチン
    IEnumerator HandleLocalInterferenceSelection(int targetID)
    {
        List<CardController> selectableCards = new List<CardController>(playerField.GetComponentsInChildren<CardController>());
        selectableCards = selectableCards.Where(card => (card.model.interference && card.cardInsID != targetID　&& card.model.canITF)).ToList();

        int resultID = INTERFERENCE_NONE;

        if (selectableCards.Count > 0)
        {
            // コルーチン版の選択処理を起動して完了を待つ
            yield return StartCoroutine(StartCardSelection(selectableCards, 1));

            // 選択結果を取得し、選択があればそのカードIDを送信
            if (SelectionResults != null && SelectionResults.Count > 0 && SelectionResults[0] != null)
            {
                resultID = SelectionResults[0].cardInsID;
            }
        }
        else
        {
            // 選択可能カードがない場合は妨害なしを示す値を送る
            resultID = INTERFERENCE_NONE;
        }

        // 結果を全員に通知する
        photonView.RPC("SelectInterferenceResultRPC", RpcTarget.All, resultID);

        yield break;
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
            card.model.canITF = canAttack; // 攻撃可能なカードは妨害も可能にする
            card.view.SetCanAttackPanel(!card.model.noaction && canAttack);
        }
    }

    public void CallDevote(CardController attackCard)
    {
        // 選択待ちモード中は操作を受け付けない
        if (isSelectingCard)
        {
            return;
        }

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
        // 選択待ちモード中はカード使用を受け付けない
        if (isSelectingCard)
        {
            return;
        }

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

            SetPlace(card, PlaceList.Field);
            card.movement.cardParent = playerField;
            card.DropField();
            StartCoroutine(ProcessEffectQueueOne());
        }
        else if (card.model.cardCategory == CardCategory.Grace)
        {
            CallReduceSeeds(card.model.cost, true);
            UseCardEffect(card, card, CardEffectType.Grace);
            StartCoroutine(ProcessEffectQueueOne());
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
