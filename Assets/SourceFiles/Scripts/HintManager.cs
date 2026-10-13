using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mazed.Core;
using Mazed.UI;
using Mazed.Gameplay;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Mazed.Gameplay
{
    public class HintManager : MonoBehaviour
    {
        public static HintManager Instance { get; private set; }

        [Header("Hint Inventory")]
        [SerializeField] private int currentHints = 1;
        [SerializeField] private int startingHints = 1;

        [Header("Statistics")]
        [SerializeField] private int hintsUsedThisAttempt = 0;

        [Header("Visual Effects")]
        [SerializeField] private GameObject activeBeacon;
        [SerializeField] private List<GameObject> activeTrailMarkers = new List<GameObject>();

        public int CurrentHints => currentHints;
        public int HintsUsedThisAttempt => hintsUsedThisAttempt;

        public event Action<int> OnHintsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            ResetForLevel(startingHints);
        }

        private void Update()
        {
            // Allow player to press [H] to trigger a hint
            if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
                {
                    UseHint();
                }
#elif ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKeyDown(KeyCode.H))
                {
                    UseHint();
                }
#endif
            }
        }

        public void ResetForLevel(int startingTokens = 1)
        {
            ClearActiveVisuals();
            hintsUsedThisAttempt = 0;
            currentHints = startingTokens;
            OnHintsChanged?.Invoke(currentHints);
        }

        public void AddHint(int amount = 1)
        {
            currentHints += amount;
            OnHintsChanged?.Invoke(currentHints);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage($"💡 +{amount} HINT TOKEN EARNED! Press [H] to use.", 3f);
            }
            Debug.Log($"[HintManager] Added {amount} hints. Total: {currentHints}");
        }

        public void UseHint()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

            if (currentHints <= 0)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMessage("NO HINTS REMAINING! Solve puzzle terminals in the maze to earn Hint Tokens!", 3.5f);
                }
                return;
            }

            // Consume 1 hint token
            currentHints--;
            hintsUsedThisAttempt++;
            OnHintsChanged?.Invoke(currentHints);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHintSFX();
            }

            ClearActiveVisuals();
            ExecuteIntelligentHint();
        }

        private void ExecuteIntelligentHint()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;

            string keyNeeded = GameManager.Instance.ActiveLevelInfo != null 
                ? GameManager.Instance.ActiveLevelInfo.requiredKeyId 
                : "";

            bool hasKey = string.IsNullOrEmpty(keyNeeded) || GameManager.Instance.HasKey(keyNeeded);

            // CASE 1: Player needs to find the key
            if (!hasKey)
            {
                KeyCollectible[] allKeys = FindObjectsByType<KeyCollectible>(FindObjectsSortMode.None);
                KeyCollectible targetKey = null;
                foreach (var k in allKeys)
                {
                    if (k != null && k.gameObject.activeInHierarchy && k.keyId.Equals(keyNeeded, StringComparison.OrdinalIgnoreCase))
                    {
                        targetKey = k;
                        break;
                    }
                }

                if (targetKey != null)
                {
                    Vector3 keyPos = targetKey.transform.position;
                    float dist = Vector3.Distance(playerPos, keyPos);
                    string dir = GetRelativeDirection(playerPos, keyPos);

                    SpawnBeacon(keyPos, Color.yellow, $"KEY BEACON ({targetKey.displayName})");
                    SpawnBreadcrumbTrail(playerPos, keyPos, Color.yellow);

                    string friendlyName = targetKey.displayName;
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowMessage($"💡 HINT: {friendlyName} is {dir} (~{Mathf.RoundToInt(dist)}m)! Follow the glowing waypoint beacon!", 4.5f);
                    }
                    return;
                }
            }

            // CASE 2: Player has the key, guide to Exit Doorway
            MazeExit exit = FindFirstObjectByType<MazeExit>();
            if (exit != null)
            {
                Vector3 exitPos = exit.transform.position;
                float dist = Vector3.Distance(playerPos, exitPos);
                string dir = GetRelativeDirection(playerPos, exitPos);

                SpawnBeacon(exitPos, Color.green, "EXIT DOOR BEACON");
                SpawnBreadcrumbTrail(playerPos, exitPos, Color.green);

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMessage($"💡 HINT: 3D Exit Door is {dir} (~{Mathf.RoundToInt(dist)}m)! Follow the emerald beacon to escape!", 4.5f);
                }
                return;
            }

            // Fallback general guidance
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("💡 HINT: Explore forward corridors and look for glowing security landmarks!", 3.5f);
            }
        }

        private void SpawnBeacon(Vector3 targetPos, Color color, string name)
        {
            activeBeacon = new GameObject(name);
            activeBeacon.transform.position = targetPos + Vector3.up * 1.5f;

            // Pulsing light
            Light beaconLight = activeBeacon.AddComponent<Light>();
            beaconLight.type = LightType.Point;
            beaconLight.color = color;
            beaconLight.range = 16f;
            beaconLight.intensity = 4f;

            // Visual vertical marker cylinder
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.transform.SetParent(activeBeacon.transform, false);
            pillar.transform.localPosition = new Vector3(0, 1.5f, 0);
            pillar.transform.localScale = new Vector3(0.4f, 2.5f, 0.4f);
            var col = pillar.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var ren = pillar.GetComponent<Renderer>();
            if (ren != null)
            {
                ren.material.color = color;
            }

            StartCoroutine(AutoClearBeaconAfter(12f));
        }

        private void SpawnBreadcrumbTrail(Vector3 fromPos, Vector3 toPos, Color color)
        {
            int steps = 6;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)(steps + 1);
                Vector3 markerPos = Vector3.Lerp(fromPos, toPos, t);
                markerPos.y = 0.6f;

                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = $"HintTrail_{i}";
                marker.transform.position = markerPos;
                marker.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
                var c = marker.GetComponent<Collider>();
                if (c != null) Destroy(c);

                var r = marker.GetComponent<Renderer>();
                if (r != null) r.material.color = color;

                Light l = marker.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = color;
                l.range = 3.5f;
                l.intensity = 1.2f;

                activeTrailMarkers.Add(marker);
            }

            StartCoroutine(AutoClearTrailAfter(8f));
        }

        private IEnumerator AutoClearBeaconAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (activeBeacon != null)
            {
                Destroy(activeBeacon);
                activeBeacon = null;
            }
        }

        private IEnumerator AutoClearTrailAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            foreach (var m in activeTrailMarkers)
            {
                if (m != null) Destroy(m);
            }
            activeTrailMarkers.Clear();
        }

        public void ClearActiveVisuals()
        {
            if (activeBeacon != null)
            {
                Destroy(activeBeacon);
                activeBeacon = null;
            }
            foreach (var m in activeTrailMarkers)
            {
                if (m != null) Destroy(m);
            }
            activeTrailMarkers.Clear();
        }

        private string GetRelativeDirection(Vector3 from, Vector3 to)
        {
            Vector3 dir = (to - from).normalized;
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            {
                return dir.x > 0 ? "East" : "West";
            }
            else
            {
                return dir.z > 0 ? "North" : "South";
            }
        }
    }
}
