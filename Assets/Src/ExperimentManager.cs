﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SimpleFileBrowser;
using System;

public class Trajectory
{
        private string Id;
        private List<Vector2> Positions;
        private List<float> Rotations;
        private List<string> Choice_sides;
        private List<bool> Choice_made;

        private int traj_indx;

        public Trajectory() {
            Id = string.Empty;
            Positions = new List<Vector2>();
            Rotations = new List<float>();
            Choice_sides = new List<string>();
            Choice_made = new List<bool>();

            traj_indx = 0;
        }
        public Trajectory( string id ) {
            Id = id;
            Positions = new List<Vector2>();
            Rotations = new List<float>();
            Choice_sides = new List<string>();
            Choice_made = new List<bool>();

            traj_indx = 0;
        }

        public string Get_id() {
            return Id;
        }

        public bool Is_empty() {
            return Id == string.Empty;
        }

        public float[] Do_step() {
            float[] step = new float[3];
            step[0] = Positions[traj_indx].x;
            step[1] = Positions[traj_indx].y;
            step[2] = Rotations[traj_indx];
            traj_indx++;

            //Reset the index to the begining if we reach the end
            if( traj_indx == Positions.Count ) {
                Reset_indx();
            }
            return step;
        }

        public void Reset_indx() {
            traj_indx = 0;
        }

        public void Add_position( Vector2 pos ) {
            Positions.Add( pos );
        }

        public void Add_rotation( float rot ) {
            Rotations.Add( rot );
        }

        public void Add_choice_side( string side ) {
            Choice_sides.Add( side );
            Choice_made.Add( side != "None" );
        }

        public string Get_choice_side() {
            return Choice_sides[traj_indx];
        }

        public bool is_choice_made() {
            return Choice_made[traj_indx];
        }
}

public class Experiment : ScriptableObject
{
        public string[] USDuration;//array Of duration to wait for US
        public string[] PrepPhaseDuration;
        public string[] CSStart; // array of wait time before CS and Arena Apperanace
        public string[] CSStop; //  array of trial duration
        public string[] Repetition; // number of repetition of each line of instructions
        public List<string[]> Sequences; // list of the different sequences of stimulation
        public List<string[]> SequencesPreStim; // list of the different sequences of Pre stimulation
        public string[] Stims_one;
        public string[] Stims_two;
        public bool is_2D = false; // is the experiment 2D only. Meaning only rotation is allowed

        public int[] selGridTest; // grid of two button to select if a line is a Test or Not
        public int[] selGridPreTest; // grid of two button to select if a line is a PreTest or Not

        public List<string> PathList;
        public string PathWall; // string path to wall texture to load
        public string Path_Floor; // string path to floor texture to load
        public List<string> Textures_to_ignore;

        public bool[] Wall_on;

        public string string_seq;
        public string string_pre;

        public List<Trajectory> Trajectories;
        public bool Is_yoke = false;
        private List<int> Traj_indexes;

        public void OnAfterDeserialize() {
            Sequences = new List<string[]>();
            SequencesPreStim = new List<string[]>();
            string[] All_seqs = string_seq.Split( new char[] { '[' }, StringSplitOptions.RemoveEmptyEntries );
            foreach( string item in All_seqs ) {
                string[] seq = item.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
                Sequences.Add( seq );
            }
            // TODO: encapsulate this to avoid repeating the code twice
            string[] All_pres = string_pre.Split( new char[] { '[' }, StringSplitOptions.RemoveEmptyEntries );
            foreach( string item in All_pres ) {
                string[] pre = item.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
                SequencesPreStim.Add( pre );
            }
        }

        public void OnBeforeSerialize() {
            foreach( string[] item in Sequences ) {
                string_seq += "[";
                foreach( string str in item ) {
                    string_seq += str + ";";
                }
            }

            //TODO: encapsulate to avoid code duplciation
            foreach( string[] item in SequencesPreStim ) {
                string_pre += "[";
                foreach( string str in item ) {
                    string_pre += str + ";";
                }
            }
        }
        public Trajectory Get_a_trajectory( ) {
            if( Traj_indexes.Count == 0 ) {
                Initialize_traj_indexes();
            }

            int indx = Traj_indexes[( int )Mathf.Round( UnityEngine.Random.Range( 0,
                                                        Traj_indexes.Count - 1 ) )];
            Trajectory Trajectory_to_return = Trajectories[indx];
            Traj_indexes.Remove( indx );
            return Trajectory_to_return;
        }

