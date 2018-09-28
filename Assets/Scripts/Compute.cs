using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Compute : MonoBehaviour {
    const int BLOCK_SIZE = 256;

    public Mesh mesh;
    public Material material;
    public ComputeShader computeShader;

    private Bounds bounds;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer visibleBuffer;
    private ComputeBuffer animationBuffer;
    private ComputeBuffer rotationMatrixBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5];
    private uint numeberOfCubes = 10000;

    private Vector3[] positions;
    private int[] visible;
    private Vector3 maxPos = new Vector3(5f, 5f, 3f);

    // Use this for initialization
    void Start () {
        // 12 = float (4 byte x 3）
        positionBuffer = new ComputeBuffer((int)numeberOfCubes, 12);
        visibleBuffer = new ComputeBuffer((int)numeberOfCubes, sizeof(int));
        animationBuffer = new ComputeBuffer((int)numeberOfCubes, sizeof(float));
        rotationMatrixBuffer = new ComputeBuffer((int)numeberOfCubes, Marshal.SizeOf(typeof(Matrix4x4)));
        positions = new Vector3[numeberOfCubes];

        visible = new int[numeberOfCubes];
        float[] animationValue = new float[numeberOfCubes];
        Matrix4x4[] mat = new Matrix4x4[numeberOfCubes];
        for (int i = 0; i < numeberOfCubes; i++)
        {
            float dist = 50f;
            float deg = numeberOfCubes / 360f;
            float rad = (deg * (float)i) * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * dist;
            float z = Mathf.Sin(rad) * dist;
            float scaleVal = Random.Range(-0.1f, 0.3f);
            Vector3 pos = new Vector3(x + Random.Range(-maxPos.x, maxPos.x), Random.Range(-maxPos.y, maxPos.y), z + Random.Range(-maxPos.x, maxPos.y));
            positions[i] = pos;
            animationValue[i] = 0f;
            visible[i] = 1;
            mat[i] = Matrix4x4.identity;
        }

        positionBuffer.SetData(positions);
        visibleBuffer.SetData(visible);
        animationBuffer.SetData(animationValue);
        rotationMatrixBuffer.SetData(mat);

        material.SetBuffer("PositionBuffer", positionBuffer);
        material.SetBuffer("VisibleBuffer", visibleBuffer);
        material.SetBuffer("AnimationBuffer", animationBuffer);
        material.SetBuffer("RotationMatrixBuffer", rotationMatrixBuffer);

        bounds = new Bounds(Vector3.zero, new Vector3(numeberOfCubes / 3, numeberOfCubes / 3, numeberOfCubes / 3));

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = mesh.GetIndexCount(0);
        args[1] = numeberOfCubes;
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        args[4] = 0;
        argsBuffer.SetData(args);
    }
	
	// Update is called once per frame
	void Update () {

        for (int i = 0; i< visible.Length; i++)
        {
            if (i< visible.Length/2)
            {
                visible[i] = 1;
            } else
            {
                visible[i] = 0;
            }
        }

        visibleBuffer.SetData(visible);
        material.SetBuffer("VisibleBuffer", visibleBuffer);

        int kernelId = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernelId, "PositionBuffer", positionBuffer);
        computeShader.SetBuffer(kernelId, "VisibleBuffer", visibleBuffer);
        computeShader.SetBuffer(kernelId, "AnimationBuffer", animationBuffer);
        computeShader.SetBuffer(kernelId, "RotationMatrixBuffer", rotationMatrixBuffer);
        int groupSize = Mathf.CeilToInt(numeberOfCubes / BLOCK_SIZE);
        computeShader.Dispatch(kernelId, groupSize, 1, 1);
        computeShader.SetFloat("_Time", Time.time);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    void OnDestroy()
    {
        rotationMatrixBuffer.Release();
        visibleBuffer.Release();
        animationBuffer.Release();
        positionBuffer.Release();
        argsBuffer.Release();
    }
}
