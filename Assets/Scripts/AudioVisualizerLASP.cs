using UnityEngine;
using UnityEngine.Audio;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System.Linq;

public class AudioVisualizerLASP : MonoBehaviour
{
    [SerializeField] Lasp.SpectrumAnalyzer _input = null;
    [SerializeField] bool _logScale = true;

    public float MaxVisualScale = 1f;
    public float visualModifier = 1.85f;
    public float smoothSpeed = 10.0f;
    public float keepPercentage = 1f;

    public bool useMic;

    private int SAMPLE_SIZE;
    private float sampleRate;

    private float[] visualScale;
    private Transform[] listGO;
    private NativeArray<float3> vertices;
    public Vector3 startSize = new Vector3(1.55f, 1.33f, 1.33f);
    private int amnVisual = 17;

    // Start is called before the first frame update
    private void Start()
    {
        visualScale = new float[amnVisual];
        listGO = GetComponentsInChildren<Transform>();

        SAMPLE_SIZE = _input.resolution;
    }

    // Update is called once per frame
    private void Update()
    {
        AnalyzeAudio();
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        int visualIndex = 0;
        int spectrumIndex = 0;
        int averageSize = (int)((SAMPLE_SIZE * keepPercentage) / amnVisual);

        while (visualIndex < amnVisual)
        {
            int j = 0;
            float sum = 0;
            while (j < averageSize)
            {
                sum += vertices[spectrumIndex].y;
                spectrumIndex++;
                j++;
            }
            float scaley = sum / averageSize * visualModifier;
            visualScale[visualIndex] -= Time.deltaTime * smoothSpeed;
            if (visualScale[visualIndex] < scaley)
                visualScale[visualIndex] = scaley;
            if (visualScale[visualIndex] > MaxVisualScale)
                visualScale[visualIndex] = MaxVisualScale;

            listGO[visualIndex].localScale = startSize + Vector3.up * visualScale[visualIndex];
            visualIndex++;
        }
    }
    private void AnalyzeAudio()
    {
        var span = _logScale ? _input.logSpectrumSpan : _input.spectrumSpan;
        vertices = CreateVertexArray(span);
    }
    NativeArray<float3> CreateVertexArray(System.ReadOnlySpan<float> source)
    {
        var vertices = new NativeArray<float3>(source.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        var xscale = 1.0f / (source.Length - 1); 
        for(var i=0; i < source.Length; i++)
            vertices[i] = math.float3(i * xscale, source[i], 0);
        return vertices;
    }
   
}
