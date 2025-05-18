using TMPro;
using UnityEngine;

public class JoinHostController : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public GameObject hostPanel;
    public GameObject joinPanel;

    public GameObject PlayerPrefab;
    public Transform PlayerListContent;
    public void OnJoinButtonClicked()
    {
        string playerName = nameInputField.text;
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log("Please enter a name.");
            return;
        }
        // Here you would typically handle the logic to join a game
        Debug.Log("Joining game as: " + playerName);
        

        joinPanel.SetActive(false);
        hostPanel.SetActive(true);

        AddPlayerToHostList(playerName);

    }
    public void AddPlayerToHostList(string playerName)
    {
        // Here you would typically handle the logic to add a player to the host list
        Debug.Log("Adding player to host list: " + playerName);
        GameObject newPlayer = Instantiate(PlayerPrefab, PlayerListContent);
        newPlayer.GetComponent<JoinedPlayerPrefab>().SetPlayerName(playerName);


    }

    public void HostStartGame()
    {
        // Here you would typically handle the logic to start a game as a host
        Debug.Log("Starting game as host");
    }
}
