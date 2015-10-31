using UnityEngine;
using System.Collections;

public class firstPersonController : MonoBehaviour, IPausable
{

    // Use this for initialization

    CharacterController _controller;

    float _mouseXAxis;
    float _mouseYAxis;
    Vector3 _mouseInitial;

    Vector3 _motionVec;

    [SerializeField]
    float _mouseAcceleration;
    [SerializeField]
    float _walkSpeed;
    [SerializeField]
    GameObject _centerObject;
    [SerializeField]
    Camera _playerCamera;
    [SerializeField]
    GameObject _playerRifle;
    [SerializeField]
    CharacterController _mainController;
    [SerializeField]
    float _jumpPower;
    [SerializeField]
    float _gravityMultiplier;
    [SerializeField]
    float _friction;
    [SerializeField]
    float _crouchDrag;
    [SerializeField]
    [Range(0.1f, 1f)]
    float _crouchHeight;
    [SerializeField]
    Vector3 _cameraOffset;
	[SerializeField]
	float _minimumYRotation = -90F;
	[SerializeField]
	float _maximumYRotation = 90F;

	[SerializeField]
	AudioClip _walkAudio;
	[SerializeField]
	AudioClip _jumpAudio;
	[SerializeField]
	AudioClip _landAudio;

	[SerializeField]
	float _landAudioTimeClamp;

	float _yRotation = 0f;

	bool _isWalking;
	bool _lastGrounded;

    KeyCode _forwardKey = PersistantSettings.keyBindings[PersistantSettings.KeyLabels.ForwardKey];
	KeyCode _backKey = PersistantSettings.keyBindings[PersistantSettings.KeyLabels.BackKey];
	KeyCode _leftKey = PersistantSettings.keyBindings[PersistantSettings.KeyLabels.LeftKey];
	KeyCode _rightKey = PersistantSettings.keyBindings[PersistantSettings.KeyLabels.RightKey];
	KeyCode _jumpKey = PersistantSettings.keyBindings[PersistantSettings.KeyLabels.JumpKey];
	KeyCode _crouchKey = PersistantSettings.keyBindings[PersistantSettings.KeyLabels.CrouchKey];

    bool _isPaused;
    bool _isCrouching;

	bool _crouchIsToggle = PersistantSettings.crouchIsToggle;

