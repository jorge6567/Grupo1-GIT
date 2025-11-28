using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    GameObject Player;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = new Vector2 (Player.transform.position.x, transform.position.y);
    }
}
