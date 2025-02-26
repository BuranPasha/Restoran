using Photon.Pun;
using UnityEngine;
using static UnityEditor.Progress;

public class Stove : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isOn = false;  // Ocak açýk mý kapalý mý?
    public GameObject fireEffect;  // Ocak açýkken gösterilecek ateþ efekti
    public Pan pan;  // Tava referansý (tavayý ocaða yerleþtirmek için)
    public float cookingTime = 10f;  // Piþirme süresi
    private float currentCookingTime = 0f;  // Geçen piþirme süresi
    private bool isCooking = false;  // Piþirme iþlemi devam ediyor mu?

    void Update()
    {
        // Sadece ocaðýn sahibi ocaðý açýp kapatabilir
        if (Input.GetKeyDown(KeyCode.F) && photonView.IsMine)
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
        fireEffect.SetActive(isOn);  // Ateþ efekti açýk/kapalý durumuna göre güncellenir

        if (!isOn)
        {
            // Ocak kapatýldýðýnda piþirme süresini sýfýrla
            currentCookingTime = 0f;
            isCooking = false;
        }
    }

    void CookItem()
    {
        if (!isCooking)
        {
            isCooking = true;
            currentCookingTime = 0f;
        }

        currentCookingTime += Time.deltaTime;

        if (currentCookingTime >= cookingTime)
        {
            // Piþirme tamamlandý
            pan.itemOnPan.GetComponent<Item>().SetCooked();
            isCooking = false;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Ocak durumunu senkronize et
            stream.SendNext(isOn);
            stream.SendNext(currentCookingTime);
        }
        else
        {
            // Ocak durumunu güncelle
            isOn = (bool)stream.ReceiveNext();
            currentCookingTime = (float)stream.ReceiveNext();
            fireEffect.SetActive(isOn);  // Ateþ efekti güncellenir
        }
    }
}