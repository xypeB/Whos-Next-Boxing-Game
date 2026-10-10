
using UnityEngine;
using TMPro;

public class FightCommentary : MonoBehaviour
{
    public TextMeshProUGUI commentaryText;

    void Start()
    {
        commentaryText.text = "FIGHT START!";
    }

    void OnEnable()
    {
        Application.logMessageReceived += ShowMessage;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= ShowMessage;
    }

    void ShowMessage(string message, string stackTrace, LogType type)
    {
        if (commentaryText == null || type != LogType.Log)
            return;

        if (message.StartsWith("PLAYER THREW") ||
            message.StartsWith("AI used") ||
            message.Contains("DODGED") ||
            message.StartsWith("Player HP:") ||
            message.StartsWith("Opponent HP:"))
        {
            commentaryText.text = message.ToUpper();
        }
    }
}
