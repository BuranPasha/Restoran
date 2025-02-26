using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum MeatState { Raw, Cooked, Burnt }
    public MeatState currentState = MeatState.Raw;

    // Piþirme durumunu güncelleyen metod
    public void SetCooked()
    {
        if (currentState != MeatState.Cooked)
        {
            currentState = MeatState.Cooked;
            photonView.RPC("RPC_SetCooked", RpcTarget.All);
        }
    }

    // Yanma durumunu güncelleyen metod
    public void SetBurnt()
    {
        if (currentState != MeatState.Burnt)
        {
            currentState = MeatState.Burnt;
            photonView.RPC("RPC_SetBurnt", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_SetCooked()
    {
        currentState = MeatState.Cooked;
        Debug.Log("Et piþti!");
        // Görsel olarak piþmiþ durumu güncelle (örneðin, materyal deðiþtir)
    }

    [PunRPC]
    void RPC_SetBurnt()
    {
        currentState = MeatState.Burnt;
        Debug.Log("Et yandý!");
        // Görsel olarak yanmýþ durumu güncelle (örneðin, materyal deðiþtir)
    }

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