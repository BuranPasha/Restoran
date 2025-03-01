using Photon.Pun;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorController doorController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tüm oyuncular için kapýlarý aç
            doorController.photonView.RPC("SetDoorState", RpcTarget.All, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tüm oyuncular için kapýlarý kapat
            doorController.photonView.RPC("SetDoorState", RpcTarget.All, false);
        }
    }
}