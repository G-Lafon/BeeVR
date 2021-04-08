using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SimpleFileBrowser;
using System;

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


        public string string_seq;
        public string string_pre;
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
}

public class ExperimentManager : MonoBehaviour
{

        public InputField INTestNb; // input field for TestNb
        public InputField INSavePath; // InputField for path to prgm save file
        public InputField INPrgName; // Name of saved prgm
        public InputField INLoadPath; // InputField for path to file to load

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




            for( int i = 0; i < LineNb; i++ ) {
                Experiment_data.USDuration[i] = string.Empty; // initialize the textfields
                Experiment_data.PrepPhaseDuration[i] = string.Empty; // initialize the textfields
                Experiment_data.CSStart[i] = string.Empty; // initialize the textfields
                Experiment_data.CSStop[i] = string.Empty; // initialize the textfields

                Experiment_data.Repetition[i] = string.Empty;
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


            FileBrowser.ShowLoadDialog( ( path ) => {
                INSavePath.text = path;
            },
            () => {
                Debug.Log( "Canceled" );
            },
            true, null, "Select Folder", "Select" ); // file browser to get save folder


        }
        public void BrowseLoad() { // find programme file to load
            FileBrowser.SetFilters( true, new FileBrowser.Filter( "Text Files", ".txt", ".pdf" ) );
            FileBrowser.SetDefaultFilter( ".txt" );
            FileBrowser.ShowLoadDialog( ( path ) => {
                INLoadPath.text = path;
            },
            () => {
                Debug.Log( "Canceled" );
            },
            false, null, "Select File", "Select" ); // file browser to find file to load

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
