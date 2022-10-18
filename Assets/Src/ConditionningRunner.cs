using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;


public class Timer
{
        private float total_time = 0.0f;
        private float curr_time = 0.0f;

        private InputField display;

        private bool is_on = false;

        private void Update_display() {
            display.text = Mathf.Round( curr_time ).ToString();
        }

        public Timer( InputField disp, float time = 0.0f ) {
            total_time = time;
            display = disp;
            display.text = total_time.ToString();
        }

        public void Set_total_time( float time ) {
            total_time = time;
            Reset_timer();
        }

        public void Start() {
            is_on = true;
            Reset_timer();
        }

        public void Stop() {
            is_on = false;
            Reset_timer();
        }

        private void Mod_curr_time( float time ) {
            curr_time += time;
            Update_display();
        }

        public void Reset_timer() {
            curr_time = total_time;
            Update_display();
        }

        public bool Run_timer( float delta_time, System.Action action_on, System.Action action_off ) {
            if( !is_on ) {
                return false;
            }
            Mod_curr_time( -delta_time );
            action_on();
            if( curr_time <= 0.0f ) {
                action_off();
                Stop();
                return true;
            }
            return false;
        }
}

public class ConditionningRunner : MonoBehaviour
{

        static Vector3 LEFT = new Vector3( -0.1f, 0.025f, 0 );
        static Vector3 RIGHT = new Vector3( 0.1f, 0.025f, 0 );
        static Vector3 CENTER = new Vector3( 0, 0.025f, 0 );


        public bool ChoiceIsMade = false; // If the Bee made a choice eg. went to one of the stimuli
        public string Choice; // choice made by the bee, gets wrote down in the txt file results
        public string Side_Chosen;
        public string CS_Chosen;
        public string Side;
        private string Side_Centered;
        public string Centered_object;
        public string Object_looked_at;
        public string Side_looked_at;
        public GameObject Edge_looked_at;
        private bool bell_notif = false; // did we played the bell yet ?

        public bool Go = false; // wether or not to begin the experiment
        private bool Power_on = false;// if Space was pressed

        public ArenaManager arenaManager;

        public string PrepPhase_Stim; // What stimulus was shown during the prep phase

        private Timer
        Prep_phase_timer;// Duration of the pre trial phase, between the inter trial interval and the actual trial
        private Timer Extra_timer;
        private Timer CS_timer;// duration before the CS appearance
        private Timer Trial_timer;// duration of the trial
        private Timer US_Timer;// Amount of time to give the US

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
        public InputField EXTRA_PRETIME;
        public InputField CSSTART; // Inputfield to display delay before CS
        public InputField CSSTOP; // inputfield to display duration of trial
        public InputField US; // inputfield to display duration of US
        public InputField where; // display number of current line and current repetition
        public InputField what; // display if test or not
        public InputField CSp; // name of the rewarded stimulus


        private AudioSource Speaker; // Audio source
        public AudioClip bell; // doorbell sound from: https://freesound.org/people/bennstir/sounds/81072/
        public AudioClip Reload; // reload sound from: https://freesound.org/people/nioczkus/sounds/396331/


        public bool BeeDone = false;
        public Button Butt_Power;

        private bool TrialSummaryDisp = false;

        private float TimerLeft2D = 0.0f;
        private float TimerRight2D = 0.0f;


        private float TimerLeft = 0;
        private float TimerRight = 0;
        public InputField INPretestChoice;

        public GameObject BeeScreen; //Screen diplaying stuff to the bee
        public RenderTexture screenText;

        private bool is_full_stim_on = false; // Is the full Screen Stim toggled

        //TODO: Remove those flags
        private string[] stim_flag = { "-0,1000_0,0250_0,0000", "0,1000_0,0250_0,0000" };
        private string[] all_stim_flag = { "-0,1000_0,0250_0,0000", "0,1000_0,0250_0,0000", "0,0000_0,0250_0,0000" };
        private string[] env_flag = { "Wall", "Floor" };

        private SerialPort serial_port;
        public InputField IN_port_name;
        public InputField IN_conection_status;

        private bool pinged = false;

        private ExperimentRecorder Recorder;
        private ExperimentManager Xpmanager;

