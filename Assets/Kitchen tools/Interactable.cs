using UnityEngine;

public class Interactable : MonoBehaviour
{
    public void PickUp(GameObject obj, Transform holdParent)
    {
        // Nesneyi al�nd���nda yap�lacak i�lemler
        Debug.Log(gameObject.name + " al�nd�!");

        // Nesneyi oyuncunun eline ta��
        transform.SetParent(holdParent);  // Elin objesini tutacak alan (�rne�in, player'�n elleri)
        transform.localPosition = Vector3.zero;  // Nesneyi elin ortas�na yerle�tir
        transform.localRotation = Quaternion.identity;  // Nesnenin y�n�n� d�zelt
        GetComponent<Rigidbody>().isKinematic = true;  // Fiziksel etkilerden ��kar
    }

    public void Drop(GameObject obj)
    {
        // Nesneyi b�rak�ld���nda yap�lacak i�lemler
        Debug.Log(gameObject.name + " b�rak�ld�!");

        transform.SetParent(null);  // Nesneyi oyuncudan ay�r
        GetComponent<Rigidbody>().isKinematic = false;  // Fiziksel etkileri geri getir
    }
}
