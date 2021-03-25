/*From http://wiki.unity3d.com/index.php/Animating_Tiled_texture */

using UnityEngine;
using System.Collections;

class AnimateTiledTexture : MonoBehaviour
{
        public float x_offset_increment = 0f;
        public float y_offset_increment = 0f;
        public float framesPerSecond = 600f;

        private Vector2 offset = Vector2.zero;
        private bool Coroutine_Running = false;

        private IEnumerator Animate;

        void Start() {
            Animate = updateTiling();
        }

        public void Play_animation() {
            if( !Coroutine_Running ) {
                StartCoroutine( Animate );
                Coroutine_Running = true;
            }
        }

        public void Stop_animation() {
            if( Coroutine_Running ) {
                StopCoroutine( Animate );
                Coroutine_Running = false;
            }
        }

        private IEnumerator updateTiling() {
            while( true ) {
                offset.x += x_offset_increment;
                offset.y += y_offset_increment;

                if( offset.x >= 1 ) {
                    offset.x = 0;
                }
                if( offset.y >= 1 ) {
                    offset.y = 0;
                }

                GetComponent<Renderer>().sharedMaterial.SetTextureOffset( "_MainTex", offset );

                yield return new WaitForSeconds( 1f / framesPerSecond );
            }

        }

}