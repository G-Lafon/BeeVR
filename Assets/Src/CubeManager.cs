using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeManager : MonoBehaviour
{

        public GameObject Edge_0;
        public GameObject Edge_1;
        public GameObject Edge_2;
        public GameObject Edge_3;


        public float Edges_scale = 0.1f;
        // Start is called before the first frame update
        void Start() {
            Set_edges();
        }

        // Update is called once per frame
        //       void Update() {
        //
        //       }

        //innelegant but simple
        public void Set_edges() {

            float locale_offset = 0.5f - Edges_scale / 2 + Edges_scale/10;

            Edge_0.transform.transform.localPosition = new Vector3( locale_offset, 0.0f, locale_offset );
            Edge_1.transform.transform.localPosition = new Vector3( locale_offset, 0.0f, -locale_offset );
            Edge_2.transform.transform.localPosition = new Vector3( -locale_offset, 0.0f, locale_offset );
            Edge_3.transform.transform.localPosition = new Vector3( -locale_offset, 0.0f, -locale_offset );

            Edge_0.transform.localScale = new Vector3( Edges_scale, 1.0f, Edges_scale );
            Edge_1.transform.localScale = new Vector3( Edges_scale, 1.0f, Edges_scale );
            Edge_2.transform.localScale = new Vector3( Edges_scale, 1.0f, Edges_scale );
            Edge_3.transform.localScale = new Vector3( Edges_scale, 1.0f, Edges_scale );
        }

}
