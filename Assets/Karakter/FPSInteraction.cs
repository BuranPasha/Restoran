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
            if (heldObject == null) TryPickUp();
            else TryDropOrPlace();
        }
    }

    void TryPickUp()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            GameObject objectToPickUp = hit.collider.gameObject;

            // Sadece "Pickable" tag'ine sahip nesneleri al
            if (!objectToPickUp.CompareTag("Pickable"))
            {
                Debug.Log("Bu nesne al�namaz: " + objectToPickUp.name);
                return;
            }

            PhotonView objectPhotonView = objectToPickUp.GetComponent<PhotonView>();

            if (objectPhotonView == null)
            {
                Debug.Log("Nesne PhotonView bile�enine sahip de�il: " + objectToPickUp.name);
                return;
            }

            // E�er ba�kas� sahip de�ilse direkt alabiliriz
            if (!objectPhotonView.IsMine)
            {
                objectPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            photonView.RPC("RPC_PickUpObject", RpcTarget.All, objectPhotonView.ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    void TryDropOrPlace()
    {
        GameObject leftPosition = GameObject.FindGameObjectWithTag(leftPositionTag);

        if (leftPosition != null && Vector3.Distance(transform.position, leftPosition.transform.position) <= interactionDistance)
        {
            photonView.RPC("RPC_PlaceOnLeftPosition", RpcTarget.All, heldObjectPhotonView.ViewID, leftPosition.transform.position);
        }
        else
        {
            DropObject();
        }
    }

    [PunRPC]
    void RPC_PickUpObject(int objectViewID, int ownerActorNumber)
    {
        GameObject objectToPickUp = PhotonView.Find(objectViewID).gameObject;
        PhotonView objectPhotonView = objectToPickUp.GetComponent<PhotonView>();

        if (objectPhotonView.OwnerActorNr != ownerActorNumber)
        {
            Debug.Log("Bu nesne ba�ka bir oyuncuya ait: " + objectToPickUp.name);
            return;
        }

        heldObject = objectToPickUp;
        heldObjectPhotonView = heldObject.GetComponent<PhotonView>();
        heldRigidbody = heldObject.GetComponent<Rigidbody>();

        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.freezeRotation = true; // D�nmeyi engelle
        }

        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.Euler(0, 90, 0);

        // Item pi�irme durumunu duraklat
        Item itemComponent = heldObject.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.PauseCooking();
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
            rb.isKinematic = true; // Sabit konumda kalmas� i�in
            rb.freezeRotation = true; // D�nmeyi engelle
        }

        // Item pi�irme durumunu yeniden aktive et
        Item itemComponent = objectToPlace.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.ResumeCooking();
        }

        heldObject = null;
    }

    void DropObject()
    {
        if (heldObject == null) return;

        PhotonView heldObjectPhotonView = heldObject.GetComponent<PhotonView>();
        if (heldObjectPhotonView != null && heldObjectPhotonView.IsMine)
        {
            // Sahipli�i MasterClient'a ver (sahipsiz gibi davranmas� i�in)
            heldObjectPhotonView.TransferOwnership(PhotonNetwork.MasterClient);
            Debug.Log("Nesne b�rak�ld� ve MasterClient'a devredildi: " + heldObject.name);
        }

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

        // Sahipli�i serbest b�rak
        if (droppedObject.GetComponent<PhotonView>().IsMine)
        {
            droppedObject.GetComponent<PhotonView>().TransferOwnership(0); // Sahipsiz yap
        }

        // heldObject'i null yap ve senkronize et
        heldObject = null;
        Debug.Log("Nesne b�rak�ld�: " + droppedObject.name);
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
