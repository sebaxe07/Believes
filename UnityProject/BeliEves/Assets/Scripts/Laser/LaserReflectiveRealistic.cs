using UnityEngine;

namespace Laser {
    public class LaserReflectiveRealistic: MonoBehaviour, ILaserReflective {
        public void Reflect(Laser laser, Ray incomingRay, RaycastHit hitInfo) {
            var outgoingDir = Vector3.Reflect(incomingRay.direction, hitInfo.normal);
            laser.CastBeam(hitInfo.point, outgoingDir);
        }
    }

    public interface ILaserReflective {
        abstract void Reflect(Laser laser, Ray incomingRay, RaycastHit hitInfo);
    }
}