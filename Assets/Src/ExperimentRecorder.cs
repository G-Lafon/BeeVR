using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SimpleFileBrowser;
using System;


public class ExperimentRecorder : MonoBehaviour
{

        public InputField Ex_Name; //Input field for name of the experiment
        public InputField Date; // ... for Date
        public InputField Path;// ... Path to the folder holding the results
        public InputField INTimeStep; // Time between two mesure
        public InputField BeeID; // ID of the Bee
        public InputField InNbTrial; // number of trial

        private StreamWriter sw;  // stream writer writting the data

        private float Chrono = 0f; // time ellapsed since beginning
        private float TimeStep = 0f; // time between two position recording

        private int ID = 0; // Current Bee ID

        private ConditionningRunner Conditionner;

        // Use this for initialization
        void Start() {
            BeeID.text = ID.ToString(); //get Bee ID
            Date.text = DateTime.Today.ToString( @"d-M-yyyy" ); // set the Date
            INTimeStep.text = "0.0"; // set delay between sampling

            Ex_Name.text = "Results"; //default name for result text file
            Conditionner = gameObject.GetComponent<ConditionningRunner>();
        }

        public void Browse() {

            FileBrowser.ShowLoadDialog( ( paths ) => {
                Path.text = paths[0];
            },
            () => {
                Debug.Log( "Canceled" );
            },
            FileBrowser.PickMode.Folders, false, null, null, "Select Folder",
            "Select" );// file browser to get save folder
        }


        public void Start_Record() {
            reset_chrono();

            ID = int.Parse( BeeID.text ); // get ID

            while( File.Exists( Path.text + "\\" + Ex_Name.text + "_" + Date.text + "_Bee" + ID.ToString() +
                                ".csv" ) ) { // If there already a file with that name
                ID += 1; // change the ID by 1 until you get to a new file name
                BeeID.text = ID.ToString();
            }

            sw = new StreamWriter( Path.text + "\\" + Ex_Name.text + "_" + Date.text + "_Bee" + ID.ToString() +
                                   ".csv" ); // initiate stream writer
            sw.WriteLine( "#" + Ex_Name.text );
            sw.WriteLine( "#" + Date.text + " " + DateTime.Now.ToString(
                              @"HH:mm:ss" ) ); // Header of the result file
            sw.WriteLine( "#" + "Bee" + ID.ToString() );
            sw.WriteLine( "Line;Trial;Time(s);PositionX;PositionZ;Rotation;DistanceTotale;Speed;PreStim;Choice;Side_Chosen;Test;Centered_Object;Looking_at;Edge;Edge_coord;Bee_Leader" );
            sw.Flush(); // need to flush to actually write in the file


        }

        void Update() {
            if( !Conditionner.bee_can_move() ) {
                return;
            }

            TimeStep += Time.deltaTime; // update timers

            process_chrono();

            if( TimeStep >= float.Parse( INTimeStep.text,
                                         System.Globalization.CultureInfo.InvariantCulture.NumberFormat ) ) {
                // if over DeltaTime, Conditionning underway and under number of trial
                Record_data_point();
            }
        }
        public void reset_chrono() {
            Chrono = 0f;
        }
        private void process_chrono() {
            Chrono += Time.deltaTime;
        }

        public void Record_data_point() {
            if( sw != null ) {
                System.Globalization.CultureInfo Inv_C = System.Globalization.CultureInfo.InvariantCulture;
                string bee_leader = "Na";
                if( Conditionner.Get_current_trajectory() != null ) {
                    bee_leader = Conditionner.Get_current_trajectory().Get_id();
                }
                sw.WriteLine( Conditionner.Line.ToString( Inv_C ) + ";"
                              +
                              Conditionner.a.ToString( Inv_C ) + ";" +
                              Chrono.ToString( Inv_C ) + ";" +
                              gameObject.transform.position.x.ToString( Inv_C )
                              + ";" + gameObject.transform.position.z.ToString(
                                  Inv_C ) + ";" +
                              gameObject.transform.rotation.eulerAngles.y.ToString(
                                  Inv_C ) + ";" +
                              Conditionner.Dist.ToString( Inv_C ) + ";" +
                              Conditionner.Speed.ToString( Inv_C ) +
                              ";" + Conditionner.PrepPhase_Stim + ";" + Conditionner.Choice + ";" +
                              Conditionner.Side_Chosen + ";" +
                              Conditionner.what.text + ";" +
                              Conditionner.Centered_object + ";" +
                              Conditionner.Object_looked_at + ";" +
                              Conditionner.Get_edge_data() + ";" + bee_leader ); // write the data
                sw.Flush();

                TimeStep = 0f; // reset timer
            }
        }

        public void Stop_Record() {
            reset_chrono();
            if( sw != null ) {
                sw.Close(); // close writer
            }
        }



}
