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

    // Varsay�lan false � pan �zerine konuldu�unda ResumeCooking ile true yap�lacak.
    public bool isCookingActive = false;

    void Start()
    {
        UpdateMaterial();
    }

    // Pan �zerine konuldu�unda �a�r�l�r.
    public void ResumeCooking()
    {
        // E�er et �i� ise pi�irme aktif olsun.
        if (currentState == MeatState.Raw)
        {
            isCookingActive = true;
            Debug.Log("Item: Pi�irme aktif edildi (Raw iken).");
        }
        else
        {
            // Zaten cooked veya burnt ise, pi�irme devam etmesin.
            isCookingActive = false;
            Debug.Log("Item: Zaten pi�mi�, pi�irme aktive de�il.");
        }
    }

    // Pan�dan al�nd���nda �a�r�l�r.
    public void PauseCooking()
    {
        isCookingActive = false;
        Debug.Log("Item: Pi�irme duraklat�ld�.");
    }

    public void SetCooked()
    {
        // Yaln�zca et �i�ken ve pi�irme aktifken ge�i� yapal�m.
        if (!isCookingActive || currentState != MeatState.Raw) return;

        currentState = MeatState.Cooked;
        UpdateMaterial();
        photonView.RPC("RPC_SetCooked", RpcTarget.AllBuffered);
    }

    public void SetBurnt()
    {
        // Sadece cooked durumundaysa ve pi�irme aktifken burnt ge�i�i yapal�m.
        if (!isCookingActive || currentState != MeatState.Cooked) return;

        currentState = MeatState.Burnt;
        UpdateMaterial();
        photonView.RPC("RPC_SetBurnt", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_SetCooked()
    {
        if (!isCookingActive) return;
        currentState = MeatState.Cooked;
        UpdateMaterial();
    }

    [PunRPC]
    void RPC_SetBurnt()
    {
        if (!isCookingActive) return;
        currentState = MeatState.Burnt;
        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        Debug.Log("Item: Materyal g�ncelleniyor -> " + currentState);
        if (meatRenderer == null) return;

        switch (currentState)
        {
            case MeatState.Raw:
                meatRenderer.material = rawMaterial;
                break;
            case MeatState.Cooked:
                meatRenderer.material = cookedMaterial;
                break;
            case MeatState.Burnt:
                meatRenderer.material = burntMaterial;
                break;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentState);
            stream.SendNext(isCookingActive);
        }
        else
        {
            MeatState receivedState = (MeatState)stream.ReceiveNext();
            bool receivedCookingActive = (bool)stream.ReceiveNext();
            currentState = receivedState;
            isCookingActive = receivedCookingActive;
            UpdateMaterial();
        }
    }
}
