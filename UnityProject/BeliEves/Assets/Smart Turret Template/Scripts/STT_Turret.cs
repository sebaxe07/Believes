using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class TurretParameters {

	[Header("Status")]
	[Tooltip("Activate or deactivate the Turret")]
	public bool active;
	public bool canFire;

	[Header("Shooting")]
	[Tooltip("Burst the force when hit")]
	public float power;
	public GameObject projectilePrefab;
	public float projectileSpeed = 50f;
	[Tooltip("Pause between shooting")]
	[Range(0.5f, 2)]
	public float ShootingDelay;
	[Tooltip("Radius of the turret view")]
	public float radius;
}

[System.Serializable]
public class TurretFX {

	[Tooltip("Muzzle transform position")]
	public Transform muzzle;
	[Tooltip("Spawn this GameObject when shooting")]
	public GameObject shotFX;

	public SoundID _shootSound = default;
}


[System.Serializable]
public class TurretTargeting {

	[Tooltip("Speed of aiming at the target")]
	public float aimingSpeed;
	[Tooltip("Pause before the aiming")]
	public float aimingDelay;
	[Tooltip("GameObject with folowing tags will be identify as enemy")]
	public string[] tagsToFire;
	public List<Collider> targets = new List<Collider>();
	public Collider target;

	public float minPitch = 35f;
	public float maxPitch = -15f;
	public float pitchOffset = 4f;

}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Animator))]
public class STT_Turret : MonoBehaviour {

	public TurretParameters parameters;
	public TurretTargeting targeting;
	public TurretFX VFX;
	private void Awake() {
		GetComponent<SphereCollider>().isTrigger = true;
		GetComponent<SphereCollider>().radius = parameters.radius;
		GetComponent<BoxCollider>().size = new Vector3(2, 2, 2);
		GetComponent<BoxCollider>().center = new Vector3(0, 1, 0);
	}

	private void FixedUpdate() {

		if (parameters.active == false) {
			return;
		}

		if (targeting.target == null) {
			ClearTargets();
		}

		if (targeting.target != null) {
			Aiming();
			Invoke("Shooting", parameters.ShootingDelay);
		}
	}

	#region Aiming and Shooting

	private void Shot() {

		//GetComponent<AudioSource>().PlayOneShot(SFX.shotClip, Random.Range(0.75f, 1));
		BroAudio.Play(VFX._shootSound, transform.position);
		GetComponent<Animator>().SetTrigger("Shot");
		GameObject newShotFX = Instantiate(VFX.shotFX, VFX.muzzle);
		Destroy(newShotFX, 2);
	}
	private void Shooting() {

		if (targeting.target == null) {
			return;
		}

		if (parameters.canFire == false) {
			return;
		}


		// Instantiate the projectile at the muzzle position and rotation
		// Add a 90 degree rotation to the projectile so it faces forward

		GameObject projectile = Instantiate(parameters.projectilePrefab, VFX.muzzle.position, VFX.muzzle.rotation * Quaternion.Euler(90, 0, 0));

		// Get the Rigidbody of the projectile and apply a forward force
		Rigidbody rb = projectile.GetComponent<Rigidbody>();
		if (rb != null) {
			rb.velocity = VFX.muzzle.transform.forward * parameters.projectileSpeed;
		}
		Shot();
		ClearTargets();
		CancelInvoke();

	}

	public void Aiming() {
		if (targeting.target == null) {
			return;
		}

		// Calculate the direction to the target
		Vector3 delta = targeting.target.transform.position - VFX.muzzle.transform.position;
		Quaternion targetRotation = Quaternion.LookRotation(delta);

		// Clamp pitch (x-axis) to limits
		Vector3 targetEulerAngles = targetRotation.eulerAngles;
		float pitch = targetEulerAngles.x > 180f ? targetEulerAngles.x - 360f : targetEulerAngles.x;
		pitch -= targeting.pitchOffset; // Add a small offset to the pitch
		pitch = Mathf.Clamp(pitch, targeting.minPitch, targeting.maxPitch); // Adjust pitch limits
		targetEulerAngles.x = pitch;

		// Draw a line from the turret to the target
		/*Debug.DrawRay(transform.position, delta, Color.red);
		Debug.Log(pitch);

		// Draw a straight line from the muzzle 
		Debug.DrawRay(VFX.muzzle.position, VFX.muzzle.forward * 100, Color.green);*/

		// Apply the clamped rotation to the entire turret structure
		Quaternion clampedRotation = Quaternion.Euler(targetEulerAngles);
		transform.rotation = Quaternion.Slerp(transform.rotation, clampedRotation, Time.deltaTime * targeting.aimingSpeed);
	}

	#endregion

	#region Targeting

	private void OnTriggerStay(Collider other) {

		if (parameters.active == false) {
			return;
		}

		ClearTargets();

		if (CheckTags(other) == true) {

			if (targeting.targets.Count == 0) {
				targeting.target = other.GetComponent<Collider>();
			}

			// Check if the target is already in the list
			if (targeting.targets.Contains(other.GetComponent<Collider>()) == false) {
				targeting.targets.Add(other.GetComponent<Collider>());
			}
		}
	}

	private void OnTriggerEnter(Collider other) {

		if (parameters.active == false) {
			return;
		}

		ClearTargets();

		if (CheckTags(other) == true) {
			if (targeting.targets.Count == 0) {
				targeting.target = other.GetComponent<Collider>();
			}

			targeting.targets.Add(other.GetComponent<Collider>());
		}
	}

	private void OnTriggerExit(Collider other) {

		if (parameters.active == false) {
			return;
		}
		ClearTargets();

		if (CheckTags(other) == true) {
			targeting.targets.Remove(other.GetComponent<Collider>());
			if (targeting.targets.Count != 0) {
				targeting.target = targeting.targets.First();
			}
			else {
				targeting.target = null;
			}
		}
	}

	private bool CheckTags(Collider toMatch) {

		bool Match = false;

		for (int i = 0; i < targeting.tagsToFire.Length; i++) {
			if (toMatch.tag == targeting.tagsToFire[i]) {
				Match = true;
			}
		}

		return (Match);
	}

	private void ClearTargets() {

		if (targeting.target != null) {
			if (targeting.target.GetComponent<Collider>().enabled == false) {
				targeting.targets.Remove(targeting.target);
			}
		}

		foreach (Collider target in targeting.targets.ToList()) {

			if (target == null) {
				targeting.targets.Remove(target);
			}

			if (targeting.targets.Count != 0) {
				targeting.target = targeting.targets.First();
			}
			else {
				targeting.target = null;
			}
		}
	}

	#endregion
}