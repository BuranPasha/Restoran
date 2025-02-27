using Photon.Pun;
using UnityEngine;

public class FPSInteraction : MonoBehaviourPunCallbacks, IPunObservable
{
    public float interactionDistance = 3f;
    public Transform holdPosition;
    public string leftPositionTag = "LeftPosition"; // B�rakma noktas�n�n etiketi
    private GameObject heldObject = null;
    private Rigidbody heldRigidbody;
    private PhotonView heldObjectPhotonView;

    void Update()
    {
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
            {
                TryPickUpOrResetOwnership();
            }
            else
            {
                TryDropOrPlace();
            }
        }
    }

    void TryPickUpOrResetOwnership()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            GameObject targetObject = hit.collider.gameObject;

            if (!targetObject.CompareTag("Pickable")) return;

            PhotonView targetPhotonView = targetObject.GetComponent<PhotonView>();
            if (targetPhotonView == null) return;

            // E�er nesne ba�ka bir oyuncunun elindeyse sahipli�i s�f�rla
            if (targetPhotonView.Owner != null)
            {
                Debug.Log("Nesne ba�ka bir oyuncunun elinde. Sahiplik s�f�rlan�yor...");
                photonView.RPC("RPC_ResetOwnership", RpcTarget.AllBuffered, targetPhotonView.ViewID);
                return;
            }

            // E�er nesne sahipsizse al
            photonView.RPC("RPC_PickUpObject", RpcTarget.AllBuffered, targetPhotonView.ViewID);
        }
    }

    [PunRPC]
    void RPC_ResetOwnership(int objectViewID)
    {
        GameObject objectToReset = PhotonView.Find(objectViewID).gameObject;
        PhotonView objectPhotonView = objectToReset.GetComponent<PhotonView>();

        if (objectPhotonView != null && objectPhotonView.Owner != null)
        {
            objectPhotonView.TransferOwnership(0); // Sahipli�i s�f�rla (kimseye ait de�il)
        }

        Debug.Log("Nesnenin sahipli�i s�f�rland�: " + objectToReset.name);
    }

    [PunRPC]
    void RPC_PickUpObject(int objectViewID)
    {
        GameObject objectToPickUp = PhotonView.Find(objectViewID).gameObject;

        if (heldObject != null)
        {
            ReleaseCurrentObject();
        }

        heldObject = objectToPickUp;
        heldObjectPhotonView = heldObject.GetComponent<PhotonView>();
        heldRigidbody = heldObject.GetComponent<Rigidbody>();

        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.freezeRotation = true;
        }

        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        Debug.Log("Nesne al�nd�: " + heldObject.name);
    }

    void TryDropOrPlace()
    {
        GameObject leftPosition = GameObject.FindGameObjectWithTag(leftPositionTag);

        if (leftPosition != null && Vector3.Distance(transform.position, leftPosition.transform.position) <= interactionDistance)
        {
            photonView.RPC("RPC_PlaceOnLeftPosition", RpcTarget.AllBuffered, heldObjectPhotonView.ViewID, leftPosition.transform.position);
        }
        else
        {
            DropObject();
        }
    }

    [PunRPC]
    void RPC_PlaceOnLeftPosition(int objectViewID, Vector3 position)
    {
        GameObject objectToPlace = PhotonView.Find(objectViewID).gameObject;

        objectToPlace.transform.SetParent(null);
        objectToPlace.transform.position = position;
        objectToPlace.transform.rotation = Quaternion.identity;

        Rigidbody rb = objectToPlace.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        heldObject = null;

        Debug.Log("Nesne yerle�tirildi: " + objectToPlace.name);
    }

    void DropObject()
    {
        if (heldObject == null) return;

        photonView.RPC("RPC_DropObject", RpcTarget.AllBuffered, heldObjectPhotonView.ViewID);
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

        Debug.Log("Nesne b�rak�ld�: " + droppedObject.name);
    }

    void ReleaseCurrentObject()
    {
        if (heldObjectPhotonView != null && heldObjectPhotonView.IsMine)
        {
            heldObjectPhotonView.TransferOwnership(PhotonNetwork.MasterClient);
        }

        if (heldObject != null)
        {
            heldObject.transform.SetParent(null);

            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            heldObject = null;
        }
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
