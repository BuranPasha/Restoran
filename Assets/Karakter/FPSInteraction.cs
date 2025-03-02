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
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.E)) // E tu�u ile nesne al
            {
                if (heldObject == null)
                {
                    TryPickUp();
                }
            }
            else if (Input.GetKeyDown(KeyCode.G)) // G tu�u ile nesne b�rak
            {
                if (heldObject != null)
                {
                    TryDropOrPlace();
                }
            }
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

            // Sahipli�i transfer etmeden �nce kontrol et
            if (!objectPhotonView.IsMine)
            {
                objectPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }

            photonView.RPC("RPC_PickUpObject", RpcTarget.AllBuffered, objectPhotonView.ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
        }
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
    void RPC_PickUpObject(int objectViewID, int ownerActorNumber)
    {
        PhotonView foundView = PhotonView.Find(objectViewID);
        if (foundView == null)
        {
            Debug.LogError("PhotonView bulunamad�: " + objectViewID);
            return;
        }

        GameObject objectToPickUp = foundView.gameObject;
        PhotonView objectPhotonView = objectToPickUp.GetComponent<PhotonView>();

        if (!objectPhotonView.IsMine)
        {
            objectPhotonView.TransferOwnership(ownerActorNumber);
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
        PhotonView foundView = PhotonView.Find(objectViewID);
        if (foundView == null)
        {
            Debug.LogError("PhotonView bulunamad�: " + objectViewID);
            return;
        }

        GameObject objectToPlace = foundView.gameObject;
        objectToPlace.transform.SetParent(null);
        objectToPlace.transform.position = position;
        objectToPlace.transform.rotation = Quaternion.identity; // Rotasyonu s�f�rla

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

        if (heldObjectPhotonView != null && heldObjectPhotonView.IsMine)
        {
            // Sahipli�i MasterClient'a ver (sahipsiz gibi davranmas� i�in)
            heldObjectPhotonView.TransferOwnership(PhotonNetwork.MasterClient);
            Debug.Log("Nesne b�rak�ld� ve MasterClient'a devredildi: " + heldObject.name);
        }

        photonView.RPC("RPC_DropObject", RpcTarget.AllBuffered, heldObjectPhotonView.ViewID);
    }

    [PunRPC]
    void RPC_DropObject(int objectViewID)
    {
        PhotonView foundView = PhotonView.Find(objectViewID);
        if (foundView == null)
        {
            Debug.LogError("PhotonView bulunamad�: " + objectViewID);
            return;
        }

        GameObject droppedObject = foundView.gameObject;
        Rigidbody droppedRigidbody = droppedObject.GetComponent<Rigidbody>();

        if (droppedRigidbody != null)
        {
            droppedRigidbody.isKinematic = false;
            droppedRigidbody.useGravity = true;
        }

        droppedObject.transform.SetParent(null);
        droppedObject.transform.rotation = Quaternion.identity; // Rotasyonu s�f�rla

        // Sahipli�i serbest b�rak
        if (foundView.IsMine)
        {
            foundView.TransferOwnership(0); // Sahipsiz yap
        }

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
            Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
            Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();

            if (heldObject != null)
            {
                heldObject.transform.position = receivedPosition;
                heldObject.transform.rotation = receivedRotation;
            }
        }
    }
}