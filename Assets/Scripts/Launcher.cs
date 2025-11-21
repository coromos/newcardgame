using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Launcher : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // Photonへ接続
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom(); // ルームに入る
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("空きルームがないため、新規ルームを作成します。");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2; // 最大人数を1に設定
        PhotonNetwork.CreateRoom(null, roomOptions); // 入れなければ新規作成
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("ルームに参加しました。");
        // 既に必要人数が揃っていればシーン移動
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            // マスタークライアントがシーン読み込みを行う（重複防止）
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("Game"); // ゲームシーンへ
            }
        }
        else
        {
            Debug.Log("他のプレイヤーを待っています...");
            // ここでは待機し、他プレイヤーが参加したときに OnPlayerEnteredRoom が呼ばれる
        }
    }

    // 他プレイヤーが参加したときに呼ばれるコールバック
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"プレイヤーが参加しました: {newPlayer.NickName} (現在 {PhotonNetwork.CurrentRoom.PlayerCount} 人)");
        // 必要人数に達したらマスタークライアントがシーンを読み込む
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("Game");
            }
        }
    }

    // （任意）プレイヤーが抜けたときのログ
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"プレイヤーが退出しました: {otherPlayer.NickName} (現在 {PhotonNetwork.CurrentRoom.PlayerCount} 人)");
    }
}
