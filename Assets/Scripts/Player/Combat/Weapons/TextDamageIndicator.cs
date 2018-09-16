using TMPro;
using UnityEngine;

public class TextDamageIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshPro damageAmount;

    private Vector3 origionalScale;
    public Vector3 idealScale;

    private float timeElapsed;
    public float timeAlive;

    void Start()
    {
        Vector2 offset = Random.insideUnitCircle * 2;
        transform.position += new Vector3(offset.x, offset.y, 0);
        Destroy(gameObject, timeAlive);

        origionalScale = transform.localScale;
    }

    private void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up);

        // Shrinking
        transform.localScale = Vector3.Slerp(origionalScale, idealScale, timeElapsed / timeAlive);
        timeElapsed += Time.deltaTime;
    }

    public void setUp(int damage, bool isHealing = false, bool isHeadShot = false, bool isShield = false)
    {
        damageAmount.text = damage + "";

        if (isHeadShot)
            damageAmount.color = Color.red;
        else if (isHealing)
            damageAmount.color = Color.green;
        else if (isShield) 
            damageAmount.color = Color.blue;
    }
}