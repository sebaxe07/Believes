using System;
using System.Collections;
using AimIK.Behaviour;
using Events;
using Events.EventsLayout;
using UnityEditor;
using UnityEngine;
using Utilities;
using Utilities.Events.EventsLayout;
using Object = UnityEngine.Object;

namespace Player {
    public class RobotAttackInput : Player {

        private EventWithGameObj _interactWithObjectChannel;
        private BasicEventChannel _interactionFinishedEventChannel;
        private BasicEventChannel _knockOutEventChannel;

        /*private new void Update() {
            if(Input.GetKeyDown(KeyCode.K)) KnockOut();
        }*/

        private void KnockOut() {
            _knockOutEventChannel = EventBroker.TryToAddEventChannel("knockOutEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
            _knockOutEventChannel.RaiseEvent();
        }

        protected override void ReleaseEve() {
            if (BodySwitchChannel == null) BodySwitchChannel = (EventBodySwitch)EventBroker.TryToAddEventChannel("bodySwitch", ScriptableObject.CreateInstance<EventBodySwitch>());

            Vector3 newPosition = new Vector3(transform.position.x, transform.transform.position.y, transform.position.z) + Vector3.forward * 3.5f;
            GameObject eve = Object.Instantiate(Resources.Load<GameObject>("Prefabs/Eve"), newPosition, Quaternion.identity);

            StartCoroutine(VFXCoroutine(eve, Resources.Load<GameObject>("Vfx/SoulsEscape")));

            //GetComponent<AimIKBehaviour3D>().enabled = false;   



            Destroy(GetComponent<Player>());

            BodySwitchChannel.UpdateValue(BodyType.Eve);

            // Get animator from the robot
            var animator = GetComponentInChildren<Animator>();
            // Set the state to 6 (knockout)
            animator.SetInteger("State", 6);
            gameObject.layer = 0;
            CoroutineRunner.Instance.StartCoroutine(RemoveBot(gameObject));
        }

        private IEnumerator RemoveBot(GameObject bot) {
            yield return new WaitForSeconds(5f);
            Destroy(bot);
        }


        private IEnumerator VFXCoroutine(GameObject eve, GameObject vfx) {
            GameObject go = Instantiate(vfx, eve.transform, true);
            go.transform.position = eve.transform.position;
            yield return new WaitForSeconds(0.2f);
            Destroy(go);
            yield return new WaitForSeconds(1.2f);
            this.gameObject.SetActive(false);
            yield return null;

        }
    }
}