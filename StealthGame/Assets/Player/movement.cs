using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{

    public Rigidbody _rgbd;
    public Vector3 _dir;
    public float _speed;
    public Transform mainCamTransform;
    float turnSmoothVelocity;
    public float smoothingTime = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        mainCamTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        _dir =new Vector3(
            Input.GetAxis("Horizontal"), 0f,
            Input.GetAxis("Vertical")
        );
        //_dir = _dir.normalized*_speed;
        //Debug.DrawRay(transform.position, _dir*10, Color.red);
        
    }

    private void FixedUpdate()
    {
        if (_dir.magnitude >= 0.1f)
        {
            //angle en degrès dus à la direction         //on tient compte aussi de la caméra      
            float targetAngle = Mathf.Atan2(_dir.x, _dir.z) * Mathf.Rad2Deg + mainCamTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothingTime);

            //rotation du player grâce aux inputs, utiliser plutôt le rigidbody
            //transform.rotation = Quaternion.Euler(0f, angle, 0f);
            _rgbd.MoveRotation(Quaternion.Euler(0f, angle, 0f));

            //transformation de l'angle en direction, merci les quaternions
            Vector3 trueDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            trueDir.y = _rgbd.velocity.y;
            //Debug.DrawRay(transform.position, trueDir.normalized * _speed, Color.red);
            _rgbd.velocity = trueDir.normalized * _speed;

            //_dir.y = _rgbd.velocity.y;

            //_rgbd.velocity = _dir.normalized * _speed;
        }
    }
}
