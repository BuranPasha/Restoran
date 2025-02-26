using Photon.Pun;
using UnityEngine;

public class Pan : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject itemOnPan;  // Tavada piþen eþya
    public Stove stove;  // Ocak referansý

    public void PlaceItem(GameObject item)
    {
        itemOnPan = item;
        item.SetActive(true);  // Et tavada görünür hale gelir
        item.transform.position = transform.position;  // Et tavaya yerleþir
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