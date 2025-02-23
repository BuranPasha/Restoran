using UnityEngine;

public class Interactable : MonoBehaviour
{
    public void Interact()
    {
        // Malzeme alýndýðýnda yapýlacak iþlemler
        Debug.Log(gameObject.name + " alýndý!");
        gameObject.SetActive(false); // Malzemeyi sahneden kaybet
    }
}
