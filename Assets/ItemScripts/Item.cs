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

    // Item dondurulmuþsa, state artýk güncellenmez.
    private bool isFrozen = false;

    void Start()
    {
        UpdateMaterial();
    }

    // Itemin durumunu dondurur.
    public void FreezeState()
    {
        isFrozen = true;
        Debug.Log("Item state frozen. Bundan sonra piþirme durumu deðiþmeyecek.");
    }

    public void SetCooked()
    {
        if (isFrozen) return; // Durum dondurulmuþsa state deðiþmez.
        if (currentState != MeatState.Raw) return;

        currentState = MeatState.Cooked;
        UpdateMaterial();
        photonView.RPC("RPC_SetCooked", RpcTarget.AllBuffered);
    }

    public void SetBurnt()
    {
        if (isFrozen) return;
        if (currentState == MeatState.Burnt) return;

        currentState = MeatState.Burnt;
        UpdateMaterial();
        photonView.RPC("RPC_SetBurnt", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_SetCooked()
    {
        if (isFrozen) return;
        currentState = MeatState.Cooked;
        UpdateMaterial();
    }

    [PunRPC]
    void RPC_SetBurnt()
    {
        if (isFrozen) return;
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
            stream.SendNext(isFrozen);
        }
        else
        {
            MeatState receivedState = (MeatState)stream.ReceiveNext();
            bool receivedFrozen = (bool)stream.ReceiveNext();

            // Eðer item dondurulmuþsa, gelen güncellemeyi yoksayalým.
            if (!isFrozen)
            {
                currentState = receivedState;
                UpdateMaterial();
            }
            // Diðer istemcilerle freeze bilgisini de senkronize edelim.
            isFrozen = receivedFrozen;
        }
    }
}
