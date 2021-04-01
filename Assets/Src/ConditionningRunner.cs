using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;


public class ConditionningRunner : MonoBehaviour
{


        public bool ChoiceIsMade = false; // If the Bee made a choice eg. went to one of the stimuli
        public string Choice; // choice made by the bee, gets wrote down in the txt file results
        public string Side_Chosen;
        public string Side;
        private string Side_looked_at;
        public string Centered_object;
        public string Object_looked_at;
        public GameObject Edge_looked_at;
        private bool bell_notif = false; // did we played the bell yet ?

        public bool Go = false; // wether or not to begin the experiment
        private bool Power_on = false;// if Space was pressed

        public ArenaManager arenaManager;


        private float
        PrepPhaseTimer; // Duration of the pre trial phase, between the inter trial interval and the actual trial
        private float USTimer; // Amount of time to give the US
        private float TrialTimer; // duration of the trial
        private float CSTimer;// duration before the CS appearance
        private string[] Stim_names;// names of the pair of stims for this trial
        private bool Stim_On = false;
        private int Repetition; // numbre of repetition of the line
        private int Test; // Is the line a test or not
        private int PreTest; // Is the line a Pretest or not
        private int BeeID;

        public InputField IN_latency; // display latency before actual start of XP
        private float latency;
        private float Tmp_latency;
        private bool Waiting;

        private float Tmp_X;
        private float Tmp_Z;

        public float Dist; // Distance walked on the ball
        private float tempsDist; // hold final value of dist in one trial for display
        public float Speed; // Walking speed of the bee

        public int Line = 0; // Index of the line in the experimental programme
        public int a = 1;// Index of repetition within the line

        private Vector3 Stay; // position of the bee when it enter the stimuli area

        private Quaternion look; // the initial orientation
        private Vector3 Stand; // the initial position


        public InputField PREPTIME; // Inputfield to display duration of the prep phase
        public InputField CSSTART; // Inputfield to display delay before CS
        public InputField CSSTOP; // inputfield to display duration of trial
        public InputField US; // inputfield to display duration of US
        public InputField where; // display number of current line and current repetition
        public InputField what; // display if test or not


        private AudioSource Speaker; // Audio source
        public AudioClip bell; // doorbell sound from: https://freesound.org/people/bennstir/sounds/81072/
        public AudioClip Reload; // reload sound from: https://freesound.org/people/nioczkus/sounds/396331/


        public bool BeeDone = false;
        public Button Butt_Power;

        private bool TrialSummaryDisp = false;

        private float TimerLeft = 0;
        private float TimerRight = 0;
        public InputField INPretestChoice;

        public GameObject BeeScreen; //Screen diplaying stuff to the bee
        public RenderTexture screenText;
        private int nbr;

        private bool is_full_stim_on = false; // Is the full Screen Stim toggled

        private int[] absolute_stim = { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };
        //private bool concept = false;
        //private bool primer = false;

        private string[] stim_flag = { "Left", "Right" };
        private string[] env_flag = { "Wall", "Floor" };

        private SerialPort serial_port;
        public InputField IN_port_name;
        public InputField IN_conection_status;

        private bool pinged = false;

        private ExperimentRecorder Recorder;
        private ExperimentManager Xpmanager;

        // Use this for initialization
        void Start() {
            nbr = -1;

            Reset_Choice();
            latency = 10;
            Tmp_latency = latency;
            IN_latency.text = latency.ToString();

            IN_port_name.text = "COM3";
            IN_conection_status.text = "None";

            Speaker = GetComponent<AudioSource>(); // initialize audiosource

            Recorder = gameObject.GetComponent<ExperimentRecorder>();
            Xpmanager = gameObject.GetComponent<ExperimentManager>();
        }

        public bool bee_can_move() {
            return gameObject.GetComponent<CharacterController>().enabled;
        }

