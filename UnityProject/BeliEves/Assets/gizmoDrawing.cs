using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gizmoDrawing : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        // Get all colliders attached to this GameObject
        Collider[] colliders = GetComponents<Collider>();

        // Iterate through each collider and draw its gizmo
        foreach (Collider collider in colliders)
        {
            Gizmos.color = Color.green; // Set the color for the gizmos

            // Draw gizmos based on the type of collider
            if (collider is BoxCollider boxCollider)
            {
                Gizmos.DrawWireCube(boxCollider.bounds.center, boxCollider.bounds.size);
            }
            else if (collider is SphereCollider sphereCollider)
            {
                Gizmos.DrawWireSphere(sphereCollider.bounds.center, sphereCollider.radius);
            }
            else if (collider is CapsuleCollider capsuleCollider)
            {
                Gizmos.DrawWireSphere(capsuleCollider.bounds.center, capsuleCollider.radius);
                Gizmos.DrawLine(capsuleCollider.bounds.center + Vector3.up * (capsuleCollider.height / 2), 
                                capsuleCollider.bounds.center - Vector3.up * (capsuleCollider.height / 2));
            }
            // Add more collider types as needed
        }
    }
}
