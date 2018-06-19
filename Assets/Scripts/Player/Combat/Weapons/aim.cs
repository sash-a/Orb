using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aim : MonoBehaviour {

    public Animator animator;
    private bool scopedIn = false;

    public GameObject weaponCamera;

    public Camera mainCamera;

    private void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            scopedIn = true;
            StartCoroutine(onScoped());

        }

        if (Input.GetButtonUp("Fire2"))
        {
            scopedIn = false;
        }

        animator.SetBool("isScoped", scopedIn);
        //Debug.Log(animator.GetBool("isScoped"));
    }

    //coroutine
    IEnumerator onScoped()
    {
        //will wait certain amount of seconds before calling rest of the code (animation transition time)
        yield return new WaitForSeconds(.15f);
    }
}
