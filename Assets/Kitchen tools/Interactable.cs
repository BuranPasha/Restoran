using UnityEngine;

public class Interactable : MonoBehaviour
{
    public void PickUp(GameObject obj, Transform holdParent)
    {
        // Nesneyi alýndýðýnda yapýlacak iþlemler
        Debug.Log(gameObject.name + " alýndý!");

        // Nesneyi oyuncunun eline taþý
        transform.SetParent(holdParent);  // Elin objesini tutacak alan (örneðin, player'ýn elleri)
        transform.localPosition = Vector3.zero;  // Nesneyi elin ortasýna yerleþtir
        transform.localRotation = Quaternion.identity;  // Nesnenin yönünü düzelt
        GetComponent<Rigidbody>().isKinematic = true;  // Fiziksel etkilerden çýkar
    }

    public void Drop(GameObject obj)
    {
        // Nesneyi býrakýldýðýnda yapýlacak iþlemler
        Debug.Log(gameObject.name + " býrakýldý!");

        transform.SetParent(null);  // Nesneyi oyuncudan ayýr
        GetComponent<Rigidbody>().isKinematic = false;  // Fiziksel etkileri geri getir
    }
}
