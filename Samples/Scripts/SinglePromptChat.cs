using ChatGPTRequest.apiData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SinglePromptChat : MonoBehaviour
{

    [SerializeField] private ChatGPTmanager gpt;
    
    [Header("UI Elements")]
    [SerializeField] private Text input;
    [SerializeField] private Text output;

    [Header("GPT settings")][Tooltip("How many messages are remembered by the bot")]
    [SerializeField] private int messageHistory = 3;

    [SerializeField] private bool Stream = false;

    [SerializeField] private string UserPrefix = "User: ";
    [SerializeField] private string UserSufix = "\n";
    [SerializeField] private string BotPrefix = "Bot: ";
    [SerializeField] private string BotSufix = "\n";

    [SerializeField] private List<Message> systemPrompts;
    [SerializeField] private List<Message> messages;

    bool _runOnce;

    //https://platform.openai.com/docs/guides/chat


    // A helper method for creating a list of chat messages based on a user message input.
    List<Message> ChatMessage(string message)
    {
        List<Message> msgList = new();

        // sysMsg (optonal)
        if (systemPrompts.Count > 0)
            msgList.AddRange(systemPrompts);
        // userMsg
        messages.Add(new Message(message, Message.Roles.user));
        msgList.AddRange(messages);
        TrimMessageHistory();

        return msgList;
    }

    /// <summary>
    /// A public method for making a chat request using the user's input message.
    /// </summary>
    public void DoRequest()
    {
        _runOnce = true;

        gpt.DoApiCompletation(ChatMessage(input.text),
            (i) => DistributeData(i),//success
            (a) => Debug.Log(a),
            Stream);
    }

    /// <summary>
    /// Method for processing the results of a chat request.
    /// </summary>
    /// <param name="data"></param>
    void DistributeData(ApiDataPackage data)
    {

        if (Stream)
        {
            if (_runOnce)
            {
                _runOnce = false;

                output.text += UserPrefix + input.text + UserSufix;
                output.text += BotPrefix;
            }

            if (data.finnish_reason == "\"stop\"")
            {
                output.text += BotSufix;
            }
            else
            {
                output.text += data.Message.message;
            }
        }
        else
        {
            output.text += (UserPrefix + input.text + UserSufix);
            output.text += BotPrefix + data.Message.message + BotSufix;

            // Add bot message to messages list for history
            messages.Add(new Message(data.Message.message, Message.Roles.assistant));
            TrimMessageHistory();

            print($"Executed ChatGPT call in: {data.ExecutionTime:F3} seconds");
        }
    }


    /// <summary>
    ///  Remove entries exceeding the messageHistory.
    /// </summary>
    void TrimMessageHistory()
    {
        if (messageHistory > 1 && messages.Count > messageHistory)
            messages.RemoveRange(messageHistory - 1, messages.Count - messageHistory);
    }
}

