using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancingObj
{
    public Vector3 pos;
    public Vector3 scale;
    public Quaternion rot;

    public Matrix4x4 matrix
    {
        get
        {
            return Matrix4x4.TRS(pos, rot, scale);
        }
    }

    public InstancingObj(Vector3 pos, Vector3 scale, Quaternion rot)
    {
        this.pos = pos;
        this.scale = scale;
        this.rot = rot;
    }
}

public class Instancing : MonoBehaviour {

    public int instances;
    public Mesh mesh;
    public Material mat;

    private List<List<InstancingObj>> batches = new List<List<InstancingObj>>();
    private List<Matrix4x4[]> matrixList = new List<Matrix4x4[]>();

    private Vector3 maxPos = new Vector3(5f,5f,3f);
    private float time = 0;

    // Use this for initialization
    void Start () {
        CreateBatches();
    }
	
    private void CreateBatches()
    {
        int batchIndexNum = 0;
        int maxBatchIndexNum = 1000;
        List<InstancingObj> currentBatch = new List<InstancingObj>();
        Matrix4x4[] currentMatrix = new Matrix4x4[maxBatchIndexNum];
        for (int i = 0; i < instances; i++)
        {
            AddInstanceObj(currentBatch, currentMatrix, batchIndexNum, i);
            batchIndexNum++;
            if (batchIndexNum > maxBatchIndexNum - 1)
            {
                batches.Add(currentBatch);
                matrixList.Add(currentMatrix);
                currentBatch = new List<InstancingObj>();
                currentMatrix = new Matrix4x4[maxBatchIndexNum];
                batchIndexNum = 0;
            }
        }
    }

    private void AddInstanceObj(List<InstancingObj> currentBatch, Matrix4x4[] currentMatrix, int index, int i)
    {
        float dist = 50f;
        float deg = instances/ 360f;
        float rad = (deg*(float)i) * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * dist;
        float z = Mathf.Sin(rad) * dist;
        float scaleVal = Random.Range(-0.1f, 0.3f);
        Vector3 pos = new Vector3(x+Random.Range(-maxPos.x, maxPos.x), Random.Range(-maxPos.y, maxPos.y), z+Random.Range(-maxPos.x, maxPos.y));
        InstancingObj obj = new InstancingObj(pos, new Vector3(0.3f+ scaleVal, 0.3f+ scaleVal, 0.3f+ scaleVal), Quaternion.Euler(Random.Range(-360f,360f), Random.Range(-360f, 360f), Random.Range(-360f, 360f)));
        currentBatch.Add(obj);
        currentMatrix[index] = obj.matrix;
    }

    // Update is called once per frame
    void Update () {
        UpdateTRS();
        RenderBatches();
    }

    private void UpdateTRS(float scalVal = 1f)
    {
        time += Time.deltaTime;

        int index = 0;
        for (int i = 0; i< batches.Count; i++)
        {
            for (int j = 0; j < batches[i].Count; j++)
            {
                InstancingObj obj = batches[i][j];
                Vector3 pos = obj.pos;
                float noise = Mathf.PerlinNoise(pos.x, pos.y);
                pos.y += Mathf.Sin(time * noise);
                matrixList[i][j] = Matrix4x4.TRS(pos, obj.rot, obj.scale);

                index++;
            }
        }
    }

    private void RenderBatches()
    {
        for (int i = 0; i< batches.Count; i++)
        {
            Graphics.DrawMeshInstanced(mesh, 0, mat, matrixList[i]);
        }
    }
}
