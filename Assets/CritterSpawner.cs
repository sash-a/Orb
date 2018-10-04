using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CritterSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject critterPrefab;
    [SerializeField] private int maxCritters;
    private int numLivingCritters;

    void Start()
    {
        if (!isServer) return;

        for (int i = 0; i < maxCritters; i++)
            NetworkServer.Spawn(Instantiate(critterPrefab, transform.position, transform.rotation));

        numLivingCritters = maxCritters;
    }

    [Command]
    public void onCritterKilled(float spawnDelay)
    {
        numLivingCritters--;
        StartCoroutine(spawnCritter(spawnDelay));
    }

    private IEnumerator spawnCritter(float delay)
    {
        yield return new WaitForSeconds(delay);
        NetworkServer.Spawn(Instantiate(critterPrefab, transform.position, transform.rotation));
        numLivingCritters++;
    }
}