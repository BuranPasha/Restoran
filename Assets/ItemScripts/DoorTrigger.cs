using UnityEngine;
using Photon.Pun;

public class DoorTrigger : MonoBehaviour
{
    public DoorController doorController;
    private int playerCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount++;
            if (playerCount == 1) // Ýlk oyuncu girdiðinde kapýyý aç
            {
                doorController.photonView.RPC("SetDoorState", RpcTarget.All, true, other.transform.position);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount--;
            if (playerCount <= 0) // Son oyuncu çýktýðýnda kapýyý kapat
            {
                doorController.photonView.RPC("SetDoorState", RpcTarget.All, false, other.transform.position);
            }
        }
    }
}
