using System;
using System.IO.Ports;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inspected_object : IEquatable<Inspected_object>
{
        //TODO: Make private
        public string Name;
        public float Looking_time;
        public string Pos_ID;

        public Inspected_object( string id, string name, float time ) {
            Name = name;
            Looking_time = time;
            Pos_ID = id;
        }

        public bool Equals( Inspected_object other ) {
            if( other == null ) {
                return false;
            }
            return other.Name == Name;
        }

        public override bool Equals( System.Object obj ) {
            if( obj == null ) {
                return false;
            }

            Inspected_object Obj_insp = obj as Inspected_object;
            if( Obj_insp == null ) {
                return false;
            } else {
                return Equals( Obj_insp );
            }
        }

        public override int GetHashCode() {
            return this.Name.GetHashCode();
        }

        public static bool operator ==( Inspected_object Obj1, Inspected_object Obj2 ) {
            if( ( ( object )Obj1 ) == null || ( ( object )Obj2 ) == null ) {
                return System.Object.Equals( Obj1, Obj2 );
            }

            return Obj1.Equals( Obj2 );
        }

        public static bool operator !=( Inspected_object Obj1, Inspected_object Obj2 ) {
            if( ( ( object )Obj1 ) == null || ( ( object )Obj2 ) == null ) {
                return !System.Object.Equals( Obj1, Obj2 );
            }

            return !( Obj1.Equals( Obj2 ) );
        }

        public static bool operator >( Inspected_object Obj1, Inspected_object Obj2 ) {
            if( ( ( object )Obj1 ) == null || ( ( object )Obj2 ) == null ) {
                return false;
            }

            return Obj1.Looking_time > Obj2.Looking_time;
        }

        public static bool operator <( Inspected_object Obj1, Inspected_object Obj2 ) {
            if( ( ( object )Obj1 ) == null || ( ( object )Obj2 ) == null ) {
                return false;
            }

            return Obj1.Looking_time < Obj2.Looking_time;
        }

        public static bool operator >=( Inspected_object Obj1, Inspected_object Obj2 ) {
            if( ( ( object )Obj1 ) == null || ( ( object )Obj2 ) == null ) {
                return false;
            }

            return Obj1.Looking_time >= Obj2.Looking_time;
        }

        public static bool operator <=( Inspected_object Obj1, Inspected_object Obj2 ) {
            if( ( ( object )Obj1 ) == null || ( ( object )Obj2 ) == null ) {
                return false;
            }

            return Obj1.Looking_time <= Obj2.Looking_time;
        }

        public void timer_tick() {
            Looking_time += Time.deltaTime;
        }

        public void timer_reset() {
            Looking_time = 0.0f;
        }
}

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

        static Vector3[] LEFT_RIGHT = new Vector3[] { new Vector3( -0.1f, 0, 0.18f ), new Vector3( 0.1f, 0, 0.18f ) };
        static Vector3 CENTER = new Vector3( 0, 0, 0.18f );


        public bool ChoiceIsMade = false; // If the Bee made a choice eg. went to one of the stimuli
        public string Choice; // choice made by the bee, gets wrote down in the txt file results
        public string Side_Chosen;
        public string CS_Chosen;
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

        private Vector3 Prev_pos;

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

        public InputField INPretestChoice;

        private List<Inspected_object> Choices;

        public GameObject BeeScreen; //Screen diplaying stuff to the bee
        public RenderTexture screenText;

        private bool is_full_stim_on = false; // Is the full Screen Stim toggled

        private SerialPort serial_port;
        public InputField IN_port_name;
        public InputField IN_conection_status;

        private bool pinged = false;

        private ExperimentRecorder Recorder;
        private ExperimentManager Xpmanager;

        public CharacterController World_controller;

        // Use this for initialization
        void Start() {

            Reset_Choice();
            latency = 0;
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

            Choices = new List<Inspected_object> { };

            Prev_pos = gameObject.transform.position;
        }

        public bool bee_can_move() {
            return World_controller.enabled;
        }

        private void Prep_phase_action_on() {
            if( Xpmanager.Experiment_data.selGridConcept[Line] > 0 ) {
                Spawn_Stim( CENTER );
                World_controller.enabled = true; // enables movement of the bee
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
            arenaManager.Clear_Shape();

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
            if( arenaManager.ChooseArena.value == 2 ) {
                Forest_update();
            } else {
                Spawn_Stim( LEFT_RIGHT );
            }
            World_controller.enabled = true; // enables movement of the bee
        }

        private void US_action_on() {
            World_controller.enabled = false; // disable movement of the bee
            transform.position = Stay; // stuck position
        }

        private void Trial_action_on() {

            float pos_x = -World_controller.transform.localPosition.x;
            float pos_z = -World_controller.transform.localPosition.z;

            Dist += Mathf.Sqrt( Mathf.Pow( pos_x - Tmp_X, 2 ) + Mathf.Pow( pos_z - Tmp_Z, 2 ) );

            Speed = Mathf.Sqrt( Mathf.Pow( pos_x - Tmp_X, 2 ) + Mathf.Pow( pos_z - Tmp_Z,
                                2 ) ) / Time.deltaTime;

            gameObject.GetComponent<walking>().Distance.text = ( Dist * 100 ).ToString();

            Tmp_X = pos_x;
            Tmp_Z = pos_z;

            if( Check_choice() ) {
                bool is_ignored = Xpmanager.Experiment_data.Textures_to_ignore.Contains( Centered_object );
                if( Xpmanager.Experiment_data.is_2D ) {
                    Make_Choice( Side_looked_at, is_ignored );
                } else {
                    Make_Choice( Side_looked_at, is_ignored );
                }
            }


            if( Xpmanager.Experiment_data.is_2D ) {
                Looking_Timer2D();
            }
        }

        private void Forest_update() {
            arenaManager.Update_forest();
            Prev_pos = gameObject.transform.position;
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

                if( arenaManager.ChooseArena.value == 2 ) {
                    if( Vector3.Distance( Prev_pos, gameObject.transform.position ) >= 0.1 ) {
                        Forest_update();
                    }
                }

                if( ChoiceIsMade == true && PreTest == 1 ) {
                    ChoiceTimer( Side_looked_at, false );
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

            World_controller.enabled = false; // disable movement of the bee
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

        private void Spawn_Stim( Vector3[] positions ) {
            arenaManager.Clear_Shape();
            foreach( var pos in positions ) {
                arenaManager.Spawn_shape( pos );
            }
            Set_stims();
            arenaManager.GetComponentInParent<Stim_Manager>().On_scale_change();
            Stim( true );
        }

        private void Spawn_Stim( Vector3 pos ) {
            arenaManager.Clear_Shape();
            arenaManager.Spawn_shape( pos );
            Set_stims();
            arenaManager.GetComponentInParent<Stim_Manager>().On_scale_change();
            Stim( true );
        }

        public void Stim( bool show ) {
            foreach( GameObject obj in arenaManager.Get_all_stim_objects() ) {
                Renderer rend_3D = obj.GetComponentInChildren<Renderer>();
                SpriteRenderer rend_2D = obj.GetComponent<SpriteRenderer>();

                if( rend_3D != null ) {
                    rend_3D.enabled = show;
                }
                if( rend_2D != null ) {
                    rend_2D.enabled = show;
                }
            }

            if( !Xpmanager.Experiment_data.is_2D ) {
                foreach( GameObject obj in arenaManager.Get_Wall_and_Floor() ) {// Enable wall and floor renderer
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
            arenaManager.ApplyTexture( "0,0000_0,0000_0,1800", PrepPhase_Stim );
            arenaManager.ApplyTexture( "0,1000_0,0000_0,1800", stim_one );
            arenaManager.ApplyTexture( "-0,1000_0,0000_0,1800",
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
            arenaManager.Clear_Shape();

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

            Choices.Clear();

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

                World_controller.enabled = false; // disable movement of the bee
                transform.position = Stand; // move position to initial
                transform.rotation = look; // rotate to initial orientation

                ToggleFullScreenStim( false );
                arenaManager.Clear_Shape();
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

            Choices.Clear();

            UpdateText();


            Tmp_X = gameObject.transform.position.x;
            Tmp_Z = gameObject.transform.position.z;

            Ping( "1" );
        }

        private void Update_choices( string id ) {
            Inspected_object obj = new Inspected_object( id, FindName( id ), 0.0f );
            if( Choices.Contains( obj ) ) {
                var indx = Choices.FindIndex( a => a == obj );
                Choices[indx].timer_tick();
            } else {
                Choices.Add( obj );
            }
        }

        private void Looking_Timer2D() {
            if( Side_looked_at != "None" ) {
                Update_choices( Side_looked_at );
            } else {
                // We only want continuous time, if something else is centered the time is reset
                Choices.ForEach( delegate( Inspected_object obj ) {
                    obj.timer_reset();
                } );
            }
        }

        private float get_Looking_Time2D( string side ) {
            foreach( var item in Choices ) {
                if( item.Pos_ID == side ) {
                    return item.Looking_time;
                }
            }
            return 0.0f;
        }

        private void ChoiceTimer( string side, bool done ) {
            if( side != "None" ) {
                Update_choices( side );
            }
            if( done ) {
                Inspected_object chosen_obj = Choices.Max();
                if( chosen_obj != null ) {
                    INPretestChoice.text = chosen_obj.Name;
                } else {
                    INPretestChoice.text = "None";
                }
            }

        }

        private bool Check_choice() {
            Collider hit_coll = gameObject.GetComponent<walking>().hit.collider;
            if( hit_coll != null && hit_coll.name.Split( ' ' ).Length > 1 ) {
                Side_Centered = gameObject.GetComponent<walking>().hit.collider.name.Split( ' ' ).Last();
                Centered_object = FindName( gameObject.GetComponent<walking>().hit.collider.gameObject );

                if( Is_close_enough( hit_coll.transform.position ) ) {
                    return true;
                }

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
                Edge_looked_at = gameObject.GetComponent<walking>().edge_ray.collider.gameObject;
            } else {
                Edge_looked_at = null;
            }

            if( Xpmanager.Experiment_data.is_2D ) {
                if( get_Looking_Time2D( Side_looked_at ) > 1.0 ) {
                    return true;
                }
            } else {
                Choice = "None";
                Side_Chosen = "None";
                CS_Chosen = "None";
                ChoiceIsMade = false;
            }

            return false;
        }

        private bool Is_close_enough( Vector3 target ) {
            Vector3 controller_pos = World_controller.transform.position;
            Vector3 Bee_pos = new Vector3();
            return Vector3.Distance( Bee_pos, target ) <= 0.08f;
        }

        private string Get_chosen_cs() {
            return Choice == CSp.text ? "+" : "-";
        }

        private void Make_Choice( string side_chosen, bool is_ignored = false ) {
            Stay = transform.position; // get position of the bee on entry of the Area
            ChoiceIsMade = !is_ignored; // choice is made

            Choice = Centered_object;

            //Simplify the side recorded for those three special cases
            switch( side_chosen ) {
                case "-0,1000_0,0000_0,1800":
                    Side_Chosen = "Left";
                    break;
                case "0,1000_0,0000_0,1800":
                    Side_Chosen = "Right";
                    break;
                case "0,0000_0,0000_0,1800":
                    Side_Chosen = "Center";
                    break;
                default:
                    Side_Chosen = side_chosen;
                    break;
            }
            CS_Chosen = Get_chosen_cs();

            if( Test == 0 && PreTest == 0 && !is_ignored ) {
                GameObject object_looked_at = arenaManager.Get_stim_object( side_chosen );
                transform.LookAt( object_looked_at.transform.position );
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

        private string FindName( string side ) {
            GameObject object_to_name = arenaManager.Get_stim_object( side );
            if( !object_to_name ) {
                return "None";
            }
            return FindName( object_to_name );
        }


        // get the name of the stimulus, which is also the name of the texture
        private string FindName( GameObject object_to_name ) {

            Renderer Renderer_in_object = object_to_name.GetComponentInChildren<Renderer>();
            Transform Parent = object_to_name.transform.parent;

            if( Renderer_in_object ) {
                return Renderer_in_object.material.mainTexture.name;
            } else if( Parent ) {
                Renderer Renderer_in_parent = Parent.gameObject.GetComponentInChildren<Renderer>();
                if( Renderer_in_parent ) {
                    return Renderer_in_parent.material.mainTexture.name;
                }
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
                Object = FindName( "0,1000_0,0250_0,0000" );
            } else {
                Object = FindName( "-0,1000_0,0250_0,0000" );
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
