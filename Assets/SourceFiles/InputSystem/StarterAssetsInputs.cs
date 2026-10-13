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



		[Header("Input Control")]
		public bool isInputEnabled = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			if (isInputEnabled)
			{
				MoveInput(value.Get<Vector2>());
			}
			else
			{
				move = Vector2.zero;
			}
		}

		public void OnLook(InputValue value)
		{
			if (cursorInputForLook && isInputEnabled)
			{
				LookInput(value.Get<Vector2>());
			}
			else
			{
				look = Vector2.zero;
			}
		}

		public void OnJump(InputValue value)
		{
			if (isInputEnabled)
			{
				JumpInput(value.isPressed);
			}
			else
			{
				jump = false;
			}
		}

		public void OnSprint(InputValue value)
		{
			if (isInputEnabled)
			{
				SprintInput(value.isPressed);
			}
			else
			{
				sprint = false;
			}
		}
#endif

		private void Awake()
		{
			SetCursorState(cursorLocked);
		}

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = isInputEnabled ? newMoveDirection : Vector2.zero;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = isInputEnabled ? newLookDirection : Vector2.zero;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = isInputEnabled && newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = isInputEnabled && newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (hasFocus)
			{
				SetCursorState(cursorLocked);
			}
		}

		public void SetCursorState(bool newState)
		{
			cursorLocked = newState;
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !newState;  
		}
	}
}