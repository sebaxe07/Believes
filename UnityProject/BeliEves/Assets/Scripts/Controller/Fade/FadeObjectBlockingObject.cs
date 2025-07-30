﻿using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using UnityEngine;
using Utilities.Events.EventsLayout;

namespace Utilities.Fade {
    public class FadeObjectBlockingObject : MonoBehaviour {
        [SerializeField]
        private LayerMask LayerMask;
        [SerializeField]
        private Transform Player;
        [SerializeField]
        private Camera Camera;
        [SerializeField]
        private float FadedAlpha = 0.33f;
        [SerializeField]
        private FadeMode FadingMode;

        [SerializeField]
        private float ChecksPerSecond = 10;
        [SerializeField]
        private int FadeFPS = 30;
        [SerializeField]
        private float FadeSpeed = 1;
        [SerializeField] private Vector3 TargetPositionOffset = Vector3.up;

        [Header("Read Only Data")]
        [SerializeField]
        private List<FadingObject> ObjectsBlockingView = new List<FadingObject>();
        private List<int> IndexesToClear = new List<int>();
        private Dictionary<FadingObject, Coroutine> RunningCoroutines = new Dictionary<FadingObject, Coroutine>();

        private RaycastHit[] Hits = new RaycastHit[10];
    
        private EventBodySwitch _bodySwitchChannel;

        private void Awake() {
            _bodySwitchChannel = (EventBodySwitch)EventBroker.TryToAddEventChannel("bodySwitch", ScriptableObject.CreateInstance<EventBodySwitch>());
            _bodySwitchChannel.Subscribe(new Action(() => SetGameObject(_bodySwitchChannel.bodyType)));
        }

        private void Start() {
            StartCoroutine(CheckForObjects());
        }

        private void SetGameObject(BodyType bodyType) {
            Player = FindObjectOfType<Player.Player>().gameObject.transform;
        }

        private IEnumerator CheckForObjects() {
            WaitForSeconds Wait = new WaitForSeconds(1f / ChecksPerSecond);
            while (true) {
                int hits = Physics.RaycastNonAlloc(Camera.transform.position, (Player.transform.position + TargetPositionOffset - Camera.transform.position).normalized, 
                    Hits, 
                    Vector3.Distance(Camera.transform.position, Player.transform.position + TargetPositionOffset), 
                    LayerMask
                    );
                if (hits > 0) {
                    for (int i = 0; i < hits; i++) {
                        FadingObject fadingObject = GetFadingObjectFromHit(Hits[i]);
                        if (fadingObject != null && !ObjectsBlockingView.Contains(fadingObject))
                        {
                            if (RunningCoroutines.ContainsKey(fadingObject))
                            {
                                if (RunningCoroutines[fadingObject] != null) // may be null if it's already ended
                                {
                                    StopCoroutine(RunningCoroutines[fadingObject]);
                                }

                                RunningCoroutines.Remove(fadingObject);
                            }

                            RunningCoroutines.Add(fadingObject, StartCoroutine(FadeObjectOut(fadingObject)));
                            ObjectsBlockingView.Add(fadingObject);
                        }
                    }
                }

                FadeObjectsNoLongerBeingHit();

                ClearHits();

                yield return Wait;
            }
        }

        private void FadeObjectsNoLongerBeingHit()
        {
            for (int i = 0; i < ObjectsBlockingView.Count; i++)
            {
                bool objectIsBeingHit = false;
                for (int j = 0; j < Hits.Length; j++)
                {
                    FadingObject fadingObject = GetFadingObjectFromHit(Hits[j]);
                    if (fadingObject != null && fadingObject == ObjectsBlockingView[i])
                    {
                        objectIsBeingHit = true;
                        break;
                    }
                }

                if (!objectIsBeingHit)
                {
                    if (RunningCoroutines.ContainsKey(ObjectsBlockingView[i]))
                    {
                        if (RunningCoroutines[ObjectsBlockingView[i]] != null)
                        {
                            StopCoroutine(RunningCoroutines[ObjectsBlockingView[i]]);
                        }
                        RunningCoroutines.Remove(ObjectsBlockingView[i]);
                    }

                    RunningCoroutines.Add(ObjectsBlockingView[i], StartCoroutine(FadeObjectIn(ObjectsBlockingView[i])));
                    ObjectsBlockingView.RemoveAt(i);
                }
            }
        }

