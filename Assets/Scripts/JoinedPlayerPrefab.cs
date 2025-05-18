using UnityEngine;
using TMPro;


public class JoinedPlayerPrefab : MonoBehaviour
{
    public TMP_Text playerNameText;

    public void SetPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

}
