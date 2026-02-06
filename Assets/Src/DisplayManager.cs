using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DisplayManager : MonoBehaviour
{

        public Camera MainCamera;
        public Canvas Menu;
        public Camera ScreenCamera;

        public GameObject Screen;

        public Texture Calib;
        public RenderTexture screenText;

        private bool a = false;
        public bool cali = false;
    
        // Use this for initialization
        void Start() {

            if( Display.displays.Length > 1 ) { // if there is more than 1 display
                Display.displays[1].Activate(); // activate the second display
            }
        }

        public void Calibrate() {
            cali = !cali;
            if( cali == true ) {
                Screen.GetComponent<Renderer>().material.mainTexture = Calib;
            } else {
                Screen.GetComponent<Renderer>().material.mainTexture = screenText;
            }
        }

        public void StopCursor() {
            Cursor.visible = !Cursor.visible;
        }


        private void OnGUI() {
            if( Event.current.Equals(
                    Event.KeyboardEvent( "f11" ) ) ) { // allows to switch between camerra pressing F11, meant for debugging
                if( !a ) {
                    MainCamera.targetDisplay = 0;
                    Menu.targetDisplay = 0;
                    ScreenCamera.targetDisplay = 1;

                    a = !a;

                } else {
                    MainCamera.targetDisplay = 1;
                    Menu.targetDisplay = 1;
                    ScreenCamera.targetDisplay = 0;

                    a = !a;

                }

            }


            if( Event.current.Equals( Event.KeyboardEvent( "f2" ) ) ) {
                StopCursor();
            }

        }


}
