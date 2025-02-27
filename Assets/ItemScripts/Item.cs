using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum MeatState { Raw, Cooked, Burnt }
    public MeatState currentState = MeatState.Raw;

    public MeshRenderer meatRenderer;
    public Material rawMaterial;
    public Material cookedMaterial;
    public Material burntMaterial;

    void Start()
    {
        UpdateMaterial();
    }

    public void SetCooked()
    {
        // Eðer zaten Cooked veya Burnt ise piþirme!
        if (currentState != MeatState.Raw) return;

        currentState = MeatState.Cooked;
        UpdateMaterial();
        photonView.RPC("RPC_SetCooked", RpcTarget.AllBuffered);
    }

    public void SetBurnt()
    {
        if (currentState == MeatState.Burnt) return; // Zaten yanmýþsa tekrar çaðýrma!

        currentState = MeatState.Burnt;
        UpdateMaterial();
        photonView.RPC("RPC_SetBurnt", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_SetCooked()
    {
        currentState = MeatState.Cooked;
        UpdateMaterial();
    }

    [PunRPC]
    void RPC_SetBurnt()
    {
        currentState = MeatState.Burnt;
        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        Debug.Log($"Materyal Güncelleniyor: {currentState}");

        if (meatRenderer == null) return;

        switch (currentState)
        {
            case MeatState.Raw:
                meatRenderer.material = rawMaterial;
                Debug.Log("Raw materyali aktif!");
                break;
            case MeatState.Cooked:
                meatRenderer.material = cookedMaterial;
                Debug.Log("Cooked materyali aktif!");
                break;
            case MeatState.Burnt:
                meatRenderer.material = burntMaterial;
                Debug.Log("Burnt materyali aktif!");
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
                UpdateMaterial();
            }
        }
    }
}
