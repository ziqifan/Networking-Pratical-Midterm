using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickMannager : MonoBehaviour
{
    public int rows;
    public int colums;
    public float spacing;
    public GameObject brickPrefab;

    private List<GameObject> bricks = new List<GameObject>();

    void Start()
    {
        ResetLevel();
    }

    public void ResetLevel()
    {
        foreach(GameObject brick in bricks)
        {
            Destroy(brick);
        }
        bricks.Clear();

        for(int x = 0; x< colums; x++){
            for(int y = 0; y < rows; y++)
            {
                Vector2 spawnPos = new Vector2(transform.position.x, transform.position.y)
                    + new Vector2(x + (brickPrefab.transform.localScale.x + spacing),
                    -y + (brickPrefab.transform.localScale.y + spacing));
                GameObject brick = Instantiate(brickPrefab, spawnPos, Quaternion.identity);
                bricks.Add(brick);
            }
        }
    }

    public void RemoveBrick(Brick brick)
    {
        bricks.Remove(brick.gameObject);
        if(bricks.Count == 0)
        {
            ResetLevel();
        }
    }
}
