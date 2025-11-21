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
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2; // 最大人数を1に設定
        PhotonNetwork.CreateRoom(null, roomOptions); // 入れなければ新規作成
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("ルームに参加しました。");
        PhotonNetwork.LoadLevel("Game"); // ゲームシーンへ
    }
}
