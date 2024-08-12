using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace RimuruDev.Internal.Codebase
{
    [DisallowMultipleComponent]
    public sealed class Game : MonoBehaviour
    {
        /// <summary>
        /// Path to audio -> Assets/StreamingAssets/BigMusic.mp3
        /// </summary>
        [Space] 
        [SerializeField] private string audioFileName = "BigMusic.mp3";
        [Space] 
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button disposeButton;
        [Space] 
        [SerializeField] private bool debugLogEnable = true;
        private AudioClip audioClip;

        private void Reset()
        {
            audioFileName = "BigMusic.mp3";

            if (audioSource == null)
                audioSource = FindObjectOfType<AudioSource>(true);

            if (loadButton == null)
                loadButton = GameObject.Find("LoadAudio").GetComponent<Button>();

            if (disposeButton == null)
                disposeButton = GameObject.Find("DisposeAudio").GetComponent<Button>();

            if (FindObjectOfType<AudioListener>(true) == null)
                new GameObject().AddComponent<AudioListener>();
        }

        private void Awake()
        {
            Debug.unityLogger.logEnabled = debugLogEnable;

            loadButton.onClick.AddListener(Load);
            disposeButton.onClick.AddListener(Dispose);
        }

        private void Start()
        {
            DebugLogAndButtonLock(null, true, this);
        }

        private void OnDestroy()
        {
            loadButton.onClick.RemoveListener(Load);
            disposeButton.onClick.RemoveListener(Dispose);
        }

        private void Load()
        {
            DebugLogAndButtonLock(nameof(Load), false, this);
            StartCoroutine(LoadAudio());
        }

        private IEnumerator LoadAudio()
        {
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, audioFileName);

            Debug.Log($"Loading from path: {path}");

            using var www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);

            yield return www.SendWebRequest();

            if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                audioClip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }

        private void Dispose()
        {
            DebugLogAndButtonLock(nameof(Dispose), true, this);

            if (audioSource.isPlaying)
                audioSource.Stop();

            if (audioClip != null)
            {
                audioSource.clip = null;
                Resources.UnloadUnusedAssets();
                audioClip = null;
            }
        }

        private void DebugLogAndButtonLock(string methodName, bool state, Object sender = null)
        {
            if (!string.IsNullOrWhiteSpace(methodName))
                Debug.Log($"Perform {methodName}", sender);

            loadButton.interactable = state;
            disposeButton.interactable = !state;
        }
    }
}