        // Update is called once per frame
        void Update() {
            if( Power_on && Waiting ) {
                if( Tmp_latency > 0 ) {
                    Tmp_latency -= Time.deltaTime;
                    IN_latency.text = Tmp_latency.ToString();
                } else if( Tmp_latency <= 0 ) {
                    Go = true;
                    Waiting = false;

                    if( Recorder.Path.text != string.Empty ) {
                        Recorder.Start_Record();
                    }
                }
            }

            if( Go == true ) { // runs the experiment
                if( CSTimer > 0 ) { // if Csstart Timer not finished
                    CSTimer -= Time.deltaTime; // decrement

                    UpdateText(); // display current time
                    transform.position = Stand; // stuck to initial position
                    transform.rotation = look; // stuck to initial rotation

                    gameObject.GetComponent<CharacterController>().enabled = false; // disable movement of the bee

                } else if( PrepPhaseTimer >
                           0 ) { // CS start timer is finished but there is a PrepPhase to go through before really starting
                    PrepPhaseTimer -= Time.deltaTime;

                    UpdateText();
                    ToggleFullScreenStim( true ); // Turn On
                } else if( Stim_On == false ) {
                    // CS start timer finished
                    ToggleFullScreenStim(); // Turn Off
                    Stim( true );
                    gameObject.GetComponent<CharacterController>().enabled = true; // enables movement of the bee
                }


                if( ChoiceIsMade == true && USTimer > 0 && Test != 1 &&
                    PreTest !=
                    1 ) {
                    // if choice is made AND Cs start timer finished AND Cs Stop timer NOT finished AND US Timer NOT finished
                    USTimer -= Time.deltaTime; // update timer
                    UpdateText();

                    gameObject.GetComponent<CharacterController>().enabled = false; // disable movement of the bee
                    transform.position = Stay; // stuck position

                } else if( TrialTimer > 0 ) {  // CS stop timer is not finished
                    Dist += Mathf.Sqrt( Mathf.Pow( gameObject.transform.position.x - Tmp_X,
                                                   2 ) + Mathf.Pow( gameObject.transform.position.z - Tmp_Z, 2 ) );

                    Speed = Mathf.Sqrt( Mathf.Pow( gameObject.transform.position.x - Tmp_X,
                                                   2 ) + Mathf.Pow( gameObject.transform.position.z - Tmp_Z, 2 ) ) / Time.deltaTime;

                    TrialTimer -= Time.deltaTime; // decrement
                    UpdateText(); //display

                    gameObject.GetComponent<walking>().Distance.text = ( Dist * 100 ).ToString();

                    Tmp_X = gameObject.transform.position.x;
                    Tmp_Z = gameObject.transform.position.z;
                    Check_choice();
                }


                if( ChoiceIsMade == true && PreTest == 1 ) {
                    ChoiceTimer( Side, false );
                }

                if( TrialTimer <= 0 ) { // trial is finished
                    NextTrial(); // start next trial

                }

                if( USTimer <= 0f && ChoiceIsMade == true && Test != 1 &&
                    PreTest != 1 ) { // if choice made and countdown finished
                    NextTrial(); // start next trial
                }

                if( Line >= int.Parse( Xpmanager.INTestNb.text ) - 1 &&
                    a > Repetition ) { // if experiment finished
                    transform.position = Stand; // move position to initial
                    transform.rotation = look; // rotate to initial orientation

                    BeeDone = true;
                    Stop();// stops the experiment
                }

                UpdateText();
            }
        }

        private void ToggleFullScreenStim( bool On = false ) {
            if( !is_full_stim_on && On ) {
                BeeScreen.GetComponent<Renderer>().material.mainTexture =
                    arenaManager.Stimulations[absolute_stim[nbr]];
                is_full_stim_on = true;
            } else if( !On ) {
                BeeScreen.GetComponent<Renderer>().material.mainTexture = screenText;
                is_full_stim_on = false;
            }
        }

        private void UpdateText() {
            PREPTIME.text = ( Mathf.Round( PrepPhaseTimer ) ).ToString();
            CSSTART.text = ( Mathf.Round( CSTimer ) ).ToString();
            CSSTOP.text = ( Mathf.Round( TrialTimer ) ).ToString();
            US.text = ( Mathf.Round( USTimer ) ).ToString();

            where.text = ( Line.ToString() + " : " + a.ToString() );
            what.text =
                Xpmanager.Experiment_data.selGridTest[Line].ToString();
        }

        public void Stim( bool show ) {

            foreach( string flag in stim_flag ) {
                Renderer rend_3D = GameObject.FindGameObjectWithTag( flag ).GetComponentInChildren<Renderer>();
                SpriteRenderer rend_2D = GameObject.FindGameObjectWithTag( flag ).GetComponent<SpriteRenderer>();

                if( rend_3D != null ) {
                    rend_3D.enabled = show;
                }
                if( rend_2D != null ) {
                    rend_2D.enabled = show;
                }
            }

            if( !Xpmanager.Experiment_data.is_2D ) {
                foreach( string flag in env_flag ) {
                    Renderer rend_env = GameObject.FindGameObjectWithTag( flag ).GetComponentInChildren<Renderer>();
                    if( rend_env != null ) {
                        rend_env.enabled = show;
                    }
                }
            } else {
                Renderer rend_back =
                    GameObject.FindGameObjectWithTag( "BackPlane" ).GetComponentInChildren<Renderer>();
                if( rend_back != null ) {
                    rend_back.enabled = show;
                }
            }

            Stim_On = show;
        }

