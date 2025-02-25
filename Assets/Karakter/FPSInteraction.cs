using Photon.Pun;
using UnityEngine;

public class FPSInteraction : MonoBehaviourPunCallbacks, IPunObservable
{
    public float interactionDistance = 3f;  // Etkile�im mesafesi
    public Transform holdPosition;  // Oyuncunun el pozisyonu
    private GameObject heldObject = null;  // �u anda tutulan nesne
    private Vector3 originalScale;  // Itemin orijinal scale'ini saklamak i�in bir de�i�ken

    void Update()
    {
        // E tu�una bas�ld���nda
        if (Input.GetKeyDown(KeyCode.E) && photonView.IsMine)
        {
            if (heldObject == null)
            {
                // E�er tutulan nesne yoksa, bir nesneyi al
                TryPickUp();
            }
            else
            {
                // E�er tutulan nesne varsa, b�rak
                DropObject();
            }
        }
    }

    void TryPickUp()
    {
        // Oyuncunun bak�� a��s�ndan belirli bir mesafedeki nesneye bak�yoruz
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Nesneyle etkile�im kurabiliriz
            if (hit.collider.CompareTag("Pickable"))
            {
                heldObject = hit.collider.gameObject;
                originalScale = heldObject.transform.localScale;  // Orijinal scale'ini sakl�yoruz
                PickUpObject(heldObject);
            }
        }
    }

    void PickUpObject(GameObject objectToPickUp)
    {
        // Nesneyi oyuncuya yak�n bir pozisyona ta��yoruz
        // Nesneyi tutarken scale'ini de�i�tirmemek i�in �ncelikle orijinal scale'i geri al�yoruz
        objectToPickUp.transform.SetParent(holdPosition); // Elin pozisyonu

        // Nesneyi fiziksel olarak etkile�ime sokmamam�z i�in rigidbody'yi kinematik yap�yoruz
        objectToPickUp.GetComponent<Rigidbody>().isKinematic = true;

        // Nesneyi el pozisyonunun merkezine yerle�tiriyoruz
        objectToPickUp.transform.localPosition = Vector3.zero;

        // Nesneyi d�nd�rme i�lemi yapabiliriz (iste�e ba�l�)
        objectToPickUp.transform.localRotation = Quaternion.Euler(0, 90, 0); // Y ekseninde 90 derece d�nd�rme

        // Scale'ini sabitleme
        objectToPickUp.transform.localScale = originalScale; // Orijinal scale'i geri getiriyoruz
    }

    void DropObject()
    {
        // Nesneyi tekrar serbest b�rak
        heldObject.GetComponent<Rigidbody>().isKinematic = false; // Fiziksel etkiye tekrar izin ver
        heldObject.transform.SetParent(null);  // Nesne art�k oyuncunun elinde de�il
        heldObject = null; // �u anda tutulan nesne yok
    }

    // Photon �zerinden senkronize etmek i�in OnPhotonSerializeView fonksiyonu
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Nesne tutuldu�unda, durumu yazd�r
            stream.SendNext(heldObject != null);
        }
        else
        {
            // Di�er oyunculardan gelen verilerle g�ncelleme
            bool isObjectHeld = (bool)stream.ReceiveNext();
            if (!isObjectHeld && heldObject != null)
            {
                DropObject();
            }
        }
    }
}
