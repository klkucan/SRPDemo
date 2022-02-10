using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EditorTools : Editor
{
    [MenuItem("Tools/ChangeRenderPipeline")]
    public static void ChangeRenderPipeline()
    {
        string name = GraphicsSettings.renderPipelineAsset
            ? GraphicsSettings.renderPipelineAsset.name
            : "null";
        string msg = "renderPipelineAsset is \n\n" + name + "\n\n switch?";
        if (EditorUtility.DisplayDialog("", msg, "ok", "cancel") == false) return;
        // RenderPipelineAsset[] a =  GraphicsSettings.allConfiguredRenderPipelines;
        if (GraphicsSettings.renderPipelineAsset == null)
        {
            string[] folder = new string[] {"Assets"};
            var renderPipelineAssets = AssetDatabase.FindAssets("t:RenderPipelineAsset", folder);
            for (int i = 0; i < renderPipelineAssets.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(renderPipelineAssets[i]);
                var renderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
                if (renderPipelineAsset.GetType() == typeof(UniversalRenderPipelineAsset))
                {
                    GraphicsSettings.renderPipelineAsset = renderPipelineAsset;
                }
            }
        }
        else
        {
            GraphicsSettings.renderPipelineAsset = null;
        }
    }
}