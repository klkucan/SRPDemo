using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    public class CustomRenderPassFeaturePass : ScriptableRenderPass
    {
        int kDepthBufferBits = 32;

        private RenderTargetHandle depthAttachmentHandle { get; set; }
        internal RenderTextureDescriptor descriptor { get; private set; }

        FilteringSettings m_FilteringSettings;
        ShaderTagId m_ShaderTagId = new ShaderTagId("DepthOnly");

        /// <summary>
        /// Create the DepthOnlyPass
        /// </summary>
        public CustomRenderPassFeaturePass(RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask)
        {
            base.profilingSampler = new ProfilingSampler(nameof(CustomRenderPassFeaturePass));
            m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            renderPassEvent = evt;
        }

        /// <summary>
        /// Configure the pass
        /// </summary>
        public void Setup(
            RenderTextureDescriptor baseDescriptor,
            RenderTargetHandle depthAttachmentHandle)
        {
            this.depthAttachmentHandle = depthAttachmentHandle;
            baseDescriptor.colorFormat = RenderTextureFormat.Depth;
            baseDescriptor.depthBufferBits = kDepthBufferBits;

            // Depth-Only pass don't use MSAA
            baseDescriptor.msaaSamples = 1;
            descriptor = baseDescriptor;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cmd.GetTemporaryRT(depthAttachmentHandle.id, descriptor, FilterMode.Point);
            // ConfigureTarget(new RenderTargetIdentifier(depthAttachmentHandle.Identifier(), 0, CubemapFace.Unknown, -1));
            ConfigureTarget(depthAttachmentHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // NOTE: Do NOT mix ProfilingScope with named CommandBuffers i.e. CommandBufferPool.Get("name").
            // Currently there's an issue which results in mismatched markers.
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("TSAI")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
                drawSettings.perObjectData = PerObjectData.None;

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);
            }
            cmd.SetGlobalTexture("_CameraDepthAttachment", depthAttachmentHandle.id);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc/>
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");

            if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(depthAttachmentHandle.id);
                depthAttachmentHandle = RenderTargetHandle.CameraTarget;
            }
        }
    }


    public RenderObjects.RenderObjectsSettings _settings = new RenderObjects.RenderObjectsSettings();
    CustomRenderPassFeaturePass m_ScriptablePass;
    private RenderTargetHandle _renderTargetHandle;
    
    /// <inheritdoc/>
    public override void Create()
    {
        // Configures where the render pass should be injected.

        // Material m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial("Hidden/Universal Render Pipeline/CopyDepth");
        m_ScriptablePass = new CustomRenderPassFeaturePass(RenderPassEvent.BeforeRenderingPrePasses, RenderQueueRange.opaque, _settings.filterSettings.LayerMask);
        m_ScriptablePass.renderPassEvent = _settings.Event;
        // 最终确定rt的名字
        _renderTargetHandle.Init("PlayerDepth");
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderingData.cameraData.cameraTargetDescriptor, _renderTargetHandle);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}