        public void Initialize_traj_indexes() {
            Traj_indexes = new List<int>();
            for( int i = 0; i < Trajectories.Count; i++ ) {
                Traj_indexes.Add( i );
            }
        }
}

public class ExperimentManager : MonoBehaviour
{

        public InputField INTestNb; // input field for TestNb
        public InputField INSavePath; // InputField for path to prgm save file
        public InputField INPrgName; // Name of saved prgm
        public InputField INLoadPath; // InputField for path to file to load

        public InputField INLoadPathYoke; // InputField for path to yoke folder

        public ArenaManager ArenaManager;

        private Rect WindowRect; // position of the programme window
        private Rect Window2Drect;
        private bool WindowON = false; // display the window or not
        public CanvasScaler canscaler; // Canvas scaler to scale UI with screen size

        private Vector2 scrollposition = Vector2.zero;  //

        public Experiment Experiment_data;

        private string[] seq;

        public bool no_flow = false; //

        private Rect WindowSeqRect;// window showing the sequences of stimulus
        private bool showseq = false;

        private string[] sides;// =["Left","Right"]
        private int sideIndex; // = 0 or =1 used to point to Value inside sides

        private string[] YeNo; // = ["Yes","No"] used to convert selgridInt into words


        private int LineNb = 0; // number of trial in prgm

        private int ID = 0; // number ID of the bee surrently in the setup

        private bool saved = false; // if prgram is saved
        private Rect SavedWindowRect = new Rect( Screen.width / 2, Screen.height / 2, 1000,
                70 ); // window for saving confirmation  /!\ Might need better way to position /!\

        private void Awake() {
            Canvas.ForceUpdateCanvases();// Force update of canvas

        }
        // Use this for initialization
        void Start() {

            WindowRect = new Rect( Screen.width / 5, Screen.height / 2, 500 * canscaler.scaleFactor,
                                   300 * canscaler.scaleFactor );
            Window2Drect = new Rect( Screen.width / 5, Screen.height / 2, 5 * canscaler.scaleFactor,
                                     3 * canscaler.scaleFactor );


            INTestNb.text = 0.ToString(); // initialize input field
            INPrgName.text = "Test";// initialize input field

            Experiment_data = ScriptableObject.CreateInstance<Experiment>();
            Experiment_data.PathList = new List<string>();
            Experiment_data.Textures_to_ignore = new List<string> { };
            Experiment_data.Trajectories = new List<Trajectory>();

            sides = new string[2];
            sides[0] = "Left";
            sides[1] = "Right";

            YeNo = new string[2];
            YeNo[0] = "No";
            YeNo[1] = "Yes";

        }

        public void Enter() { // when the button 'Enter' is pressed

            LineNb = int.Parse( INTestNb.text ); // get test Nb from inputfield

            Experiment_data.USDuration = new
            string[LineNb]; // initialize size of the arrays according to the number of line of instructions
            Experiment_data.PrepPhaseDuration = new string[LineNb];
            Experiment_data.CSStart = new string[LineNb];
            Experiment_data.CSStop = new string[LineNb];
            Experiment_data.Repetition = new string[LineNb];

            Experiment_data.Stims_one = new string[LineNb];
            Experiment_data.Stims_two = new string[LineNb];

            Experiment_data.selGridTest = new int[LineNb];
            Experiment_data.selGridPreTest = new int[LineNb];

            Experiment_data.Wall_on = new bool[LineNb];


            for( int i = 0; i < LineNb; i++ ) {
                Experiment_data.USDuration[i] = string.Empty; // initialize the textfields
                Experiment_data.PrepPhaseDuration[i] = string.Empty; // initialize the textfields
                Experiment_data.CSStart[i] = string.Empty; // initialize the textfields
                Experiment_data.CSStop[i] = string.Empty; // initialize the textfields

                Experiment_data.Repetition[i] = string.Empty;

                Experiment_data.Wall_on[i] = false;
            }

            WindowON = true;

        }

