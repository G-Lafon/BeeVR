using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{

        public walking Walker;
        public Camera beeview;
        public Camera Output;

        public GameObject OutputScreen;
        public DisplayManager displayManager;
        public ConditionningRunner conditionningRunner;

        Rect windowRectSetting = new Rect( Screen.width / 2, Screen.height / 2, 200, 170 );

        Rect windowRectEscape = new Rect( Screen.width / 2, Screen.height / 2, 150, 170 );

        Rect windowrectquit = new Rect( Screen.width / 2, Screen.height / 2, 150, 120 );

        Vector2 scrollPosition;

        private string INSensX; // XSensitivity
        private string INSensY; // YSensitivity
        private string FOV; // bee FOV
        private string size;

        private string OutputScaleX;
        private string OutputScaleY;
        private string OutputScaleZ;
        private Vector3 scale;

        private bool draw = false;
        private bool Menu = false;
        private bool quit = false;

        private bool initcali = false;

        // Use this for initialization
        void Start() {
            Output = displayManager.ScreenCamera;
            OutputScreen = displayManager.Screen;


            INSensX = Walker.INXsensitivity.text;
            INSensY = Walker.INYSensitivity.text;
            FOV = beeview.fieldOfView.ToString();
            size = Output.orthographicSize.ToString();

            OutputScaleX = OutputScreen.transform.localScale.x.ToString();
            OutputScaleY = OutputScreen.transform.localScale.y.ToString();
            OutputScaleZ = OutputScreen.transform.localScale.z.ToString();
        }


        private void Update() {
            UpdateOutpuCamera();


            if( displayManager.cali != initcali ) {
                 initcali = displayManager.cali;
            }



            if( Input.GetKeyDown( KeyCode.F1 ) ) { // F1 to open the setting window, meant for debugging
                if( INSensX == string.Empty || INSensY == string.Empty ) {
                    INSensX = Walker.INXsensitivity.text;
                    INSensY = Walker.INYSensitivity.text;
                }
                draw = !draw;
            }

            if( Input.GetKeyDown( KeyCode.Escape ) ) {
                Menu = !Menu;
            }

        }

        private void OnGUI() {
            if( draw ) {
                windowRectSetting = GUI.Window( 5, windowRectSetting, WindowSetting, "Settings" ); //draw the window

                Walker.INXsensitivity.text = INSensX;
                Walker.INYSensitivity.text = INSensY;
                beeview.fieldOfView = float.Parse( FOV );
                Output.orthographicSize = float.Parse( size );

                scale.x = float.Parse( OutputScaleX );
                scale.y = float.Parse( OutputScaleY );
                scale.z = float.Parse( OutputScaleZ );
                OutputScreen.transform.localScale = scale;

            }

            if( Menu ) {
                windowRectEscape = GUI.Window( 11, windowRectEscape, WindowMenu, "Menu" );
            }

            if( quit ) {
                windowrectquit = GUILayout.Window( 12, windowrectquit, Windowquit, "Quit ?" );
            }

        }

        void WindowMenu( int WindowID ) {
            if( GUILayout.Button( "Quit" ) ) {
                quit = !quit;
                Menu = !Menu;
            }

            GUI.DragWindow();

        }


        void Windowquit( int WindowID ) {

            GUILayout.BeginVertical();
            GUILayout.Box( "Are You Sure ? \n Unsaved progess will be lost" );

            GUILayout.BeginHorizontal();
            if( GUILayout.Button( "Yes" ) ) {
                conditionningRunner.Close_port();
                Application.Quit();
            } else if( GUILayout.Button( "No" ) ) {
                quit = !quit;
                Menu = !Menu;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        void WindowSetting( int WindowID ) {
            scrollPosition = GUILayout.BeginScrollView( scrollPosition );

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Box( "X Sensitivity :" );
            INSensX = GUILayout.TextField( INSensX, 10 );
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box( "Y Sensitivity :" );
            INSensY = GUILayout.TextField( INSensY, 10 );
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box( "Bee FOV :" );
            FOV = GUILayout.TextField( FOV, 3 );
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if( GUILayout.Button( "Allow Side Move" ) ) {
                Walker.SideMove = !Walker.SideMove;
            };
            if( Walker.SideMove == true ) {
                GUILayout.Box( "Yes" );
            } else {
                GUILayout.Box( "No" );
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box( "Camera size :" );
            size = GUILayout.TextField( size, 10 );
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box( "Screen X :" );
            OutputScaleX = GUILayout.TextField( OutputScaleX, 10 );
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box( "Screen Y :" );
            OutputScaleY = GUILayout.TextField( OutputScaleY, 10 );
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box( "Screen Z :" );
            OutputScaleZ = GUILayout.TextField( OutputScaleZ, 10 );
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        private void UpdateOutpuCamera() {
            Output = displayManager.ScreenCamera;
            OutputScreen = displayManager.Screen;

            OutputScaleX = OutputScreen.transform.localScale.x.ToString();
            OutputScaleY = OutputScreen.transform.localScale.y.ToString();
            OutputScaleZ = OutputScreen.transform.localScale.z.ToString();

        }


}
