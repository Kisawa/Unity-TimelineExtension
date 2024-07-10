using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTimeline
{
    [Serializable]
    public class ParticleSetting
    {
        public bool autoRandomSeed = true;
        public uint randomSeed;

        public void ApplyRandomSeed(ParticleSystem particleSystem)
        {
            if (particleSystem == null)
                return;
            if (!autoRandomSeed)
            {
                uint seed = randomSeed;
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ApplyRandomSeed(particleSystem, seed);
                for (int i = 0; i < particleSystem.subEmitters.subEmittersCount; i++)
                {
                    seed += 1;
                    ApplyRandomSeed(particleSystem.subEmitters.GetSubEmitterSystem(i), seed);
                }
            }
        }

        void ApplyRandomSeed(ParticleSystem particleSystem, uint randomSeed)
        {
            particleSystem.useAutoRandomSeed = false;
            particleSystem.randomSeed = randomSeed;
        }
    }
}