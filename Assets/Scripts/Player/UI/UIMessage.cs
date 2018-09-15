using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMessage
{
    float displayDuration;
    float startTime;
    public GameEvent gameEvent; //doesnt have to be linked to a game event 
    string message;

    public UIMessage(GameEvent ev)
    {
        gameEvent = ev;
        if (ev.UIMessageObject == null)
        {
            ev.UIMessageObject = this;
            MessageDisplay.singleton.addMessage(this);
        }
        else
        {
            Debug.LogError("the game event " + ev + " already has a UImessage " + this);
        }
    }

    public UIMessage(string message, float duration)
    {
        startTime = GameEventManager.clockTime;
        displayDuration = duration;
        this.message = message;
        MessageDisplay.singleton.addMessage(this);
    }


    public string getMessage()
    {
        if (message != null)
        {
            return message;
        }
        else
        {
            return gameEvent.getMessage();
        }
    }

    /// <summary>
    /// returns true when its time for this message to stop being displayed
    /// </summary>
    /// <returns></returns>
    public bool isFinished()
    {
        if (gameEvent == null)
        {
            if (GameEventManager.clockTime > startTime + displayDuration)
            {
                return true;
            }
        }
        else
        {
            if (GameEventManager.clockTime > gameEvent.startTime)
            {
                return true;
            }
        }

        return false;
    }
}