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

    // Varsayýlan false – pan üzerine konulduðunda ResumeCooking ile true yapýlacak.
    public bool isCookingActive = false;

    void Start()
    {
        UpdateMaterial();
    }

    // Pan üzerine konulduðunda çaðrýlýr.
    public void ResumeCooking()
    {
        // Eðer et çið ise piþirme aktif olsun.
        if (currentState == MeatState.Raw)
        {
            isCookingActive = true;
            Debug.Log("Item: Piþirme aktif edildi (Raw iken).");
        }
        else
        {
            // Zaten cooked veya burnt ise, piþirme devam etmesin.
            isCookingActive = false;
            Debug.Log("Item: Zaten piþmiþ, piþirme aktive deðil.");
        }
    }

    // Pan’dan alýndýðýnda çaðrýlýr.
    public void PauseCooking()
    {
        isCookingActive = false;
        Debug.Log("Item: Piþirme duraklatýldý.");
    }

    public void SetCooked()
    {
        // Yalnýzca et çiðken ve piþirme aktifken geçiþ yapalým.
        if (!isCookingActive || currentState != MeatState.Raw) return;

        currentState = MeatState.Cooked;
        UpdateMaterial();
        photonView.RPC("RPC_SetCooked", RpcTarget.AllBuffered);
    }

    public void SetBurnt()
    {
        // Sadece cooked durumundaysa ve piþirme aktifken burnt geçiþi yapalým.
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
        Debug.Log("Item: Materyal güncelleniyor -> " + currentState);
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
