using UnityEngine;

public class Interactable : MonoBehaviour
{
    public void Interact()
    {
        // Malzeme al�nd���nda yap�lacak i�lemler
        Debug.Log(gameObject.name + " al�nd�!");
        gameObject.SetActive(false); // Malzemeyi sahneden kaybet
    }
}
