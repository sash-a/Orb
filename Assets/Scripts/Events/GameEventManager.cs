using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameEventManager : NetworkBehaviour
{
    public static int gameLength = 200;//number of seconds before max map shredding


    public static float clockTime;
    public static GameEventManager singleton;

    HashSet<GameEvent> events;
    Dictionary<string, GameEvent> namedEvents;
    List<GameEvent> countDownEvents;


    // Use this for initialization
    public override void OnStartClient()
    {
        base.OnStartClient();

        events = new HashSet<GameEvent>();
        countDownEvents = new List<GameEvent>();
        namedEvents = new Dictionary<string, GameEvent>();
        singleton = this;
        StartCoroutine(setUpStartGame());
        setUpStartGame();
    }



    private IEnumerator setUpStartGame()
    {
        yield return new WaitForSecondsRealtime(1f);//wait for map to begin and all players to spawn
        WaitForAllMapsToComplete wait = new WaitForAllMapsToComplete();
        addEvent(wait);

        int shreds = 4;
        for (int i = 0; i < shreds; i++)
        {//4 evenly spaced out shreds
            ShredMap shred = new ShredMap(clockTime + ((i+1)* gameLength / shreds));
            addEvent(shred);
        }
    }

    public void addEvent(GameEvent e)
    {
        events.Add(e);
        e.start();
        if (e.name != "none" && !namedEvents.ContainsKey(e.name))
        {
            namedEvents.Add(e.name, e);
        }
    }

    public void passMessage(string name, string message)
    {
        if (namedEvents.ContainsKey(name))//even server side events are stored locally
        {
            GameEvent ev = namedEvents[name];
            if (ev.serverOnly)
            {
                //Debug.Log("locally passing message to server only event");
                CmdPassMessage(name, message);
            }
            else
            {
                namedEvents[name].passMessage(message);
            }
        }
        else
        {
            Debug.LogError("no such named event as: " + name);
        }
    }

    [Command]
    private void CmdPassMessage(string name, string message)
    {
        if (namedEvents.ContainsKey(name))//even server side events are stored locally
        {
            namedEvents[name].passMessage(message);
        }
        else
        {
            Debug.LogError("no such named event as: " + name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        clockTime += Time.deltaTime;
        List<GameEvent> executedEvents = new List<GameEvent>();
        foreach (GameEvent ev in events)
        {
            int countdown = ev.getCountDownValue();
            if (!ev.serverOnly || isServer)
            { //only consider this event if this is the server or if this event happens on clients
                if (countdown >= 0)
                {//is in count down phase
                    countDownEvents.Add(ev);
                }
                else
                {
                    if (ev.isStarted())
                    {
                        ev.execute();
                        executedEvents.Add(ev);
                    }
                }
            }
            else
            {
                executedEvents.Add(ev);
            }
        }
        foreach (GameEvent ev in executedEvents)
        {
            events.Remove(ev);
        }

        countDown();
    }

    private void countDown()
    {
        GameEvent closestEvent = null;
        List<GameEvent> startedEvents = new List<GameEvent>();
        foreach (GameEvent ev in countDownEvents)
        {
            if (closestEvent == null || closestEvent.startTime > ev.startTime)
            {
                closestEvent = ev;
            }
            if (ev.getCountDownValue() < 0)
            {
                startedEvents.Add(ev);
            }
        }
        foreach (GameEvent ev in startedEvents)
        {
            countDownEvents.Remove(ev);
        }
        if (closestEvent != null)
        {
//            DisplayCountDownFor(closestEvent);
        }
    }

    private void DisplayCountDownFor(GameEvent closestEvent)
    {
        if (closestEvent.getCountDownValue() >= 0)
        {
            Debug.Log(closestEvent.name + " : " + closestEvent.getCountDownValue());
        }
    }

    public void printEventList()
    {
        foreach (GameEvent ev in events)
        {

        }
    }
}
