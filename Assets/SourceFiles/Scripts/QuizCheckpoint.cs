using UnityEngine;
using UnityEngine.Events;
using Mazed.Core;
using Mazed.Quiz;
using Mazed.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Mazed.Gameplay
{
    public class QuizCheckpoint : MonoBehaviour
    {
        [Header("Checkpoint Setup")]
        public string checkpointId = "Checkpoint_OOP";
        public string subject = "Object-Oriented Programming";

        [Header("Rewards & Progression")]
        [Tooltip("Door unlocked when this checkpoint quiz is correctly answered")]
        public DoorController linkedDoor;

        [Tooltip("Optional Key unlocked or activated upon completion")]
        public GameObject rewardObject;

        [Header("State")]
        [SerializeField] private bool isCompleted = false;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer terminalRenderer;
        [SerializeField] private Material pendingMaterial;
        [SerializeField] private Material completedMaterial;
        [SerializeField] private Light terminalLight;
        [SerializeField] private Color pendingLightColor = Color.cyan;
        [SerializeField] private Color completedLightColor = Color.green;

        [Header("Events")]
        public UnityEvent onCheckpointCompleted;

        private bool playerInRange = false;

        public bool IsCompleted => isCompleted;

        private void Start()
        {
            UpdateVisuals();
        }

        private void Update()
        {
            // Only activate when player is in range and explicitly presses [E]
            if (playerInRange && !isCompleted)
            {
                bool interactPressed = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    interactPressed = true;
                }
#elif ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactPressed = true;
                }
#endif
                if (interactPressed)
                {
                    ActivateCheckpoint();
                }
            }
        }

        private bool IsPlayer(Collider other)
        {
            if (other == null) return false;
            if (other.CompareTag("Player")) return true;
            if (other.transform.root != null && other.transform.root.CompareTag("Player")) return true;
            if (other.GetComponentInParent<CharacterController>() != null) return true;
            if (other.GetComponentInParent<StarterAssets.ThirdPersonController>() != null) return true;
            if (other.gameObject.name.IndexOf("Player", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayer(other))
            {
                playerInRange = true;

                if (!isCompleted)
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowInteractionPrompt("Press [E] to access Puzzle Terminal");
                    }
                    // NEVER directly open question automatically on enter! Only on [E] key.
                }
                else
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowMessage("Terminal already solved!", 2f);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsPlayer(other))
            {
                playerInRange = false;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.HideInteractionPrompt();
                }
            }
        }

        public void ActivateCheckpoint()
        {
            if (isCompleted) return;

            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.OpenQuizForCheckpoint(this);
            }
            else
            {
                Debug.LogError("[QuizCheckpoint] QuizManager.Instance is null!");
            }
        }

        public void MarkCompleted()
        {
            if (isCompleted) return;
            isCompleted = true;

            UpdateVisuals();

            // Unlock linked door if assigned
            if (linkedDoor != null)
            {
                linkedDoor.UnlockAndOpen();
            }

            // Activate reward if assigned
            if (rewardObject != null)
            {
                rewardObject.SetActive(true);
            }

            onCheckpointCompleted?.Invoke();
            Debug.Log($"[QuizCheckpoint] Checkpoint {checkpointId} solved!");
        }

        private void UpdateVisuals()
        {
            if (terminalRenderer != null)
            {
                if (isCompleted && completedMaterial != null)
                {
                    terminalRenderer.material = completedMaterial;
                }
                else if (!isCompleted && pendingMaterial != null)
                {
                    terminalRenderer.material = pendingMaterial;
                }
            }

            if (terminalLight != null)
            {
                terminalLight.color = isCompleted ? completedLightColor : pendingLightColor;
            }
        }
    }
}
