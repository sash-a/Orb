using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorManager : MonoBehaviour
{



    PlayerController spectatee;
    public static Team team;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            spectateNext();
        }


    }

    public void spectateNext()
    {

        bool selectNext = false;

        foreach (PlayerController player in team.players)
        {
            if (selectNext)
            {
                spectatee = player;
                selectNext = false;
                break;
            }
            else
            {

                if (spectatee == null)
                {
                    spectatee = player;
                    break;
                }
                else
                {
                    if (player == spectatee)
                    {
                        selectNext = true;
                        //wait for next pass and assign
                    }
                }
            }
        }
        if (selectNext)
        {
            foreach (PlayerController player in team.players)
            {
                spectatee = player;
                break;
            }
        }

        if (spectatee == null)
        {
            Debug.LogError("could not find a player to spectate");
        }
        else {
            spectatePlayer();
        }

    }

    private void spectatePlayer()
    {
        //enable camera of spectatee

    }
}
