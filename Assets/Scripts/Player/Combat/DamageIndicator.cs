using System.Collections;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    public Transform damageIndicator;
    public Transform target;

    private bool recentlyDamaged;


    private void Start()
    {
        damageIndicator.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (recentlyDamaged)
        {
            orientIndicator();
        }
    }

    public void hit(Transform shooter)
    {
        target = shooter;
        Debug.Log("in hit");
        StopAllCoroutines();
        StartCoroutine(showDamage());
    }

    IEnumerator showDamage()
    {
        damageIndicator.gameObject.SetActive(true);
        recentlyDamaged = true;
        
        yield return new WaitForSeconds(10);
        
        damageIndicator.gameObject.SetActive(false);
        recentlyDamaged = false;
    }

    private void orientIndicator()
    {
        Vector3 dir = Camera.main.WorldToScreenPoint(target.position);
        Vector3 newDirection = Vector3.zero;
        newDirection.z =
            Mathf.Atan2(damageIndicator.position.y - dir.y, -damageIndicator.position.x + dir.x) *
            Mathf.Rad2Deg - 90;
        
        damageIndicator.transform.rotation = Quaternion.Euler(newDirection);
    }
}