using Photon.Pun;
using UnityEngine;

public class Stove : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isOn = false;
    public GameObject fireEffect;
    public Light stoveLight;
    public Pan pan;
    public float cookingTime = 10f;
    public float burningTime = 15f;
    private float currentCookingTime = 0f;
    private bool isCooking = false;

    void Update()
    {
        if (photonView == null) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleStove();
        }

        if (!isOn) return;
        if (pan == null) return;

        // E�er pan �zerinde et varsa pi�irme �al���yor.
        if (pan.itemOnPan != null)
        {
            CookItem();
        }
        else
        {
            // Et al�nd�ysa timer�� resetleyelim.
            isCooking = false;
            currentCookingTime = 0f;
        }
    }

    void ToggleStove()
    {
        isOn = !isOn;
        photonView.RPC("RPC_ToggleStove", RpcTarget.AllBuffered, isOn);
    }

    [PunRPC]
    void RPC_ToggleStove(bool stoveState)
    {
        isOn = stoveState;
        fireEffect?.SetActive(isOn);
        if (stoveLight != null)
            stoveLight.enabled = isOn;
    }

    void CookItem()
    {
        if (!isCooking)
        {
            isCooking = true;
            // Yeni pi�irilen etlerde timer s�f�rdan ba�las�n.
            currentCookingTime = 0f;
        }

        currentCookingTime += Time.deltaTime;

        Item item = pan.itemOnPan?.GetComponent<Item>();
        if (item == null) return;

        if (item.currentState == Item.MeatState.Burnt)
        {
            Debug.Log("Stove: Et zaten yanm��, pi�irme durdu.");
            isCooking = false;
            return;
        }

        // E�er s�re pi�irme aral���ndaysa (�rne�in 10s'da raw ? cooked ge�i�i)
        if (currentCookingTime >= cookingTime && currentCookingTime < burningTime)
        {
            if (item.currentState == Item.MeatState.Raw)
            {
                item.SetCooked();
            }
        }
        else if (currentCookingTime >= burningTime)
        {
            // E�er et pan �zerindeyken uygunsa burnt'a ge�sin.
            item.SetBurnt();
            isCooking = false;
        }
    }

    public void ResetCooking()
    {
        photonView.RPC("RPC_ResetCooking", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_ResetCooking()
    {
        currentCookingTime = 0f;
        isCooking = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isOn);
            stream.SendNext(currentCookingTime);
        }
        else
        {
            isOn = (bool)stream.ReceiveNext();
            currentCookingTime = (float)stream.ReceiveNext();
            fireEffect?.SetActive(isOn);
            if (stoveLight != null)
                stoveLight.enabled = isOn;
        }
    }
}
