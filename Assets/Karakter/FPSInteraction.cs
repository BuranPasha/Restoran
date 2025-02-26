using Photon.Pun;
using UnityEngine;

public class FPSInteraction : MonoBehaviourPunCallbacks, IPunObservable
{
    public float interactionDistance = 3f;  // Etkileþim mesafesi
    public Transform holdPosition;  // Oyuncunun el pozisyonu
    public string leftPositionTag = "LeftPosition"; // Býrakma noktasýnýn etiketi
    private GameObject heldObject = null;  // Þu anda tutulan nesne
    private Rigidbody heldRigidbody;  // Tutulan nesnenin Rigidbody bileþeni
    private PhotonView heldObjectPhotonView;  // Nesnenin PhotonView bileþeni

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && photonView.IsMine)
        {
            if (heldObject == null) TryPickUp();
            else TryDropOrPlace();
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

            // Eðer item zaten bir oyuncunun elindeyse, baþka bir oyuncu alamaz
            if (objectPhotonView.Owner != null && !objectPhotonView.IsMine)
            {
                Debug.Log("Bu item zaten baþka bir oyuncunun elinde!");
                return;
            }

            // Sahipliði talep et
            objectPhotonView.RequestOwnership();
            photonView.RPC("RPC_PickUpObject", RpcTarget.All, objectPhotonView.ViewID);
        }
    }

    void TryDropOrPlace()
    {
        // "LeftPosition" etiketli bir obje arýyoruz
        GameObject leftPosition = GameObject.FindGameObjectWithTag(leftPositionTag);

        if (leftPosition != null && Vector3.Distance(transform.position, leftPosition.transform.position) <= interactionDistance)
        {
            // LeftPosition'a yerleþtirme iþlemi
            photonView.RPC("RPC_PlaceOnLeftPosition", RpcTarget.All, heldObjectPhotonView.ViewID, leftPosition.transform.position);
        }
        else
        {
            // Normal býrakma iþlemi
            DropObject();
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
            rb.isKinematic = true; // Sabit konumda kalmasý için
        }

        // Sahipliði tamamen sýfýrla
        PhotonView itemPhotonView = objectToPlace.GetComponent<PhotonView>();
        if (itemPhotonView != null)
        {
            itemPhotonView.TransferOwnership(0); // Sahipliði tamamen sýfýrla
        }

        heldObject = null;
    }

    void DropObject()
    {
        if (heldObject == null) return;

        // Sahipliði tamamen sýfýrla
        heldObjectPhotonView.TransferOwnership(0); // 0, sahipliði serbest býrakýr
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

        // Sahipliði tamamen sýfýrla
        PhotonView droppedPhotonView = droppedObject.GetComponent<PhotonView>();
        if (droppedPhotonView != null)
        {
            droppedPhotonView.TransferOwnership(0); // Sahipliði tamamen sýfýrla
        }

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