        private void Set_values( bool startup = false ) {
            PrepPhaseTimer = float.Parse(
                                 Xpmanager.Experiment_data.PrepPhaseDuration[Line] ); // gets the trial duration
            TrialTimer = float.Parse(
                             Xpmanager.Experiment_data.CSStop[Line] ); // gets the trial duration
            CSTimer = float.Parse(
                          Xpmanager.Experiment_data.CSStart[Line] ); // gets the delay before CS
            USTimer = float.Parse(
                          Xpmanager.Experiment_data.USDuration[Line] ); // gets the amount of time to give the reward

            Stim_names = new string[2];
            Stim_names[0] = Xpmanager.Experiment_data.Stims_one[Line] ;
            Stim_names[1] = Xpmanager.Experiment_data.Stims_two[Line];

            if( startup ) {
                Repetition = int.Parse(
                                 Xpmanager.Experiment_data.Repetition[Line] );
                Test = Xpmanager.Experiment_data.selGridTest[Line];
                PreTest = Xpmanager.Experiment_data.selGridPreTest[Line];
            }
        }

        private void Set_stims() {
            //updates sides of the stimuli\\
            if( Stim_names[0] == string.Empty || Stim_names[1] == string.Empty ) {
                arenaManager.ApplyTexture(
                    Xpmanager.Experiment_data.Sequences[Line][a - 1].Split( '/' )[0],
                    0 );
                arenaManager.ApplyTexture(
                    Xpmanager.Experiment_data.Sequences[Line][a - 1].Split( '/' )[1],
                    1 );
            } else {
                arenaManager.ApplyTexture(
                    Xpmanager.Experiment_data.Sequences[Line][a - 1].Split( '/' )[0],
                    Stim_names[0] );
                arenaManager.ApplyTexture(
                    Xpmanager.Experiment_data.Sequences[Line][a - 1].Split( '/' )[1],
                    Stim_names[1] );
            }
        }

        private void NextTrial() {
            Speaker.PlayOneShot( Reload, 0.5f ); // play Reload sound once

            ToggleFullScreenStim( false ); // make sure the full screen stim is off now

            tempsDist = Dist;
            TrialSummaryDisp = true;
            Dist = 0;
            Speed = 0;

            bell_notif = false;
            Reset_Choice();

            Recorder.reset_chrono();

            // makes CS invisible\\
            Stim( false );

            transform.position = Stand; // move position to initial
            transform.rotation = look; // rotate to initial orientation

            Tmp_X = gameObject.transform.position.x;
            Tmp_Z = gameObject.transform.position.z;

            if( PreTest == 1 ) {
                ChoiceTimer( "None", true );
            }
            if( Line <= int.Parse( Xpmanager.INTestNb.text ) - 1 &&  a < Repetition ) {
                a += 1;
                Set_values();

                Set_stims();
            } else if( Line < int.Parse( Xpmanager.INTestNb.text ) - 1 &&
                       a >= Repetition ) { // if Line < number of lines and current line is finished /!\ Line can get out of range !! /!\

                Line += 1; // move to next line

                Set_values( true );

                a = 1;

                Set_stims();

            } else if( Line == int.Parse( Xpmanager.INTestNb.text ) - 1 &&
                       a == Repetition ) { // we do the last repetition, go overboard and stop
                a += 1;
            }

            TimerLeft = 0;
            TimerRight = 0;

        }


        public void Stop() {
            if( Power_on ) {// No need to do anythhing if the XP is not running
                Recorder.Stop_Record();
                Dist = 0;
                Line = 0; //
                a = 1;
                Go = false;
                Butt_Power.interactable = true;
                //NB : Go and Butt_Power.interactable might contain the same information
                Power_on = false;
                IN_latency.text = latency.ToString();
                Waiting = false;

                gameObject.GetComponent<CharacterController>().enabled = false; // disable movement of the bee
                transform.position = Stand; // move position to initial
                transform.rotation = look; // rotate to initial orientation

                Stim( false );
                Reset_Choice();
                Ping( "0" );
            }
        }

