using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // Photon�֐ڑ�
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom(); // ���[���ɓ���
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1 }); // ����Ȃ���ΐV�K�쐬
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("���[���ɎQ�����܂����B");
        PhotonNetwork.LoadLevel("Game"); // �Q�[���V�[����
    }
}
