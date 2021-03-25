using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaManager : MonoBehaviour
{
        public int segments;
        public float radius;

        private LineRenderer line;

        private GameObject Bee;

        private Color color;
        private string MySide;

        void Start() {
            color = Color.grey;
            MySide = gameObject.GetComponent<SphereCollider>().name.Split( ' ' )[1];

            Bee = GameObject.FindGameObjectWithTag( "Player" );

            line = gameObject.GetComponent<LineRenderer>();

            line.positionCount = segments + 1;
            line.useWorldSpace = true;

            line.startWidth = 0.01f;
            line.endWidth = 0.01f;
            line.loop = true;

        }


        void CreatePoints( Color color ) {
            float x;
            float y = 0.005f;
            float z;
            float angle = 20f;

            radius = gameObject.GetComponent<SphereCollider>().radius;

            line.startColor = color;
            line.endColor = color;

            for( int i = 0; i < ( segments + 1 ); i++ ) {
                x = gameObject.transform.position.x + Mathf.Sin( Mathf.Deg2Rad * angle ) * radius;
                z = gameObject.transform.position.z + Mathf.Cos( Mathf.Deg2Rad * angle ) * radius;

                line.SetPosition( i, new Vector3( x, y, z ) );


                angle += ( 360f / segments );
            }
        }

        private void Update() {

            if( MySide == Bee.GetComponent<ConditionningRunner>().Side ) {
                color = Color.green;
            } else {
                color = Color.grey;
            }

            CreatePoints( color );

        }

}