        public void Show() {

            WindowON = true;
        }

        public void Copy() { //Copies first line onto every lines
            for( int i = 1; i < LineNb; i++ ) {
                Experiment_data.USDuration[i] = Experiment_data.USDuration[0]; /// ith line = 0th line
                Experiment_data.PrepPhaseDuration[i] = Experiment_data.PrepPhaseDuration[0];
                Experiment_data.CSStart[i] = Experiment_data.CSStart[0];
                Experiment_data.CSStop[i] = Experiment_data.CSStop[0];
                Experiment_data.Wall_on[i] = Experiment_data.Wall_on[0];
            }
        }

        public void Flow() {
            no_flow = !no_flow;
            Renderer rend_Wall;
            if( no_flow ) {
                foreach( GameObject Wall_Obj in GameObject.FindGameObjectsWithTag( "Wall" ) ) {
                    rend_Wall = Wall_Obj.GetComponentInChildren<Renderer>();
                    rend_Wall.material.shader = Shader.Find( "Custom/ScreenSpaceTextureShader" );
                }
            } else {
                foreach( GameObject Wall_Obj in GameObject.FindGameObjectsWithTag( "Wall" ) ) {
                    rend_Wall = Wall_Obj.GetComponentInChildren<Renderer>();
                    rend_Wall.material.shader = Shader.Find( "Standard" );
                }
            }
        }

        public void Make_Template() {
            //Copies first line onto every lines
            for( int i = 1; i < LineNb; i++ ) {
                Experiment_data.USDuration[i] = Experiment_data.USDuration[0]; /// ith line = 0th line
                Experiment_data.PrepPhaseDuration[i] = Experiment_data.PrepPhaseDuration[0];
                Experiment_data.CSStart[i] = Experiment_data.CSStart[0];
                Experiment_data.CSStop[i] = Experiment_data.CSStop[0];
                Experiment_data.Repetition[i] = "1";
            }
        }

        public void BrowseSave() {
            // find folder to save programme to

            FileBrowser.ShowLoadDialog( ( paths ) => {
                INSavePath.text = paths[0];
            },
            () => {
                Debug.Log( "Canceled" );
            },
            FileBrowser.PickMode.Folders, false, null, null, "Select Folder",
            "Select" ); // file browser to get save folder


        }
        public void BrowseLoad() { // find programme file to load
            FileBrowser.SetFilters( true, new FileBrowser.Filter( "Text Files", ".txt", ".pdf" ) );
            FileBrowser.SetDefaultFilter( ".txt" );

            FileBrowser.ShowLoadDialog( ( paths ) => {
                INLoadPath.text = paths[0];
            },
            () => {
                Debug.Log( "Canceled" );
            },
            FileBrowser.PickMode.Files, false, null, null, "Select Folder",
            "Select" ); // file browser to find file to load

        }

        //TODO: Unify those browse functions
        public void BrowseLoadYoke() {
            // find folder containing the Yoke files
            FileBrowser.ShowLoadDialog( ( paths ) => {
                INLoadPathYoke.text = paths[0];
            },
            () => {
                Debug.Log( "Canceled" );
            },
            FileBrowser.PickMode.Folders, false, null, null, "Select Folder",
            "Select" ); // file browser to get save folder
        }

        public void LoadYokes() {
            Experiment_data.Is_yoke = true;
            DirectoryInfo Dir = new DirectoryInfo( INLoadPathYoke.text );
            if( Dir.Exists ) {
                foreach( FileInfo file in Dir.GetFiles() ) {
                    Trajectory new_traj = Check_yoke( file );
                    if( !new_traj.Is_empty() ) {
                        Experiment_data.Trajectories.Add( new_traj );
                        Debug.Log( file.Name + " added." );
                    }
                }
            }
            Experiment_data.Initialize_traj_indexes();
        }

