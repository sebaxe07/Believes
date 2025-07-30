using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesCollection : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] lightAttackParticles;
    [SerializeField] private ParticleSystem[] heavyAttackParticles;
    [SerializeField] private ParticleSystem[] specialAttackParticles;
    
    public ParticleSystem[] GetLightAttackParticles()
    {
        return lightAttackParticles;
    }
    
    public ParticleSystem[] GetHeavyAttackParticles()
    {
        return heavyAttackParticles;
    }
    
    public ParticleSystem[] GetSpecialAttackParticles()
    {
        return specialAttackParticles;
    }
}
