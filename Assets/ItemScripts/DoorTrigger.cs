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
            if (playerCount == 1) // �lk oyuncu girdi�inde kap�y� a�
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
            if (playerCount <= 0) // Son oyuncu ��kt���nda kap�y� kapat
            {
                doorController.photonView.RPC("SetDoorState", RpcTarget.All, false, other.transform.position);
            }
        }
    }
}
