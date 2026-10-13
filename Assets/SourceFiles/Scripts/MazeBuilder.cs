using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using Mazed.Core;
using Mazed.Gameplay;
using Mazed.Quiz;
using Mazed.UI;

namespace Mazed.Environment
{
    [ExecuteAlways]
    public class MazeBuilder : MonoBehaviour
    {
        [Header("Materials")]
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material gateMaterial;
        [SerializeField] private Material terminalMaterial;
        [SerializeField] private Material completedMaterial;

        [Header("Prefabs & Resources")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject keyModelPrefab;

        [Header("Settings")]
        [SerializeField] private bool autoBuildOnStart = true;
        [SerializeField] private float wallHeight = 4f;

        private void Start()
        {
            if (Application.isPlaying && autoBuildOnStart)
            {
                InitializeEnvironment();
            }
        }

        [ContextMenu("Build Active Level Maze")]
        public void InitializeEnvironment()
        {
            CleanupLegacyEnvironment();
            LoadDefaultAssets();
            SetupManagers();
            SetupPlayer();
            SetupUI();

            DifficultyLevel targetDiff = LevelManager.Instance != null ? LevelManager.Instance.CurrentDifficulty : DifficultyLevel.Medium;
            int targetLevel = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevelNumber : 1;
            BuildLevel(targetDiff, targetLevel);

            Debug.Log($"[MazeBuilder] 3D Maze Environment successfully initialized for {targetDiff} Level {targetLevel}!");
        }

        private void CleanupLegacyEnvironment()
        {
            string[] legacyNames = new string[] { "Ground", "Secret_Area", "Tutorial", "Colliders", "Platform_Green_Floating", "Platform_Teal_Floating", "Hidden_Area_1", "Hidden_Area_2", "Hidden_Area_3", "Collectibles" };
            foreach (var name in legacyNames)
            {
                GameObject go = GameObject.Find(name);
                if (go != null)
                {
                    go.SetActive(false);
                }
            }

            var pickups = FindObjectsByType<Pickup>(FindObjectsSortMode.None);
            foreach (var p in pickups)
            {
                if (p != null && (p.transform.parent == null || !p.transform.IsChildOf(transform)))
                {
                    p.gameObject.SetActive(false);
                }
            }
        }

        private void LoadDefaultAssets()
        {
            if (floorMaterial == null)
                floorMaterial = Resources.Load<Material>("Materials/Material_Checkerboard") ?? LoadAssetAtPath<Material>("Assets/Materials/Material_Checkerboard.mat");

            if (wallMaterial == null)
                wallMaterial = Resources.Load<Material>("Materials/Material_Circuits") ?? LoadAssetAtPath<Material>("Assets/Materials/Material_Circuits.mat");

            if (gateMaterial == null)
                gateMaterial = Resources.Load<Material>("Materials/Material_HexagonPurple") ?? LoadAssetAtPath<Material>("Assets/Materials/Material_HexagonPurple.mat");

            if (terminalMaterial == null)
                terminalMaterial = Resources.Load<Material>("Materials/Material_HexagonBlue") ?? LoadAssetAtPath<Material>("Assets/Materials/Material_HexagonBlue.mat");

            if (completedMaterial == null)
                completedMaterial = Resources.Load<Material>("Materials/Material_GoldGlow") ?? LoadAssetAtPath<Material>("Assets/Materials/Material_GoldGlow.mat");

            if (playerPrefab == null)
                playerPrefab = LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerRobot.prefab");

            if (keyModelPrefab == null)
                keyModelPrefab = LoadAssetAtPath<GameObject>("Assets/Prefabs/Collectible_Star.prefab");
        }

        private T LoadAssetAtPath<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
#else
            return null;
#endif
        }

        public void BuildLevel(int levelNumber)
        {
            DifficultyLevel diff = LevelManager.Instance != null ? LevelManager.Instance.CurrentDifficulty : DifficultyLevel.Medium;
            BuildLevel(diff, levelNumber);
        }

        public void BuildLevel(DifficultyLevel diff, int levelNumber)
        {
            LoadDefaultAssets();

            // Clear previous geometry
            Transform existingMaze = transform.Find("MazeGeometry");
            if (existingMaze != null)
            {
                if (Application.isPlaying)
                    Destroy(existingMaze.gameObject);
                else
                    DestroyImmediate(existingMaze.gameObject);
            }

            GameObject mazeRoot = new GameObject("MazeGeometry");
            mazeRoot.transform.SetParent(transform, false);

            GameObject wallsParent = new GameObject("Walls");
            wallsParent.transform.SetParent(mazeRoot.transform, false);

            GameObject floorsParent = new GameObject("Floors");
            floorsParent.transform.SetParent(mazeRoot.transform, false);

            GameObject terminalsParent = new GameObject("Terminals");
            terminalsParent.transform.SetParent(mazeRoot.transform, false);

            GameObject doorsParent = new GameObject("Doors");
            doorsParent.transform.SetParent(mazeRoot.transform, false);

            GameObject itemsParent = new GameObject("Items");
            itemsParent.transform.SetParent(mazeRoot.transform, false);

            GameObject lightsParent = new GameObject("Lights");
            lightsParent.transform.SetParent(mazeRoot.transform, false);

            // Build appropriate level blueprint (1 to 10)
            switch (levelNumber)
            {
                case 1:
                    BuildLevel1(diff, mazeRoot.transform, wallsParent.transform, floorsParent.transform, terminalsParent.transform, doorsParent.transform, itemsParent.transform, lightsParent.transform);
                    break;
                case 2:
                    BuildLevel2(diff, mazeRoot.transform, wallsParent.transform, floorsParent.transform, terminalsParent.transform, doorsParent.transform, itemsParent.transform, lightsParent.transform);
                    break;
                case 3:
                    BuildLevel3(diff, mazeRoot.transform, wallsParent.transform, floorsParent.transform, terminalsParent.transform, doorsParent.transform, itemsParent.transform, lightsParent.transform);
                    break;
                case 4:
                    BuildLevel4(diff, mazeRoot.transform, wallsParent.transform, floorsParent.transform, terminalsParent.transform, doorsParent.transform, itemsParent.transform, lightsParent.transform);
                    break;
                case 5:
                    BuildLevel5(diff, mazeRoot.transform, wallsParent.transform, floorsParent.transform, terminalsParent.transform, doorsParent.transform, itemsParent.transform, lightsParent.transform);
                    break;
                case 6:
                    BuildLevel6(diff, mazeRoot.transform, wallsParent.transform, floorsParent.transform, terminalsParent.transform, doorsParent.transform, itemsParent.transform, lightsParent.transform);
                    break;
                case 7:
                    BuildLevel7(diff, mazeRoot.transform, wallsParent.transform, floorsParent.transform, terminalsParent.transform, doorsParent.transform, itemsParent.transform, lightsParent.transform);
                    break;
                case 8:
                    BuildLevel8(diff, mazeRoot.transform, wallsParent.transform, floorsParent.transform, terminalsParent.transform, doorsParent.transform, itemsParent.transform, lightsParent.transform);
                    break;
                case 9:
                    BuildLevel9(diff, mazeRoot.transform, wallsParent.transform, floorsParent.transform, terminalsParent.transform, doorsParent.transform, itemsParent.transform, lightsParent.transform);
                    break;
                case 10:
                default:
                    BuildLevel10(diff, mazeRoot.transform, wallsParent.transform, floorsParent.transform, terminalsParent.transform, doorsParent.transform, itemsParent.transform, lightsParent.transform);
                    break;
            }

            // Ensure player is at level spawn point
            RepositionPlayer(diff, levelNumber);
        }

