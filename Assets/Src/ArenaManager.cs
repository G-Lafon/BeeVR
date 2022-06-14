using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleFileBrowser;

public class ArenaManager : MonoBehaviour
{

        public Dropdown ChooseArena; // dropdown list to choose Arena
        public Dropdown ChooseShape; // dropdown list to choose stim shape


        public GameObject Ymaze; // The 3D object YMaze
        public GameObject StimulusLeft; // The 2D stimulus with the detection area attached to it
        public GameObject StimulusRight;// same but on the right

        public GameObject Corridor_Wall; // The 3D object Corridor Wall
        public GameObject Corridor_Floor; // The 3D object Corridor Floor

        public GameObject OpenArena_Wall; // The 3D object OpenArena Wall
        public GameObject OpenArena_Floor; // The 3D object OpenArena Floor

        public GameObject OpenArena_Wall_Smooth; // The 3D object OpenArena Wall
        public GameObject OpenArena_Floor_Smooth; // The 3D object OpenArena Floor

        public GameObject SquareArena_Wall; // The 3D object SquareArena Wall
        public GameObject SquareArena_Floor; // The 3D object SquareArena Floor

        public GameObject Stimulus3DLeft; // 3D stimulus and area attached to it
        public GameObject Stimulus3DRight;
        public GameObject Stimulus3DLeft_Cylinder;
        public GameObject Stimulus3DRight_Cylinder;

        public GameObject Stimulus3DCenter;
        public GameObject Stimulus3DCenter_Cylinder;

        public GameObject Stimulus2DLeft; // The 2D stimulus with the detection area attached to it
        public GameObject Stimulus2DRight;// same but on the right

        /** The 2D stimulus with the camera tracking script with the detection area attached to it*/
        public GameObject StimulusSpriteLeft;
        public GameObject StimulusSpriteRight;// same but on the right

        public GameObject Teleporter_Left;
        public GameObject Teleporter_Right;


        public GameObject Plane_BackGround;

        public ExperimentManager Xpmanager;

        public GameObject bee; // The bee object wuith the camera and character controller

        public InputField INScale; // scale of the image to put in the 2D stuimulus in pixel/m

        public InputField Edge_Scale;

        private Vector3 pos_Y; // Initial coordinate of the Bee in the Y maze
        private Vector3 pos_O; // same but in Open Arena
        private Vector3 pos_C; // Corridor

        private Renderer rend; // renderer of 3D object stimulus
        private SpriteRenderer rend2D; // renderer 2d Object stimulus

        public InputField INpathPic; // Inputfield with path to image to load
        private string Path; // string of path to image to load
        private string Name; // name of the picture file

        private Texture pic; // www to get the image
        public List<Texture> Stimulations; // list of stimulations
        private bool ShowList = false; // wether or not to show the list
        private bool duplicate = false;// is there such a texture in the list already


        public InputField INpathWall; // Inputfield with path to wall texture to load
        private Texture Wall; // www to get the wall texture


        public InputField INpath_Floor; // Inputfield with path to floor texture to load
        private Texture Texture_Floor; // www to get the floor texture

        private Renderer rend_Wall; // renderer of arena Wall
        private Renderer rend_Floor; // renderer of arena Wall

        private bool Texture_menu_on = false;
        float Tiling_x = 0;
        float Tiling_y = 0;
        float Offset_x = 0;
        float Offset_y = 0;
        string Target = "Wall";

        private bool prev = false;// if the preview is on


        // Animate Tile
        private string Nbr_columns = "0";
        private string Nbr_rows = "0";
        private string Fps = "600";
        private bool Show = false;

        Rect windowRect1 = new Rect( Screen.width / 2, Screen.height / 2, Screen.width / 3.5f,
                                     Screen.height / 2 );
        Rect windowRect2 = new Rect( Screen.width / 2, Screen.height / 2, Screen.width / 3.5f,
                                     Screen.height / 2 );
        AnimateTiledTexture Animate;