        private IEnumerator FadeObjectOut(FadingObject FadingObject)
        {
            float waitTime = 1f / FadeFPS;
            WaitForSeconds Wait = new WaitForSeconds(waitTime);
            int ticks = 1;

            for (int i = 0; i < FadingObject.Materials.Count; i++) {
                FadingObject.Materials[i].SetFloat("_Surface", 1); // 1 = Transparent, 0 = Opaque
                FadingObject.Materials[i].SetFloat("_Blend", 0); // 0 = Alpha blending
                FadingObject.Materials[i].SetFloat("_ZWrite", 0); // Disable ZWrite for transparency
                FadingObject.Materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha); // affects both "Transparent" and "Fade" options
                FadingObject.Materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); // affects both "Transparent" and "Fade" options
                FadingObject.Materials[i].SetInt("_ZWrite", 0); // disable Z writing
                if (FadingMode == FadeMode.Fade)
                {
                    FadingObject.Materials[i].EnableKeyword("_ALPHABLEND_ON");
                }
                else
                {
                    FadingObject.Materials[i].EnableKeyword("_ALPHAPREMULTIPLY_ON");
                }

                FadingObject.Materials[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            if (FadingObject.Materials[0].HasProperty("_Color"))
            {
                while (FadingObject.Materials[0].color.a > FadedAlpha)
                {
                    for (int i = 0; i < FadingObject.Materials.Count; i++)
                    {
                        if (FadingObject.Materials[i].HasProperty("_Color"))
                        {
                            FadingObject.Materials[i].color = new Color(
                                FadingObject.Materials[i].color.r,
                                FadingObject.Materials[i].color.g,
                                FadingObject.Materials[i].color.b,
                                Mathf.Lerp(FadingObject.InitialAlpha, FadedAlpha, waitTime * ticks * FadeSpeed)
                            );
                        }
                    }

                    ticks++;
                    yield return Wait;
                }
            }

            if (RunningCoroutines.ContainsKey(FadingObject))
            {
                StopCoroutine(RunningCoroutines[FadingObject]);
                RunningCoroutines.Remove(FadingObject);
            }
        }

        private IEnumerator FadeObjectIn(FadingObject FadingObject)
        {
            float waitTime = 1f / FadeFPS;
            WaitForSeconds Wait = new WaitForSeconds(waitTime);
            int ticks = 1;

            if (FadingObject.Materials[0].HasProperty("_Color"))
            {
                while (FadingObject.Materials[0].color.a < FadingObject.InitialAlpha)
                {
                    for (int i = 0; i < FadingObject.Materials.Count; i++)
                    {
                        if (FadingObject.Materials[i].HasProperty("_Color"))
                        {
                            FadingObject.Materials[i].color = new Color(
                                FadingObject.Materials[i].color.r,
                                FadingObject.Materials[i].color.g,
                                FadingObject.Materials[i].color.b,
                                Mathf.Lerp(FadedAlpha, FadingObject.InitialAlpha, waitTime * ticks * FadeSpeed)
                            );
                        }
                    }

                    ticks++;
                    yield return Wait;
                }
            }

            for (int i = 0; i < FadingObject.Materials.Count; i++)
            {
                FadingObject.Materials[i].SetFloat("_Surface", 0); // 0 = Opaque
                FadingObject.Materials[i].SetFloat("_Blend", 0); // No blending for opaque materials
                FadingObject.Materials[i].SetFloat("_ZWrite", 1); // Enable ZWrite
                if (FadingMode == FadeMode.Fade)
                {
                    FadingObject.Materials[i].DisableKeyword("_ALPHABLEND_ON");
                }
                else
                {
                    FadingObject.Materials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                }
                FadingObject.Materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                FadingObject.Materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                FadingObject.Materials[i].SetInt("_ZWrite", 1); // re-enable Z Writing
                FadingObject.Materials[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            }

            if (RunningCoroutines.ContainsKey(FadingObject))
            {
                StopCoroutine(RunningCoroutines[FadingObject]);
                RunningCoroutines.Remove(FadingObject);
            }
        }

        private FadingObject GetFadingObjectFromHit(RaycastHit Hit)
        {
            return Hit.collider != null ? Hit.collider.GetComponent<FadingObject>() : null;
        }

        private void ClearHits()
        {
            RaycastHit hit = new RaycastHit();
            for (int i = 0; i < Hits.Length; i++)
            {
                Hits[i] = hit;
            }
        }

        public enum FadeMode
        {
            Transparent,
            Fade
        }
    }
}