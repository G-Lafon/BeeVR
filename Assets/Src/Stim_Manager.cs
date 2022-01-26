using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stim_Manager : MonoBehaviour
{
        public InputField Scale_X;
        public InputField Scale_Z;
        public InputField Scale_Y;

        private Vector3 scale;

        private GameObject[] Stim_objects;

        // Start is called before the first frame update
        void Start() {
            scale = new Vector3( 0.05f, 0.05f, 0.05f );
            Scale_X.text = ( 0.05f ).ToString( System.Globalization.CultureInfo.InvariantCulture.NumberFormat );
            Scale_Y.text = ( 0.05f ).ToString( System.Globalization.CultureInfo.InvariantCulture.NumberFormat );
            Scale_Z.text = ( 0.05f ).ToString( System.Globalization.CultureInfo.InvariantCulture.NumberFormat );
        }

        public void Update_scale() {
            Stim_objects =
                GameObject.FindGameObjectsWithTag( "Stim_object" ); // left and right are always the same size
            int num_of_object = Stim_objects.Length;
            if( num_of_object > 0 ) {
                // here we look at the last object because the first one might be slated for destruction
                // destroy() only happen at the end of the loop
                Scale_X.text = Stim_objects[num_of_object - 1].transform.localScale.x.ToString(
                                   System.Globalization.CultureInfo.InvariantCulture.NumberFormat );
                Scale_Y.text = Stim_objects[num_of_object - 1].transform.localScale.y.ToString(
                                   System.Globalization.CultureInfo.InvariantCulture.NumberFormat );
                Scale_Z.text = Stim_objects[num_of_object - 1].transform.localScale.z.ToString(
                                   System.Globalization.CultureInfo.InvariantCulture.NumberFormat );
            }
        }


        public void On_scale_change() {
            scale = new Vector3( float.Parse( Scale_X.text,
                                              System.Globalization.CultureInfo.InvariantCulture.NumberFormat ), float.Parse( Scale_Y.text,
                                                      System.Globalization.CultureInfo.InvariantCulture.NumberFormat ), float.Parse( Scale_Z.text,
                                                              System.Globalization.CultureInfo.InvariantCulture.NumberFormat ) );

            Stim_objects = GameObject.FindGameObjectsWithTag( "Stim_object" );

            foreach( GameObject obj in Stim_objects ) {
                obj.transform.localScale = scale;
            }
        }
}