        // Use this for initialization
        void Start() {
            pos_Y = new Vector3( 0, 0.017905f, -0.095f );
            pos_O = new Vector3( 0, 0.012905f, -0.189f );
            pos_C = new Vector3( 0, 0.013f, -0.38f );

            Edge_Scale.text = 0.1f.ToString( System.Globalization.CultureInfo.InvariantCulture.NumberFormat );

            INScale.text = 10000f.ToString(
                               System.Globalization.CultureInfo.InvariantCulture.NumberFormat ); // default scale 10000pixel/m = 10 pix/mm

            Stimulations = new List<Texture> { };
        }

        public void Spawn() { // instantiate the arena and the stimulus according to choice in ChooseArena


            switch( ChooseArena.value ) {
                case 1:
                    Clear_arena();

                    Instantiate<GameObject>( Ymaze ); // instantiate Ymaze
                    Instantiate<GameObject>( StimulusLeft );
                    Instantiate<GameObject>( StimulusRight );

                    bee.transform.position = pos_Y; // place the bbe at the start
                    bee.GetComponent<walking>().enabled = true; // enables control of the bee from the track ball


                    Get_Animate( "Wall" );
                    break;
                case 2:
                    Clear_arena();

                    if( Xpmanager.Experiment_data.is_2D ) {
                        Instantiate<GameObject>( Plane_BackGround );
                        Instantiate<GameObject>( Stimulus2DLeft );
                        Instantiate<GameObject>( Stimulus2DRight );
                        Instantiate<GameObject>( Teleporter_Left );
                        Instantiate<GameObject>( Teleporter_Right );
                        Get_Animate( "BackPlane" );
                    } else {
                        Instantiate<GameObject>( OpenArena_Wall ); // instantiate OpenArena
                        Instantiate<GameObject>( OpenArena_Floor );

                        Get_Animate( "Wall" );
                    }

                    bee.transform.position = pos_O; // place the bee
                    bee.GetComponent<walking>().enabled = true; // enables control of the bee from the track ball

                    break;
                case 3:
                    Clear_arena();


                    Instantiate<GameObject>( SquareArena_Wall );
                    Instantiate<GameObject>( SquareArena_Floor );

                    Get_Animate( "Wall" );


                    bee.transform.position = pos_O; // place the bee
                    bee.GetComponent<walking>().enabled = true; // enables control of the bee from the track ball

                    break;
                case 4:
                    Clear_arena();


                    Instantiate<GameObject>( OpenArena_Wall_Smooth );
                    Instantiate<GameObject>( OpenArena_Floor_Smooth );

                    Get_Animate( "Wall" );

                    bee.transform.position = pos_O; // place the bee
                    bee.GetComponent<walking>().enabled = true; // enables control of the bee from the track ball

                    break;
                case 5:
                    Clear_arena();

                    Instantiate<GameObject>( Corridor_Floor );
                    Instantiate<GameObject>( Corridor_Wall );

                    Instantiate<GameObject>( Stimulus3DLeft );
                    Instantiate<GameObject>( Stimulus3DRight );

                    bee.transform.position = pos_C; // place the bee
                    bee.GetComponent<walking>().enabled = true; // enables control of the bee from the track ball

                    Get_Animate( "Wall" );
                    break;

                default:
                    Clear_arena();


                    bee.GetComponent<walking>().enabled = false; // disable control of the bee from the track ball
                    break;
            }


        }

        public void Spawn_shape() {

            switch( ChooseShape.value ) {
                case 0://cube
                    Clear_Shape();
                    Instantiate<GameObject>( Stimulus3DLeft );
                    Instantiate<GameObject>( Stimulus3DRight );
                    Set_edge_scale();
                    break;
                case 1://cylinder
                    Clear_Shape();
                    Instantiate<GameObject>( Stimulus3DLeft_Cylinder );
                    Instantiate<GameObject>( Stimulus3DRight_Cylinder );
                    break;
                case 2://sprite
                    Clear_Shape();
                    Instantiate<GameObject>( StimulusSpriteLeft );
                    Instantiate<GameObject>( StimulusSpriteRight );

                    break;
                default:
                    break;
            }
        }

