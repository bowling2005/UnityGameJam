using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CreateCirclePlane : MonoBehaviour
{
    public int segments = 64; // 分段数，越高越圆滑
    public float radius = 1f; // 半径

    void Start()
    {
        CreateCircle();
    }

    void CreateCircle()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        // 创建顶点
        Vector3[] vertices = new Vector3[segments + 1];
        Vector2[] uv = new Vector2[segments + 1];

        vertices[0] = Vector3.zero; // 中心点
        uv[0] = new Vector2(0.5f, 0.5f); // UV中心

        float angleStep = 360f / segments;
        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);

            // 将UV映射到0-1范围
            uv[i] = new Vector2(
                (vertices[i].x / radius + 1) * 0.5f,
                (vertices[i].y / radius + 1) * 0.5f
            );
        }

        // 创建三角形
        int[] triangles = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            int index = i * 3;
            triangles[index] = 0;
            triangles[index + 1] = i + 1;
            triangles[index + 2] = i + 2 > segments ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // 添加材质
        Renderer renderer = GetComponent<Renderer>();
        // 在这里创建或指定你的材质
        // renderer.material = yourMaterial;
    }
}