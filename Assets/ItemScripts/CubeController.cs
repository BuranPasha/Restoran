using Photon.Pun;
using UnityEngine;

public class CubeController : MonoBehaviourPun
{
    void Start()
    {
        // Küpün sahipliðini MasterClient'a ver (veya null yap)
        if (photonView != null)
        {
            photonView.TransferOwnership(PhotonNetwork.MasterClient); // Sahipliði MasterClient'a ver
            // veya
            // photonView.TransferOwnership(0); // Sahipliði null yap (sahipsiz)
        }
    }
}