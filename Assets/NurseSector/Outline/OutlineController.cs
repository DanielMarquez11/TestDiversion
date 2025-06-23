using UnityEngine;

public class OutlineController : MonoBehaviour
{
    public GameObject instantiatedObject = null;
    private static Material sharedOutlineMaterial;
    public bool alternateOutline;

    private void Start()
    {
        if (gameObject.layer == LayerMask.NameToLayer("InteractableItem"))
        {
            MeshRenderer meshRenderer;
            MeshFilter meshFilter;
            GameObject targetObject = FindMeshObject(out meshRenderer, out meshFilter);

            if (targetObject != null && instantiatedObject == null)
            {
                SetupOutlineMaterial();
                CreateOutline(targetObject, meshRenderer, meshFilter);
            }
        }
    }

    private GameObject FindMeshObject(out MeshRenderer meshRenderer, out MeshFilter meshFilter)
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        if (meshRenderer != null && meshFilter != null)
        {
            return gameObject;
        }

        meshFilter = GetComponentInChildren<MeshFilter>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (meshRenderer != null && meshFilter != null && meshFilter.gameObject == meshRenderer.gameObject)
        {
            return meshFilter.gameObject;
        }

        meshRenderer = null;
        meshFilter = null;
        return null;
    }

    private void SetupOutlineMaterial()
    {
        if (sharedOutlineMaterial == null)
        {
            Shader itemInteractionShader = FindFirstObjectByType<ItemInteraction>().itemOutlineShader;
            sharedOutlineMaterial = new Material(itemInteractionShader);
        }
    }

    private void CreateOutline(GameObject targetObject, MeshRenderer meshRenderer, MeshFilter meshFilter)
    {
        instantiatedObject = new GameObject($"{targetObject.name}_Outline");
        instantiatedObject.transform.SetParent(targetObject.transform, false);

        MeshFilter newMeshFilter = instantiatedObject.AddComponent<MeshFilter>();
        newMeshFilter.mesh = meshFilter.sharedMesh;

        MeshRenderer newMeshRenderer = instantiatedObject.AddComponent<MeshRenderer>();
        Material[] outlineMaterials = new Material[meshRenderer.materials.Length];

        for (int i = 0; i < meshRenderer.materials.Length; i++)
        {
            outlineMaterials[i] = sharedOutlineMaterial;
        }

        newMeshRenderer.materials = outlineMaterials;
        newMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        newMeshRenderer.receiveShadows = false;
        newMeshRenderer.enabled = true;
    }

    public void HandleThickness(MaterialPropertyBlock materialPropertyBlock)
    {
        if (alternateOutline)
        {
            materialPropertyBlock.SetFloat("_Thickness", 0.002f);
        }
    }
}