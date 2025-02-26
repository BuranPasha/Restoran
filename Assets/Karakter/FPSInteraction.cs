using Photon.Pun;
using UnityEngine;

public class FPSInteraction : MonoBehaviourPunCallbacks, IPunObservable
{
    public float interactionDistance = 3f;  // Etkile�im mesafesi
    public Transform holdPosition;  // Oyuncunun el pozisyonu
    public string leftPositionTag = "LeftPosition"; // B�rakma noktas�n�n etiketi
    private GameObject heldObject = null;  // �u anda tutulan nesne
    private Rigidbody heldRigidbody;  // Tutulan nesnenin Rigidbody bile�eni
    private PhotonView heldObjectPhotonView;  // Nesnenin PhotonView bile�eni

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

            // E�er item zaten bir oyuncunun elindeyse, ba�ka bir oyuncu alamaz
            if (objectPhotonView.Owner != null && !objectPhotonView.IsMine)
            {
                Debug.Log("Bu item zaten ba�ka bir oyuncunun elinde!");
                return;
            }

            // Sahipli�i talep et
            objectPhotonView.RequestOwnership();
            photonView.RPC("RPC_PickUpObject", RpcTarget.All, objectPhotonView.ViewID);
        }
    }

    void TryDropOrPlace()
    {
        // "LeftPosition" etiketli bir obje ar�yoruz
        GameObject leftPosition = GameObject.FindGameObjectWithTag(leftPositionTag);

        if (leftPosition != null && Vector3.Distance(transform.position, leftPosition.transform.position) <= interactionDistance)
        {
            // LeftPosition'a yerle�tirme i�lemi
            photonView.RPC("RPC_PlaceOnLeftPosition", RpcTarget.All, heldObjectPhotonView.ViewID, leftPosition.transform.position);
        }
        else
        {
            // Normal b�rakma i�lemi
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
            rb.isKinematic = true; // Sabit konumda kalmas� i�in
        }

        // Sahipli�i tamamen s�f�rla
        PhotonView itemPhotonView = objectToPlace.GetComponent<PhotonView>();
        if (itemPhotonView != null)
        {
            itemPhotonView.TransferOwnership(0); // Sahipli�i tamamen s�f�rla
        }

        heldObject = null;
    }

    void DropObject()
    {
        if (heldObject == null) return;

        // Sahipli�i tamamen s�f�rla
        heldObjectPhotonView.TransferOwnership(0); // 0, sahipli�i serbest b�rak�r
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

        // Sahipli�i tamamen s�f�rla
        PhotonView droppedPhotonView = droppedObject.GetComponent<PhotonView>();
        if (droppedPhotonView != null)
        {
            droppedPhotonView.TransferOwnership(0); // Sahipli�i tamamen s�f�rla
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