    Vector3 tmpPos;
    //Vector3 tmpLocation;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _motionVec = Vector3.zero;
        _isCrouching = false;
		_lastGrounded = true;
        Cursor.lockState = CursorLockMode.Locked;
        // Hide cursor when locking
        Cursor.visible = false;
        //Vector3 _mouseInitial = Input.GetAxis("X Axis")
    }

    // Update is called once per frame
	void Update () {
        if (!_isPaused) {
            Controls();
            Movement();
        }
	}

    public void OnPause()
    {
        _isPaused = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnResume()
    {
        _isPaused = false;
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Movement()
    {
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, _controller.radius, Vector3.up, out hitInfo, 1f);
        if (_isCrouching)
        {
            _controller.height = 2 * _crouchHeight;
            _playerCamera.transform.position = _controller.transform.position + _cameraOffset - new Vector3(0, _crouchHeight, 0);
            //transform.localScale = new Vector3(1, _crouchHeight, 1);
        }
        if ((!_isCrouching) && (hitInfo.collider == null))
        {
            _controller.height = 2f;
            _playerCamera.transform.position = _controller.transform.position + _cameraOffset;
            //transform.localScale = new Vector3(1, 1, 1);
        }
        _mouseXAxis = Input.GetAxis("Mouse X");
        _mouseYAxis = Input.GetAxis("Mouse Y");

		//float _yRotation = _playerCamera.transform.localEulerAngles.y;
		_yRotation += _mouseYAxis * _mouseAcceleration;
		_yRotation = Mathf.Clamp(_yRotation, _minimumYRotation, _maximumYRotation);

		_playerCamera.transform.localEulerAngles = new Vector3(-_yRotation, 0, 0);
		//_playerCamera.transform.rotation = Quaternion.Lerp()
		//_playerCamera.transform.Rotate(-_mouseYAxis * _mouseAcceleration, 0, 0);		
        _controller.transform.Rotate(0, _mouseXAxis * _mouseAcceleration, 0);


        _motionVec.z -= _motionVec.z * _friction;
        _motionVec.x -= _motionVec.x * _friction;

		if (!_controller.isGrounded)
		{
			_isWalking = false;
			_motionVec = _motionVec + Physics.gravity * _gravityMultiplier * Time.fixedDeltaTime;
		}
		
		
		if (_isWalking) {
			if ((!gameObject.GetComponent<AudioSource>().isPlaying) || ((gameObject.GetComponent<AudioSource>().clip == _landAudio) && (gameObject.GetComponent<AudioSource>().timeSamples >= _landAudioTimeClamp * (float)gameObject.GetComponent<AudioSource>().clip.frequency)))
			{
				gameObject.GetComponent<AudioSource>().clip = _walkAudio;
				gameObject.GetComponent<AudioSource>().loop = true;
				if ((gameObject.GetComponent<AudioSource>().clip == _walkAudio)&&(!gameObject.GetComponent<AudioSource>().isPlaying))
					gameObject.GetComponent<AudioSource>().Play();
		}
		}
		if (!_isWalking)
		{
			if (gameObject.GetComponent<AudioSource>().clip == _walkAudio) 
				gameObject.GetComponent<AudioSource>().Stop();
		}
		

        _controller.Move(transform.TransformVector(_motionVec) * Time.fixedDeltaTime);

		if ((_controller.isGrounded) && (!_lastGrounded))
		{
			_isWalking = false;
			gameObject.GetComponent<AudioSource>().Stop();
			gameObject.GetComponent<AudioSource>().clip = _landAudio;
			gameObject.GetComponent<AudioSource>().loop = false;
			gameObject.GetComponent<AudioSource>().Play();
		}
		//if (gameObject.GetComponent<AudioSource>().clip == _landAudio)
		//	Debug.Log((float)gameObject.GetComponent<AudioSource>().timeSamples / (float)gameObject.GetComponent<AudioSource>().clip.frequency);
		_lastGrounded = _controller.isGrounded;
    }

	float ClampAngle (float angle, float min, float max)
 {

	if (angle < -360F)
         angle += 360F;
     if (angle > 360F)
         angle -= 360F;
     return Mathf.Clamp (angle, min, max);
     //return Mathf.Clamp (angle, min, max);
 }

    void Controls()
    {
        //Debug.Log(Input.GetAxis("Mouse ScrollWheel") * 10);
        //_greyscale += Input.GetAxis("Mouse ScrollWheel") * 10
		_isWalking = false;
        if (_crouchIsToggle == true)
        {
            if (Input.GetKeyDown(_crouchKey))
            {
                _isCrouching = (_isCrouching) ? false : true;
            }
        }
        if (_crouchIsToggle == false)
        {
            _isCrouching = (Input.GetKey(_crouchKey)) ? true : false;
        }
        if (Input.GetKey(_forwardKey))
        {
            _motionVec.z = _walkSpeed;
			_isWalking = true;
        }
        if (Input.GetKey(_backKey))
        {
            _motionVec.z = -_walkSpeed;
			_isWalking = true;
        }
        if (Input.GetKey(_leftKey))
        {
            _motionVec.x = Vector3.left.x * _walkSpeed;
			_isWalking = true;
        }
        if (Input.GetKey(_rightKey))
        {
            _motionVec.x = Vector3.right.x * _walkSpeed;
			_isWalking = true;
        }
        if ((Input.GetKeyDown(_jumpKey)) && ((_controller.isGrounded)) && (!_isCrouching))
        {
            _isCrouching = false;
			_isWalking = false;
			_motionVec.y = Vector3.up.y * _jumpPower;
			gameObject.GetComponent<AudioSource>().Stop();
			gameObject.GetComponent<AudioSource>().clip = _jumpAudio;
			gameObject.GetComponent<AudioSource>().loop = false;
			gameObject.GetComponent<AudioSource>().Play();
        }
    }
}
