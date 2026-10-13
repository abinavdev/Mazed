using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mazed.Core;
using Mazed.UI;

namespace Mazed.Gameplay
{
    public class MazeExit : MonoBehaviour
    {
        [Header("Exit Door Requirements")]
        [Tooltip("Optional minimum quizzes if applicable (defaults to 0: completion is via key acquisition)")]
        public int requiredQuizzes = 0;

        [Tooltip("Required key ID to unlock the Exit Door, e.g. BronzeKey, SilverKey, MasterKey")]
        public string requiredKeyId = "";

        [Header("Door Visuals & Animation")]
        public Transform doorMeshTransform;
        public ParticleSystem portalParticles;
        public Light exitLight;
        public float slideHeight = 4.8f;
        public float openSpeed = 3.5f;

        private bool victoryTriggered = false;
        private float lastMessageTime = -10f;

        private void OnTriggerEnter(Collider other)
        {
            if (victoryTriggered) return;

            if (IsPlayer(other))
            {
                CheckExitConditions(showMessage: true);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (victoryTriggered) return;

            if (IsPlayer(other))
            {
                CheckExitConditions(showMessage: false);
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
            if (other.transform.root != null && other.transform.root.name.IndexOf("Player", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        public void CheckExitConditions(bool showMessage = true)
        {
            if (GameManager.Instance == null || victoryTriggered) return;

            string keyNeeded = requiredKeyId;
            if (GameManager.Instance.ActiveLevelInfo != null && !string.IsNullOrEmpty(GameManager.Instance.ActiveLevelInfo.requiredKeyId))
            {
                keyNeeded = GameManager.Instance.ActiveLevelInfo.requiredKeyId;
            }

            // Check if player has the required key to unlock the exit door
            if (!string.IsNullOrEmpty(keyNeeded) && !GameManager.Instance.HasKey(keyNeeded))
            {
                if (showMessage && (Time.unscaledTime - lastMessageTime > 3f))
                {
                    lastMessageTime = Time.unscaledTime;
                    string friendlyKey = keyNeeded.Replace("Key", " Key");
                    string msg = $"EXIT DOOR LOCKED! Find the {friendlyKey} hidden in the maze to escape!";
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowMessage(msg, 3.5f);
                    }
                    Debug.Log($"[MazeExit] {msg}");
                }
                return;
            }

            // Key acquired! Start door opening & exit sequence!
            StartCoroutine(PerformExitSequence());
        }

        private IEnumerator PerformExitSequence()
        {
            victoryTriggered = true;
            Debug.Log("[MazeExit] Exit Door unlocked with key! Level Completed!");

            if (exitLight != null)
            {
                exitLight.color = Color.green;
                exitLight.intensity = 5.5f;
            }

            if (portalParticles != null)
            {
                portalParticles.Play();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDoorUnlockSFX();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("KEY ACCEPTED! EXIT DOOR OPENING...", 2.5f);
            }

            // Smoothly slide 3D door panel upward
            if (doorMeshTransform != null)
            {
                Vector3 startPos = doorMeshTransform.position;
                Vector3 targetPos = startPos + Vector3.up * slideHeight;
                float progress = 0f;

                while (progress < 1f)
                {
                    progress += Time.unscaledDeltaTime * (openSpeed / slideHeight);
                    doorMeshTransform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, progress));
                    yield return null;
                }
                doorMeshTransform.position = targetPos;
            }

            yield return new WaitForSecondsRealtime(0.35f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }
        }
    }
}
