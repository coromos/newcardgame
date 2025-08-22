using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

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
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1 }); // 入れなければ新規作成
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("ルームに参加しました。");
        PhotonNetwork.LoadLevel("Game"); // ゲームシーンへ
    }
}
