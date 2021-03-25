using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**From http://wiki.unity3d.com/index.php?title=CameraFacingBillboard */

public class Camera_facing : MonoBehaviour
{
        private Camera m_Camera;

        void Start() {
            m_Camera = GameObject.FindGameObjectWithTag( "Player" ).GetComponentInChildren<Camera>();
        }

        //Orient the camera after all movement is completed this frame to avoid jittering
        void LateUpdate() {
            transform.LookAt( transform.position + m_Camera.transform.rotation * Vector3.forward,
                              m_Camera.transform.rotation * Vector3.up );
        }
}
