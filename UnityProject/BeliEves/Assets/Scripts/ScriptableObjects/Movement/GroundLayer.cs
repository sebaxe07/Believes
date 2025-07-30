using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects.Movement
{    
    [CreateAssetMenu (menuName="ScriptableObjects/Movement/Ground Layer")]
    public class GroundLayer: ScriptableObject{
        [FormerlySerializedAs("GroundMask")] public LayerMask groundMask;
    }
}