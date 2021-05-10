using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class SRP1 : RenderPipeline
{
    private RenderPipelineAsset _asset;
    CommandBuffer commandbuffer = new CommandBuffer() {name = "SRP1Command"};

    SortingSettings sortingSetting;
    FilteringSettings filterSetting;
    Material errorMaterial;

    public SRP1(RenderPipelineAsset asset)
    {
        _asset = asset;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        for (int i = 0; i < cameras.Length; i++)
        {
            //Debug.Log("[" + cameras[i].name + "]:" + "[" + cameras[i].cameraType + "]");
            Camera camera = cameras[i];

            // glColorClear
            CameraClearFlags flag = camera.clearFlags;
            if (flag == CameraClearFlags.Skybox)
            {
                context.DrawSkybox(camera);
            }
            else
            {
                commandbuffer.ClearRenderTarget(
                    flag <= CameraClearFlags.Depth,
                    flag <= CameraClearFlags.Color,
                    new Color(0.1f, 0.4f, 0.6f)
                );
                context.ExecuteCommandBuffer(commandbuffer);
                commandbuffer.Clear();
            }


            context.SetupCameraProperties(camera);
            // get shader
            ShaderTagId shaderTagId = new ShaderTagId("SRP1Shader");
            // set draw setting
            sortingSetting = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            // shaderTagId 和 filterSetting 都起到了过滤的作用
            filterSetting = new FilteringSettings(RenderQueueRange.opaque);
            var drawingSetting = new DrawingSettings(shaderTagId, sortingSetting);

            // cull
            CullingResults cullingResults = new CullingResults();
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                cullingResults = context.Cull(ref p);
                context.DrawRenderers(cullingResults, ref drawingSetting, ref filterSetting);
            }

            DrawDefaultPipeline(context, camera, cullingResults);
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                context.DrawSkybox(camera);
            }

            context.Submit();
        }
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera, CullingResults cullingResults)
    {
        if (errorMaterial == null)
        {
            Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
            errorMaterial = new Material(errorShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        ShaderTagId shaderTagId = new ShaderTagId("ForwardBase");

        var drawSettings = new DrawingSettings(shaderTagId, sortingSetting);
        drawSettings.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
        drawSettings.SetShaderPassName(2, new ShaderTagId("Always"));
        drawSettings.SetShaderPassName(3, new ShaderTagId("Vertex"));
        drawSettings.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
        drawSettings.SetShaderPassName(5, new ShaderTagId("VertexLM"));
        drawSettings.overrideMaterial = errorMaterial;
        context.DrawRenderers(cullingResults, ref drawSettings, ref filterSetting);
    }
}