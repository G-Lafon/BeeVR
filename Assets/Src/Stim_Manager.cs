using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stim_Manager : MonoBehaviour
{
        public InputField Scale_X;
        public InputField Scale_Z;
        public InputField Scale_Y;

        private Vector3 scale_stim;
        private Vector3 scale_center;
        private Vector3 scale_2D;

        private GameObject[] Stim_objects;
        private GameObject[] Stim_centered;


        // Start is called before the first frame update
        void Start() {
            Scale_X.text = ( 0.0f ).ToString( System.Globalization.CultureInfo.InvariantCulture.NumberFormat );
            Scale_Y.text = ( 0.0f ).ToString( System.Globalization.CultureInfo.InvariantCulture.NumberFormat );
            Scale_Z.text = ( 0.0f ).ToString( System.Globalization.CultureInfo.InvariantCulture.NumberFormat );
        }

        private void Update_scale() {
            Stim_objects = GameObject.FindGameObjectsWithTag( "Stim_object" );
            Stim_centered = GameObject.FindGameObjectsWithTag( "Centered" );
            int num_of_object = Stim_objects.Length;
            if( num_of_object > 0 ) {
                // here we look at the last object because the first one might be slated for destruction
                // destroy() only happen at the end of the loop
                scale_stim.x = Stim_objects[num_of_object - 1].transform.localScale.x;
                scale_stim.y = Stim_objects[num_of_object - 1].transform.localScale.y;
                scale_stim.z = Stim_objects[num_of_object - 1].transform.localScale.z;

                scale_center.x = Stim_centered[num_of_object - 1].transform.localScale.x;
                scale_center.y = Stim_centered[num_of_object - 1].transform.localScale.y;
                scale_center.z = Stim_centered[num_of_object - 1].transform.localScale.z;
            }
        }

        public void On_scale_change() {

            Update_scale();

            Vector3 temp_scale = new Vector3( float.Parse( Scale_X.text,
                                              System.Globalization.CultureInfo.InvariantCulture.NumberFormat ), float.Parse( Scale_Y.text,
                                                      System.Globalization.CultureInfo.InvariantCulture.NumberFormat ), float.Parse( Scale_Z.text,
                                                              System.Globalization.CultureInfo.InvariantCulture.NumberFormat ) );


            scale_stim.x = temp_scale.x != 0.0f ? temp_scale.x : scale_stim.x;
            scale_stim.y = temp_scale.y != 0.0f ? temp_scale.y : scale_stim.y;
            scale_stim.z = temp_scale.z != 0.0f ? temp_scale.z : scale_stim.z;

            scale_center.x = temp_scale.x != 0.0f ? temp_scale.x : scale_center.x;
            scale_center.y = temp_scale.y != 0.0f ? temp_scale.y : scale_center.y;
            scale_center.z = temp_scale.z != 0.0f ? temp_scale.z : scale_center.z;

            scale_2D.x = scale_stim.x;
            scale_2D.y = scale_stim.z;
            scale_2D.z = scale_stim.y;

            Stim_objects = GameObject.FindGameObjectsWithTag( "Stim_object" );
            Stim_centered = GameObject.FindGameObjectsWithTag( "Centered" );

            for( int i = 0; i < Stim_objects.Length; i++ ) {
                if( Stim_objects[i].name.Split( ' ' )[0] == "Plane" ) {
                    Stim_objects[i].transform.localScale = scale_2D;
                } else {
                    Stim_objects[i].transform.localScale = scale_stim;
                }

            }
            for( int i = 0; i < Stim_centered.Length; i++ ) {
                Stim_centered[i].transform.localScale = scale_center;
            }
        }


}
