using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    IDLE,
    JOGGING,
    RUNNING,
    JUMPING,
    FALLING,
    SNEAKING
}
[RequireComponent( typeof( Rigidbody ) )]
public class PlayerStateMachine : MonoBehaviour
{
    #region Exposed

    [Header("Movement Speeds")]
    [SerializeField]
    private float _joggingSpeed = 5f;

    [SerializeField]
    private float _runningSpeed = 10f;

    [SerializeField]
    private float _sneakingSpeed = 2f;

    [SerializeField]
    private float _jumpForce = 5f;

    [SerializeField]
    private float _turnSpeed = 10f;

    [Header("Floor Detection")]
    [SerializeField]
    private LayerMask _groundMask;

    [SerializeField]
    private Vector3 _boxDimension;

    [SerializeField]
    private Transform _groundChecker;

    [SerializeField]
    private float _yFloorOffset = 1f;

    #endregion

    #region Unity Lifecycle

    void Awake(){
        _rigidbody = GetComponent<Rigidbody>();
        _floorDetector = GetComponentInChildren<FloorDetector>();
        _currentSpeed = _joggingSpeed;
    }

    void Start()
    {
        _cameraTransform = Camera.main.transform;
        _groundChecker = transform.Find( "GroundChecker" );
        TransitionToState( PlayerState.IDLE );
    }

    void Update()
    {
        OnStateUpdate();

        Collider[] groundColliders = Physics.OverlapBox( _groundChecker.position, _boxDimension, Quaternion.identity, _groundMask );
        _isGrounded = groundColliders.Length > 0;
    }
   
    void FixedUpdate()
    {
        
        if( _currentState == PlayerState.JUMPING || _currentState == PlayerState.FALLING )
        {
            _direction.y = _rigidbody.velocity.y;
        }
        else
        {
            StickToGround();
        }

        if( _isJumping )
        {
            _direction.y = _jumpForce;
            _isJumping = false;
        }

        RotateTowardsCamera();

        _rigidbody.velocity = _direction;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube( _groundChecker.position, _boxDimension * 2f );
    }

    #endregion

    #region Main Methods

    private void OnStateEnter()
    {
        switch( _currentState )
        {
            case PlayerState.IDLE:
                break;
            case PlayerState.JOGGING:
                _currentSpeed = _joggingSpeed;
                break;
            case PlayerState.RUNNING:
                _currentSpeed = _runningSpeed;
                break;
            case PlayerState.JUMPING:
                _isJumping = true;
                break;
            case PlayerState.FALLING:
                break;
            case PlayerState.SNEAKING:
                _currentSpeed = _sneakingSpeed;
                break;
            default:
                break;
        }

    }

    private void OnStateUpdate()
    {
        switch( _currentState )
        {
            case PlayerState.IDLE:
                Move();

                if( _direction.magnitude > 0 )
                {
                    if( Input.GetButton( "Fire3" ) )
                    {
                        TransitionToState( PlayerState.RUNNING );
                    }else if( Input.GetButton( "Fire2" ) )
                    {
                        TransitionToState( PlayerState.SNEAKING );
                    }
                    else
                    {
                        TransitionToState( PlayerState.JOGGING );
                    }
                }
                else if( Input.GetButtonDown( "Jump" ) ) 
                {
                    TransitionToState( PlayerState.JUMPING );
                }
                else if( !_isGrounded )
                {
                    TransitionToState( PlayerState.FALLING );
                }

                break;
            case PlayerState.JOGGING:
                Move();

                if( _direction.magnitude == 0 )
                {
                    TransitionToState( PlayerState.IDLE );
                }
                else if( Input.GetButtonDown( "Jump" ) )
                {
                    TransitionToState( PlayerState.JUMPING );
                }
                else if( !_isGrounded )
                {
                    TransitionToState( PlayerState.FALLING );
                }
                else if( Input.GetButton( "Fire3" ) )
                {
                    TransitionToState( PlayerState.RUNNING );
                }
                else if( Input.GetButton( "Fire2" ) )
                {
                    TransitionToState( PlayerState.SNEAKING );
                }

                break;
            case PlayerState.RUNNING:
                Move();

                if( Input.GetButtonDown( "Jump" ) )
                {
                    TransitionToState( PlayerState.JUMPING );
                }
                else if( !_isGrounded )
                {
                    TransitionToState( PlayerState.FALLING );
                }
                else if( !Input.GetButton( "Fire3" ) )
                {
                    TransitionToState( PlayerState.JOGGING );
                }

                break;
            case PlayerState.JUMPING:
                Move();

                if( _rigidbody.velocity.y < -0.2f && !_isGrounded )
                {
                    TransitionToState( PlayerState.FALLING );
                }

                break;
            case PlayerState.FALLING:
                Move();

                if( _isGrounded )
                {
                    TransitionToState( PlayerState.IDLE );
                }

                break;
            case PlayerState.SNEAKING:
                Move();

                if( Input.GetButtonDown( "Jump" ) )
                {
                    TransitionToState( PlayerState.JUMPING );
                }else if( Input.GetButton( "Fire3" ) )
                {
                    TransitionToState( PlayerState.RUNNING );
                }else if( !Input.GetButton( "Fire2" ) )
                {
                    TransitionToState( PlayerState.JOGGING );
                }
                else if( !_isGrounded )
                {
                    TransitionToState( PlayerState.FALLING );
                }
                break;
            default:
                break;
        }
    }

    private void OnStateExit()
    {
        switch( _currentState )
        {
            case PlayerState.IDLE:
                break;
            case PlayerState.JOGGING:
                break;
            case PlayerState.RUNNING:
                break;
            case PlayerState.JUMPING:
                break;
            case PlayerState.FALLING:
                break;
            case PlayerState.SNEAKING:
                break;
            default:
                break;
        }
    }

    private void TransitionToState( PlayerState nextState )
    {
        OnStateExit();
        _currentState = nextState;
        OnStateEnter();
    }

    private void Move() 
    {
                    //Deplacement AVANT / ARRIERE                     
        _direction = (_cameraTransform.forward * Input.GetAxis( "Vertical" )
                    //Deplacement Droite / GAUCHE
                    + _cameraTransform.right * Input.GetAxis( "Horizontal" )) * _currentSpeed;
        _direction.y = 0;
    }

    private void StickToGround()
    {
        Vector3 averagePosition = _floorDetector.AverageHeight();

        Vector3 newPosition = new Vector3( _rigidbody.position.x, averagePosition.y + _yFloorOffset, _rigidbody.position.z );
        _rigidbody.MovePosition( newPosition );
        _direction.y = 0;
    }

    private void RotateTowardsCamera()
    {
        Vector3 cameraForward = _cameraTransform.forward;
        cameraForward.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation( cameraForward );
        //Optionelle
        Quaternion rotation = Quaternion.RotateTowards( _rigidbody.rotation, lookRotation, _turnSpeed * Time.fixedDeltaTime );
        _rigidbody.MoveRotation( rotation );
    }

    private void OnGUI()
    {
        if( GUI.Button( new Rect( 10, 10, 80, 20 ), _currentState.ToString() ) )
        {
            TransitionToState( PlayerState.IDLE );
        }
    }

    #endregion

    #region Privates & Protected

    private Rigidbody _rigidbody;
    private Transform _cameraTransform;
    private FloorDetector _floorDetector;

    private PlayerState _currentState;
    private Vector3 _direction;
    private float _currentSpeed;

    private bool _isJumping = false;
    private bool _isGrounded = true;

    #endregion

}