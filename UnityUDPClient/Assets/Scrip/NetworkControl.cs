using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Jobs;
using UnityEngine;
using static Networking;
using static Networking.PResult;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkControl : MonoBehaviour
{
    public Text ipText;
    public Text nameText;
    public Text waiting;
    static bool theWaiting = false;
    public static user thisUser = new user("", 0);
    public static user enemyUser = new user("", 0);

    static bool goGame = false;

    static bool inIP = true;
    static bool inGame = false;

    public static Vector3 enemyBallPosition = new Vector3(0, 0, 0);
    public static Vector3 enemyPaddlePosition = new Vector3(0, 0, 0);

    static SocketData sock;
    static IPEndpointData endp;
    NetworkClientJob jobClient;
    JobHandle hnd;
    [HideInInspector] public static bool close;
    [StructLayout(LayoutKind.Sequential)]
    struct vec3
    {
        public float x, y, z;
        public vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static vec3 operator -(vec3 v1, vec3 v2) => new vec3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);

        public float length() => Mathf.Sqrt(x * x + y * y + z * z);

        public static float dist(vec3 v1, vec3 v2) => (v1 - v2).length();

        public override string ToString() => "(" + x + ", " + y + ", " + z + ")";
    }

    public struct user
    {
        public user(string n, int d)
        {
            this._name = n;
            this._id = d;
        }
        public string _name;
        public int _id;
    }

    void PrintError(object msg) => Debug.LogError(msg);

    public struct NetworkClientJob : IJob
    {
        public SocketData sock;
        public IPEndpointData endp;
        public void Execute()
        {
            int size = 512;
            string recv = "";
            for (;;)
            {
                bool printable = true;
                if (recvFromPacket(ref sock, ref recv, size, ref endp) == P_GenericError)
                {
                    printable = false;
                    Debug.LogError(getLastNetworkError());
                }

                if (printable)
                {
                    if (inIP)
                    {
                        if (recv == "1")
                        {
                            thisUser._id = 1;
                            theWaiting = true;
                        }
                        else if (recv[0] == '2')
                        {
                            recv = recv.Substring(1);
                            thisUser._id = 2;
                            enemyUser._id = 1;
                            enemyUser._name = recv;
                        }
                        else if (recv[0] == '!')
                        {
                            recv = recv.Substring(1);
                            enemyUser._id = 2;
                            enemyUser._name = recv;
                        }
                        else if (recv == "@")
                        {
                            inIP = false;
                            inGame = true;
                            goGame = true;
                        }
                    }
                    else if (inGame)
                    {
                        if (recv[0] != thisUser._id)
                        {
                            print("here????");
                            recv = recv.Substring(1);
                            string[] strings = recv.Split(':');
                            enemyBallPosition.x = int.Parse(strings[0]);
                            enemyBallPosition.y = int.Parse(strings[1]);
                            enemyPaddlePosition.x = int.Parse(strings[2]);
                            enemyPaddlePosition.y = int.Parse(strings[3]);
                        }
                    }
                    else
                        print(recv);
                }

                if (NetworkControl.close)
                    break;

            }
        }
    }

    #region Singleton
    public static NetworkControl Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public void connectToInternet()
    {

        string serverAddr = ipText.text;
        string userName = nameText.text;

        thisUser._name = userName;

        // Create UDP Client
        shutdownNetwork(); //just incase there was some sort of error last time
        Networking.initNetwork();
        endp = createIPEndpointData(serverAddr, 8888);
        sock = initSocketData();

        if (createSocket(ref sock, SocketType.UDP) == P_GenericError)
            Debug.LogError(getLastNetworkError());

        if (connectEndpoint(ref endp, ref sock) == P_GenericError)
            Debug.LogError(getLastNetworkError());

        jobClient = new NetworkClientJob()
        {
            sock = sock,
            endp = endp
        };
        hnd = jobClient.Schedule(); //schedules the job to start asynchronously like std::detach in c++

        string tmp = "@" + userName;

        int dump = 0;

        if (sendToPacket(ref sock, ref tmp, ref dump, ref endp) == P_GenericError)
        {
            Debug.LogError(getLastNetworkError());
        }

    }

    private void Start()
    {
        waiting.color = Color.clear;
        inIP = true;
        inGame = false;
    }

    private void Update()
    {
        if (goGame)
        {
            goGame = false;
            // Go to Game
            SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
        if (theWaiting)
        {
            waiting.color = Color.white;
            theWaiting = false;
        }
        if (inGame)
        {
            string tmp = thisUser._id + GM.ballPos.x + ":" + GM.ballPos.y + ":" + GM.pandPos.x + ":" + GM.pandPos.y;

            print(tmp);

            int dump = 0;

            if (sendToPacket(ref sock, ref tmp, ref dump, ref endp) == P_GenericError)
            {
                Debug.LogError(getLastNetworkError());
            }
        }
    }

    private void OnApplicationQuit()
    {
        close = true;

        closeSocket(ref sock);
        shutdownNetwork();
        hnd.Complete(); //should be the same as thread::join in c++
    }
}