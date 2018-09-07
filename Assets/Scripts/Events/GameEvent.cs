using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameEvent {

    public float startTime = int.MaxValue;//time at which to start this event
    //public int duration=0;
    public int countDownPeriod=5;//the number of seconds before the start time the event starts a count down timer
    public bool isTimeBased=false;//some events are triggered by hooks
    public string name ="none";//some events can be triggered by name in network message passing
    /// <summary>
    /// some events run locally on all systems - some do not
    /// those that run server only are stored by each local event manager but are only ever executed on the server
    /// </summary>
    public bool serverOnly = false;//

    public bool isStarted()
    {
        if (isTimeBased)
        {
            return GameEventManager.clockTime >= startTime;
        }
        else {
            return hook();
        }
    }

    public int getCountDownValue() {
        if (GameEventManager.clockTime <= startTime && GameEventManager.clockTime >= startTime - countDownPeriod && isTimeBased)
        {
            return Mathf.RoundToInt(startTime - GameEventManager.clockTime);
        }
        else {
            return -1;
        }
    }

    public void validateParameters() {
        if (isTimeBased)
        {
            if(startTime == int.MaxValue || startTime < 0)
            {
                Debug.LogError("invalid start time (" + startTime + ") for event " + name);
            }
        }
        else {
            startTime = int.MaxValue;
            countDownPeriod = -1;//has no count down period
        }
    }

    /// <summary>
    /// if an event triggered event - must override this method with a function which correctly reflects true when the event hass been triggered
    /// </summary>
    /// <returns></returns>
    public abstract bool hook();
    /// <summary>
    /// call on initialisation of a new event - sets up its parameters
    /// </summary>
    public abstract void start();
    public abstract void execute();
    public abstract void passMessage(string message);//can be used for passing special instructions to an event

}
