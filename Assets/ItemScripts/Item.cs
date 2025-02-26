using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum MeatState { Raw, Cooked, Burnt }
    public MeatState currentState = MeatState.Raw;

    public GameObject rawModel;    // �i� et modeli
    public GameObject cookedModel; // Pi�mi� et modeli
    public GameObject burntModel;  // Yanm�� et modeli

    void Start()
    {
        UpdateModel(); // Ba�lang��ta modeli g�ncelle
    }

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
        UpdateModel(); // Modeli g�ncelle
        Debug.Log("Et pi�ti!");
    }

    [PunRPC]
    void RPC_SetBurnt()
    {
        currentState = MeatState.Burnt;
        UpdateModel(); // Modeli g�ncelle
        Debug.Log("Et yand�!");
    }

    // Modeli g�ncelleyen metod
    void UpdateModel()
    {
        // T�m modelleri gizle
        if (rawModel != null) rawModel.SetActive(false);
        if (cookedModel != null) cookedModel.SetActive(false);
        if (burntModel != null) burntModel.SetActive(false);

        // Duruma g�re ilgili modeli g�ster
        switch (currentState)
        {
            case MeatState.Raw:
                if (rawModel != null) rawModel.SetActive(true);
                break;
            case MeatState.Cooked:
                if (cookedModel != null) cookedModel.SetActive(true);
                break;
            case MeatState.Burnt:
                if (burntModel != null) burntModel.SetActive(true);
                break;
        }
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
            UpdateModel(); // Modeli g�ncelle
        }
    }
}