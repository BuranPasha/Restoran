using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum MeatState { Raw, Cooked, Burnt }
    public MeatState currentState = MeatState.Raw;

    // Pi�irme durumunu g�ncelleyen metod
    public void SetCooked()
    {
        currentState = MeatState.Cooked;
        photonView.RPC("RPC_SetCooked", RpcTarget.All);
    }

    // Pi�irme durumunu t�m oyunculara senkronize eden RPC metodu
    [PunRPC]
    void RPC_SetCooked()
    {
        currentState = MeatState.Cooked;
        // G�rsel olarak pi�mi� durumu g�ncelle (�rne�in, materyal de�i�tir)
    }

    // Photon senkronizasyonu i�in gerekli metod
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentState);
        }
        else
        {
            currentState = (MeatState)stream.ReceiveNext();
        }
    }
}