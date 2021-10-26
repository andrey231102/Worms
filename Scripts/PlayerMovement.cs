using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _speedCoefficient;
    [SerializeField] private float _gravityCoefficient;
    [SerializeField] private LayerMask _layerMask;

    private float _direction;
    private float _horizontalSpeed;
    private bool _isOnGround;
    private Rigidbody2D _rigidBody;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _targetVelocity;
    private Vector2 _velocity;
    private Vector2 _groundNormal;
    private ContactFilter2D _contactFilter;
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D>(16);

    public float Direction => _direction;

    private void OnEnable()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _contactFilter.useTriggers = false;
        _contactFilter.SetLayerMask(_layerMask);
        _contactFilter.useLayerMask = true;
    }

    private void Update()
    {
        _direction = Input.GetAxis("Horizontal");
        _horizontalSpeed = _direction * _speedCoefficient;
        _targetVelocity = new Vector2(_horizontalSpeed, 0);

        if (Input.GetKey(KeyCode.Space) && _isOnGround && _horizontalSpeed == 0)
        {
            _velocity.y = 0;
           // _rigidBody.velocity = Vector2.zero;
            Jump(_jumpForce);
        }
    }

    private void FixedUpdate()
    {
        _velocity += _gravityCoefficient * Physics2D.gravity * Time.deltaTime;
        _velocity.x = _targetVelocity.x;

        _isOnGround = false;

        Vector2 deltaPosition = _velocity * Time.deltaTime;
        Vector2 moveAlongGround = new Vector2(_groundNormal.y, -_groundNormal.x);
        Vector2 move = moveAlongGround * deltaPosition.x;

        Movement(move,false);

        move = Vector2.up * deltaPosition.y;

        Movement(move, true);
    }

    private void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if (distance > .001f)
        {
            int count = _rigidBody.Cast(move, _contactFilter,_hitBuffer,distance);

            _hitBufferList.Clear();

            for (int i = 0; i < count; i++)
            {
                _hitBufferList.Add(_hitBuffer[i]);
            }

            for (int i = 0; i < _hitBufferList.Count; i++)
            {
                Vector2 currentNormal = _hitBuffer[i].normal;
                if (currentNormal.y > .65f)
                {
                    _isOnGround = true;
                    if (yMovement)
                    {
                        _groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot(_velocity, currentNormal);
                if (projection < 0)
                {
                    _velocity -= projection * currentNormal;
                }

                float modifiedDistance = _hitBufferList[i].distance;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }
        _rigidBody.position = _rigidBody.position + move.normalized * distance;
    }

    private void Jump(float jumpForce)
    {
        _rigidBody.AddForce(Vector2.one * jumpForce);
    }
}
