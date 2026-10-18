using Benchmarking;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		public PlayerManager CameraManager;
		
		private bool m_IgnoreInput;
		private Joystick m_MoveJoystick;
		private Vector2 m_DeviceMove;
		private bool m_UsingOnScreenMove;
		private bool m_DefaultAnalogMovement;
		private int m_JoystickSearchFrames;

		private static bool m_FocusActionsSetUp;

		private void Start()
		{
			m_DefaultAnalogMovement = analogMovement;

			if (SystemInfo.deviceType == DeviceType.Handheld)
			{
				cursorLocked = false;
				SetCursorState(false);
			}

			TryBindMoveJoystick();

			if (!m_FocusActionsSetUp)
			{
#if UNITY_EDITOR
				var ignoreInput = new InputAction(binding: "/Keyboard/escape");
				ignoreInput.performed += context => m_IgnoreInput = true;
				ignoreInput.Enable();

				var enableInput = new InputAction(binding: "/Mouse/leftButton");
				enableInput.performed += context => m_IgnoreInput = false;
				enableInput.Enable();
#endif
				
				var touchFocus = new InputAction(binding: "<pointer>/press");
				touchFocus.performed += context => CameraManager.NotifyPlayerMoved();
				touchFocus.Enable();
				
				m_FocusActionsSetUp = true;
			}
		}

		private void Update()
		{
			if (m_MoveJoystick == null && m_JoystickSearchFrames < 30)
			{
				m_JoystickSearchFrames++;
				TryBindMoveJoystick();
			}

			ApplyOnScreenMove();
		}

		private void TryBindMoveJoystick()
		{
			GameObject joystickRoot = GameObject.Find("Joystick");
			if (joystickRoot != null)
				m_MoveJoystick = joystickRoot.GetComponentInChildren<Joystick>(true);

			if (m_MoveJoystick == null)
				m_MoveJoystick = FindObjectOfType<Joystick>();

			if (m_MoveJoystick == null)
				return;

			// Overlay sticks cannot receive pointer events while the cursor is locked.
			cursorLocked = false;
			SetCursorState(false);
		}

		private void ApplyOnScreenMove()
		{
			if (m_MoveJoystick == null)
				return;

			Vector2 stick = m_MoveJoystick.Direction;
			if (stick.sqrMagnitude > 0.0001f)
			{
				m_UsingOnScreenMove = true;
				analogMovement = true;
				if (CameraManager != null)
					CameraManager.NotifyPlayerMoved();
				MoveInput(stick);
				return;
			}

			if (m_UsingOnScreenMove)
			{
				m_UsingOnScreenMove = false;
				analogMovement = m_DefaultAnalogMovement;
				MoveInput(m_DeviceMove);
			}
		}

		private void OnDestroy()
		{
			m_FocusActionsSetUp = false;
		}


#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			m_DeviceMove = m_IgnoreInput ? Vector2.zero : value.Get<Vector2>();

			if (m_UsingOnScreenMove)
				return;

			if (m_IgnoreInput)
			{
				MoveInput(Vector2.zero);
				return;
			}
			
			if (CameraManager != null)
			{
				CameraManager.NotifyPlayerMoved();
			}
			MoveInput(m_DeviceMove);
		}

		public void OnLook(InputValue value)
		{
			if (m_IgnoreInput)
			{
				LookInput(Vector2.zero);
				return;
			}
			
			if (CameraManager != null)
			{
				CameraManager.NotifyPlayerMoved();
			}
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			if (CameraManager != null)
			{
				CameraManager.NotifyPlayerMoved();
			}
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			if (CameraManager != null)
			{
				CameraManager.NotifyPlayerMoved();
			}
			SprintInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{ 
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
			m_IgnoreInput = !hasFocus;
		}

		private void SetCursorState(bool newState)
		{
			if (PerformanceTest.RunningBenchmark)
				return;

			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}