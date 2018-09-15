using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameTextObject : MonoBehaviour {


    public Transform anchor;
    TextMeshProUGUI text;
    RectTransform pos;

    public Color defaultColour;
    public string defaultText;

    public float size = 1;



    // Use this for initialization
    void Start () {
        init();
    }

    void init() {
        text = GetComponent<TextMeshProUGUI>();
        pos = GetComponent<RectTransform>();
        try
        {
            transform.parent = GameObject.FindGameObjectWithTag("HUD").transform.GetChild(3);
            transform.rotation = GameObject.FindGameObjectWithTag("HUD").transform.rotation;
        }
        catch { }
    }

	bool started = false;
    // Update is called once per frame
    void Update () {
        if (anchor != null)
        {
            started = true;
            //Debug.Log(Camera.main.transform.position);

            double distance = Vector3.Distance(anchor.position, Camera.main.transform.position);
            //Vector3 screenPos = Camera.main.WorldToScreenPoint(anchor.position);
            Vector3 screenPos = Camera.main.WorldToViewportPoint(anchor.position);
            screenPos.z = 0;
            //Debug.Log("sending " + gameObject.name + " to screen pos: " + screenPos); 

            //pos.anchoredPosition.Set(screenPos.x, screenPos.y + 1f);
            // pos.anchoredPosition3D = screenPos;
            pos.transform.position = // screenPos;
            Camera.main.ViewportToScreenPoint(screenPos);
            //pos.z
            float fontSize = Mathf.Max(Mathf.Min(100, 100/ Mathf.Pow((float)distance/30,3f)), 8);
            text.fontSize = fontSize* size;
        }
        else {
            if (started) {
                Destroy(gameObject);
            }
        }

        

    }

    public void setValues(Color colour, string uiText, Transform trans) {
        init();
        anchor = trans;
        text.text = uiText;
        text.color = colour;
    }

    public void setValues(Color colour, string uiText)
    {
        init();
        text.text = uiText;
        text.color = colour;
    }
}
