using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GM : MonoBehaviour
{
    public GameObject Ball1;
    public GameObject Ball2;
    public GameObject Paddle1;
    public GameObject Paddle2;
    public GameObject TheButtom;
    public GameObject BrickManager1;
    public GameObject BrickManager2;
    public Text Name1;
    public Text Name2;

    public static Vector3 ballPos = new Vector3(0, 0, 0);
    public static Vector3 pandPos = new Vector3(0, 0, 0);

    public float ballSpeed = 5.0f;
    Vector3 spawnPos;
    
    public float paddleSpeed = 5.0f;
    private float input;

    public bool palyer1 = true;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkControl.thisUser._id == 1)
        {
            palyer1 = true;
            Name1.text = NetworkControl.thisUser._name + " (You)";
            Name2.text = NetworkControl.enemyUser._name;
        }
        else if (NetworkControl.thisUser._id == 2)
        {
            palyer1 = false;
            Name2.text = NetworkControl.thisUser._name + " (You)";
            Name1.text = NetworkControl.enemyUser._name;
        }

        if (palyer1)
        {
            spawnPos = Ball1.transform.position;
            Ball1.GetComponent<Rigidbody2D>().velocity = Random.insideUnitCircle.normalized * ballSpeed;
        }
        else
        {
            spawnPos = Ball2.transform.position;
            Ball2.GetComponent<Rigidbody2D>().velocity = Random.insideUnitCircle.normalized * ballSpeed;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (palyer1)
        {
            ballPos = Ball1.transform.position;
            pandPos = Paddle1.transform.position;

            input = Input.GetAxisRaw("Horizontal");
            Paddle1.GetComponent<Rigidbody2D>().velocity = Vector2.right * input * paddleSpeed;
            if (Ball1.transform.position.y < TheButtom.transform.position.y)
            {
                Ball1.transform.position = spawnPos;
                Ball1.GetComponent<Rigidbody2D>().velocity = Random.insideUnitCircle.normalized * ballSpeed;
            }

            Ball2.transform.position = NetworkControl.enemyBallPosition;
            Paddle2.transform.position = NetworkControl.enemyPaddlePosition;
        }
        else
        {
            ballPos = Ball2.transform.position;
            pandPos = Paddle2.transform.position;

            input = Input.GetAxisRaw("Horizontal");
            Paddle2.GetComponent<Rigidbody2D>().velocity = Vector2.right * input * paddleSpeed;
            if (Ball2.transform.position.y < TheButtom.transform.position.y)
            {
                Ball2.transform.position = spawnPos;
                Ball2.GetComponent<Rigidbody2D>().velocity = Random.insideUnitCircle.normalized * ballSpeed;
            }

            Ball1.transform.position = NetworkControl.enemyBallPosition;
            Paddle1.transform.position = NetworkControl.enemyPaddlePosition;
        }
    }

    public void Respawn()
    {
        transform.position = spawnPos;
        GetComponent<Rigidbody2D>().velocity = Random.insideUnitCircle.normalized * ballSpeed;
    }
}
