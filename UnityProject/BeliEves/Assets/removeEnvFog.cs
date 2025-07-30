using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class removeEnvFog : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Called when another collider enters the trigger
    void OnTriggerEnter(Collider other)
    {
        RenderSettings.fog = false; // Ensure fog is disabled when the player enters
    }
}
