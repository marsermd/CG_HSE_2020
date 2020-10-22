using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class CubemapToSphericalHarmonic : MonoBehaviour
{
    private const int SIZE = 1024 * 1;
    private int _kernel;

    public float L0;
    public float3 L1;
    public float4 L2_1;
    public float L2_2;
    
    public bool UseCompute;
    
    
    public Cubemap Env;

    public ComputeShader Compute;
    
    private BufferSetup[] _setups;

    private void SetGrey(string name, Vector4 val)
    {
        Shader.SetGlobalVector(name + "_r", val);
        Shader.SetGlobalVector(name + "_g", val);
        Shader.SetGlobalVector(name + "_b", val);
    }

    void OnEnable()
    {
        _setups = new []{
            new BufferSetup("SH_0_1_r"), 
            new BufferSetup("SH_0_1_g"), 
            new BufferSetup("SH_0_1_b"), 
        
            new BufferSetup("SH_2_r"), 
            new BufferSetup("SH_2_g"), 
            new BufferSetup("SH_2_b"), 
        
            new BufferSetup("SH_2_rgb")
        };
    }
    
    // We don't really have to recalculate SH every frame. It's just for ease of use here.
    void Start()
    {
        if (UseCompute)
        {
            _kernel = Compute.FindKernel("ComputeHarmonics");
            foreach (var bufferSetup in _setups)
            {
                bufferSetup.Bind(Compute, _kernel);
            }
            Compute.SetTexture(_kernel, "_Env", Env);
            
            Compute.Dispatch(_kernel, SIZE, 1, 1);

            foreach (var bufferSetup in _setups)
            {
                bufferSetup.Push();
            }
        }
        else
        {
            SetGrey("SH_0_1", new Vector4(L1.x, L1.y, L1.z, L0));
            SetGrey("SH_2", L2_1);
            Shader.SetGlobalVector("SH_2_rgb", new Vector4(L2_2, L2_2, L2_2));
        }
    }
    
    [ContextMenu("Check My Homework!")]
    private void CheckHW()
    {
        Dictionary<string, float4> correctValues = new Dictionary<string, float4>{
            {"SH_0_1_r", new float4(0.0036f, 0.0526f, -0.0017f, 0.2530f)},
            {"SH_0_1_g", new float4(0.0016f, 0.1011f,  0.0048f, 0.3180f)},
            {"SH_0_1_b", new float4(0.0020f, 0.2076f,  0.0093f, 0.3641f)},
            {"SH_2_r"  , new float4(0.0024f, 0.0501f, -0.0076f, 0.0021f)},
            {"SH_2_g"  , new float4(0.0025f, 0.0519f, -0.0046f, 0.0041f)},
            {"SH_2_b"  , new float4(0.0031f, 0.0584f, -0.0035f, 0.0034f)},
            {"SH_2_rgb", new float4(0.0001f, 0.0002f,  0.0001f, 0.0000f)},
        };

        bool foundDifference = false;
        foreach (var bufferSetup in _setups)
        {
            float4 current = Shader.GetGlobalVector(bufferSetup.Name);
            float4 correct = correctValues[bufferSetup.Name];
            for (int i = 0; i < 4; i++)
            {
                if (Mathf.Abs(current[i] - correct[i]) > 0.004f)
                {
                    Debug.LogError($"Difference in {current:F4} and {correct:F4} in position {i}");
                    foundDifference = true;
                }
            }
        }

        if (!foundDifference)
        {
            Debug.Log("Everything is correct! Congrats!");
        }
    }

    private void OnDisable()
    {
        if (_setups != null)
        {
            foreach (var setup in _setups)
            {
                setup.Dispose();
            }

            _setups = null;
        }
    }
    
    private class BufferSetup: IDisposable
    {
        private readonly ComputeBuffer _buffer;
        private readonly float4[] _destination;
        public readonly string Name;

        public unsafe BufferSetup(string name)
        {
            Name = name;
            
            _buffer = new ComputeBuffer(SIZE, sizeof(float4));
            
            _destination = new float4[SIZE];
        }
        
        public void Push()
        {
            _buffer.GetData(_destination);
            
            float4 sum = 0;
            for (int i = 0; i < SIZE; i++)
            {
                sum += _destination[i];
            }
            
            Shader.SetGlobalVector(Name, sum / SIZE);
        }
        
        public void Bind(ComputeShader computeShader, int kernel)
        {
            computeShader.SetBuffer(kernel, Name, _buffer);
        }
        
        public void Dispose()
        {
            _buffer?.Dispose();
        }
    }
}