        public void Set_edge_scale() {
            CubeManager Current_cube_right =
                GameObject.FindGameObjectWithTag( "Right" ).GetComponentInChildren<CubeManager>();
            CubeManager Current_cube_left =
                GameObject.FindGameObjectWithTag( "Left" ).GetComponentInChildren<CubeManager>();
            if( Current_cube_left != null && Current_cube_right != null ) {
                Current_cube_left.Edges_scale = float.Parse( Edge_Scale.text,
                                                System.Globalization.CultureInfo.InvariantCulture.NumberFormat );
                Current_cube_left.Set_edges();
                Current_cube_right.Edges_scale = float.Parse( Edge_Scale.text,
                                                 System.Globalization.CultureInfo.InvariantCulture.NumberFormat );
                Current_cube_right.Set_edges();

            }
        }

        void Get_Animate( string target ) {
            // Animate.Initialize();
            Animate = GameObject.FindGameObjectWithTag( target ).GetComponentInChildren<AnimateTiledTexture>();
        }

        void Clear_Shape() {
            Destroy( GameObject.FindGameObjectWithTag( "Left" ) );
            Destroy( GameObject.FindGameObjectWithTag( "Right" ) );
            Destroy( GameObject.FindGameObjectWithTag( "Center" ) );
        }
        void Clear_arena() {
            Destroy( GameObject.FindGameObjectWithTag( "Wall" ) );
            Destroy( GameObject.FindGameObjectWithTag( "Floor" ) );
            Destroy( GameObject.FindGameObjectWithTag( "Left" ) );
            Destroy( GameObject.FindGameObjectWithTag( "Right" ) );

            Destroy( GameObject.Find( "Plane(Clone)" ) );
            Destroy( GameObject.Find( "Teleporter Right(Clone)" ) );
            Destroy( GameObject.Find( "Teleporter Left(Clone)" ) );
        }

        public void
        BrowsePicture() { // find programme file to load https://github.com/yasirkula/UnitySimpleFileBrowser/blob/master/README.md
            FileBrowser.SetFilters( true, new FileBrowser.Filter( "Image", ".png", ".jpg" ) );
            FileBrowser.SetDefaultFilter( ".png" );

            FileBrowser.ShowLoadDialog( ( paths ) => {
                INpathPic.text = paths[0];
            },
            () => {
                Debug.Log( "Canceled" );
            },
            FileBrowser.PickMode.Files, false, null, null, "Select File",
            "Select" );// file browser to find file to load
        }

        public void LoadTolist() {

            if( !Xpmanager.Experiment_data.PathList.Contains( INpathPic.text ) &&
                INpathPic.text != string.Empty ) {
                Xpmanager.Experiment_data.PathList.Add( INpathPic.text ); // add path to the list
            }


            StartCoroutine( LoadToList() );

            if( Stimulations != null ) {
                ShowList = true;
            }

        }

        IEnumerator LoadToList() {
            foreach( var path in Xpmanager.Experiment_data.PathList ) {
                Path = string.Concat( "File:///", path ); // add correct formatting at the beginning of path
                using( UnityWebRequest uwr = UnityWebRequestTexture.GetTexture( Path ) ) {
                    yield return uwr.SendWebRequest();
                    bool v = uwr.result == UnityWebRequest.Result.ConnectionError ||
                             uwr.result == UnityWebRequest.Result.ProtocolError;
                    if( v ) {
                        Debug.Log( uwr.error );
                    } else {
                        pic = DownloadHandlerTexture.GetContent( uwr );
                    }
                }

                Name = path.Split( '\\' )[Path.Split( '\\' ).Length - 1]; // get the name of the file


                foreach( var item in Stimulations ) { //search if the texture is already in the list
                    if( item.name == Name.Split( '.' )[0] ) { // check for names
                        duplicate = true; // there is already a texture with the same name
                    }
                }


                if( duplicate == false ) { // if the texture is not already there
                    Stimulations.Add( pic ); //add the texture

                    Stimulations[Stimulations.Count - 1].name =
                        Name.Split( '.' )[0]; // sets the name of the stimulation, use split to remove the extention

                } else {
                    duplicate = false; // reset the bool
                }
            }

        }
        public void Add_to_ignore_list() {
            string tmp_name = INpathPic.text.Split( '\\' )[INpathPic.text.Split( '\\' ).Length -
                              1].Split( '.' )[0];
            if( !Xpmanager.Experiment_data.Textures_to_ignore.Contains( tmp_name ) ) {
                Xpmanager.Experiment_data.Textures_to_ignore.Add( tmp_name );
            }

        }

