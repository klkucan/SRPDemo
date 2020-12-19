﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "CustomRP/SPR1")]
public class SRP1Asset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new SRP1(this);
    }
}
