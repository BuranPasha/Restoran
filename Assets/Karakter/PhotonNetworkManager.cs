using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Debug.Log("Photon’a baðlanýyor...");
        PhotonNetwork.ConnectUsingSettings(); // Photon’a baðlan
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon’a baðlandý!");
        PhotonNetwork.JoinLobby(); // Lobiye katýl
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Lobiye giriþ yapýldý.");
        CreateOrJoinRoom();
    }

    void CreateOrJoinRoom()
    {
        string roomName = "RestaurantRoom"; // Oda adý
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4; // 4 kiþilik oyun

        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Odaya giriþ yapýldý!");
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Vector3 spawnPos = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
        PhotonNetwork.Instantiate("FPS_Player", spawnPos, Quaternion.identity);
    }
}
