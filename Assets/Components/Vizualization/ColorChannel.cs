using UnityEngine;

public class ColorChannel : BaseChannel
{
    private Material ObjectMaterial;
    void Start()
    {
        var renderer = gameObject.GetComponent<Renderer>();
        ObjectMaterial = renderer.material;
    }

    private void Update()
    {
        if(IsValueUpdated)
        {
            ObjectMaterial.SetColor("_Color", Color.red);
        }
    }
}
