using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum MeatState { Raw, Cooked, Burnt }
    public MeatState currentState = MeatState.Raw;

    public GameObject rawModel;
    public GameObject cookedModel;
    public GameObject burntModel;

    void Start()
    {
        UpdateModel();
    }

    public void SetCooked()
    {
        if (currentState != MeatState.Cooked)
        {
            currentState = MeatState.Cooked;
            UpdateModel();
            photonView.RPC("RPC_SetCooked", RpcTarget.AllBuffered);
        }
    }

    public void SetBurnt()
    {
        if (currentState != MeatState.Burnt)
        {
            currentState = MeatState.Burnt;
            UpdateModel();
            photonView.RPC("RPC_SetBurnt", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void RPC_SetCooked()
    {
        currentState = MeatState.Cooked;
        UpdateModel();
    }

    [PunRPC]
    void RPC_SetBurnt()
    {
        currentState = MeatState.Burnt;
        UpdateModel();
    }

    void UpdateModel()
    {
        Debug.Log($"Model Güncelleniyor: {currentState}");

        rawModel?.SetActive(false);
        cookedModel?.SetActive(false);
        burntModel?.SetActive(false);

        switch (currentState)
        {
            case MeatState.Raw:
                rawModel?.SetActive(true);
                Debug.Log("Raw model aktif!");
                break;
            case MeatState.Cooked:
                cookedModel?.SetActive(true);
                Debug.Log("Cooked model aktif!");
                break;
            case MeatState.Burnt:
                burntModel?.SetActive(true);
                Debug.Log("Burnt model aktif!");
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
            MeatState receivedState = (MeatState)stream.ReceiveNext();
            if (currentState != receivedState)
            {
                currentState = receivedState;
                UpdateModel();
            }
        }
    }
}
