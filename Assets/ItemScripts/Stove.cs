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

    public float interactionRange = 3f; // Oyuncunun en fazla ne kadar uzaktan ocakla etkileþime girebileceði
    private Transform player; // Oyuncunun transform'u

    void Update()
    {
        if (player == null)
        {
            FindLocalPlayer(); // Oyuncu henüz atanmamýþsa bul
            return; // Oyuncu yoksa devam etme
        }

        if (Input.GetKeyDown(KeyCode.F) && IsPlayerClose())
        {
            ToggleStove();
        }

        if (!isOn) return;
        if (pan == null) return;

        if (pan.itemOnPan != null)
        {
            CookItem();
        }
        else
        {
            isCooking = false;
            currentCookingTime = 0f;
        }
    }

    void FindLocalPlayer()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (obj.GetComponent<PhotonView>().IsMine) // Yalnýzca bizim kontrol ettiðimiz oyuncuyu al
            {
                player = obj.transform;
                Debug.Log("Local player found!");
                break;
            }
        }
    }

    bool IsPlayerClose()
    {
        if (player == null) return false;
        return Vector3.Distance(player.position, transform.position) <= interactionRange;
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
            currentCookingTime = 0f;
        }

        currentCookingTime += Time.deltaTime;

        Item item = pan.itemOnPan?.GetComponent<Item>();
        if (item == null) return;

        if (item.currentState == Item.MeatState.Burnt)
        {
            Debug.Log("Stove: Et zaten yanmýþ, piþirme durdu.");
            isCooking = false;
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
