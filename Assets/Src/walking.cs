using UnityEngine;
using RawInputSharp;
using UnityEngine.UI;

public class walking : MonoBehaviour
{
        /// <summary>
        /// RawMouseDriver from :  Peter Brumblay
        /// http://www.jstookey.com/arcade/rawmouse/
        /// </summary>
        RawMouseDriver.RawMouseDriver mousedriver;

        //Mice Sensitivity
        public InputField INXsensitivity;// sensibility of the X axis of the mice : for rotations
        public InputField INYSensitivity;// sensibility of the y axis of the mice : for translations


        public InputField INMouseDPI; // Inputfield for mouse DPI
        public InputField INBallRadius; // Inputfield for radius of ball from track ball

        public InputField Heading; // display heading
        public InputField Position; // display Position
        public InputField Distance; // display total distance walked

        //Declare the Raw Mice
        //  private RawMouse mouse1;
        //   private RawMouse mouse2;
        //   private RawMouse mouse3;

        private RawMouse[] rawMice;
        private RawMouseInput rawmouseinput;
        private int MiceCount;

        bool testhead = false;
        bool testdistance = false;


        private float temphead = 0;// temp variable to calculate cumualted heading during test
        private float tempdist = 0;// temp variable to calculate cumualted distances

        //Delta Y For detector 1 and 2
        private float[] YDelta;
        //Delta X For Detector 1 and 2
        private float[] XDelta;

        public float RotationY; // angle to rotate on y axis
        public Vector3 Move; // movement vector

        private int Dect1 = 0; // mouse index of detector 1
        private int Dect2 = 0; // mouse index of detector 2

        private bool Show_Mice_Select_Window = true; // display or not window to select detectors

        private CharacterController BeeController;

        public bool SideMove = false;

        public RaycastHit hit;
        public RaycastHit looking;
        public RaycastHit edge_ray;
        public LineRenderer line;
        private float raycast_line_dist;

        private GameObject Teleporter_Right;
        private GameObject Teleporter_Left;

        public float dist = 16f;
        //Declare Windows Rect For Mouse Selection
        Rect windowRect1 = new Rect( Screen.width / 2, Screen.height / 2, Screen.width / 3.5f,
                                     Screen.height / 2 );


        void GetMice() {
            MiceCount = rawmouseinput.Mice.Count;
            rawMice = new RawMouse[MiceCount];
            for( int i = 0; i < MiceCount; i++ ) {
                mousedriver.GetMouse( i, ref rawMice[i] );
            }

            //set detector 1
            YDelta[0] = rawMice[Dect1].YDelta;
            XDelta[0] = rawMice[Dect1].XDelta;
            //set detector2
            YDelta[1] = rawMice[Dect2].YDelta;
            XDelta[1] = rawMice[Dect2].XDelta;
        }

        public void SetMice() {
            Show_Mice_Select_Window = true;
        }

        // Use this for initialization
        void Start() {
            Application.targetFrameRate = 600;
            SideMove = false;

            line.enabled = true;

            rawmouseinput = new RawMouseInput();
            mousedriver = new RawMouseDriver.RawMouseDriver();

            YDelta = new float[2];
            XDelta = new float[2];

            BeeController = GetComponent<CharacterController>();
            BeeController.minMoveDistance = 0;

            INBallRadius.text = 2.45.ToString(); // initialize ball radius to 5cm
            INMouseDPI.text = 1000.ToString(); // initialize mouse DPI to 1000dpi

            INXsensitivity.text = ( -1 ).ToString(); // -1 because bee is on the other side of the screen
            INYSensitivity.text = 1.ToString(); //

            position_teleporter();

        }


        void Update() {

            GetMice();

            if( gameObject.GetComponent<ExperimentManager>().Experiment_data.is_2D ) {

                Move = transform.right * ( ( XDelta[1] + XDelta[0] ) / 2 ) *
                       ( 2.54f / float.Parse( INMouseDPI.text ) ) * float.Parse( INXsensitivity.text );
                Move = Move * 2 * dist / ( 2 * Mathf.PI * float.Parse( INBallRadius.text,
                                           System.Globalization.CultureInfo.InvariantCulture.NumberFormat ) );
            } else {
                float dpi_to_cm = 2.54f / float.Parse( INMouseDPI.text );

                float recorded_ball_rotation = ( XDelta[1] + XDelta[0] ) * dpi_to_cm / 2;
                float radius = float.Parse(
                                   INBallRadius.text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat );

                RotationY =  recorded_ball_rotation * 180 / ( Mathf.PI * radius );


                float recored_ball_movement = Mathf.Sqrt( Mathf.Pow( YDelta[0], 2 ) + Mathf.Pow( YDelta[1], 2 ) );


                Move = transform.forward * recored_ball_movement * dpi_to_cm * float.Parse(
                           INYSensitivity.text ); //forward movement

                // this probably should never be turned on as the bees can't walk sideways in the VR
                if( SideMove == true ) {
                    Move += transform.right * ( -YDelta[0] + YDelta[1] ) * ( 2.54f / float.Parse(
                                INMouseDPI.text ) ) * float.Parse( INYSensitivity.text ); // lateral movement
                }
            }

            if( BeeController.enabled == true ) {
                BeeController.Move( Move * 0.01f );

                if( !gameObject.GetComponent<ExperimentManager>().Experiment_data.is_2D ) {
                    transform.Rotate( 0, RotationY * float.Parse( INXsensitivity.text ), 0 );
                }

            }

            if( testhead == true ) {
                temphead += RotationY;
                Heading.text = temphead.ToString();
            } else if( testdistance == true ) {
                tempdist += Mathf.Sqrt( Mathf.Pow( Move.x, 2 ) + Mathf.Pow( Move.z, 2 ) );
                Distance.text = ( tempdist ).ToString();

            } else {
                Position.text = ( transform.position * 10 ).ToString();
                Heading.text = transform.rotation.eulerAngles.y.ToString();
            }
            //Raycasting and display of line of sight
            var layerMask = ~( ( 1 << 2 ) | ( 1 << 5 ) );



            Physics.Raycast( transform.position, transform.forward, out hit, 50, layerMask );
            Physics.Raycast( transform.position, transform.forward, out looking, 50 );
            Physics.Raycast( transform.position, transform.forward, out edge_ray, 50,
                             LayerMask.GetMask( "Edge", "Ignore Raycast" ) );
            /*  if( hit.collider != null ) {
                  raycast_line_dist = hit.distance;
              } else {
                  raycast_line_dist = 50;
              }*/
            if( edge_ray.collider != null ) {
                raycast_line_dist = edge_ray.distance;
            } else {
                raycast_line_dist = 50;
            }
            Debug.DrawRay( transform.position, transform.forward * raycast_line_dist, Color.yellow );

            line.SetPosition( 0, transform.position );
            line.SetPosition( 1, transform.position + transform.forward * raycast_line_dist );
        }