        #region Level 1: Starter Circuit (Introductory Fundamentals)
        private void BuildLevel1(DifficultyLevel diff, Transform root, Transform walls, Transform floors, Transform terminals, Transform doors, Transform items, Transform lights)
        {
            float s = diff == DifficultyLevel.Easy ? 1.0f : (diff == DifficultyLevel.Medium ? 1.15f : 1.3f);
            CreateFloor(floors, new Vector3(0, 0, 10 * s), new Vector3(24 * s, 0.5f, 32 * s));

            // Perimeter walls
            CreateWall(walls, new Vector3(0, 2, -6 * s), new Vector3(25 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(0, 2, 26 * s), new Vector3(25 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-12.5f * s, 2, 10 * s), new Vector3(1, wallHeight, 33 * s));
            CreateWall(walls, new Vector3(12.5f * s, 2, 10 * s), new Vector3(1, wallHeight, 33 * s));

            // Corridors
            CreateWall(walls, new Vector3(-6 * s, 2, 2 * s), new Vector3(12 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(4 * s, 2, -1 * s), new Vector3(1, wallHeight, 8 * s));
            CreateWall(walls, new Vector3(6 * s, 2, 8 * s), new Vector3(12 * s, wallHeight, 1));

            // Security Gate
            DoorController gate = CreateGate(doors, "Gate_L1", "Bronze Security Gate", new Vector3(0, 2, 9.5f * s), new Vector3(4 * s, 4, 0.8f));
            gate.requiredKeyId = "BronzeKey";

            // Key (Hidden in west wing corridor)
            CreateKey(items, "BronzeKey", "Bronze Starter Key", new Vector3(-8 * s, 1.2f, -2 * s), new Color(0.8f, 0.5f, 0.2f));

            // Optional Bonus Puzzle Terminal (tucked safely in East alcove, only opens on [E])
            CreateTerminal(terminals, "CP_L1_Bonus", "Bonus Puzzle Alpha", new Vector3(8 * s, 0, 16 * s), null, new Color(0.2f, 0.8f, 1f));

            // 3D Exit Door
            CreateExitPortal(root, new Vector3(0, 0, 22 * s), 0, "BronzeKey");

            // Lights
            CreateLight(lights, new Vector3(0, 5, 0), new Color(0.7f, 0.9f, 1f), 1.6f, 18f);
            CreateLight(lights, new Vector3(0, 5, 16 * s), new Color(0.4f, 0.9f, 1f), 1.8f, 18f);
            CreateLight(lights, new Vector3(0, 5, 22 * s), new Color(0.3f, 1f, 0.6f), 2.2f, 20f);
        }
        #endregion

        #region Level 2: Logic Gates & Loops / Polymorphic Junction
        private void BuildLevel2(DifficultyLevel diff, Transform root, Transform walls, Transform floors, Transform terminals, Transform doors, Transform items, Transform lights)
        {
            float s = diff == DifficultyLevel.Easy ? 1.0f : (diff == DifficultyLevel.Medium ? 1.15f : 1.3f);
            CreateFloor(floors, new Vector3(0, 0, 15 * s), new Vector3(36 * s, 0.5f, 44 * s));

            // Perimeter walls
            CreateWall(walls, new Vector3(0, 2, -7 * s), new Vector3(37 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(0, 2, 37 * s), new Vector3(37 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-18.5f * s, 2, 15 * s), new Vector3(1, wallHeight, 45 * s));
            CreateWall(walls, new Vector3(18.5f * s, 2, 15 * s), new Vector3(1, wallHeight, 45 * s));

            // Central dividers (leaving wide corridors)
            CreateWall(walls, new Vector3(-6 * s, 2, -2 * s), new Vector3(1, wallHeight, 8 * s));
            CreateWall(walls, new Vector3(6 * s, 2, -2 * s), new Vector3(1, wallHeight, 8 * s));
            CreateWall(walls, new Vector3(-6 * s, 2, 13 * s), new Vector3(1, wallHeight, 12 * s));
            CreateWall(walls, new Vector3(6 * s, 2, 13 * s), new Vector3(1, wallHeight, 12 * s));

            // Wings dividers
            CreateWall(walls, new Vector3(-12 * s, 2, 0), new Vector3(10 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(12 * s, 2, 0), new Vector3(10 * s, wallHeight, 1));

            // North Chamber barrier
            CreateWall(walls, new Vector3(-10 * s, 2, 20 * s), new Vector3(16 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(10 * s, 2, 20 * s), new Vector3(16 * s, wallHeight, 1));

            // Central Gate
            DoorController gate = CreateGate(doors, "Gate_OOP", "OOP Encapsulation Gate", new Vector3(0, 2, 20 * s), new Vector3(4 * s, 4, 0.8f));
            gate.requiredKeyId = "SilverKey";

            // Silver Key & Bonus
            CreateKey(items, "SilverKey", "Silver Key", new Vector3(-12 * s, 1.2f, 14 * s), new Color(0.7f, 0.9f, 1f));
            CreateKey(items, "Bonus_Star_2", "Bonus Star", new Vector3(12 * s, 1.2f, 14 * s), new Color(0.9f, 0.4f, 1f));

            // Terminals (Mixed OOP & OS)
            CreateTerminal(terminals, "CP_OOP_1", "Object-Oriented Programming", new Vector3(-10 * s, 0, 5 * s), null, new Color(0.2f, 0.8f, 1f));
            CreateTerminal(terminals, "CP_OS_2", "Operating Systems", new Vector3(10 * s, 0, 5 * s), null, new Color(0.9f, 0.4f, 0.8f));

            // Exit Portal (requires 2 quizzes + SilverKey)
            CreateExitPortal(root, new Vector3(0, 0, 28 * s), 2, "SilverKey");

            // Lights
            CreateLight(lights, new Vector3(0, 5, 0), new Color(0.6f, 0.8f, 1f), 1.6f, 20f);
            CreateLight(lights, new Vector3(-10 * s, 5, 10 * s), new Color(0.4f, 0.9f, 1f), 1.8f, 20f);
            CreateLight(lights, new Vector3(10 * s, 5, 10 * s), new Color(0.9f, 0.5f, 1f), 1.8f, 20f);
            CreateLight(lights, new Vector3(0, 5, 28 * s), new Color(0.3f, 1f, 0.6f), 2.2f, 22f);
        }
        #endregion

        #region Level 3: The Class Hierarchy / Subnet Matrix / BGP Maze
        private void BuildLevel3(DifficultyLevel diff, Transform root, Transform walls, Transform floors, Transform terminals, Transform doors, Transform items, Transform lights)
        {
            float s = diff == DifficultyLevel.Easy ? 1.0f : (diff == DifficultyLevel.Medium ? 1.15f : 1.3f);
            CreateFloor(floors, new Vector3(0, 0, 19 * s), new Vector3(44 * s, 0.5f, 52 * s));

            // Perimeter walls
            CreateWall(walls, new Vector3(0, 2, -7 * s), new Vector3(45 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(0, 2, 45 * s), new Vector3(45 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-22.5f * s, 2, 19 * s), new Vector3(1, wallHeight, 53 * s));
            CreateWall(walls, new Vector3(22.5f * s, 2, 19 * s), new Vector3(1, wallHeight, 53 * s));

            // Kernel multi-route layout
            CreateWall(walls, new Vector3(-6 * s, 2, 4 * s), new Vector3(12 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(6 * s, 2, 4 * s), new Vector3(12 * s, wallHeight, 1));

            // Process wing (West) & Memory wing (East)
            CreateWall(walls, new Vector3(-12 * s, 2, 16 * s), new Vector3(1, wallHeight, 22 * s));
            CreateWall(walls, new Vector3(-18 * s, 2, 12 * s), new Vector3(8 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(12 * s, 2, 16 * s), new Vector3(1, wallHeight, 22 * s));
            CreateWall(walls, new Vector3(18 * s, 2, 12 * s), new Vector3(8 * s, wallHeight, 1));

            // Middle labyrinth
            CreateWall(walls, new Vector3(0, 2, 16 * s), new Vector3(14 * s, wallHeight, 1));

            // Sanctum entrance
            CreateWall(walls, new Vector3(-13 * s, 2, 33 * s), new Vector3(18 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(13 * s, 2, 33 * s), new Vector3(18 * s, wallHeight, 1));

            // Security Gate
            DoorController gate = CreateGate(doors, "Gate_Kernel_1", "Kernel Security Gate", new Vector3(0, 2, 33 * s), new Vector3(4 * s, 4, 0.8f));
            gate.requiredKeyId = "GoldKey";

            // Keys & Bonus
            CreateKey(items, "GoldKey", "Gold Key", new Vector3(18 * s, 1.2f, 18 * s), new Color(1f, 0.85f, 0.1f));

            // 3 Terminals (Mixed OS, Networks, DBMS)
            CreateTerminal(terminals, "CP_OS_Proc", "Operating Systems", new Vector3(-18 * s, 0, 6 * s), null, new Color(0.9f, 0.4f, 0.1f));
            CreateTerminal(terminals, "CP_NET_Subnet", "Computer Networks", new Vector3(18 * s, 0, 6 * s), null, new Color(0.2f, 0.9f, 0.9f));
            CreateTerminal(terminals, "CP_DBMS_Norm", "Database Management Systems", new Vector3(0, 0, 26 * s), null, new Color(0.3f, 0.9f, 0.5f));

            int reqQ = diff == DifficultyLevel.Easy ? 2 : (diff == DifficultyLevel.Medium ? 2 : 3);
            CreateExitPortal(root, new Vector3(0, 0, 40 * s), reqQ, "GoldKey");

            // Lights
            CreateLight(lights, new Vector3(0, 5, 0), new Color(0.8f, 0.8f, 1f), 1.6f, 20f);
            CreateLight(lights, new Vector3(-16 * s, 5, 16 * s), new Color(1f, 0.5f, 0.2f), 1.8f, 20f);
            CreateLight(lights, new Vector3(16 * s, 5, 16 * s), new Color(0.2f, 0.9f, 1f), 1.8f, 20f);
            CreateLight(lights, new Vector3(0, 5, 40 * s), new Color(0.3f, 1f, 0.6f), 2.2f, 22f);
        }
        #endregion

        #region Level 4: Process Pathways / Relational Normalizer / WAL Sanctum
        private void BuildLevel4(DifficultyLevel diff, Transform root, Transform walls, Transform floors, Transform terminals, Transform doors, Transform items, Transform lights)
        {
            float s = diff == DifficultyLevel.Easy ? 1.0f : (diff == DifficultyLevel.Medium ? 1.15f : 1.3f);
            CreateFloor(floors, new Vector3(0, 0, 22 * s), new Vector3(50 * s, 0.5f, 58 * s));

            // Perimeter walls
            CreateWall(walls, new Vector3(0, 2, -7 * s), new Vector3(51 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(0, 2, 51 * s), new Vector3(51 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-25.5f * s, 2, 22 * s), new Vector3(1, wallHeight, 59 * s));
            CreateWall(walls, new Vector3(25.5f * s, 2, 22 * s), new Vector3(1, wallHeight, 59 * s));

            // Central divider corridor
            CreateWall(walls, new Vector3(-2 * s, 2, 10 * s), new Vector3(1, wallHeight, 16 * s));
            CreateWall(walls, new Vector3(2 * s, 2, 10 * s), new Vector3(1, wallHeight, 16 * s));

            // West Wing (DBMS) & East Wing (Networks)
            CreateWall(walls, new Vector3(-14 * s, 2, 10 * s), new Vector3(12 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-14 * s, 2, 22 * s), new Vector3(12 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(14 * s, 2, 10 * s), new Vector3(12 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(14 * s, 2, 22 * s), new Vector3(12 * s, wallHeight, 1));

            // North Confluence
            CreateWall(walls, new Vector3(-15 * s, 2, 36 * s), new Vector3(20 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(15 * s, 2, 36 * s), new Vector3(20 * s, wallHeight, 1));

            // Gate
            DoorController gateSQL = CreateGate(doors, "Gate_SQL", "SQL Transaction Gate", new Vector3(0, 2, 36 * s), new Vector3(4 * s, 4, 0.8f));
            gateSQL.requiredKeyId = "CyanKey";

            // Cyan Key
            CreateKey(items, "CyanKey", "Cyan Key", new Vector3(-21 * s, 1.2f, 16 * s), new Color(0.2f, 0.9f, 0.9f));

            // 3 Terminals (Mixed DBMS, Networks, OOP)
            CreateTerminal(terminals, "CP_DBMS_1", "Database Management Systems", new Vector3(-14 * s, 0, 4 * s), null, new Color(0.3f, 0.9f, 0.5f));
            CreateTerminal(terminals, "CP_NET_1", "Computer Networks", new Vector3(14 * s, 0, 4 * s), null, new Color(0.7f, 0.3f, 0.9f));
            CreateTerminal(terminals, "CP_OOP_Overload", "Object-Oriented Programming", new Vector3(14 * s, 0, 28 * s), null, new Color(0.2f, 0.8f, 1f));

            int reqQ = diff == DifficultyLevel.Easy ? 2 : 3;
            CreateExitPortal(root, new Vector3(0, 0, 45 * s), reqQ, "CyanKey");

            // Lights
            CreateLight(lights, new Vector3(0, 5, 0), new Color(0.8f, 0.9f, 1f), 1.6f, 20f);
            CreateLight(lights, new Vector3(-14 * s, 5, 16 * s), new Color(0.3f, 0.9f, 0.5f), 1.8f, 20f);
            CreateLight(lights, new Vector3(14 * s, 5, 16 * s), new Color(0.7f, 0.3f, 0.9f), 1.8f, 20f);
            CreateLight(lights, new Vector3(0, 5, 45 * s), new Color(0.3f, 1f, 0.6f), 2.2f, 22f);
        }
        #endregion

        #region Level 5: Network Crossroads / Virtual Memory Vaults / Page Fault Catacomb
        private void BuildLevel5(DifficultyLevel diff, Transform root, Transform walls, Transform floors, Transform terminals, Transform doors, Transform items, Transform lights)
        {
            float s = diff == DifficultyLevel.Easy ? 1.0f : (diff == DifficultyLevel.Medium ? 1.15f : 1.3f);
            CreateFloor(floors, new Vector3(0, 0, 24 * s), new Vector3(52 * s, 0.5f, 62 * s));

            CreateWall(walls, new Vector3(0, 2, -7 * s), new Vector3(53 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(0, 2, 55 * s), new Vector3(53 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-26.5f * s, 2, 24 * s), new Vector3(1, wallHeight, 63 * s));
            CreateWall(walls, new Vector3(26.5f * s, 2, 24 * s), new Vector3(1, wallHeight, 63 * s));

            // H-Shape corridor architecture
            CreateWall(walls, new Vector3(-8 * s, 2, 12 * s), new Vector3(1, wallHeight, 26 * s));
            CreateWall(walls, new Vector3(8 * s, 2, 12 * s), new Vector3(1, wallHeight, 26 * s));
            CreateWall(walls, new Vector3(-18 * s, 2, 24 * s), new Vector3(12 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(18 * s, 2, 24 * s), new Vector3(12 * s, wallHeight, 1));

            // Cross barrier
            CreateWall(walls, new Vector3(-12 * s, 2, 40 * s), new Vector3(18 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(12 * s, 2, 40 * s), new Vector3(18 * s, wallHeight, 1));

            DoorController gate = CreateGate(doors, "Gate_L5", "Emerald Vault Gate", new Vector3(0, 2, 40 * s), new Vector3(4 * s, 4, 0.8f));
            gate.requiredKeyId = "EmeraldKey";

            CreateKey(items, "EmeraldKey", "Emerald Key", new Vector3(-20 * s, 1.2f, 10 * s), new Color(0.2f, 0.9f, 0.4f));

            // 3 Terminals (Mixed OS, Networks, DBMS)
            CreateTerminal(terminals, "CP_L5_OS", "Operating Systems", new Vector3(20 * s, 0, 10 * s), null, new Color(0.9f, 0.4f, 0.1f));
            CreateTerminal(terminals, "CP_L5_NET", "Computer Networks", new Vector3(0, 0, 26 * s), null, new Color(0.2f, 0.8f, 1f));
            CreateTerminal(terminals, "CP_L5_DBMS", "Database Management Systems", new Vector3(-18 * s, 0, 32 * s), null, new Color(0.3f, 0.9f, 0.5f));

            CreateExitPortal(root, new Vector3(0, 0, 48 * s), 3, "EmeraldKey");

            CreateLight(lights, new Vector3(0, 5, 0), new Color(0.8f, 1f, 0.8f), 1.6f, 22f);
            CreateLight(lights, new Vector3(0, 5, 48 * s), new Color(0.3f, 1f, 0.6f), 2.2f, 22f);
        }
        #endregion

        #region Level 6: Database Foundations / Transport Bastion / Packet Sniffer
        private void BuildLevel6(DifficultyLevel diff, Transform root, Transform walls, Transform floors, Transform terminals, Transform doors, Transform items, Transform lights)
        {
            float s = diff == DifficultyLevel.Easy ? 1.0f : (diff == DifficultyLevel.Medium ? 1.15f : 1.3f);
            CreateFloor(floors, new Vector3(0, 0, 26 * s), new Vector3(54 * s, 0.5f, 66 * s));

            CreateWall(walls, new Vector3(0, 2, -7 * s), new Vector3(55 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(0, 2, 59 * s), new Vector3(55 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-27.5f * s, 2, 26 * s), new Vector3(1, wallHeight, 67 * s));
            CreateWall(walls, new Vector3(27.5f * s, 2, 26 * s), new Vector3(1, wallHeight, 67 * s));

            // Outer ring & central core
            CreateWall(walls, new Vector3(-10 * s, 2, 16 * s), new Vector3(1, wallHeight, 30 * s));
            CreateWall(walls, new Vector3(10 * s, 2, 16 * s), new Vector3(1, wallHeight, 30 * s));
            CreateWall(walls, new Vector3(-18 * s, 2, 10 * s), new Vector3(10 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(18 * s, 2, 10 * s), new Vector3(10 * s, wallHeight, 1));

            // Barrier
            CreateWall(walls, new Vector3(-12 * s, 2, 44 * s), new Vector3(20 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(12 * s, 2, 44 * s), new Vector3(20 * s, wallHeight, 1));

            DoorController gate = CreateGate(doors, "Gate_L6", "Ruby Barrier", new Vector3(0, 2, 44 * s), new Vector3(4 * s, 4, 0.8f));
            gate.requiredKeyId = "RubyKey";

            CreateKey(items, "RubyKey", "Ruby Key", new Vector3(22 * s, 1.2f, 20 * s), new Color(0.9f, 0.2f, 0.3f));

            // 3 Terminals (Mixed DBMS, OOP, Networks)
            CreateTerminal(terminals, "CP_L6_DBMS", "Database Management Systems", new Vector3(-20 * s, 0, 18 * s), null, new Color(0.3f, 0.9f, 0.5f));
            CreateTerminal(terminals, "CP_L6_OOP", "Object-Oriented Programming", new Vector3(0, 0, 20 * s), null, new Color(0.2f, 0.8f, 1f));
            CreateTerminal(terminals, "CP_L6_NET", "Computer Networks", new Vector3(-20 * s, 0, 36 * s), null, new Color(0.8f, 0.4f, 1f));

            CreateExitPortal(root, new Vector3(0, 0, 52 * s), 3, "RubyKey");

            CreateLight(lights, new Vector3(0, 5, 0), new Color(1f, 0.8f, 0.8f), 1.6f, 22f);
            CreateLight(lights, new Vector3(0, 5, 52 * s), new Color(0.3f, 1f, 0.6f), 2.2f, 22f);
        }
        #endregion

        #region Level 7: Thread Symphony / Design Patterns / MVCC Labyrinth
        private void BuildLevel7(DifficultyLevel diff, Transform root, Transform walls, Transform floors, Transform terminals, Transform doors, Transform items, Transform lights)
        {
            float s = diff == DifficultyLevel.Easy ? 1.0f : (diff == DifficultyLevel.Medium ? 1.15f : 1.3f);
            CreateFloor(floors, new Vector3(0, 0, 28 * s), new Vector3(56 * s, 0.5f, 70 * s));

            CreateWall(walls, new Vector3(0, 2, -7 * s), new Vector3(57 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(0, 2, 63 * s), new Vector3(57 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-28.5f * s, 2, 28 * s), new Vector3(1, wallHeight, 71 * s));
            CreateWall(walls, new Vector3(28.5f * s, 2, 28 * s), new Vector3(1, wallHeight, 71 * s));

            // S-Curve dividers
            CreateWall(walls, new Vector3(-8 * s, 2, 14 * s), new Vector3(24 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(8 * s, 2, 28 * s), new Vector3(24 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-8 * s, 2, 42 * s), new Vector3(24 * s, wallHeight, 1));

            // Gate
            DoorController gate = CreateGate(doors, "Gate_L7", "Sapphire Gate", new Vector3(0, 2, 48 * s), new Vector3(4 * s, 4, 0.8f));
            gate.requiredKeyId = "SapphireKey";

            CreateKey(items, "SapphireKey", "Sapphire Key", new Vector3(20 * s, 1.2f, 36 * s), new Color(0.2f, 0.4f, 1f));

            // 3 Terminals (Mixed OS, OOP, Networks)
            CreateTerminal(terminals, "CP_L7_OS", "Operating Systems", new Vector3(-18 * s, 0, 6 * s), null, new Color(0.9f, 0.4f, 0.1f));
            CreateTerminal(terminals, "CP_L7_OOP", "Object-Oriented Programming", new Vector3(18 * s, 0, 20 * s), null, new Color(0.2f, 0.8f, 1f));
            CreateTerminal(terminals, "CP_L7_NET", "Computer Networks", new Vector3(-18 * s, 0, 36 * s), null, new Color(0.7f, 0.3f, 0.9f));

            CreateExitPortal(root, new Vector3(0, 0, 56 * s), 3, "SapphireKey");

            CreateLight(lights, new Vector3(0, 5, 0), new Color(0.8f, 0.9f, 1f), 1.6f, 22f);
            CreateLight(lights, new Vector3(0, 5, 56 * s), new Color(0.3f, 1f, 0.6f), 2.2f, 22f);
        }
        #endregion

        #region Level 8: Web Protocol Concourse / Concurrency & Locks / Microkernel
        private void BuildLevel8(DifficultyLevel diff, Transform root, Transform walls, Transform floors, Transform terminals, Transform doors, Transform items, Transform lights)
        {
            float s = diff == DifficultyLevel.Easy ? 1.0f : (diff == DifficultyLevel.Medium ? 1.15f : 1.3f);
            CreateFloor(floors, new Vector3(0, 0, 30 * s), new Vector3(58 * s, 0.5f, 74 * s));

            CreateWall(walls, new Vector3(0, 2, -7 * s), new Vector3(59 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(0, 2, 67 * s), new Vector3(59 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-29.5f * s, 2, 30 * s), new Vector3(1, wallHeight, 75 * s));
            CreateWall(walls, new Vector3(29.5f * s, 2, 30 * s), new Vector3(1, wallHeight, 75 * s));

            // Triple sector dividers
            CreateWall(walls, new Vector3(-12 * s, 2, 20 * s), new Vector3(1, wallHeight, 36 * s));
            CreateWall(walls, new Vector3(12 * s, 2, 20 * s), new Vector3(1, wallHeight, 36 * s));
            CreateWall(walls, new Vector3(-18 * s, 2, 36 * s), new Vector3(14 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(18 * s, 2, 36 * s), new Vector3(14 * s, wallHeight, 1));

            DoorController gate = CreateGate(doors, "Gate_L8", "Amethyst Gate", new Vector3(0, 2, 52 * s), new Vector3(4 * s, 4, 0.8f));
            gate.requiredKeyId = "AmethystKey";

            CreateKey(items, "AmethystKey", "Amethyst Key", new Vector3(-22 * s, 1.2f, 26 * s), new Color(0.7f, 0.2f, 0.9f));

            // 3-4 Terminals (Mixed across all 4 subjects)
            CreateTerminal(terminals, "CP_L8_NET", "Computer Networks", new Vector3(22 * s, 0, 10 * s), null, new Color(0.7f, 0.3f, 0.9f));
            CreateTerminal(terminals, "CP_L8_OS", "Operating Systems", new Vector3(0, 0, 22 * s), null, new Color(0.9f, 0.4f, 0.1f));
            CreateTerminal(terminals, "CP_L8_DBMS", "Database Management Systems", new Vector3(22 * s, 0, 44 * s), null, new Color(0.3f, 0.9f, 0.5f));
            if (diff != DifficultyLevel.Easy)
            {
                CreateTerminal(terminals, "CP_L8_OOP", "Object-Oriented Programming", new Vector3(-22 * s, 0, 44 * s), null, new Color(0.2f, 0.8f, 1f));
            }

            int reqQ = diff == DifficultyLevel.Hard ? 4 : 3;
            CreateExitPortal(root, new Vector3(0, 0, 60 * s), reqQ, "AmethystKey");

            CreateLight(lights, new Vector3(0, 5, 0), new Color(0.9f, 0.8f, 1f), 1.6f, 24f);
            CreateLight(lights, new Vector3(0, 5, 60 * s), new Color(0.3f, 1f, 0.6f), 2.2f, 24f);
        }
        #endregion

        #region Level 9: ACID Citadel / Distributed Gateway / High-Availability Fortress
        private void BuildLevel9(DifficultyLevel diff, Transform root, Transform walls, Transform floors, Transform terminals, Transform doors, Transform items, Transform lights)
        {
            float s = diff == DifficultyLevel.Easy ? 1.0f : (diff == DifficultyLevel.Medium ? 1.15f : 1.3f);
            CreateFloor(floors, new Vector3(0, 0, 32 * s), new Vector3(60 * s, 0.5f, 78 * s));

            CreateWall(walls, new Vector3(0, 2, -7 * s), new Vector3(61 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(0, 2, 71 * s), new Vector3(61 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-30.5f * s, 2, 32 * s), new Vector3(1, wallHeight, 79 * s));
            CreateWall(walls, new Vector3(30.5f * s, 2, 32 * s), new Vector3(1, wallHeight, 79 * s));

            // Grid Matrix
            CreateWall(walls, new Vector3(-12 * s, 2, 16 * s), new Vector3(1, wallHeight, 28 * s));
            CreateWall(walls, new Vector3(12 * s, 2, 16 * s), new Vector3(1, wallHeight, 28 * s));
            CreateWall(walls, new Vector3(0, 2, 32 * s), new Vector3(22 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-15 * s, 2, 48 * s), new Vector3(18 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(15 * s, 2, 48 * s), new Vector3(18 * s, wallHeight, 1));

            DoorController gate = CreateGate(doors, "Gate_L9", "Diamond Vault Gate", new Vector3(0, 2, 54 * s), new Vector3(4 * s, 4, 0.8f));
            gate.requiredKeyId = "DiamondKey";

            CreateKey(items, "DiamondKey", "Diamond Key", new Vector3(22 * s, 1.2f, 40 * s), new Color(0.9f, 0.95f, 1f));

            // 4 Terminals (All 4 core subjects covered!)
            CreateTerminal(terminals, "CP_L9_DBMS", "Database Management Systems", new Vector3(-22 * s, 0, 10 * s), null, new Color(0.3f, 0.9f, 0.5f));
            CreateTerminal(terminals, "CP_L9_NET", "Computer Networks", new Vector3(22 * s, 0, 10 * s), null, new Color(0.7f, 0.3f, 0.9f));
            CreateTerminal(terminals, "CP_L9_OS", "Operating Systems", new Vector3(-22 * s, 0, 40 * s), null, new Color(0.9f, 0.4f, 0.1f));
            CreateTerminal(terminals, "CP_L9_OOP", "Object-Oriented Programming", new Vector3(0, 0, 40 * s), null, new Color(0.2f, 0.8f, 1f));

            int reqQ = diff == DifficultyLevel.Easy ? 3 : 4;
            CreateExitPortal(root, new Vector3(0, 0, 64 * s), reqQ, "DiamondKey");

            CreateLight(lights, new Vector3(0, 5, 0), new Color(0.9f, 0.95f, 1f), 1.6f, 24f);
            CreateLight(lights, new Vector3(0, 5, 64 * s), new Color(0.3f, 1f, 0.6f), 2.2f, 24f);
        }
        #endregion

        #region Level 10: Grand Citadel of Computing (10th Capstone Level)
        private void BuildLevel10(DifficultyLevel diff, Transform root, Transform walls, Transform floors, Transform terminals, Transform doors, Transform items, Transform lights)
        {
            float s = diff == DifficultyLevel.Easy ? 1.0f : (diff == DifficultyLevel.Medium ? 1.15f : 1.3f);
            CreateFloor(floors, new Vector3(0, 0, 36 * s), new Vector3(64 * s, 0.5f, 86 * s));

            CreateWall(walls, new Vector3(0, 2, -7 * s), new Vector3(65 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(0, 2, 79 * s), new Vector3(65 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-32.5f * s, 2, 36 * s), new Vector3(1, wallHeight, 87 * s));
            CreateWall(walls, new Vector3(32.5f * s, 2, 36 * s), new Vector3(1, wallHeight, 87 * s));

            // Grand Citadel Departmental Wings
            CreateWall(walls, new Vector3(-10 * s, 2, 8 * s), new Vector3(18 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(10 * s, 2, 8 * s), new Vector3(18 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-18 * s, 2, 24 * s), new Vector3(1, wallHeight, 30 * s));
            CreateWall(walls, new Vector3(18 * s, 2, 24 * s), new Vector3(1, wallHeight, 30 * s));
            CreateWall(walls, new Vector3(-10 * s, 2, 40 * s), new Vector3(16 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(10 * s, 2, 40 * s), new Vector3(16 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(-12 * s, 2, 58 * s), new Vector3(20 * s, wallHeight, 1));
            CreateWall(walls, new Vector3(12 * s, 2, 58 * s), new Vector3(20 * s, wallHeight, 1));

            // Grand Master Gate
            DoorController masterGate = CreateGate(doors, "Gate_FinalCitadel", "Grand Citadel Master Portal", new Vector3(0, 2, 58 * s), new Vector3(4 * s, 4, 0.8f));
            masterGate.requiredKeyId = "MasterKey";

            CreateKey(items, "MasterKey", "Master IT Key", new Vector3(24 * s, 1.2f, 48 * s), new Color(1f, 0.85f, 0.1f));

            // 4-5 Checkpoints covering all computing disciplines
            CreateTerminal(terminals, "CP_L10_OOP", "Object-Oriented Programming", new Vector3(-24 * s, 0, 16 * s), null, new Color(0.2f, 0.8f, 1f));
            CreateTerminal(terminals, "CP_L10_OS", "Operating Systems", new Vector3(24 * s, 0, 16 * s), null, new Color(0.9f, 0.4f, 0.1f));
            CreateTerminal(terminals, "CP_L10_NET", "Computer Networks", new Vector3(0, 0, 30 * s), null, new Color(0.7f, 0.3f, 0.9f));
            CreateTerminal(terminals, "CP_L10_DBMS", "Database Management Systems", new Vector3(-24 * s, 0, 48 * s), null, new Color(0.3f, 0.9f, 0.5f));
            if (diff == DifficultyLevel.Hard)
            {
                CreateTerminal(terminals, "CP_L10_SE", "Software Engineering", new Vector3(0, 0, 48 * s), null, new Color(0.9f, 0.2f, 0.5f));
            }

            int reqQ = diff == DifficultyLevel.Easy ? 4 : (diff == DifficultyLevel.Medium ? 4 : 5);
            CreateExitPortal(root, new Vector3(0, 0, 70 * s), reqQ, "MasterKey");

            CreateLight(lights, new Vector3(0, 5, 0), new Color(1f, 0.95f, 0.7f), 1.8f, 26f);
            CreateLight(lights, new Vector3(0, 5, 70 * s), new Color(0.3f, 1f, 0.6f), 2.5f, 28f);
        }
        #endregion

        #region Helper Geometry Builders
        private void CreateFloor(Transform parent, Vector3 pos, Vector3 size)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(parent, false);
            floor.transform.position = pos;
            floor.transform.localScale = size;

            if (floorMaterial != null)
            {
                floor.GetComponent<Renderer>().material = floorMaterial;
            }
        }

        private void CreateWall(Transform parent, Vector3 pos, Vector3 size)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "WallSection";
            wall.transform.SetParent(parent, false);
            wall.transform.position = pos;
            wall.transform.localScale = size;

            if (wallMaterial != null)
            {
                wall.GetComponent<Renderer>().material = wallMaterial;
            }
        }

        private DoorController CreateGate(Transform parent, string doorId, string doorName, Vector3 pos, Vector3 size)
        {
            GameObject gateGO = new GameObject(doorId);
            gateGO.transform.SetParent(parent, false);
            gateGO.transform.position = pos;

            GameObject meshGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshGO.name = "GateMesh";
            meshGO.transform.SetParent(gateGO.transform, false);
            meshGO.transform.localScale = size;

            Collider childCol = meshGO.GetComponent<Collider>();
            if (childCol != null) DestroyImmediate(childCol);

            if (gateMaterial != null)
            {
                meshGO.GetComponent<Renderer>().material = gateMaterial;
            }

            BoxCollider col = gateGO.AddComponent<BoxCollider>();
            col.size = size;

            GameObject trigGO = new GameObject("GateTriggerZone");
            trigGO.transform.SetParent(gateGO.transform, false);
            BoxCollider trigCol = trigGO.AddComponent<BoxCollider>();
            trigCol.isTrigger = true;
            trigCol.size = new Vector3(size.x + 1.2f, size.y + 1.2f, size.z + 3.2f);

            DoorController door = gateGO.AddComponent<DoorController>();
            door.doorId = doorId;
            door.doorName = doorName;

            SetPrivateField(door, "doorMeshTransform", meshGO.transform);
            SetPrivateField(door, "statusRenderer", meshGO.GetComponent<Renderer>());
            SetPrivateField(door, "lockedMaterial", gateMaterial);
            SetPrivateField(door, "unlockedMaterial", completedMaterial);

            return door;
        }

        private void CreateTerminal(Transform parent, string id, string subject, Vector3 pos, DoorController linkedDoor, Color color)
        {
            GameObject terminalGO = new GameObject(id);
            terminalGO.transform.SetParent(parent, false);
            terminalGO.transform.position = pos;

            // Pedestal base
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(terminalGO.transform, false);
            pedestal.transform.localPosition = new Vector3(0, 0.6f, 0);
            pedestal.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f);

            // Screen top
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "TerminalScreen";
            screen.transform.SetParent(terminalGO.transform, false);
            screen.transform.localPosition = new Vector3(0, 1.6f, 0);
            screen.transform.localScale = new Vector3(0.9f, 0.7f, 0.2f);
            screen.transform.localRotation = Quaternion.Euler(20, 0, 0);

            if (terminalMaterial != null)
            {
                pedestal.GetComponent<Renderer>().material = terminalMaterial;
                screen.GetComponent<Renderer>().material = terminalMaterial;
            }

            // Light
            GameObject lightGO = new GameObject("TerminalLight");
            lightGO.transform.SetParent(terminalGO.transform, false);
            lightGO.transform.localPosition = new Vector3(0, 2f, 0);
            Light tLight = lightGO.AddComponent<Light>();
            tLight.type = LightType.Point;
            tLight.color = color;
            tLight.range = 6f;
            tLight.intensity = 1.5f;

            // Trigger Zone
            SphereCollider trigger = terminalGO.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 2.5f;

            QuizCheckpoint cp = terminalGO.AddComponent<QuizCheckpoint>();
            cp.checkpointId = id;
            cp.subject = subject;
            cp.linkedDoor = linkedDoor;

            SetPrivateField(cp, "terminalRenderer", screen.GetComponent<Renderer>());
            SetPrivateField(cp, "pendingMaterial", terminalMaterial);
            SetPrivateField(cp, "completedMaterial", completedMaterial);
            SetPrivateField(cp, "terminalLight", tLight);
            SetPrivateField(cp, "pendingLightColor", color);
            SetPrivateField(cp, "completedLightColor", Color.green);
        }

        private void CreateKey(Transform parent, string keyId, string displayName, Vector3 pos, Color color)
        {
            GameObject keyGO;
            if (keyModelPrefab != null)
            {
                keyGO = Instantiate(keyModelPrefab, pos, Quaternion.identity, parent);
                keyGO.name = keyId;
            }
            else
            {
                keyGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                keyGO.name = keyId;
                keyGO.transform.SetParent(parent, false);
                keyGO.transform.position = pos;
                keyGO.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            }

            var existingPickup = keyGO.GetComponent<Pickup>();
            if (existingPickup != null) DestroyImmediate(existingPickup);

            KeyCollectible key = keyGO.AddComponent<KeyCollectible>();
            key.keyId = keyId;
            key.displayName = displayName;

            Collider col = keyGO.GetComponent<Collider>();
            if (col == null) col = keyGO.AddComponent<SphereCollider>();
            col.isTrigger = true;

            GameObject lGO = new GameObject("KeyGlow");
            lGO.transform.SetParent(keyGO.transform, false);
            Light l = lGO.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = 4f;
            l.intensity = 1.8f;
        }

        private void CreateExitPortal(Transform parent, Vector3 pos, int reqQuizzes, string reqKey)
        {
            GameObject exitGO = new GameObject("ExitDoorway");
            exitGO.transform.SetParent(parent, false);
            exitGO.transform.position = pos;

            // 1. Threshold Floor Pad (Glowing futuristic platform)
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = "DoorThresholdPad";
            pad.transform.SetParent(exitGO.transform, false);
            pad.transform.localPosition = new Vector3(0, 0.05f, 0);
            pad.transform.localScale = new Vector3(4.5f, 0.08f, 4.0f);
            Collider padCol = pad.GetComponent<Collider>();
            if (padCol != null) { if (Application.isPlaying) Destroy(padCol); else DestroyImmediate(padCol); }
            if (completedMaterial != null) pad.GetComponent<Renderer>().material = completedMaterial;

            // 2. Door Frame - Left Pillar
            GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPillar.name = "Pillar_Left";
            leftPillar.transform.SetParent(exitGO.transform, false);
            leftPillar.transform.localPosition = new Vector3(-2.2f, 2.0f, 0);
            leftPillar.transform.localScale = new Vector3(0.6f, 4.0f, 0.7f);
            if (wallMaterial != null) leftPillar.GetComponent<Renderer>().material = wallMaterial;

            // 3. Door Frame - Right Pillar
            GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPillar.name = "Pillar_Right";
            rightPillar.transform.SetParent(exitGO.transform, false);
            rightPillar.transform.localPosition = new Vector3(2.2f, 2.0f, 0);
            rightPillar.transform.localScale = new Vector3(0.6f, 4.0f, 0.7f);
            if (wallMaterial != null) rightPillar.GetComponent<Renderer>().material = wallMaterial;

            // 4. Door Frame - Top Arch / Header
            GameObject arch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arch.name = "DoorArch";
            arch.transform.SetParent(exitGO.transform, false);
            arch.transform.localPosition = new Vector3(0, 4.15f, 0);
            arch.transform.localScale = new Vector3(5.0f, 0.5f, 0.8f);
            if (wallMaterial != null) arch.GetComponent<Renderer>().material = wallMaterial;

            // 5. Glowing EXIT Sign / Beacon on Arch
            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "ExitSign";
            sign.transform.SetParent(exitGO.transform, false);
            sign.transform.localPosition = new Vector3(0, 4.45f, 0.1f);
            sign.transform.localScale = new Vector3(2.4f, 0.35f, 0.25f);
            Collider signCol = sign.GetComponent<Collider>();
            if (signCol != null) { if (Application.isPlaying) Destroy(signCol); else DestroyImmediate(signCol); }
            if (completedMaterial != null) sign.GetComponent<Renderer>().material = completedMaterial;

            // 6. Sliding 3D Door Mesh Panel
            GameObject doorPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorPanel.name = "SlidingDoorPanel";
            doorPanel.transform.SetParent(exitGO.transform, false);
            doorPanel.transform.localPosition = new Vector3(0, 2.0f, 0);
            doorPanel.transform.localScale = new Vector3(3.8f, 3.8f, 0.3f);
            Collider panelCol = doorPanel.GetComponent<Collider>();
            if (panelCol != null) { if (Application.isPlaying) Destroy(panelCol); else DestroyImmediate(panelCol); }
            if (gateMaterial != null) doorPanel.GetComponent<Renderer>().material = gateMaterial;

            // 7. Dynamic Gateway Lighting (Pulsing Amber/Cyan when locked, bright Emerald Green on unlock)
            GameObject lightGO = new GameObject("ExitDoorLight");
            lightGO.transform.SetParent(exitGO.transform, false);
            lightGO.transform.localPosition = new Vector3(0, 2.5f, 0);
            Light pLight = lightGO.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.color = new Color(0.2f, 0.85f, 1f);
            pLight.range = 10f;
            pLight.intensity = 2.5f;

            // 8. Trigger Zone (generous bounds for effortless entry)
            BoxCollider col = exitGO.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(6f, 4.5f, 6f);
            col.center = new Vector3(0, 1.8f, 0);

            // 9. Wire MazeExit
            MazeExit exit = exitGO.AddComponent<MazeExit>();
            exit.requiredQuizzes = reqQuizzes;
            exit.requiredKeyId = reqKey;
            exit.doorMeshTransform = doorPanel.transform;
            exit.exitLight = pLight;
            exit.slideHeight = 4.8f;
            exit.openSpeed = 3.5f;
        }

        private void CreateLight(Transform parent, Vector3 pos, Color color, float intensity, float range)
        {
            GameObject lGO = new GameObject("CorridorLight");
            lGO.transform.SetParent(parent, false);
            lGO.transform.position = pos;

            Light l = lGO.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
        }

        private void RepositionPlayer(DifficultyLevel diff, int levelNumber)
        {
            Vector3 spawnPos = new Vector3(0, 0.5f, 0);
            if (LevelManager.Instance != null)
            {
                spawnPos = LevelManager.Instance.GetLevelInfo(diff, levelNumber).spawnPosition;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                player.transform.position = spawnPos;
                player.transform.rotation = Quaternion.identity;

                if (cc != null) cc.enabled = true;

                var tpc = player.GetComponentInChildren<StarterAssets.ThirdPersonController>();
                if (tpc != null)
                {
                    tpc.ResetMovementState();
                    tpc.ResetCameraRotation(0f);
                }

                var rb = player.GetComponentInChildren<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
        #endregion

        #region Player & UI Setup
        private void SetupManagers()
        {
            if (FindFirstObjectByType<GameManager>() == null)
            {
                GameObject gmGO = new GameObject("GameManager");
                gmGO.AddComponent<GameManager>();
            }

            if (FindFirstObjectByType<LevelManager>() == null)
            {
                GameObject lmGO = new GameObject("LevelManager");
                lmGO.AddComponent<LevelManager>();
            }

            if (FindFirstObjectByType<ScoreManager>() == null)
            {
                GameObject smGO = new GameObject("ScoreManager");
                smGO.AddComponent<ScoreManager>();
            }

            if (FindFirstObjectByType<DifficultyManager>() == null)
            {
                GameObject dmGO = new GameObject("DifficultyManager");
                dmGO.AddComponent<DifficultyManager>();
            }

            if (FindFirstObjectByType<QuestionBank>() == null)
            {
                GameObject qbGO = new GameObject("QuestionBank");
                qbGO.AddComponent<QuestionBank>();
            }

            if (FindFirstObjectByType<QuizManager>() == null)
            {
                GameObject qmGO = new GameObject("QuizManager");
                qmGO.AddComponent<QuizManager>();
            }

            if (FindFirstObjectByType<AudioManager>() == null)
            {
                GameObject amGO = new GameObject("AudioManager");
                amGO.AddComponent<AudioManager>();
            }

            if (FindFirstObjectByType<UIManager>() == null)
            {
                GameObject uiGO = new GameObject("UIManager");
                uiGO.AddComponent<UIManager>();
            }

            if (FindFirstObjectByType<HintManager>() == null)
            {
                GameObject hmGO = new GameObject("HintManager");
                hmGO.AddComponent<HintManager>();
            }
        }

        private void SetupPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null && playerPrefab != null)
            {
                player = Instantiate(playerPrefab, new Vector3(0, 0.5f, 0), Quaternion.identity);
                player.name = "Player";
                player.tag = "Player";
            }

            if (player != null)
            {
                ConfigureTPPPlayer(player);
            }
        }

        private void ConfigureTPPPlayer(GameObject playerRoot)
        {
            Transform cameraRoot = playerRoot.transform.Find("Robot/PlayerCameraRoot") ?? playerRoot.transform.Find("PlayerCameraRoot");
            if (cameraRoot == null)
            {
                var allTransforms = playerRoot.GetComponentsInChildren<Transform>();
                foreach (var t in allTransforms)
                {
                    if (t.name == "PlayerCameraRoot")
                    {
                        cameraRoot = t;
                        break;
                    }
                }
            }

            var controller = playerRoot.GetComponentInChildren<StarterAssets.ThirdPersonController>();
            if (controller != null && cameraRoot != null)
            {
                controller.CinemachineCameraTarget = cameraRoot.gameObject;
            }

            var cmCam = playerRoot.GetComponentInChildren<CinemachineCamera>();
            if (cmCam != null)
            {
                cmCam.Lens.FieldOfView = 65f;
                cmCam.Lens.NearClipPlane = 0.05f;

                if (cameraRoot != null)
                {
                    cmCam.Target.TrackingTarget = cameraRoot;
                }
            }

            var tppFollow = playerRoot.GetComponentInChildren<CinemachineThirdPersonFollow>();
            if (tppFollow != null)
            {
                tppFollow.CameraDistance = 3.2f;
                tppFollow.ShoulderOffset = new Vector3(0.2f, 0.35f, 0f);
                tppFollow.VerticalArmLength = 0.2f;
                tppFollow.CameraSide = 0.5f;
                tppFollow.Damping = new Vector3(0.1f, 0.15f, 0.2f);
                tppFollow.AvoidObstacles.CameraRadius = 0.12f;
                tppFollow.AvoidObstacles.DampingFromCollision = 0.2f;
            }

            if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                GameManager.Instance.SetCursorLocked(true);
            }
        }

        private void SetupUI()
        {
            UIManager uiManager = FindFirstObjectByType<UIManager>();
            QuizManager quizManager = FindFirstObjectByType<QuizManager>();

            if (uiManager != null && quizManager != null)
            {
                UIBuilder.BuildCanvas(uiManager, quizManager);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null) return;
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
        #endregion
    }
}
