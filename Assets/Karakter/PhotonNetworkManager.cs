using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Debug.Log("Photon�a ba�lan�yor...");
        PhotonNetwork.ConnectUsingSettings(); // Photon�a ba�lan
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon�a ba�land�!");
        PhotonNetwork.JoinLobby(); // Lobiye kat�l
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Lobiye giri� yap�ld�.");
        CreateOrJoinRoom();
    }

    void CreateOrJoinRoom()
    {
        string roomName = "RestaurantRoom"; // Oda ad�
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4; // 4 ki�ilik oyun

        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Odaya giri� yap�ld�!");
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Vector3 spawnPos = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
        PhotonNetwork.Instantiate("FPS_Player", spawnPos, Quaternion.identity);
    }
}
