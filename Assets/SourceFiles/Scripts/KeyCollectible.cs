using UnityEngine;
using Mazed.Core;

namespace Mazed.Gameplay
{
    public class KeyCollectible : MonoBehaviour
    {
        [Header("Key Properties")]
        [Tooltip("Unique identifier matching door requirements, e.g. SilverKey, GoldKey")]
        public string keyId = "SilverKey";

        [Tooltip("Player-friendly name shown in UI messages")]
        public string displayName = "Silver Key";

        [Tooltip("Points awarded for collecting this key")]
        public int scoreReward = 50;

        [Header("Visual Animation")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float bobbingSpeed = 2f;
        [SerializeField] private float bobbingAmount = 0.15f;

        [Header("Effects & Audio")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private AudioClip pickupSound;

        private Vector3 startPosition;
        private float timer;
        private bool isCollected = false;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            if (isCollected) return;

            // Continuous rotation
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            // Gentle bobbing motion
            timer += Time.deltaTime * bobbingSpeed;
            float newY = startPosition.y + Mathf.Sin(timer) * bobbingAmount;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);

            // Proximity check fallback so player always collects it reliably
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist <= 1.8f)
                {
                    Collect();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;

            if (IsPlayer(other))
            {
                Collect();
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

        public void Collect()
        {
            if (isCollected) return;
            isCollected = true;

            // Notify GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectKey(keyId, displayName, scoreReward);
            }

            // Spawn particle effect
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            // Play sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1f);
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayKeyCollectSFX();
            }

            // Deactivate and destroy
            gameObject.SetActive(false);
            Destroy(gameObject, 0.1f);
        }
    }
}
