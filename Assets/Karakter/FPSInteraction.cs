using Photon.Pun;
using UnityEngine;

public class FPSInteraction : MonoBehaviourPunCallbacks, IPunObservable
{
    public float interactionDistance = 3f;  // Etkileþim mesafesi
    public Transform holdPosition;  // Oyuncunun el pozisyonu
    private GameObject heldObject = null;  // Þu anda tutulan nesne
    private Rigidbody heldRigidbody;  // Tutulan nesnenin Rigidbody bileþeni
    private PhotonView heldObjectPhotonView;  // Nesnenin PhotonView bileþeni

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && photonView.IsMine)
        {
            if (heldObject == null) TryPickUp();
            else DropObject();
        }
    }

    void TryPickUp()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, interactionDistance) && hit.collider.CompareTag("Pickable"))
        {
            GameObject objectToPickUp = hit.collider.gameObject;
            PhotonView objectPhotonView = objectToPickUp.GetComponent<PhotonView>();

            if (objectPhotonView.Owner != null && !objectPhotonView.IsMine) return;

            objectPhotonView.RequestOwnership();
            photonView.RPC("RPC_PickUpObject", RpcTarget.All, objectPhotonView.ViewID);
        }
    }

    [PunRPC]
    void RPC_PickUpObject(int objectViewID)
    {
        GameObject objectToPickUp = PhotonView.Find(objectViewID).gameObject;
        heldObject = objectToPickUp;
        heldObjectPhotonView = heldObject.GetComponent<PhotonView>();
        heldRigidbody = heldObject.GetComponent<Rigidbody>();

        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
        }

        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.Euler(0, 90, 0);
    }

    void DropObject()
    {
        if (heldObject == null) return;

        photonView.RPC("RPC_DropObject", RpcTarget.All, heldObjectPhotonView.ViewID);
    }

    [PunRPC]
    void RPC_DropObject(int objectViewID)
    {
        GameObject droppedObject = PhotonView.Find(objectViewID).gameObject;
        Rigidbody droppedRigidbody = droppedObject.GetComponent<Rigidbody>();

        if (droppedRigidbody != null)
        {
            droppedRigidbody.isKinematic = false;
            droppedRigidbody.useGravity = true;
        }

        droppedObject.transform.SetParent(null);
        heldObject = null;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(heldObject != null ? heldObject.transform.position : Vector3.zero);
            stream.SendNext(heldObject != null ? heldObject.transform.rotation : Quaternion.identity);
        }
        else
        {
            if (heldObject != null)
            {
                Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
                Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();
                heldObject.transform.position = receivedPosition;
                heldObject.transform.rotation = receivedRotation;
            }
        }
    }
}