        public void Pause() {
            if( Power_on ) { // No need to do anything if Start is not engaged
                if( Tmp_latency > 0 ) {
                    Waiting = !Waiting;
                } else {
                    Go = !Go;
                }
            }
        }

        public void Power() {

            if( Xpmanager.Experiment_data.is_2D ) {
                gameObject.GetComponent<walking>().position_teleporter();
            }
            Reset_Choice();
            Line = 0;
            a = 1;
            Dist = 0;

            if( nbr <= 10 ) {
                nbr += 1; // hacky way to alternate color at start when absolute = true
            } else {
                nbr = 0;
            }


            Butt_Power.interactable =
                false;// Power button is engaged, can't be re-click while experiment is running
            look = transform.rotation; // gets initial rotation
            Stand = transform.position; // gets initial position

            latency = float.Parse( IN_latency.text );//latency before real start of XP
            Tmp_latency = latency;// Latency timer to be decremented each cycle
            Waiting = true;

            Power_on = true;

            Set_values( true );

            TimerLeft = 0;
            TimerRight = 0;

            UpdateText();


            Tmp_X = gameObject.transform.position.x;
            Tmp_Z = gameObject.transform.position.z;

            Set_stims();
            Ping( "1" );
        }

        private void ChoiceTimer( string side, bool done ) {


            if( side == "Left" ) {
                TimerLeft += Time.deltaTime;
            } else if( side == "Right" ) {
                TimerRight += Time.deltaTime;
            }

            if( done ) {
                if( TimerLeft > TimerRight ) {
                    INPretestChoice.text = FindName( "Left" );
                } else if( TimerRight > TimerLeft ) {
                    INPretestChoice.text = FindName( "Right" );
                } else {
                    INPretestChoice.text = "None";
                }
            }

        }

        private void Check_choice() {
            bool is_ignored = false;
            Collider hit_coll = gameObject.GetComponent<walking>().hit.collider;
            if( hit_coll != null && hit_coll.name.Split( ' ' ).Length > 1 ) {
                Side_looked_at = gameObject.GetComponent<walking>().hit.collider.name.Split( ' ' )[1];
                Centered_object = FindName( Side_looked_at );
                is_ignored = Xpmanager.Experiment_data.Textures_to_ignore.Contains( Centered_object );
            } else {// never happens
                Side_looked_at = "Wall";
                Centered_object = "Wall";
            }

            if( gameObject.GetComponent<walking>().looking.collider != null &&
                gameObject.GetComponent<walking>().looking.collider.name.Split( ' ' ).Length > 1 ) {
                Object_looked_at = FindName(
                                       gameObject.GetComponent<walking>().looking.collider.name.Split( ' ' )[1] );
            }

            if( gameObject.GetComponent<walking>().edge_ray.collider != null &&
                gameObject.GetComponent<walking>().edge_ray.collider.tag == "Edge" ) {
                Edge_looked_at = gameObject.GetComponent<walking>().hit.collider.gameObject;
            } else {
                Edge_looked_at = null;
            }

            if( ( Side != null && Side != "None" ) ||
                Xpmanager.Experiment_data.is_2D ) {
                if( Xpmanager.Experiment_data.is_2D ) {
                    if( Side_looked_at != "Wall" ) {
                        Make_Choice( Side_looked_at, is_ignored );
                    }
                } else if( Side_looked_at == Side ) {
                    Make_Choice( Side, is_ignored );
                } else {
                    Choice = "None";
                    Side_Chosen = "None";
                    ChoiceIsMade = false;
                }
            }
        }

        private void Make_Choice( string side_chosen, bool is_ignored = false ) {
            Stay = transform.position; // get position of the bee on entry of the Area
            ChoiceIsMade = !is_ignored; // choice is made
            if( Test == 0 && PreTest == 0 && !is_ignored ) {
                transform.LookAt( gameObject.GetComponent<walking>().hit.collider.transform.position );
            }

            if( PreTest == 0 && !bell_notif && !is_ignored ) {
                Speaker.PlayOneShot( bell, 0.5f ); // play DoorBell sound once
                bell_notif = true;
            }
            Choice = FindName( side_chosen );
            Side_Chosen = side_chosen;

            // If the timestep is not 0 we might miss the choice
            if( float.Parse( Recorder.INTimeStep.text ) != 0.0f ) {
                Recorder.Record_data_point();
            }
        }

        private void Reset_Choice() {
            Side = "None";
            Choice = "None"; // reset the choice to none
            Side_Chosen = "None";
            Side_looked_at = "Wall";
            Centered_object = "Wall";
            Object_looked_at = "None";
            Edge_looked_at = null;
            ChoiceIsMade = false;
        }

