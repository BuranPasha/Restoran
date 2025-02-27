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

        if (isOn && pan != null && pan.itemOnPan != null)
        {
            CookItem();
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
        if (stoveLight != null) stoveLight.enabled = isOn;
    }

    void CookItem()
    {
        if (!isCooking)
        {
            isCooking = true;
            currentCookingTime = 0f;
        }

        currentCookingTime += Time.deltaTime;

        Item item = pan.itemOnPan?.GetComponent<Item>();
        if (item == null) return;

        // Eger et yanm��sa, pi�irmeyi durdur.
        if (item.currentState == Item.MeatState.Burnt)
        {
            Debug.Log("Yanm�� et tekrar pi�irilemez!");
            return;
        }

        if (currentCookingTime >= cookingTime && currentCookingTime < burningTime)
        {
            if (item.currentState == Item.MeatState.Raw)
            {
                item.SetCooked();
            }
        }
        else if (currentCookingTime >= burningTime)
        {
            item.SetBurnt();
            isCooking = false;
        }
    }

    // Item tavadan al�nd���nda oca��n pi�irme zaman�n� s�f�rlay�p, oca�� kapatan metot
    public void ResetCookingAndTurnOff()
    {
        photonView.RPC("RPC_ResetCookingAndTurnOff", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_ResetCookingAndTurnOff()
    {
        currentCookingTime = 0f;
        isCooking = false;
        // Oca�� kapatal�m...
        isOn = false;
        fireEffect?.SetActive(isOn);
        if (stoveLight != null) stoveLight.enabled = isOn;
        Debug.Log("Oca��n pi�irme zaman� s�f�rland� ve ocak kapat�ld�!");
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
            if (stoveLight != null) stoveLight.enabled = isOn;
        }
    }
}