        private Trajectory Check_yoke( FileInfo file ) {
            Trajectory temp_traj = new Trajectory( file.Name );
            if( !file.Exists ) {
                return new Trajectory();
            }
            using( StreamReader sr = file.OpenText() ) {
                string s;
                int line_index = 0;
                while( ( s = sr.ReadLine() ) != null ) {
                    line_index++;
                    if( line_index <= 4 ) {
                        continue;
                    }
                    string[] row = s.Split( new char[] { ';' } );
                    int curr_line;
                    if( !int.TryParse( row[0], out curr_line ) ) {
                        Debug.Log( file.Name + " can't parse row 0" );
                        return new Trajectory();
                    }
                    if( curr_line > Experiment_data.Repetition.Length ) {
                        Debug.Log( file.Name + " too many lines" );
                        return new Trajectory();
                    }

                    int curr_repetition;
                    if( !int.TryParse( Experiment_data.Repetition[curr_line], out curr_repetition ) ) {
                        Debug.Log( file.Name + " can't parse lines" );
                        return new Trajectory();
                    }

                    int in_file_repetition;
                    if( !int.TryParse( row[1], out in_file_repetition ) ) {
                        Debug.Log( file.Name + " can't parse row 1" );
                        return new Trajectory();
                    }
                    if( curr_repetition < in_file_repetition ) {
                        Debug.Log( file.Name + " too many repetition" );
                        return new Trajectory();
                    }

                    float trial_duration;
                    if( !float.TryParse( Experiment_data.CSStop[curr_line], out trial_duration ) ) {
                        Debug.Log( file.Name + " can't parse CSSTop" );
                        return new Trajectory();
                    }

                    float US_duration;
                    if( !float.TryParse( Experiment_data.USDuration[curr_line], out US_duration ) ) {
                        Debug.Log( file.Name + " can't aprse US duration" );
                        return new Trajectory();
                    }

                    float in_file_duration = float.Parse( row[2], System.Globalization.CultureInfo.InvariantCulture );

                    if( trial_duration + US_duration < in_file_duration || ( trial_duration < in_file_duration &&
                            row[8] != "None" ) ) {
                        Debug.Log( file.Name + " trial duration too long" );
                        return new Trajectory();
                    }

                    float pos_x = float.Parse( row[3], System.Globalization.CultureInfo.InvariantCulture );
                    float pos_z = float.Parse( row[4], System.Globalization.CultureInfo.InvariantCulture );
                    float rot = float.Parse( row[5], System.Globalization.CultureInfo.InvariantCulture );

                    temp_traj.Add_position( new Vector2( pos_x, pos_z ) );
                    temp_traj.Add_rotation( rot );
                    temp_traj.Add_choice_side( row[9] );
                }
            }
            return temp_traj;
        }

        public void
        Save() { // Create a text file named PrgName.text.txt and writes the values of US Start , CS Start and CS stop inside
            while( File.Exists( INSavePath.text + "\\" + INPrgName.text + ID.ToString() +
                                ".txt" ) ) { // if file already exists
                ID += 1;//Increment the ID until it finds a new one
            }
            StreamWriter sw = new StreamWriter( INSavePath.text + "\\" + INPrgName.text + ID.ToString() +
                                                ".txt" ); // initiate stream writer

            Experiment_data.OnBeforeSerialize();
            sw.Write( JsonUtility.ToJson( Experiment_data, true ) );

            sw.Close(); // close  the stream writer
            sw.Dispose();//dispose of the stream writer

            saved = true;
        }




        public void Load() { // read program specifications from text file
            StreamReader sr = new StreamReader( INLoadPath.text ); // initiate stream reader

            Enter(); // initialise US Start, CS Start and CS Stop

            JsonUtility.FromJsonOverwrite( sr.ReadToEnd(), Experiment_data );
            sr.Close();// close the reader
            sr.Dispose();
            Experiment_data.OnAfterDeserialize();

            ArenaManager.INpathWall.text = Experiment_data.PathWall;
            ArenaManager.INpath_Floor.text = Experiment_data.Path_Floor;
            LineNb = Experiment_data.CSStart.GetLength( 0 );
            INTestNb.text = LineNb.ToString(); // Fill TestNbIn with the right value

            //Load texture to list\\
            ArenaManager.LoadTolist();
            //Load wall texture to walls\\
            ArenaManager.Walls();
            //Load wall texture to floor\\
            ArenaManager.Floor();

        }