        public void ApplyTexture( string side, int stimIndex ) {
            if( GameObject.FindGameObjectWithTag( side ).GetComponentInChildren<Renderer>() !=
                null ) { // if renderer exist
                rend = GameObject.FindGameObjectWithTag( side ).GetComponentInChildren<Renderer>(); // find renderer
                rend.material.mainTexture = Stimulations[stimIndex]; // change texture with image
            }


            if( GameObject.FindGameObjectWithTag( side ).GetComponentInChildren<SpriteRenderer>() !=
                null ) { // same with Sprite  renderer
                rend2D = GameObject.FindGameObjectWithTag(
                             side ).GetComponentInChildren<SpriteRenderer>(); // find renderer
                rend2D.sprite = Sprite.Create( ( Texture2D )Stimulations[stimIndex], new Rect( 0, 0,
                                               Stimulations[stimIndex].width, Stimulations[stimIndex].height ), new Vector2( 0.5f, 0.5f ),
                                               float.Parse( INScale.text ) ); // create sprite from texture in www
            }

        }

        public void ApplyTexture( string side, string name ) {
            GameObject obj = GameObject.FindGameObjectWithTag( side );
            if( obj ) {

                rend = obj.GetComponentInChildren<Renderer>(); // find renderer
                rend2D = obj.GetComponent<SpriteRenderer>(); // find renderer

                // if renderer exist
                if( rend != null ) {
                    foreach( Texture stm in Stimulations ) {
                        if( stm.name == name ) {
                            rend.material.mainTexture = stm; // change texture with image
                            break;
                        }
                    }
                }

                // if Sprite  renderer exist
                if( rend2D != null ) {
                    // same with Sprite renderer
                    foreach( Texture stm in Stimulations ) {
                        if( stm.name == name ) {
                            rend2D.sprite = Sprite.Create( ( Texture2D )stm, new Rect( 0, 0,
                                                           stm.width, stm.height ), new Vector2( 0.5f, 0.5f ),
                                                           float.Parse( INScale.text ) ); // create sprite from texture in www
                            rend.material.mainTexture = stm; // change texture with image
                            break;
                        }
                    }
                }
            }
        }

        public void ApplyTexture( Renderer Renderer_to_texture, string name ) {
            if( Renderer_to_texture != null ) {
                foreach( Texture stm in Stimulations ) {
                    if( stm.name == name ) {
                        Renderer_to_texture.material.mainTexture = stm; // change texture with image
                        break;
                    }
                }
            }
        }

        public void Texture_menu() {
            Texture_menu_on = !Texture_menu_on;
        }
        private void Adjust_texture( string target, Vector2 Scale, Vector2 offset ) {
            Renderer tmp_rend = GameObject.FindGameObjectWithTag( target ).GetComponentInChildren<Renderer>();
            tmp_rend.material.mainTextureScale = Scale;
            tmp_rend.material.mainTextureOffset = offset;
        }

        public void Preview() {

            prev = !prev;
            ShowList = !ShowList;
            if( prev ) {
                Spawn_shape();
                ApplyTexture( "Left", 0 );
                ApplyTexture( "Right", 1 );
                bee.GetComponent<ConditionningRunner>().Stim( prev ); // show stim
            } else {
                Clear_Shape();
            }

        }

