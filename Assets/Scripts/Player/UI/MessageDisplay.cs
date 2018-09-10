using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageDisplay : MonoBehaviour {

    List<UIMessage> messages;
    int displayLength = 3;//max messages to display out the queue
    public static MessageDisplay singleton;
    TextMeshProUGUI text;

	// Use this for initialization
	void Start () {
        singleton = this;
        text = GetComponent<TextMeshProUGUI>();
        if (text == null) {
            Debug.LogError("no text element found in message display");
        }
        messages = new List<UIMessage>();
        new UIMessage("Loading Map...", 15);
	}
	
	// Update is called once per frame
	void Update () {
        string disp = "";
        for (int i = 0; i < Mathf.Min(messages.Count , displayLength); i++)
        {
            if (messages[i].isFinished())
            {
                messages.RemoveAt(i);
                i--;
                continue;
            }
            else {
                disp += messages[i].getMessage() + "\n";
            }
        }
        text.text = disp;
	}

    public void addMessage(UIMessage message) {
        if (!messages.Contains(message)) {
            //Debug.Log("adding message: " + message.getMessage());

            messages.Add(message);
        }
        else
        {
            Debug.LogError("adding same message twice");
        }
    }


    IEnumerator setDisplay() {
        yield return new WaitForSecondsRealtime(0.2f);
        GameEventManager.singleton.display = this;
    }

}
