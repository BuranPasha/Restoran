using Photon.Pun;
using UnityEngine;

public class Stove : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isOn = false;  // Ocak açýk mý kapalý mý?
    public GameObject fireEffect;  // Ocak açýkken gösterilecek ateþ efekti
    public Light stoveLight;  // Ocak açýkken yanacak ýþýk (opsiyonel)
    public Pan pan;  // Tava referansý
    public float cookingTime = 10f;  // Piþirme süresi (saniye)
    public float burningTime = 15f;  // Yanma süresi (saniye)
    private float currentCookingTime = 0f;  // Geçen piþirme süresi
    private bool isCooking = false;  // Piþirme iþlemi devam ediyor mu?
    public float interactionDistance = 3f;  // Etkileþim mesafesi

    void Update()
    {
        // PhotonView null ise iþlem yapma
        if (photonView == null) return;

        // Herhangi bir oyuncu ocaðý açýp kapatabilir
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleStove();
        }

        // Ocak açýkken ve tavada et varsa piþirme iþlemi baþlat
        if (isOn && pan != null && pan.itemOnPan != null)
        {
            CookItem();
        }
    }

    void ToggleStove()
    {
        isOn = !isOn;
        photonView.RPC("RPC_ToggleStove", RpcTarget.All, isOn);
    }

    [PunRPC]
    void RPC_ToggleStove(bool stoveState)
    {
        isOn = stoveState;
        if (fireEffect != null) fireEffect.SetActive(isOn);  // Ateþ efekti açýk/kapalý durumuna göre güncellenir
        if (stoveLight != null) stoveLight.enabled = isOn;  // Iþýk açýk/kapalý durumuna göre güncellenir
    }

    void CookItem()
    {
        if (!isCooking)
        {
            isCooking = true;
            currentCookingTime = 0f;
        }

        currentCookingTime += Time.deltaTime;

        if (currentCookingTime >= cookingTime && currentCookingTime < burningTime)
        {
            // Piþirme tamamlandý
            pan.itemOnPan.GetComponent<Item>().SetCooked();
        }
        else if (currentCookingTime >= burningTime)
        {
            // Yanma durumu
            pan.itemOnPan.GetComponent<Item>().SetBurnt();
            isCooking = false;  // Piþirme iþlemi durdurulur
        }
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
            if (fireEffect != null) fireEffect.SetActive(isOn);  // Ateþ efekti güncellenir
            if (stoveLight != null) stoveLight.enabled = isOn;  // Iþýk güncellenir
        }
    }
}