        private void OnGUI() {
            if( prev == true ) { // if preview is on
                if( GUILayout.Button( "End preview" ) ) {
                    prev = false; // preview is off

                    bee.GetComponent<ConditionningRunner>().Stim( false ); // hide stim

                }
            }
            if( Texture_menu_on ) {
                windowRect2 = GUI.Window( 7, windowRect2, WindowAdjust, "Adjust Textures" );
            }

            if( ShowList == true ) {
                GUILayout.BeginVertical();
                foreach( Texture i in Stimulations ) {
                    int k = 0;
                    GUILayout.BeginHorizontal();
                    GUILayout.Box( i.name );
                    if( GUILayout.Button( "Remove" ) ) {
                        Stimulations.Remove( i );
                        Xpmanager.Experiment_data.PathList.Remove( Xpmanager.Experiment_data.PathList[k] );
                    }
                    GUILayout.EndHorizontal();
                    k++;
                }
                GUILayout.Box( "Ignored items:" );
                foreach( string ignore_item in Xpmanager.Experiment_data.Textures_to_ignore ) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Box( ignore_item );
                    if( GUILayout.Button( "Remove" ) ) {
                        Xpmanager.Experiment_data.Textures_to_ignore.Remove( ignore_item );
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                if( GUILayout.Button( "Hide" ) ) {
                    ShowList = false;
                }
            }

            if( Show ) {
                windowRect1 = GUI.Window( 0, windowRect1, WindowAnimation, "Animate Textures" );
            }

        }


        // find programme file to load https://github.com/yasirkula/UnitySimpleFileBrowser/blob/master/README.md
        public void BrowseWall() {
            FileBrowser.SetFilters( true, new FileBrowser.Filter( "Image", ".png", ".jpg" ) );
            FileBrowser.SetDefaultFilter( ".png" );

            FileBrowser.ShowLoadDialog( ( paths ) => {
                INpathWall.text = paths[0];
            },
            () => {
                Debug.Log( "Canceled" );
            },
            FileBrowser.PickMode.Files, false, null, null, "Select File",
            "Select" ); // file browser to find file to load

        }
        public void BrowseFloor() {
            FileBrowser.SetFilters( true, new FileBrowser.Filter( "Image", ".png", ".jpg" ) );
            FileBrowser.SetDefaultFilter( ".png" );

            FileBrowser.ShowLoadDialog( ( paths ) => {
                INpath_Floor.text = paths[0];
            },
            () => {
                Debug.Log( "Canceled" );
            },
            FileBrowser.PickMode.Files, false, null, null, "Select File",
            "Select" ); // file browser to find file to load
        }


        public void Walls() {

            StartCoroutine( LoadWalls() );
        }

        IEnumerator LoadWalls() { // load walls
            if( Xpmanager.Experiment_data.PathWall == string.Empty ||
                Xpmanager.Experiment_data.PathWall == null ) {
                Xpmanager.Experiment_data.PathWall = string.Concat( "File:///", INpathWall.text );
            }
            using( var uwr = new UnityWebRequest( Xpmanager.Experiment_data.PathWall,
                                                  UnityWebRequest.kHttpVerbGET ) ) {
                uwr.downloadHandler = new DownloadHandlerTexture();
                yield return uwr.SendWebRequest();
                Wall = DownloadHandlerTexture.GetContent( uwr );
            }

            if( !Xpmanager.Experiment_data.is_2D ) {
                foreach( GameObject Wall_Obj in GameObject.FindGameObjectsWithTag( "Wall" ) ) {
                    rend_Wall = Wall_Obj.GetComponentInChildren<Renderer>();
                    rend_Wall.material.mainTexture = Wall;
                    if( Xpmanager.no_flow ) {
                        rend_Wall.material.shader = Shader.Find( "Custom/ScreenSpaceTextureShader" );
                    } else {
                        rend_Wall.material.shader = Shader.Find( "Standard" );
                    }
                }

            } else {
                if( GameObject.FindGameObjectWithTag( "BackPlane" ).GetComponentInChildren<Renderer>() != null ) {
                    Renderer rendBackPlane =
                        GameObject.FindGameObjectWithTag( "BackPlane" ).GetComponentInChildren<Renderer>();
                    rendBackPlane.material.mainTexture = Wall;
                    // rendBackPlane.material.shader = Shader.Find( "Transparent/Cutout/Diffuse" );
                    rendBackPlane.material.mainTextureScale = new Vector2( 40, 40 );

                }
            }

        }

        public void Floor() {

            StartCoroutine( LoadFloor() );
        }

        IEnumerator LoadFloor() {
            if( Xpmanager.Experiment_data.Path_Floor == string.Empty ||
                Xpmanager.Experiment_data.Path_Floor == null ) {
                Xpmanager.Experiment_data.Path_Floor = string.Concat( "File:///", INpath_Floor.text );
            }
            using( var uwr = new UnityWebRequest( Xpmanager.Experiment_data.Path_Floor,
                                                  UnityWebRequest.kHttpVerbGET ) ) {
                uwr.downloadHandler = new DownloadHandlerTexture();
                yield return uwr.SendWebRequest();
                Texture_Floor = DownloadHandlerTexture.GetContent( uwr );
            }

            if( !Xpmanager.Experiment_data.is_2D ) {
                foreach( GameObject Wall_Obj in GameObject.FindGameObjectsWithTag( "Floor" ) ) {
                    rend_Floor = Wall_Obj.GetComponentInChildren<Renderer>();
                    rend_Floor.material.mainTexture = Texture_Floor;
                    //  rend_Floor.material.shader = Shader.Find( "Transparent/Cutout/Diffuse" );
                }
            }
        }

        public void Open_Anim_menu() {
            Show = !Show;
        }

        void WindowAnimation( int windowID ) {
            GUILayout.BeginHorizontal();
            GUILayout.Box( "x offset increment" );
            Nbr_columns = GUILayout.TextField( Nbr_columns, 10, GUILayout.Width( 90 ) );
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box( "y offset increment" );
            Nbr_rows = GUILayout.TextField( Nbr_rows, 10, GUILayout.Width( 90 ) );
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box( "Fps" );
            Fps = GUILayout.TextField( Fps, 3, GUILayout.Width( 90 ) );
            GUILayout.EndHorizontal();

            if( GUILayout.Button( "Update" ) ) {
                Update_values();
            }
            GUILayout.BeginHorizontal();
            if( GUILayout.Button( "Play" ) ) {
                Animate.Play_animation();
            }
            if( GUILayout.Button( "Stop" ) ) {

                Animate.Stop_animation();

            }
            GUILayout.EndHorizontal();

            if( GUILayout.Button( "Close" ) ) {
                Show = false;
            }
            GUI.DragWindow(); // Make window draggable
        }

        void WindowAdjust( int windowID ) {
            GUILayout.BeginVertical();

            GUILayout.Box( "Target" );
            Target =  GUILayout.TextField( Target ) ;


            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Box( "Tiling x" );
            Tiling_x = float.Parse( GUILayout.TextField( Tiling_x.ToString( "0.000" ) ) );
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Box( "Tiling y" );
            Tiling_y = float.Parse( GUILayout.TextField( Tiling_y.ToString( "0.000" ) ) );
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Box( "Offset x" );
            Offset_x = float.Parse( GUILayout.TextField( Offset_x.ToString( "0.000" ) ) );
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Box( "Offset y" );
            Offset_y = float.Parse( GUILayout.TextField( Offset_y.ToString( "0.000" ) ) );
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if( GUILayout.Button( "Update" ) ) {
                Vector2 tiling = new Vector2( Tiling_x, Tiling_y );
                Vector2 offset = new Vector2( Offset_x, Offset_y );
                Adjust_texture( Target, tiling, offset );
            }

            if( GUILayout.Button( "Close" ) ) {
                Texture_menu_on = false;
            }
            GUI.DragWindow(); // Make window draggable
        }

        void Update_values() {
            Animate.x_offset_increment = float.Parse( Nbr_columns );
            Animate.y_offset_increment = float.Parse( Nbr_rows );
            Animate.framesPerSecond = int.Parse( Fps );
        }

}
