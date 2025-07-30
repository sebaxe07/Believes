using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Utilities
{
    public class CameraFade : MonoBehaviour {
        [SerializeField] private Canvas fadeCanvas;
        [SerializeField] private UnityEngine.UI.Image fadeImage;
        [SerializeField] private float speedScale = 1f;
        [SerializeField] private Color fadeColor;
        // Rather than Lerp or Slerp, we allow adaptability with a configurable curve
        [SerializeField] private AnimationCurve curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0.5f, -1.5f, -1.5f), new Keyframe(1, 0));
        [FormerlySerializedAs("_alpha")] [SerializeField] private float alpha = 0f; 
        //private Texture2D _texture;
        private int _direction = 0;
        private float _time = 0f;

        private void Start() {
            if(fadeCanvas==null) return;
            
            fadeCanvas.gameObject.SetActive(true);
            //_texture = new Texture2D(1, 1);
            //_texture.SetPixel(0, 0, new Color(fadeColor.r, fadeColor.g, fadeColor.b, _alpha));
            //_texture.Apply();
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            fadeCanvas.sortingOrder = 10; 
        }

        public void StartFade() {
            if(fadeCanvas==null) return;
            
            if (_direction == 0) {
                if (alpha != 0f){ // Fully faded out
                    //alpha = 1f;
                    _time = 0f;
                    _direction = 1;
                }
                else{ // Fully faded in
                    //alpha = 0f;
                    _time = 1f;
                    _direction = -1;
                }
                 
            }
        }

        public void PrepareFade() {
            if(fadeCanvas==null) return;
            
            alpha = 1f;
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
        }
        
        /*private void Update() {
            if (UnityEngine.Input.GetKeyDown(KeyCode.K)) StartFade();
        }*/

        public void OnGUI() {
            if(fadeCanvas==null) return;
            //if (_alpha > 0f) GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _texture);
            if (_direction != 0) {
                _time += _direction * Time.deltaTime * speedScale;
                alpha -= curve.Evaluate(_time);
                if (alpha < 0f || alpha > 1f) {
                    _direction = 0;
                    return;
                }

                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            }
        }
    }
}