        // Use this for initialization
        void Start() {

            Reset_Choice();
            latency = 10;
            Tmp_latency = latency;
            IN_latency.text = latency.ToString();

            IN_port_name.text = "COM3";
            IN_conection_status.text = "None";

            Speaker = GetComponent<AudioSource>(); // initialize audiosource

            Recorder = gameObject.GetComponent<ExperimentRecorder>();
            Xpmanager = gameObject.GetComponent<ExperimentManager>();

            Prep_phase_timer = new Timer( PREPTIME );
            Extra_timer = new Timer( EXTRA_PRETIME );
            CS_timer = new Timer( CSSTART );
            US_Timer = new Timer( US );
            Trial_timer = new Timer( CSSTOP );
        }

        public bool bee_can_move() {
            return gameObject.GetComponent<CharacterController>().enabled;
        }

        private void Prep_phase_action_on() {
            if( Xpmanager.Experiment_data.selGridConcept[Line] > 0 ) {
                Spawn_Stim( true, "Center" );
                gameObject.GetComponent<CharacterController>().enabled = true; // enables movement of the bee
                if( Check_choice() ) {
                    Reset_Choice();
                    Prep_phase_action_off();
                    Prep_phase_timer.Stop();
                }
            } else {
                Stick_the_bee();// stick bee to initial position
                ToggleFullScreenStim( true ); // Turn On
            }

        }
        private void Prep_phase_action_off() {
            Stick_the_bee(); // Reset position

            ToggleFullScreenStim(); // Turn Off
            Spawn_Stim( false, "Center" );

            Extra_timer.Start();
        }

        private void CS_action_on() {
            Stick_the_bee();// stick bee to initial position
        }
        private void CS_action_off() {
            // CS start timer finished
            Prep_phase_timer.Start();
        }

        private void Extra_time_action_off() {
            Trial_timer.Start();
            Spawn_Stim( true, stim_flag );
            gameObject.GetComponent<CharacterController>().enabled = true; // enables movement of the bee
        }

        private void US_action_on() {
            gameObject.GetComponent<CharacterController>().enabled = false; // disable movement of the bee
            transform.position = Stay; // stuck position
        }

