using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal
{
    public class CopyDepthToRT : ScriptableRendererFeature
    {
        public class CopyDepthToRTPass : ScriptableRenderPass
        {
            private RenderTargetHandle source { get; set; }
            private RenderTargetHandle destination { get; set; }
            internal bool AllocateRT { get; set; }
            Material m_CopyDepthMaterial;

            public CopyDepthToRTPass(RenderPassEvent evt, Material copyDepthMaterial)
            {
                base.profilingSampler = new ProfilingSampler(nameof(CopyDepthPass));
                AllocateRT = true;
                m_CopyDepthMaterial = copyDepthMaterial;
                renderPassEvent = evt;
            }

            public void Setup(RenderTargetHandle source, RenderTargetHandle destination)
            {
                this.source = source;
                this.destination = destination;
                this.AllocateRT = AllocateRT && !destination.HasInternalRenderTargetId();
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.colorFormat = RenderTextureFormat.Depth;
                descriptor.depthBufferBits = 32; //TODO: do we really need this. double check;
                descriptor.msaaSamples = 1;
                if (this.AllocateRT)
                    cmd.GetTemporaryRT(destination.id, descriptor, FilterMode.Point);

                // On Metal iOS, prevent camera attachments to be bound and cleared during this pass.
                ConfigureTarget(new RenderTargetIdentifier(destination.Identifier(), 0, CubemapFace.Unknown, 128));
                // ConfigureClear(ClearFlag.None, Color.black);
            }

            /// <inheritdoc/>
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (m_CopyDepthMaterial == null)
                {
                    Debug.LogErrorFormat(
                        "Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.",
                        m_CopyDepthMaterial, GetType().Name);
                    return;
                }

                CommandBuffer cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, new ProfilingSampler("CopyDepthToRT")))
                {
                    RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
                    int cameraSamples = descriptor.msaaSamples;

                    CameraData cameraData = renderingData.cameraData;

                    switch (cameraSamples)
                    {
                        case 8:
                            cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa2);
                            cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa4);
                            cmd.EnableShaderKeyword(ShaderKeywordStrings.DepthMsaa8);
                            break;

                        case 4:
                            cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa2);
                            cmd.EnableShaderKeyword(ShaderKeywordStrings.DepthMsaa4);
                            cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa8);
                            break;

                        case 2:
                            cmd.EnableShaderKeyword(ShaderKeywordStrings.DepthMsaa2);
                            cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa4);
                            cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa8);
                            break;

                        // MSAA disabled
                        default:
                            cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa2);
                            cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa4);
                            cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa8);
                            break;
                    }

                    cmd.SetGlobalTexture("PlayerDepth", source.Identifier());

                    float flipSign = (cameraData.IsCameraProjectionMatrixFlipped()) ? -1.0f : 1.0f;
                    Vector4 scaleBiasRt = (flipSign < 0.0f)
                        ? new Vector4(flipSign, 1.0f, -1.0f, 1.0f)
                        : new Vector4(flipSign, 0.0f, 1.0f, 1.0f);
                    // cmd.SetGlobalVector(ShaderPropertyId.scaleBiasRt, scaleBiasRt);
                    

                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_CopyDepthMaterial);
                }


                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            /// <inheritdoc/>
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                if (cmd == null)
                    throw new ArgumentNullException("cmd");

                if (this.AllocateRT)
                    cmd.ReleaseTemporaryRT(destination.id);
                destination = RenderTargetHandle.CameraTarget;
            }
        }

        CopyDepthToRTPass m_CopyDepthPass;
        private RenderTargetHandle m_DepthTexture, m_CameraDepthAttachment;

        /// <inheritdoc/>
        public override void Create()
        {
            Material m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial("Hidden/Universal Render Pipeline/CopyDepth");
            m_CopyDepthPass = new CopyDepthToRTPass(RenderPassEvent.BeforeRenderingOpaques, m_CopyDepthMaterial);

            // Configures where the render pass should be injected.
            m_CopyDepthPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_CameraDepthAttachment.Init("PlayerDepth");
            m_DepthTexture.Init("TSAI_DEPTH");
            m_CopyDepthPass.Setup(m_CameraDepthAttachment, m_DepthTexture);
            renderer.EnqueuePass(m_CopyDepthPass);
        }
    }
}