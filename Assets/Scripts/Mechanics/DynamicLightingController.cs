using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicLightingController : MonoBehaviour
{


    public static PlayerController localPlayer;
    public static DynamicLightingController singleton;

    float SurfaceRadius = MapManager.mapSize * 0.53f;

    float ambientIntensity = 0;
    float intensityChangeSpeed = 0.02f;

    Color baseColour;//the color displayed when no effects are on
    Color warningColour;//the warning colour - red
    Color displayColour;//the eefective display colour

    float warningPeriod = 1;//the period of the red colour siren

    // Use this for initialization
    void Start()
    {
        //Debug.Log("starting dynamic lighting comp");
        baseColour = RenderSettings.ambientLight;
        displayColour = baseColour;
        warningColour = new Color(1f, 0f, 0f);
        singleton = this;

        Camera.main.farClipPlane = MapManager.mapSize * 2f;
        Camera.main.nearClipPlane = 1f;

        if (localPlayer == null) {
            localPlayer = TeamManager.localPlayer;
            if (localPlayer == null) {
                StartCoroutine(findLocalPlayer());
            }
        }

    }

    IEnumerator findLocalPlayer() {
        for (int i = 0; i < 5; i++)
        {
            if (localPlayer == null)
            {
                localPlayer = TeamManager.localPlayer;
            }
            yield return new WaitForSecondsRealtime(1f);
        }
        if (localPlayer == null) {
            Debug.LogError("no local player found in lighting manager or team manager");
        }
    }

    // Update is called once per frame
    void Update()
    {
        float idealAmbientIntensity;
        if (playerBelowSurface() )
        {
            //Debug.Log("you are below the surface" + "intensity: " + RenderSettings.ambientIntensity);

            idealAmbientIntensity = inShredWarning? 0.55f :0.15f;
        }
        else
        {
            idealAmbientIntensity = 1;
        }

        ambientIntensity += (idealAmbientIntensity - ambientIntensity) * intensityChangeSpeed;
        if (localPlayer == null) {
            localPlayer = TeamManager.localPlayer;
            if (localPlayer == null)
            {
                return;
            }
        }
        if (localPlayer.isInSphere())
        {
            RenderSettings.ambientLight = new Color(displayColour.r * ambientIntensity, displayColour.g * ambientIntensity, displayColour.b * ambientIntensity);

        }
        else {
            RenderSettings.ambientLight = Color.white ;

        }
    }

    bool playerBelowSurface()
    {
        return localPlayer != null && localPlayer.transform.position.magnitude > SurfaceRadius && localPlayer.isInSphere();
    }


    bool inShredWarning = false;
    public void informInShredZone() {
        if (!inShredWarning) {
            inShredWarning = true;
            StartCoroutine(warnPlayer());
        }

    }

    IEnumerator warnPlayer() {

        //Debug.Log("in warning zone - lighting goiing red");

        float iterationSize = 0.05f;//num seconds between iterationss
        int iterations = Mathf.RoundToInt(warningPeriod / 2 / iterationSize);//num iterations to go one direction in colour change

        float changeSpeed = iterationSize*6;
        for (int i = 0; i < iterations; i++)
        {
            displayColour.r += (warningColour.r - displayColour.r)* changeSpeed;
            displayColour.g += (warningColour.g - displayColour.g) * changeSpeed;
            displayColour.b += (warningColour.b- displayColour.b) * changeSpeed;
            //Debug.Log(displayColour);
            yield return new WaitForSecondsRealtime(iterationSize);
        }

        for (int i = 0; i < iterations; i++)
        {
            displayColour.r += (baseColour.r - displayColour.r) * changeSpeed;
            displayColour.g += (baseColour.g - displayColour.g) * changeSpeed;
            displayColour.b += (baseColour.b - displayColour.b) * changeSpeed;
            yield return new WaitForSecondsRealtime(iterationSize);
        }

        inShredWarning = false;
    }


}
