using UnityEngine;
using Photon.Pun;

public class DoorTrigger : MonoBehaviour
{
    public DoorController doorController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Oyuncu kapýya yaklaþýnca, kapýyý aç
            doorController.photonView.RPC("SetDoorState", RpcTarget.All, true, other.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Oyuncu kapýyý terk ederse, kapýyý kapat
            doorController.photonView.RPC("SetDoorState", RpcTarget.All, false, other.transform.position);
        }
    }
}
