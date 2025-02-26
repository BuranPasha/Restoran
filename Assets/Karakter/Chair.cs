using UnityEngine;

public class Chair : MonoBehaviour
{
    public Transform sitPoint; // Sandalyede oturulacak nokta

    private PlayerMovement sittingPlayer = null;

    public void SitOrStand(PlayerMovement player)
    {
        if (!gameObject.CompareTag("Chair"))
        {
            Debug.LogError("Oturulmaya �al���lan obje 'Chair' tag'ine sahip de�il!");
            return;
        }

        if (sittingPlayer == null)
        {
            Sit(player);
        }
        else if (sittingPlayer == player)
        {
            StandUp();
        }
    }

    void Sit(PlayerMovement player)
    {
        Debug.Log(player.gameObject.name + " sandalyeye oturdu.");
        sittingPlayer = player;

        player.characterController.enabled = false; // Hareketi kapat

        if (sitPoint != null)
        {
            // Karakterin pozisyonunu SitPosition noktas�na ta��
            player.transform.position = sitPoint.position;
            player.transform.rotation = sitPoint.rotation;
        }
        else
        {
            Debug.LogWarning("Chair objesinde sitPoint atanmad�! L�tfen bir oturma noktas� (SitPosition) ekleyin.");
        }

        player.isSitting = true;
    }

    void StandUp()
    {
        if (sittingPlayer != null)
        {
            Debug.Log(sittingPlayer.gameObject.name + " sandalyeden kalkt�.");
            sittingPlayer.characterController.enabled = true; // Hareketi a�

            sittingPlayer.isSitting = false;
            sittingPlayer = null;
        }
    }
}