        public void Testheading() {
            testhead = !testhead;
            temphead = 0;

        }
        public void Testdistance() {
            testdistance = !testdistance;
            tempdist = 0;

        }

        public void OnGUI() {

            if( Show_Mice_Select_Window ) {
                // Register the window.
                windowRect1 = GUI.Window( 0, windowRect1, WindowDetector1, "Detectors" );
            }
            if( testdistance == true ) {
                GUILayout.BeginVertical();
                GUILayout.Box( "Distance expected: " + ( float.Parse( INBallRadius.text,
                               System.Globalization.CultureInfo.InvariantCulture.NumberFormat ) * 2 * Mathf.PI ).ToString() +
                               " cm" );
                GUILayout.Box( "Xsensitivity should be :" + ( ( float.Parse( INBallRadius.text,
                               System.Globalization.CultureInfo.InvariantCulture.NumberFormat ) * 2 * Mathf.PI ) /
                               tempdist ).ToString() );
                GUILayout.EndVertical();
            }
            if( testhead == true ) {
                GUILayout.BeginVertical();
                GUILayout.Box( "Angle expected: 360°" );
                GUILayout.Box( "Ysensitivity should be :" + ( Mathf.Abs( 360 / temphead ) ).ToString() );
                GUILayout.EndVertical();
            }


        }


        // Make the contents of the window
        void WindowDetector1( int windowID ) {

            GUILayout.BeginHorizontal();//Begins detectors selection
            // each button assigns the corresponding mouse to the role of detector 1
            GUILayout.BeginVertical();//Begins detector 1 selection
            GUILayout.Box( "Detector 1" );
            for( int i = 0; i < MiceCount; i++ ) {
                if( GUILayout.Button( "Mouse " + i + ": " + rawMice[i].X + ";" + rawMice[i].Y ) ) {
                    Dect1 = i;
                }
            }
            GUILayout.EndVertical();//ends detector 1 selection

            // each button assigns the corresponding mouse to the role of detector 2
            GUILayout.BeginVertical();// begins detector 2 selection
            GUILayout.Box( "Detector 2" );
            for( int i = 0; i < MiceCount; i++ ) {
                if( GUILayout.Button( "Mouse " + i + ": " + rawMice[i].X + ";" + rawMice[i].Y ) ) {
                    Dect2 = i;
                }
            }
            GUILayout.EndVertical();//ends detector 2 selection

            GUILayout.EndHorizontal();// ends detector selection

            GUILayout.BeginVertical();// begins detectors feddback display
            GUILayout.BeginHorizontal();
            GUILayout.Box( "Detector 1 Delta Y: " + YDelta[0] );
            GUILayout.Box( "Detector 2 Delta Y: " + YDelta[1] );
            GUILayout.EndHorizontal();// ends detector feedback display

            if( GUILayout.Button( "Done " ) ) { // close the window
                Show_Mice_Select_Window = false;
            }
            GUILayout.EndVertical();// ends feedback display

            GUI.DragWindow(); // Make window draggable
        }



        void OnApplicationQuit() {
            if( mousedriver != null ) {
                mousedriver.Dispose();
            }
        }

        private void OnTriggerEnter( Collider other ) {
            if( other.name == "Teleporter Left(Clone)" ) {
                transform.position = Teleporter_Right.transform.position + new Vector3(
                                         -Teleporter_Right.transform.localScale.x - 0.005f, 0, 0 );
            } else if( other.name == "Teleporter Right(Clone)" ) {
                transform.position = Teleporter_Left.transform.position + new Vector3(
                                         Teleporter_Left.transform.localScale.x + 0.005f, 0, 0 );
            }
        }

        public void position_teleporter() {
            if( gameObject.GetComponent<ExperimentManager>().Experiment_data.is_2D ) {
                dist = 2 * Mathf.PI * float.Parse( INBallRadius.text );
                if( GameObject.Find( "Teleporter Left(Clone)" ) != null &&
                    GameObject.Find( "Teleporter Right(Clone)" ) != null ) {
                    Teleporter_Left = GameObject.Find( "Teleporter Left(Clone)" );
                    Teleporter_Right = GameObject.Find( "Teleporter Right(Clone)" );

                    Teleporter_Left.transform.position = new Vector3( -dist * 0.01f - 0.05f,
                            Teleporter_Left.transform.position.y, Teleporter_Left.transform.position.z );
                    Teleporter_Right.transform.position = new Vector3( dist * 0.01f + 0.05f,
                            Teleporter_Right.transform.position.y, Teleporter_Right.transform.position.z );
                    print( dist.ToString() );
                }
            }
        }
}
