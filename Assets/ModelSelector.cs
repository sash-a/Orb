using UnityEngine;

public class ModelSelector : MonoBehaviour
{
    public GameObject healthModel;
    public GameObject damageModel;
    public GameObject telekenModel;

    public ParticleSystem effect;

    public void setModel(PickUpItem.ItemType type)
    {
        if (type == PickUpItem.ItemType.DAMAGE_ARTIFACT)
        {
            damageModel.SetActive(true);
            effect.startColor = damageModel.GetComponent<MeshRenderer>().material.color;
        }
        else if (type == PickUpItem.ItemType.HEALER_ARTIFACT)
        {
            healthModel.SetActive(true);
            effect.startColor = healthModel.GetComponent<MeshRenderer>().material.color;
        }
        else if (type == PickUpItem.ItemType.TELEPATH_ARTIFACT)
        {
            telekenModel.SetActive(true);
            effect.startColor = telekenModel.GetComponent<MeshRenderer>().material.color;
        }
    }
    
}