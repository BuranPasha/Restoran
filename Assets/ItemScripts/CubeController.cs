using Photon.Pun;
using UnityEngine;

public class CubeController : MonoBehaviourPun
{
    void Start()
    {
        // K�p�n sahipli�ini MasterClient'a ver (veya null yap)
        if (photonView != null)
        {
            photonView.TransferOwnership(PhotonNetwork.MasterClient); // Sahipli�i MasterClient'a ver
            // veya
            // photonView.TransferOwnership(0); // Sahipli�i null yap (sahipsiz)
        }
    }
}