        private void Trial_action_on() {
            Dist += Mathf.Sqrt( Mathf.Pow( gameObject.transform.position.x - Tmp_X,
                                           2 ) + Mathf.Pow( gameObject.transform.position.z - Tmp_Z, 2 ) );

            Speed = Mathf.Sqrt( Mathf.Pow( gameObject.transform.position.x - Tmp_X,
                                           2 ) + Mathf.Pow( gameObject.transform.position.z - Tmp_Z, 2 ) ) / Time.deltaTime;

            gameObject.GetComponent<walking>().Distance.text = ( Dist * 100 ).ToString();

            Tmp_X = gameObject.transform.position.x;
            Tmp_Z = gameObject.transform.position.z;

            if( Check_choice() ) {
                bool is_ignored = Xpmanager.Experiment_data.Textures_to_ignore.Contains( Centered_object );
                if( Xpmanager.Experiment_data.is_2D ) {
                    Make_Choice( Side_looked_at, is_ignored );
                } else {
                    Make_Choice( Side, is_ignored );
                }
            }


            if( Xpmanager.Experiment_data.is_2D ) {
                Looking_Timer2D();
            }
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
                    CS_timer.Start();

                    if( Recorder.Path.text != string.Empty ) {
                        Recorder.Start_Record();
                    }
                }
            }

            if( Go == true ) { // runs the experiment
                CS_timer.Run_timer( Time.deltaTime, CS_action_on, CS_action_off );
                Prep_phase_timer.Run_timer( Time.deltaTime, Prep_phase_action_on, Prep_phase_action_off );
                Extra_timer.Run_timer( Time.deltaTime, CS_action_on, Extra_time_action_off );

                US_Timer.Run_timer( Time.deltaTime, US_action_on, NextTrial );

                Trial_timer.Run_timer( Time.deltaTime, Trial_action_on, NextTrial );

                if( ChoiceIsMade == true && PreTest == 1 ) {
                    ChoiceTimer( Side, false );
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

        private void Stick_the_bee() {
            transform.position = Stand; // stuck to initial position
            transform.rotation = look; // stuck to initial rotation

            gameObject.GetComponent<CharacterController>().enabled = false; // disable movement of the bee
        }

        private void ToggleFullScreenStim( bool On = false ) {
            if( !is_full_stim_on && On ) {
                arenaManager.ApplyTexture( BeeScreen.GetComponent<Renderer>(), PrepPhase_Stim );
                is_full_stim_on = true;

            } else if( !On ) {
                BeeScreen.GetComponent<Renderer>().material.mainTexture = screenText;
                is_full_stim_on = false;
            }
        }

        private void UpdateText() {
            where.text = ( Line.ToString() + " : " + a.ToString() );
            what.text =
                Xpmanager.Experiment_data.selGridTest[Line].ToString();
        }

        private void Spawn_Stim( bool spawn, string flg ) {
            GameObject flg_stim = arenaManager.Get_stim_object( flg );
            if( spawn ) {
                if( flg_stim ) {
                    flg_stim.GetComponentInChildren<Renderer>().enabled = true;
                } else {

                    switch( flg ) {
                        case "0,0000_0,0250_0,0000":
                            arenaManager.Clear_Shape();
                            arenaManager.Spawn_shape( CENTER );
                            break;
                        case "-0,1000_0,0250_0,0000":
                        case "0,1000_0,0250_0,0000":
                            arenaManager.Clear_Shape();
                            arenaManager.Spawn_shape( LEFT );
                            arenaManager.Spawn_shape( RIGHT );
                            break;
                        default:
                            break;
                    }

                }
                Set_stims();
                arenaManager.GetComponentInParent<Stim_Manager>().On_scale_change();
                Stim( true );
            } else if( flg_stim ) {
                Destroy( flg_stim );
            }
        }

        public void Spawn_Stim( bool spawn, string[] flag ) {
            foreach( string flg in flag ) {
                Spawn_Stim( spawn, flg );
            }
        }

        public void Stim( bool show ) {

            foreach( string flag in stim_flag ) {
                GameObject obj = arenaManager.Get_stim_object( flag );
                if( obj ) {
                    Renderer rend_3D = obj.GetComponentInChildren<Renderer>();
                    SpriteRenderer rend_2D = obj.GetComponent<SpriteRenderer>();

                    if( rend_3D != null ) {
                        rend_3D.enabled = show;
                    }
                    if( rend_2D != null ) {
                        rend_2D.enabled = show;
                    }
                }
            }

            if( !Xpmanager.Experiment_data.is_2D ) {
                foreach( string flag in env_flag ) {// Enable wall and floor renderer
                    GameObject obj = arenaManager.Get_stim_object( flag );
                    if( obj ) {
                        Renderer rend_env = obj.GetComponentInChildren<Renderer>();
                        if( rend_env != null ) {
                            //TODO: differentiate between wall and floor
                            if( Xpmanager.Experiment_data.Wall_on != null ) {
                                // if show = false then we want the thing to be off
                                // if show = true then we want to follow Wall_on instruction
                                rend_env.enabled = Xpmanager.Experiment_data.Wall_on[Line] & show;
                            } else {
                                rend_env.enabled = show;
                            }
                        }
                    }
                }
            } else {
                Renderer rend_back =
                    GameObject.FindGameObjectWithTag( "BackPlane" ).GetComponentInChildren<Renderer>();
                if( rend_back != null ) {
                    if( Xpmanager.Experiment_data.Wall_on != null ) {
                        // if show = false then we want the thing to be off
                        // if show = true then we want to follow Wall_on instruction
                        rend_back.enabled = Xpmanager.Experiment_data.Wall_on[Line] & show;
                    } else {
                        rend_back.enabled = show;
                    }
                }
            }

            Stim_On = show;
        }

        private void Set_values( bool startup = false ) {
            Prep_phase_timer.Set_total_time( float.Parse(
                                                 Xpmanager.Experiment_data.PrepPhaseDuration[Line] ) );
            Extra_timer.Set_total_time( float.Parse(
                                            Xpmanager.Experiment_data.ExtraTimeDuration[Line] ) );
            Trial_timer.Set_total_time( float.Parse(
                                            Xpmanager.Experiment_data.CSStop[Line] ) ); // gets the trial duration
            CS_timer.Set_total_time( float.Parse(
                                         Xpmanager.Experiment_data.CSStart[Line] ) );
            US_Timer.Set_total_time( float.Parse(
                                         Xpmanager.Experiment_data.USDuration[Line] ) ); // gets the amount of time to give the reward

            PrepPhase_Stim = Xpmanager.Experiment_data.SequencesPreStim[Line][a - 1];

            if( Xpmanager.Experiment_data.selGridConcept[Line] > 0 ) {
                Set_CSp( Line );
            }

            if( startup ) {
                Repetition = int.Parse(
                                 Xpmanager.Experiment_data.Repetition[Line] );
                Test = Xpmanager.Experiment_data.selGridTest[Line];
                PreTest = Xpmanager.Experiment_data.selGridPreTest[Line];
            }

        }

        private void Set_stims() {
            //updates sides of the stimuli\\
            string stim_one = Xpmanager.Experiment_data.Sequences[Line][a - 1];
            arenaManager.ApplyTexture( "0,0000_0,0250_0,0000", PrepPhase_Stim );
            arenaManager.ApplyTexture( "0,1000_0,0250_0,0000", stim_one );
            arenaManager.ApplyTexture( "-0,1000_0,0250_0,0000",
                                       Xpmanager.Experiment_data.pick_opposite_stim( stim_one, Line ) );
        }

        private void Set_CSp( int index ) {
            int concept = Xpmanager.Experiment_data.selGridConcept[index];
            switch( concept ) {
                case 1:
                    CSp.text = Xpmanager.Experiment_data.SequencesPreStim[Line][a - 1];
                    return;
                case 2:
                    CSp.text = Xpmanager.Experiment_data.pick_opposite_stim(
                                   Xpmanager.Experiment_data.SequencesPreStim[Line][a - 1], index );
                    return;
                default:
                    break;
            }


            if( CSp.text == string.Empty ) {
                int coin_flip = ( int )Mathf.Round( UnityEngine.Random.value ); //flip a coin
                if( coin_flip < 1 ) {
                    CSp.text = Xpmanager.Experiment_data.Stims_one[index];
                } else {
                    CSp.text = Xpmanager.Experiment_data.Stims_two[index];
                }
            } else {
                CSp.text = Xpmanager.Experiment_data.pick_opposite_stim( CSp.text, index );
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

            // Despawn CS
            Spawn_Stim( false, stim_flag );

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

            } else if( Line < int.Parse( Xpmanager.INTestNb.text ) - 1 &&
                       a >= Repetition ) { // if Line < number of lines and current line is finished /!\ Line can get out of range !! /!\

                Line += 1; // move to next line

                a = 1;

                Set_values( true );

            } else if( Line == int.Parse( Xpmanager.INTestNb.text ) - 1 &&
                       a == Repetition ) { // we do the last repetition, go overboard and stop
                a += 1;
            }

            TimerLeft = 0;
            TimerRight = 0;

            CS_timer.Start();
        }

        private void Stop_all_timers() {
            Prep_phase_timer.Stop();
            Extra_timer.Stop();
            CS_timer.Stop();
            Trial_timer.Stop();
            US_Timer.Stop();
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

                ToggleFullScreenStim( false );
                Spawn_Stim( false, all_stim_flag );
                Reset_Choice();
                Stop_all_timers();
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

            Set_CSp( a - 1 );

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

            Ping( "1" );
        }

        private void Looking_Timer2D() {
            if( Side_looked_at == "Left" ) {
                TimerLeft2D += Time.deltaTime;
            } else if( Side_looked_at == "Right" ) {
                TimerRight2D += Time.deltaTime;
            } else { // We only want continuous time, if something elese is centered the time is reset
                TimerRight2D = 0.0f;
                TimerLeft2D = 0.0f;
            }
        }

        private float get_Looking_Time2D( string side ) {
            if( side == "Left" ) {
                return TimerLeft2D;
            } else if( side == "Right" ) {
                return TimerRight2D;
            }
            return 0.0f;
        }

        private void ChoiceTimer( string side, bool done ) {

            if( side == "-0,1000_0,0250_0,0000") {
                TimerLeft += Time.deltaTime;
            } else if( side == "0,1000_0,0250_0,0000") {
                TimerRight += Time.deltaTime;
            }

            if( done ) {
                if( TimerLeft > TimerRight ) {
                    INPretestChoice.text = FindName("-0,1000_0,0250_0,0000");
                } else if( TimerRight > TimerLeft ) {
                    INPretestChoice.text = FindName("0,1000_0,0250_0,0000");
                } else {
                    INPretestChoice.text = "None";
                }
            }

        }

        private bool Check_choice() {
            Collider hit_coll = gameObject.GetComponent<walking>().hit.collider;
            if( hit_coll != null && hit_coll.name.Split( ' ' ).Length > 1 ) {
                Side_Centered = gameObject.GetComponent<walking>().hit.collider.name.Split( ' ' )[1];
                Centered_object = FindName( gameObject.GetComponent<walking>().hit.collider.gameObject );
            } else {
                Side_Centered = "Wall";
                Centered_object = "Wall";
            }

            if( gameObject.GetComponent<walking>().looking.collider != null &&
                gameObject.GetComponent<walking>().looking.collider.name.Split( ' ' ).Length > 1 ) {
                Side_looked_at = gameObject.GetComponent<walking>().looking.collider.name.Split( ' ' )[1];
                Object_looked_at = FindName( gameObject.GetComponent<walking>().looking.collider.gameObject );
            } else {
                Side_looked_at = "None";
                Object_looked_at = "None";
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
                    if( get_Looking_Time2D( Side_looked_at ) > 1.0 ) {
                        return true;
                    }
                } else if( Side_Centered == Side ) {
                    return true;
                } else {
                    Choice = "None";
                    Side_Chosen = "None";
                    CS_Chosen = "None";
                    ChoiceIsMade = false;
                }
            }
            return false;
        }

        private string Get_chosen_cs() {
            return Choice == CSp.text ? "+" : "-";
        }

        private void Make_Choice( string side_chosen, bool is_ignored = false ) {
            Stay = transform.position; // get position of the bee on entry of the Area
            ChoiceIsMade = !is_ignored; // choice is made

            Choice = FindName( side_chosen );
            Side_Chosen = side_chosen;
            CS_Chosen = Get_chosen_cs();

            if( Test == 0 && PreTest == 0 && !is_ignored ) {
            GameObject object_looked_at = arenaManager.Get_stim_object(side_chosen);
                transform.LookAt(object_looked_at.transform.position );
                US_Timer.Start();
                Trial_timer.Stop();
                Ping( Get_chosen_cs() );
            }

            if( PreTest == 0 && !bell_notif && !is_ignored ) {
                Speaker.PlayOneShot( bell, 0.5f ); // play DoorBell sound once
                bell_notif = true;
            }

            // If the timestep is not 0 we might miss the choice
            if( float.Parse( Recorder.INTimeStep.text,
                             System.Globalization.CultureInfo.InvariantCulture.NumberFormat ) != 0.0f ) {
                Recorder.Record_data_point();
            }
        }

        private void Reset_Choice() {
            Side = "None";
            Choice = "None"; // reset the choice to none
            Side_Chosen = "None";
            CS_Chosen = "None";
            Side_Centered = "Wall";
            Centered_object = "Wall";
            Side_looked_at = "None";
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
            GameObject object_to_name = arenaManager.Get_stim_object( side );
            return FindName( object_to_name );
        }


        private string FindName( GameObject object_to_name ) {
            if( object_to_name.GetComponentInChildren<Renderer>() ) {
                return object_to_name.GetComponentInChildren<Renderer>().material.mainTexture.name; // get the name of the stimulus, which is also the name of the texture
            }

            if( object_to_name.GetComponent<SpriteRenderer>() ) {
                return object_to_name.GetComponent<SpriteRenderer>().sprite.name; // get the name of the stimulus, which is the name of the sprite
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
            Set_CSp( a - 1 );
        }

        public string Get_edge_data( ) {
            string data = "none;NA";
            if( Edge_looked_at == null ) {
                return data;
            }
            string Object;
            if( Edge_looked_at.transform.position.x > 0 ) {
                Object = FindName("0,1000_0,0250_0,0000");
            } else {
                Object = FindName("-0,1000_0,0250_0,0000");
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
                if( serial_port.IsOpen ) {
                    serial_port.WriteLine( to_ping );
                    serial_port.BaseStream.Flush();
                    Debug.Log( to_ping );
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
