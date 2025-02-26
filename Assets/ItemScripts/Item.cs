using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum MeatState { Raw, Cooked, Burnt }
    public MeatState currentState = MeatState.Raw;

    public GameObject rawModel;    // Çið et modeli
    public GameObject cookedModel; // Piþmiþ et modeli
    public GameObject burntModel;  // Yanmýþ et modeli

    void Start()
    {
        UpdateModel(); // Baþlangýçta modeli güncelle
    }

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
        UpdateModel(); // Modeli güncelle
        Debug.Log("Et piþti!");
    }

    [PunRPC]
    void RPC_SetBurnt()
    {
        currentState = MeatState.Burnt;
        UpdateModel(); // Modeli güncelle
        Debug.Log("Et yandý!");
    }

    // Modeli güncelleyen metod
    void UpdateModel()
    {
        // Tüm modelleri gizle
        if (rawModel != null) rawModel.SetActive(false);
        if (cookedModel != null) cookedModel.SetActive(false);
        if (burntModel != null) burntModel.SetActive(false);

        // Duruma göre ilgili modeli göster
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
            UpdateModel(); // Modeli güncelle
        }
    }
}