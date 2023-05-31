using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMapRender : MonoBehaviour
{
        public Camera cam;

        public RenderTexture render_text;
        // Update is called once per frame
        void Update() {

            cam.RenderToCubemap( render_text );
        }
}