        public void MakeSequences() {
            SideSequence();
            PreStimSequence();
        }


        private void SideSequence() {

            Experiment_data.Sequences = new List<string[]>(); // initilise list of sequences of stimuli

            for( int i = 0; i < LineNb; i++ ) { // for each line of instruction
                seq = new string[int.Parse( Experiment_data.Repetition[i] )]; // initialise the sequence

                for( int j = 0; j < int.Parse( Experiment_data.Repetition[i] );
                     j++ ) { // for each repetition of the line

                    if( j <= 1 ) { // for the 2 first repetition
                        sideIndex = ( int )Mathf.Round( UnityEngine.Random.value ); //flip a coin
                        seq[j] = sides[sideIndex] + "/" + sides[1 -
                                                                sideIndex]; //set the sides on which to display the stimuli


                    } else if( seq[j - 2] == seq[j - 1] ) { // if the 2 previous repetition were identical
                        seq[j] = sides[1 - sideIndex] + "/" + sides[sideIndex]; // do the opposite
                    } else {
                        sideIndex = ( int )Mathf.Round( UnityEngine.Random.value ); //flip a coin
                        seq[j] = sides[sideIndex] + "/" + sides[1 - sideIndex];//set the sides

                    }
                }

                Experiment_data.Sequences.Add( seq ); //add the sequence to the list

            }

        }

        private void PreStimSequence() {

            Experiment_data.SequencesPreStim = new
            List<string[]>(); // initilise list of sequences of  pre stimuli

            int Index = 0;
            string[] Preseq;

            for( int i = 0; i < LineNb; i++ ) {
                // for each line of instruction
                Preseq = new string[int.Parse( Experiment_data.Repetition[i] )]; // initialise the sequence

                for( int j = 0; j < int.Parse( Experiment_data.Repetition[i] );
                     j++ ) {
                    // for each repetition of the line
                    if( int.Parse( Experiment_data.PrepPhaseDuration[i] ) <= 0 ) {
                        Preseq[j] = "None";
                        continue;
                    }
                    if( j <= 1 || Preseq[j - 2] != Preseq[j - 1] ) {
                        // for the 2 first repetition or when the two previous ones were different
                        Index = ( int )Mathf.Round( UnityEngine.Random.value ); //flip a coin
                        if( Index < 1 ) {
                            Preseq[j] = Experiment_data.Stims_one[i];
                        } else {
                            Preseq[j] = Experiment_data.Stims_two[i];
                        }
                    } else {
                        // if the 2 previous repetition were identical
                        if( Index < 1 ) {
                            Preseq[j] = Experiment_data.Stims_two[i];
                        } else {
                            Preseq[j] = Experiment_data.Stims_one[i];
                        } // do the opposite
                    }
                }
                Experiment_data.SequencesPreStim.Add( Preseq ); //add the sequence to the list
            }
        }

        public void showsequences() {

            showseq = !showseq;

        }


        private void OnGUI() {

            if( WindowON ) {
                WindowRect = GUI.Window( 10, WindowRect, WindowMaker, "Programme" );
                Window2Drect = GUILayout.Window( 11, Window2Drect, windowIs2D, "Is 2D ?" );
            }




            if( saved ) {
                SavedWindowRect = GUI.Window( 1, SavedWindowRect, DoMyWindow, "Done" );

            }

            if( showseq ) {
                WindowSeqRect = GUILayout.Window( 2, WindowSeqRect, WindowSeq, "Sequences" );
            }
        }

