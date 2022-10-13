using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody _rgbd;
    [SerializeField]
    private Vector3 _dir;
    public float _speed = 30f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        Debug.DrawRay(transform.position, _dir, Color.red);
        _rgbd.velocity = _dir * _speed * Time.deltaTime;
    }
}
