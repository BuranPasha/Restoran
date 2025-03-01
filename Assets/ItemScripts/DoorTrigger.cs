using Photon.Pun;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorController doorController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // T�m oyuncular i�in kap�lar� a�
            doorController.photonView.RPC("SetDoorState", RpcTarget.All, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // T�m oyuncular i�in kap�lar� kapat
            doorController.photonView.RPC("SetDoorState", RpcTarget.All, false);
        }
    }
}