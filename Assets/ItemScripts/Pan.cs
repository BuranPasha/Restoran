using Photon.Pun;
using UnityEngine;

public class Pan : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject itemOnPan;  // Tavada pi�en e�ya
    public Stove stove;  // Ocak referans�

    public void PlaceItem(GameObject item)
    {
        itemOnPan = item;
        item.SetActive(true);  // Et tavada g�r�n�r hale gelir
        item.transform.position = transform.position;  // Et tavaya yerle�ir
    }

    public GameObject TakeItem()
    {
        GameObject item = itemOnPan;
        itemOnPan = null;
        return item;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(itemOnPan != null ? itemOnPan.transform.position : Vector3.zero);
            stream.SendNext(itemOnPan != null ? itemOnPan.transform.rotation : Quaternion.identity);
        }
        else
        {
            if (itemOnPan != null)
            {
                Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
                Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();
                itemOnPan.transform.position = receivedPosition;
                itemOnPan.transform.rotation = receivedRotation;
            }
        }
    }
}