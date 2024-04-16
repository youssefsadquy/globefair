using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using StarterAssets;
using Firebase.Storage;
using UMA.CharacterSystem;

// Script to save and load game states from firebase under user UID
public class GameStateManager : MonoBehaviour
{
    FirebaseFirestore db;
    FirebaseAuth auth;
    FirebaseStorage storage;
    public static bool GameStateLoaded = false;
    public CameraShowcase cameraShowcase;
    public CinemachineVirtualCamera thirdPersonCinemachineCamera;
    public GameObject thirdPersonCharacter;
    public GameObject skipPanel;
    public DynamicCharacterAvatar avatar;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        storage = FirebaseStorage.DefaultInstance;

        GameStateLoaded = false;

        LoadGameState();
    }

    // Method that saves the player position and rotation on firebase
    public void SaveGameState()
    {
        if (auth.CurrentUser == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 playerPosition = player.transform.position;
        Quaternion playerRotation = player.transform.rotation;

        var gameState = new Dictionary<string, object>
    {
        { "playerPosition", SerializeVector3(playerPosition) },
        { "playerRotation", SerializeQuaternion(playerRotation) }
    };

        DocumentReference docRef = db.Collection("gameStates").Document(auth.CurrentUser.UserId);
        docRef.SetAsync(gameState).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error saving game state: " + task.Exception);
                return;
            }
            Debug.Log("Game state saved successfully.");
        });
    }

    // Method to load saved position and skip showcase
    public void LoadGameState()
    {
        if (auth.CurrentUser == null)
        {
            // Reset flag
            GameStateLoaded = false;
            cameraShowcase.StartShowcase();
            return;
        }

        DocumentReference docRef = db.Collection("gameStates").Document(auth.CurrentUser.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error loading game state: " + task.Exception);

                GameStateLoaded = false;
                cameraShowcase.StartShowcase();
                return;
            }
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Debug.Log("Game state loaded successfully.");
                GameStateLoaded = true;

                ApplyGameState(snapshot.ToDictionary());

                if (avatar != null)
                {
                    // Fetch the avatar data from Firebase Storage
                    LoadAvatarCustomization();
                }

                ActivateGamePlay();
            }
            else
            {
                Debug.LogError("No game state found.");

                GameStateLoaded = false;
                cameraShowcase.StartShowcase();
            }
        });
    }

    // Method that allows to load avatar customisation if existing
    private void LoadAvatarCustomization()
    {
        var avatarDataRef = storage.GetReference($"umaData/{auth.CurrentUser.UserId}.json");
        avatarDataRef.GetBytesAsync(long.MaxValue).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error getting avatar data from Storage: " + task.Exception);
                return;
            }

            string avatarDataString = System.Text.Encoding.UTF8.GetString(task.Result);
            AvatarDefinition avatarDef = AvatarDefinition.FromCompressedString(avatarDataString, '|');
            avatar.LoadAvatarDefinition(avatarDef);
            avatar.BuildCharacter();
            Debug.Log("Avatar customization loaded successfully.");
        });
    }

    // Serialising all the data to make it compatible with firebase, on loading the data
    // is converted back for unity
    Dictionary<string, float> SerializeVector3(Vector3 vector)
    {
        return new Dictionary<string, float> { { "x", vector.x }, { "y", vector.y }, { "z", vector.z } };
    }
    
    Dictionary<string, float> SerializeQuaternion(Quaternion quaternion)
    {
        return new Dictionary<string, float> { { "x", quaternion.x }, { "y", quaternion.y }, { "z", quaternion.z }, { "w", quaternion.w } };
    }

    Vector3 DeserializeVector3(Dictionary<string, object> dictionary)
    {
        return new Vector3(
            float.Parse(dictionary["x"].ToString()),
            float.Parse(dictionary["y"].ToString()),
            float.Parse(dictionary["z"].ToString())
        );
    }

    Quaternion DeserializeQuaternion(Dictionary<string, object> dictionary)
    {
        return new Quaternion(
            float.Parse(dictionary["x"].ToString()),
            float.Parse(dictionary["y"].ToString()),
            float.Parse(dictionary["z"].ToString()),
            float.Parse(dictionary["w"].ToString())
        );
    }

    // Method to apply the game save to the player armature using tag
    private void ApplyGameState(Dictionary<string, object> gameState)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        var characterController = player.GetComponent<CharacterController>();
        var thirdPersonController = player.GetComponent<ThirdPersonController>();

        // Interfering componets
        if (characterController) characterController.enabled = false;
        if (thirdPersonController) thirdPersonController.enabled = false;

        player.transform.position = DeserializeVector3((Dictionary<string, object>)gameState["playerPosition"]);
        player.transform.rotation = DeserializeQuaternion((Dictionary<string, object>)gameState["playerRotation"]);

        if (characterController) characterController.enabled = true;
        if (thirdPersonController) thirdPersonController.enabled = true;
    }

    private void ActivateGamePlay()
    {
        // Disable the showcase camera
        cameraShowcase.gameObject.SetActive(false);
        skipPanel.SetActive(false);

        // Enable the player character and camera
        thirdPersonCharacter.SetActive(true);
        thirdPersonCinemachineCamera.gameObject.SetActive(true);
        thirdPersonCinemachineCamera.Priority = 20;
    }
}
