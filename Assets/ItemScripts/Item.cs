using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum MeatState { Raw, Cooked, Burnt }
    public MeatState currentState = MeatState.Raw;

    // Pi�irme durumunu g�ncelleyen metod
    public void SetCooked()
    {
        if (currentState != MeatState.Cooked)
        {
            currentState = MeatState.Cooked;
            photonView.RPC("RPC_SetCooked", RpcTarget.All);
        }
    }

    // Yanma durumunu g�ncelleyen metod
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
        Debug.Log("Et pi�ti!");
        // G�rsel olarak pi�mi� durumu g�ncelle (�rne�in, materyal de�i�tir)
    }

    [PunRPC]
    void RPC_SetBurnt()
    {
        currentState = MeatState.Burnt;
        Debug.Log("Et yand�!");
        // G�rsel olarak yanm�� durumu g�ncelle (�rne�in, materyal de�i�tir)
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