        private void OnTriggerEnter( Collider other ) { // when enter stimulus area
            Side = other.name.Split( ' ' )[1]; // get the side of the collision Left or Right
        }
        private void OnTriggerExit( Collider other ) { // when leave stimulus area
            Reset_Choice();
        }

        private string FindName( string side ) {
            side = side.Split( '(' )[0]; // Hacky way to get rid of the "(Clone)" part in the name
            if( GameObject.FindGameObjectWithTag( side ).GetComponentInChildren<Renderer>() !=
                null ) { // if it exist
                return GameObject.FindGameObjectWithTag(
                           side ).GetComponentInChildren<Renderer>().material.mainTexture.name; // get the name of the stimulus, which is also the name of the texture
            }

            if( GameObject.FindGameObjectWithTag( side ).GetComponent<SpriteRenderer>() !=
                null ) { // /!\ Might need a more straightforward implemantation /!\
                return GameObject.FindGameObjectWithTag(
                           side ).GetComponent<SpriteRenderer>().sprite.name; // get the name of the stimulus, which is the name of the sprite
            }

            return string.Empty;
        }

        private void OnGUI() {
            if( Event.current.Equals(
                    Event.KeyboardEvent( "Space" ) ) ) {
                if( !Power_on ) {
                    Power();
                } else {
                    Pause();
                }
            } else if( Event.current.Equals(
                           Event.KeyboardEvent( "^Space" ) ) ) {
                if( Power_on ) {
                    Stop();
                }
            } else if( Event.current.Equals(
                           Event.KeyboardEvent( "#Space" ) ) ) {
                NextBee();
                if( BeeDone ) {
                    BeeDone = false;
                }
            }



            if( BeeDone == true ) {
                if( GUI.Button( new Rect( Screen.width / 2, Screen.height / 2, 90, 60 ), "Next Bee" ) ) {
                    NextBee();

                    BeeDone = false;
                }

            }
            if( TrialSummaryDisp == true ) {
                if( GUILayout.Button( "Dist. Walked: " + tempsDist.ToString() + "cm" ) ) {
                    tempsDist = 0;
                    TrialSummaryDisp = false;
                }
            }
        }

        public void NextBee() {
            // change the BeeID and reset Line and Repetition ("a") to 0
            BeeID = int.Parse( Recorder.BeeID.text );
            BeeID += 1;
            Recorder.BeeID.text = BeeID.ToString();
            Line = 0;
            a = 1;
        }

        public string Get_edge_data( ) {
            string data = "none;NA";
            if( Edge_looked_at == null ) {
                return data;
            }
            string Object;
            if( Edge_looked_at.transform.position.x > 0 ) {
                Object = FindName( "Right" );
            } else {
                Object = FindName( "Left" );
            }
            data = Object + "_" + Edge_looked_at.name + ";" +
                   Edge_looked_at.transform.localPosition;

            return data;
        }

        public void Set_serial_port() {
            if( IN_port_name.text != string.Empty ) {
                serial_port = new SerialPort( IN_port_name.text, 9600 );
            }
            Update_port_status();
        }
        public void Connect_to_serial_port() {
            if( serial_port != null ) {
                if( serial_port.IsOpen ) {
                    serial_port.Close();
                }
                serial_port.Open();
            }
            Update_port_status();
        }

        private void Update_port_status() {
            if( serial_port != null ) {
                IN_conection_status.text = serial_port.PortName;
                if( serial_port.IsOpen ) {
                    IN_conection_status.text = serial_port.PortName + "_Open" ;
                } else {
                    IN_conection_status.text = serial_port.PortName + "_Close";
                }
            } else {
                IN_conection_status.text = "No port";
            }
        }

        public void Ping_button() {
            if( pinged ) {
                Ping( "0" );
            } else {
                Ping( "1" );
            }
            pinged = !pinged;
        }

        private void Ping( string to_ping = "1" ) {
            if( serial_port != null ) {
                serial_port.Open();
                if( serial_port.IsOpen ) {
                    serial_port.WriteLine( to_ping );
                    serial_port.BaseStream.Flush();
                    Debug.Log( to_ping );
                    serial_port.Close();
                }
            }
        }
        public void Close_port() {
            if( serial_port != null ) {
                if( serial_port.IsOpen ) {
                    serial_port.Close();
                }
            }
            Update_port_status();
        }
}
