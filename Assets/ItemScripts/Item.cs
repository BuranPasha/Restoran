using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum MeatState { Raw, Cooked, Burnt }
    public MeatState currentState = MeatState.Raw;

    // Piþirme durumunu güncelleyen metod
    public void SetCooked()
    {
        currentState = MeatState.Cooked;
        photonView.RPC("RPC_SetCooked", RpcTarget.All);
    }

    // Piþirme durumunu tüm oyunculara senkronize eden RPC metodu
    [PunRPC]
    void RPC_SetCooked()
    {
        currentState = MeatState.Cooked;
        // Görsel olarak piþmiþ durumu güncelle (örneðin, materyal deðiþtir)
    }

    // Photon senkronizasyonu için gerekli metod
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