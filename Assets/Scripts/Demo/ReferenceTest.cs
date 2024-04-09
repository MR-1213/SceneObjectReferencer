using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceTest : MonoBehaviour
{
    public GameObject player;
    [SerializeField] private Transform enemy;
    [SerializeField] private Rigidbody enemyRb;

    private void Start() 
    {
        Debug.Log("Player: " + player);
        Debug.Log("Enemy: " + enemy);
        Debug.Log("Enemy Rigidbody: " + enemyRb);
    }
}
