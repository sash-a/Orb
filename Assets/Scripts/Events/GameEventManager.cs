using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameEventManager : NetworkBehaviour
{
    public static int gameLength = 450;//number of seconds before max map shredding


    public static float clockTime;
    public static GameEventManager singleton;

    HashSet<GameEvent> events;
    Dictionary<string, GameEvent> namedEvents;
    List<GameEvent> countDownEvents;

    public MessageDisplay display;

    bool clockStarted = false;

    private void Start()
    {
        singleton = this;
    }

    // Use this for initialization
    public override void OnStartClient()
    {
        singleton = this;

        //Debug.Log("started game event manager");
        base.OnStartClient();
        events = new HashSet<GameEvent>();

        countDownEvents = new List<GameEvent>();
        namedEvents = new Dictionary<string, GameEvent>();
        singleton = this;
        //StartCoroutine(setUpStartGame());
        setUpStartGame();
    }



    private void setUpStartGame()
    {
        //Debug.Log("setting up game event man ");
        //yield return new WaitForSecondsRealtime(1f);//wait for map to begin and all players to spawn
        WaitForAllMapsToComplete wait = new WaitForAllMapsToComplete();
        //Debug.Log(wait);
        if (wait == null)
        {
            Debug.LogError("failed to create wait object");
        }
        addEvent(wait);
        //Time.
    }

    [Command]
    public void CmdAddShredEvents()
    {
        StartCoroutine(sendRedundantAddShredMessages());
    }

    IEnumerator sendRedundantAddShredMessages() {//server sending
        for (int i = 0; i < 5; i++)
        {
            RpcAddShreddingEvents();
            yield return new WaitForSecondsRealtime(1f);
        }
    }


    bool addedShreds = false;
    [ClientRpc]
    void RpcAddShreddingEvents()//client receiving maybe?
    {
        if (addedShreds) {
            return;
        }
        addedShreds = true;
        int shreds = 4;
        //double diff = Network.time - netTime; // the latency between the server sending the rpc and this client starting the method
                                              //clockTime = 0;
        //Debug.Log("adding shredding event to event manager");

        for (int i = 0; i < shreds; i++)
        {//4 evenly spaced out shreds
            ///ShredMap shred = new ShredMap(clockTime + (float)-diff + ((i + 1) * gameLength / shreds));
            ShredMap shred = new ShredMap(clockTime +  ((i + 1) * gameLength / shreds));

            events.Add(shred);
            //printEventList();
            addEvent(shred);
        }
    }

    public void addEvent(GameEvent e)
    {
        //Debug.Log("adding event: " + e + " contained = " + events.Contains(e));
        if (!events.Contains(e))
        {
            events.Add(e);
        }
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
                //Debug.Log("locally passing message " + message + " to server only event: " + name);
                BuildLog.writeLog("locally passing message " + message + " to server only event: " + name);
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
    public void CmdPassMessage(string name, string message)
    {
        if (namedEvents.ContainsKey(name))//even server side events are stored locally
        {
            BuildLog.writeLog("passing message " + message + " on server event: " + name);
            //Debug.Log("passing message " + message + " on server event: " + name);
            namedEvents[name].passMessage(message);
        }
        else
        {
            Debug.LogError("no such named event as: " + name);
            BuildLog.writeLog("no such named event as: " + name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //printEventList();
        if (clockStarted || true)
        {
            clockTime += Time.deltaTime;
        }
        List<GameEvent> executedEvents = new List<GameEvent>();
        List<GameEvent> removedEvents = new List<GameEvent>();

        foreach (GameEvent ev in events)
        {
            int countdown = ev.getCountDownValue();
            if ((!ev.serverOnly) || isServer)//remove non time based server only events from clients
            { //only consider this event if this is the server or if this event happens on clients
                if (countdown >= 0)
                {//is in count down phase
                    //Debug.Log("adding count down for event: " + ev.name);
                    countDownEvents.Add(ev);
                }
                else
                {
                    if (ev.isStarted())
                    {
                        if (ev.name.Contains("ait"))
                        {
                            //Debug.Log("executing wait for map");
                        }
                        executedEvents.Add(ev);
                    }
                }
            }
            else
            {
                if (!ev.isTimeBased)
                {
                    //Debug.Log("removing non time based server only event: " + ev.name);
                    removedEvents.Add(ev);
                }
                else
                {
                    if (countdown >= 0)
                    {//is in count down phase
                     //Debug.Log("adding count down for event: " + ev.name);
                        countDownEvents.Add(ev);
                    }
                }
            }
        }
        foreach (GameEvent ev in executedEvents)
        {
            //Debug.Log("removing event:  " + ev.name + " and executing it ");
            if (!ev.serverOnly || isServer)
            {
                ev.execute();
            }
            events.Remove(ev);
        }
        foreach (GameEvent ev in removedEvents)
        {
            //if (ev.name == "map shredding") {
            //Debug.Log("removing event:  " + ev.name + " from events queue ");
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
            DisplayCountDownFor(closestEvent);
        }
    }

    float timeSinceLastNetCountMessage = 10;
    private void DisplayCountDownFor(GameEvent closestEvent)
    {
        if (!isServer) {
            return;
        }

        timeSinceLastNetCountMessage += Time.deltaTime;
        if (closestEvent.getCountDownValue() >= 0 && closestEvent.UIMessageObject == null && closestEvent.displayMessage && closestEvent.isTimeBased)
        {
            //Debug.Log(closestEvent.name + " : " + closestEvent.getCountDownValue() + " sending message to message board ");
            //NetworkMessagePasser.add
            
            UIMessage mess = new UIMessage(closestEvent);

            if (closestEvent.serverOnly && timeSinceLastNetCountMessage >= 1) {
                NetworkMessagePasser.singleton.addSyncUIMessage(closestEvent.getMessageNoCountDown() + " "+  (closestEvent.getCountDownValue()), false, 1);
                timeSinceLastNetCountMessage = 0;
            }

            //UIMessage mess = new UIMessage(closestEvent);

        }
    }

    public void printEventList()
    {
        string list = "t = " + clockTime + "events in queue: \t";
        foreach (GameEvent ev in events)
        {
            list += ev.name + (ev.isTimeBased ? "(" + ev.startTime + " )" : "") + " ; ";
        }
        Debug.Log(list);
    }
}
