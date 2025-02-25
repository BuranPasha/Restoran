using Photon.Pun;
using UnityEngine;

public class FPSInteraction : MonoBehaviourPunCallbacks, IPunObservable
{
    public float interactionDistance = 3f;  // Etkileþim mesafesi
    public Transform holdPosition;  // Oyuncunun el pozisyonu
    private GameObject heldObject = null;  // Þu anda tutulan nesne
    private Vector3 originalScale;  // Itemin orijinal scale'ini saklamak için bir deðiþken
    private bool isItemHeld = false;  // Item tutma durumu (local oyuncu için)

    void Update()
    {
        // E tuþuna basýldýðýnda
        if (Input.GetKeyDown(KeyCode.E) && photonView.IsMine)
        {
            if (heldObject == null)
            {
                // Eðer tutulan nesne yoksa, bir nesneyi al
                TryPickUp();
            }
            else
            {
                // Eðer tutulan nesne varsa, býrak
                DropObject();
            }
        }
    }

    void TryPickUp()
    {
        // Oyuncunun bakýþ açýsýndan belirli bir mesafedeki nesneye bakýyoruz
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Nesneyle etkileþim kurabiliriz
            if (hit.collider.CompareTag("Pickable"))
            {
                heldObject = hit.collider.gameObject;
                originalScale = heldObject.transform.localScale;  // Orijinal scale'ini saklýyoruz
                PickUpObject(heldObject);
            }
        }
    }

    void PickUpObject(GameObject objectToPickUp)
    {
        // Nesneyi oyuncuya yakýn bir pozisyona taþýyoruz
        // Nesneyi tutarken scale'ini deðiþtirmemek için öncelikle orijinal scale'i geri alýyoruz
        objectToPickUp.transform.SetParent(holdPosition); // Elin pozisyonu

        // Nesneyi fiziksel olarak etkileþime sokmamamýz için rigidbody'yi kinematik yapýyoruz
        objectToPickUp.GetComponent<Rigidbody>().isKinematic = true;

        // Nesneyi el pozisyonunun merkezine yerleþtiriyoruz
        objectToPickUp.transform.localPosition = Vector3.zero;

        // Nesneyi döndürme iþlemi yapabiliriz (isteðe baðlý)
        objectToPickUp.transform.localRotation = Quaternion.Euler(0, 90, 0); // Y ekseninde 90 derece döndürme

        // Scale'ini sabitleme
        objectToPickUp.transform.localScale = originalScale; // Orijinal scale'i geri getiriyoruz

        // Item tutulma durumunu iþaretle
        isItemHeld = true;
    }

    void DropObject()
    {
        // Nesneyi tekrar serbest býrak
        heldObject.GetComponent<Rigidbody>().isKinematic = false; // Fiziksel etkiye tekrar izin ver
        heldObject.transform.SetParent(null);  // Nesne artýk oyuncunun elinde deðil
        heldObject = null; // Þu anda tutulan nesne yok

        // Item tutulma durumunu sýfýrla
        isItemHeld = false;
    }

    // Photon üzerinden senkronize etmek için OnPhotonSerializeView fonksiyonu
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Nesne tutulduðunda, durumu yazdýr
            stream.SendNext(isItemHeld);
        }
        else
        {
            // Diðer oyunculardan gelen verilerle güncelleme
            isItemHeld = (bool)stream.ReceiveNext();

            if (isItemHeld && heldObject == null)
            {
                // Eðer diðer oyuncu itemi tutuyorsa, bu itemi yerel oyuncu için al
                // Burada nesneyi elle alma iþlemi yapýlabilir (örneðin, onu fiziksel olarak yerleþtirme)
            }
            else if (!isItemHeld && heldObject != null)
            {
                // Eðer item býrakýlmýþsa, bunu yerel oyuncuya senkronize et
                DropObject();
            }
        }
    }
}
