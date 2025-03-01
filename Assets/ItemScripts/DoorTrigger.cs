using UnityEngine;
using Photon.Pun;

public class DoorTrigger : MonoBehaviour
{
    public DoorController doorController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Oyuncu kap�ya yakla��nca, kap�y� a�
            doorController.photonView.RPC("SetDoorState", RpcTarget.All, true, other.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Oyuncu kap�y� terk ederse, kap�y� kapat
            doorController.photonView.RPC("SetDoorState", RpcTarget.All, false, other.transform.position);
        }
    }
}
