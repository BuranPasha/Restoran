using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class TableScript : MonoBehaviourPun
{
    // Masa üzerindeki slotlar
    public Transform yemekSlot;
    public Transform tatliSlot;
    public Transform salataSlot;
    public Transform icecekSlot;
    public Transform catalSlot;
    public Transform kasikSlot;
    public Transform býçakSlot;

    private Dictionary<string, Transform> slotDictionary;

    // Baþlangýçta her slotu dictionary'ye ekleyelim
    void Start()
    {
        slotDictionary = new Dictionary<string, Transform>
        {
            { "Yemek", yemekSlot },
            { "Tatlý", tatliSlot },
            { "Salata", salataSlot },
            { "Ýçecek", icecekSlot },
            { "Çatal", catalSlot },
            { "Kaþýk", kasikSlot },
            { "Býçak", býçakSlot }
        };
    }

    [PunRPC]
    public void RPC_PlaceItemsFromTray(int[] itemViewIDs)
    {
        // Tepsiden item'larý alýp, masa üzerindeki doðru slot'a yerleþtiriyoruz
        for (int i = 0; i < itemViewIDs.Length; i++)
        {
            if (itemViewIDs[i] == -1) continue; // Boþ slot

            PhotonView itemView = PhotonView.Find(itemViewIDs[i]);
            if (itemView == null) continue;

            GameObject item = itemView.gameObject;
            ItemScript itemScript = item.GetComponent<ItemScript>();

            if (itemScript != null)
            {
                // ItemType'a göre doðru slot'u bulup item'ý oraya yerleþtiriyoruz
                string itemType = itemScript.itemType.ToString();
                if (slotDictionary.ContainsKey(itemType))
                {
                    Transform targetSlot = slotDictionary[itemType];
                    item.transform.SetParent(targetSlot);
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;

                    Rigidbody rb = item.GetComponent<Rigidbody>();
                    if (rb != null) rb.isKinematic = true;
                }
            }
        }
    }

    public bool HasItemsOnTable()
    {
        // Masa üzerinde herhangi bir item olup olmadýðýný kontrol ediyoruz
        foreach (Transform slot in slotDictionary.Values)
        {
            if (slot.childCount > 0) return true; // Eðer herhangi bir slotta item varsa true döner
        }
        return false;
    }
}
