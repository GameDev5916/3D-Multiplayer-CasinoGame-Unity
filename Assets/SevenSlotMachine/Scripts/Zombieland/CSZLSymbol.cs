using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSZLSymbol : CSSymbol {
    public List<Texture> paylineFrameTextures;

    public override void AddParticle(CSPayline payline = null, float mult = 1)
    {
        base.AddParticle(payline, mult);
        if (payline == null || _particle == null)
            return;
        _particle.GetComponent<ParticleSystemRenderer>().material.SetTexture("_MainTex", FrameForPayline(payline));
    }

    private Texture FrameForPayline(CSPayline payline)
    {
        int idx = payline.line == null ? 0 : payline.line.number - 1;
        return paylineFrameTextures[idx];
    }
}
