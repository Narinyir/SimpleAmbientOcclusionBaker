using UnityEditor;
using UnityEngine;

public class AmbientOcclusionBaker : ScriptableWizard
{
    public int sampleCount = 250;
    public float coneAngle = 30;
    public float shadowStrength = 1;

    Vector3[] directions;

    [MenuItem("Tools/Ambient Occlusion Baker")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<AmbientOcclusionBaker>("Ambient Occlusion Baker", "Bake", "Reset");
    }

    void OnWizardCreate()
    {
        BakeAmbientOcclusion();
        CreateWizard();
    }

    void OnWizardUpdate()
    {
    }

    void OnWizardOtherButton()
    {
    }

    float CalculateAmbientIntenisty(Vector3 position)
    {
        int visibleCount = 0;

        for (int i = 0; i < sampleCount; i++)
        {
            Ray ray = new Ray(position, directions[i]);

            if (!Physics.Raycast(ray))
            {
                visibleCount++;
            }
        }

        return (float)visibleCount / sampleCount;
    }

    void BakeAmbientOcclusion()
    {
        GameObject selectedObject = Selection.activeTransform.gameObject;

        Vector3[] vertices = selectedObject.GetComponent<MeshFilter>().mesh.vertices;

        Bounds bounds = selectedObject.GetComponent<Renderer>().bounds;

        Color[] newColors = new Color[vertices.Length];

        directions = new Vector3[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            Vector3 direction;
            float angle;

            do
            {
                direction = Random.onUnitSphere;
                direction.y = Mathf.Abs(direction.y);
                angle = Vector3.Angle(direction, Vector3.up);
            } while (angle > coneAngle);

            directions[i] = direction;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPosition = selectedObject.transform.TransformPoint(vertices[i]);
            float ambientIntensity = CalculateAmbientIntenisty(worldPosition);
            Color color = Color.Lerp(Color.black, Color.white, ambientIntensity);
            color = Color.black;
            color.a = 1 - ambientIntensity;

            float minDistance = (worldPosition.x - bounds.min.x);
            minDistance = Mathf.Min(minDistance, bounds.max.x - worldPosition.x);
            minDistance = Mathf.Min(minDistance, worldPosition.z - bounds.min.z);
            minDistance = Mathf.Min(minDistance, bounds.max.z - worldPosition.z);
            minDistance = Mathf.Min(minDistance, 2) / 2;
            color.a *= minDistance * shadowStrength;

            newColors[i] = color;
        }

        selectedObject.GetComponent<MeshFilter>().mesh.colors = newColors;
    }
}