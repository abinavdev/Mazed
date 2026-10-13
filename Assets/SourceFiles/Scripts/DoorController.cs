using System.Collections;
using UnityEngine;
using Mazed.Core;
using Mazed.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Mazed.Gameplay
{
    public class DoorController : MonoBehaviour
    {
        [Header("Door Identification")]
        public string doorId = "Door_1";
        public string doorName = "Security Gate A";

        [Header("Unlock Requirements")]
        [Tooltip("Required key ID in inventory (leave empty if none)")]
        public string requiredKeyId = "";

        [Tooltip("Required quiz checkpoint ID (leave empty if none)")]
        public string requiredQuizId = "";

        [Tooltip("Number of total quizzes required to unlock this door (0 if not applicable)")]
        public int requiredQuizCount = 0;

        [Header("Door State")]
        [SerializeField] private bool isLocked = true;
        [SerializeField] private bool isOpen = false;

        [Header("Opening Animation")]
        [Tooltip("Transform that moves or rotates when opening (defaults to this transform)")]
        [SerializeField] private Transform doorMeshTransform;

        [Tooltip("Vertical slide distance when opening")]
        [SerializeField] private float openSlideHeight = 4.5f;

        [Tooltip("Speed of opening animation")]
        [SerializeField] private float openSpeed = 2.5f;

        [Header("Visuals & Audio")]
        [SerializeField] private Renderer statusRenderer;
        [SerializeField] private Material lockedMaterial;
        [SerializeField] private Material unlockedMaterial;
        [SerializeField] private AudioClip unlockSound;

        private Vector3 closedPosition;
        private Vector3 targetOpenPosition;
        private Coroutine openCoroutine;
        private Collider doorCollider;
        private float lastPromptTime = -10f;

        public bool IsLocked => isLocked;
        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (doorMeshTransform == null)
            {
                doorMeshTransform = transform;
            }

            doorCollider = GetComponent<Collider>();
            if (doorCollider == null)
            {
                doorCollider = doorMeshTransform.GetComponent<Collider>();
            }

            closedPosition = doorMeshTransform.position;
            targetOpenPosition = closedPosition + Vector3.up * openSlideHeight;

            UpdateVisualStatus();
        }

        private void Update()
        {
            if (!isLocked || isOpen) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist <= 3.5f)
            {
                if (CanUnlock(out _))
                {
                    TryUnlockAndOpen();
                }
                else
                {
                    bool ePressed = false;
#if ENABLE_INPUT_SYSTEM
                    if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                    {
                        ePressed = true;
                    }
#endif
                    if (ePressed || (dist <= 2.2f && Time.time - lastPromptTime > 3.5f))
                    {
                        lastPromptTime = Time.time;
                        TryUnlockAndOpen();
                    }
                }
            }
        }

        public void TryUnlockAndOpen()
        {
            if (isOpen) return;

            // Check conditions
            if (CanUnlock(out string failureReason))
            {
                UnlockAndOpen();
            }
            else
            {
                // Display message to player
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMessage(failureReason, 3f);
                }
                Debug.Log($"[DoorController] {doorName} locked: {failureReason}");
            }
        }

        public bool CanUnlock(out string failureReason)
        {
            failureReason = string.Empty;

            // Check key requirement
            if (!string.IsNullOrEmpty(requiredKeyId))
            {
                if (GameManager.Instance == null || !GameManager.Instance.HasKey(requiredKeyId))
                {
                    failureReason = $"Locked! Requires {requiredKeyId} to open.";
                    return false;
                }
            }

            // Check specific quiz requirement
            if (!string.IsNullOrEmpty(requiredQuizId))
            {
                if (GameManager.Instance == null || !GameManager.Instance.IsQuizCompleted(requiredQuizId))
                {
                    failureReason = $"Locked! Pass quiz at {requiredQuizId} terminal first.";
                    return false;
                }
            }

            // Check total quiz count requirement
            if (requiredQuizCount > 0)
            {
                int completed = GameManager.Instance != null ? GameManager.Instance.CompletedQuizzesCount : 0;
                if (completed < requiredQuizCount)
                {
                    failureReason = $"Locked! Requires {requiredQuizCount} solved quizzes (You have {completed}).";
                    return false;
                }
            }

            return true;
        }

        public void UnlockAndOpen()
        {
            if (isOpen) return;

            isLocked = false;
            isOpen = true;

            // Make collider permeable immediately so player can pass through smoothly
            if (doorCollider != null)
            {
                doorCollider.isTrigger = true;
            }

            UpdateVisualStatus();

            if (openCoroutine != null) StopCoroutine(openCoroutine);
            openCoroutine = StartCoroutine(AnimateOpen());

            if (unlockSound != null)
            {
                AudioSource.PlayClipAtPoint(unlockSound, transform.position, 1f);
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDoorUnlockSFX();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage($"{doorName} unlocked and opened!", 2.5f);
            }
        }

        private IEnumerator AnimateOpen()
        {
            Vector3 startPos = doorMeshTransform.position;
            float progress = 0f;

            while (progress < 1f)
            {
                progress += Time.deltaTime * (openSpeed / openSlideHeight);
                doorMeshTransform.position = Vector3.Lerp(startPos, targetOpenPosition, Mathf.SmoothStep(0f, 1f, progress));
                yield return null;
            }

            doorMeshTransform.position = targetOpenPosition;

            if (doorCollider != null)
            {
                doorCollider.isTrigger = true;
            }
        }

        private void UpdateVisualStatus()
        {
            if (statusRenderer != null)
            {
                if (!isLocked && unlockedMaterial != null)
                {
                    statusRenderer.material = unlockedMaterial;
                }
                else if (isLocked && lockedMaterial != null)
                {
                    statusRenderer.material = lockedMaterial;
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

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider != null && IsPlayer(collision.collider))
            {
                if (isLocked)
                {
                    TryUnlockAndOpen();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayer(other))
            {
                if (isLocked)
                {
                    TryUnlockAndOpen();
                }
            }
        }
    }
}