        private void WindowSeq( int windowID ) {
            GUILayout.BeginVertical();
            GUILayout.BeginVertical();
            for( int k = 0; k < LineNb; k++ ) {
                GUILayout.BeginHorizontal();
                for( int l = 0; l < Experiment_data.Sequences[k].Length; l++ ) {
                    GUILayout.Box( Experiment_data.Sequences[k][l] );
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            //Pre Stim Sequences
            for( int k = 0; k < LineNb; k++ ) {
                GUILayout.BeginHorizontal();
                for( int l = 0; l < Experiment_data.SequencesPreStim[k].Length; l++ ) {
                    GUILayout.Box( Experiment_data.SequencesPreStim[k][l] );
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.EndVertical();
            if( GUILayout.Button( "Hide" ) ) {
                showseq = !showseq;
            }

            GUI.DragWindow();
        }

        private void WindowMaker( int windowID ) {

            scrollposition = GUILayout.BeginScrollView( scrollposition );
            for( int i = 0; i < LineNb; i++ ) {
                GUILayout.BeginHorizontal();
                GUILayout.Box( "US Duration", GUILayout.Width( 90 ) );
                GUILayout.Box( "Prep Duration", GUILayout.Width( 90 ) );
                GUILayout.Box( "CS Start", GUILayout.Width( 90 ) );
                GUILayout.Box( "CS Stop", GUILayout.Width( 90 ) );
                GUILayout.Box( "Repetiton", GUILayout.Width( 90 ) );
                GUILayout.Box( "Stim 1", GUILayout.Width( 90 ) );
                GUILayout.Box( "Stim 2", GUILayout.Width( 90 ) );
                GUILayout.Box( "Test ?" + YeNo[Experiment_data.selGridTest[i]], GUILayout.Width( 90 ) );
                GUILayout.Box( "PreTest ?" + YeNo[Experiment_data.selGridPreTest[i]], GUILayout.Width( 90 ) );
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                Experiment_data.USDuration[i] = GUILayout.TextField( Experiment_data.USDuration[i], 3,
                                                GUILayout.Width( 90 ) );
                Experiment_data.PrepPhaseDuration[i] = GUILayout.TextField( Experiment_data.PrepPhaseDuration[i], 3,
                                                       GUILayout.Width( 90 ) );
                Experiment_data.CSStart[i] = GUILayout.TextField( Experiment_data.CSStart[i], 3,
                                             GUILayout.Width( 90 ) );
                Experiment_data.CSStop[i] = GUILayout.TextField( Experiment_data.CSStop[i], 3,
                                            GUILayout.Width( 90 ) );
                Experiment_data.Repetition[i] = GUILayout.TextField( Experiment_data.Repetition[i], 3,
                                                GUILayout.Width( 90 ) );

                Experiment_data.Stims_one[i] = GUILayout.TextField( Experiment_data.Stims_one[i],
                                               GUILayout.Width( 90 ) );
                Experiment_data.Stims_two[i] = GUILayout.TextField( Experiment_data.Stims_two[i],
                                               GUILayout.Width( 90 ) );

                Experiment_data.selGridTest[i] = GUILayout.SelectionGrid( Experiment_data.selGridTest[i], YeNo, 2,
                                                 GUILayout.Width( 90 ) );
                Experiment_data.selGridPreTest[i] = GUILayout.SelectionGrid( Experiment_data.selGridPreTest[i],
                                                    YeNo, 2, GUILayout.Width( 90 ) );
                Experiment_data.Wall_on[i] = GUILayout.Toggle( Experiment_data.Wall_on[i], "Wall On ?" );
                GUILayout.EndHorizontal();

            }
            GUILayout.EndScrollView();

            if( GUILayout.Button( "Hide" ) ) {
                WindowON = !WindowON;
            }
            GUI.DragWindow();

        }

        private void windowIs2D( int windowID ) {
            Experiment_data.is_2D = GUILayout.Toggle( Experiment_data.is_2D, "Is 2D ?" );
            GUI.DragWindow();
        }


        void DoMyWindow( int windowID ) {
            if( GUILayout.Button( "Program Saved at " + INSavePath.text + "\\" + INPrgName.text + ID.ToString()
                                  + ".txt" ) ) {
                saved = false;
            }
            GUI.DragWindow();

        }



}
