using UnityEngine;

#if NETFX_CORE
    using System.Threading.Tasks;
#endif

namespace BestHTTP
{
    /// <summary>
    /// Will route some U3D calls to the HTTPManager.
    /// </summary>
    [ExecuteInEditMode]
    public sealed class HTTPUpdateDelegator : MonoBehaviour
    {
        #region Public Properties

        /// <summary>
        /// The singleton instance of the HTTPUpdateDelegator
        /// </summary>
        public static HTTPUpdateDelegator Instance { get; private set; }

        /// <summary>
        /// True, if the Instance property should hold a valid value.
        /// </summary>
        public static bool IsCreated { get; private set; }

        /// <summary>
        /// Set it true before any CheckInstance() call, or before any request sent to dispatch callbacks on another thread.
        /// </summary>
        public static bool IsThreaded { get; set; }

        /// <summary>
        /// It's true if the dispatch thread running.
        /// </summary>
        public static bool IsThreadRunning { get; private set; }

        /// <summary>
        /// How much time the plugin should wait between two update call. Its default value 100 ms.
        /// </summary>
        public static int ThreadFrequencyInMS { get; set; }

        /// <summary>
        /// Called in the OnApplicationQuit function. If this function returns False, the plugin will not start to
        /// shut down itself.
        /// </summary>
        public static System.Func<bool> OnBeforeApplicationQuit;

        public static System.Action<bool> OnApplicationForegroundStateChanged;

        #endregion

        private static bool IsSetupCalled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Reset()
        {
            IsSetupCalled = false;
        }

        static HTTPUpdateDelegator()
        {
            ThreadFrequencyInMS = 100;
        }

        /// <summary>
        /// Will create the HTTPUpdateDelegator instance and set it up.
        /// </summary>
        public static void CheckInstance()
        {
            try
            {
                if (!IsCreated)
                {
                    GameObject go = GameObject.Find("HTTP Update Delegator");

                    if (go != null)
                        Instance = go.GetComponent<HTTPUpdateDelegator>();

                    if (Instance == null)
                    {
                        go = new GameObject("HTTP Update Delegator");
                        go.hideFlags = HideFlags.DontSave;

                        Instance = go.AddComponent<HTTPUpdateDelegator>();
                    }
                    IsCreated = true;

#if UNITY_EDITOR
                    if (!UnityEditor.EditorApplication.isPlaying)
                    {
                        UnityEditor.EditorApplication.update -= Instance.Update;
                        UnityEditor.EditorApplication.update += Instance.Update;
                    }

#if UNITY_2017_2_OR_NEWER
                    UnityEditor.EditorApplication.playModeStateChanged -= Instance.OnPlayModeStateChanged;
                    UnityEditor.EditorApplication.playModeStateChanged += Instance.OnPlayModeStateChanged;
#else
                    UnityEditor.EditorApplication.playmodeStateChanged -= Instance.OnPlayModeStateChanged;
                    UnityEditor.EditorApplication.playmodeStateChanged += Instance.OnPlayModeStateChanged;
#endif
#endif
                    HTTPManager.Logger.Information("HTTPUpdateDelegator", "Instance Created!");
                }
            }
            catch
            {
                HTTPManager.Logger.Error("HTTPUpdateDelegator", "Please call the BestHTTP.HTTPManager.Setup() from one of Unity's event(eg. awake, start) before you send any request!");
            }
        }

        private void Setup()
        {
            if (IsSetupCalled)
                return;
            IsSetupCalled = true;

            HTTPManager.Setup();

#if UNITY_WEBGL && !UNITY_EDITOR
            // Threads are not implemented in WEBGL builds, disable it for now.
            IsThreaded = false;
#endif
            if (IsThreaded)
                PlatformSupport.Threading.ThreadedRunner.RunLongLiving(ThreadFunc);

            // Unity doesn't tolerate well if the DontDestroyOnLoad called when purely in editor mode. So, we will set the flag
            //  only when we are playing, or not in the editor.
            if (!Application.isEditor || Application.isPlaying)
                GameObject.DontDestroyOnLoad(this.gameObject);

            HTTPManager.Logger.Information("HTTPUpdateDelegator", "Setup done!");
        }

        private void ThreadFunc()
        {
            HTTPManager.Logger.Information ("HTTPUpdateDelegator", "Update Thread Started");

            try
            {
                IsThreadRunning = true;
                while (IsThreadRunning)
                {
                    HTTPManager.OnUpdate();

#if NETFX_CORE
	                await Task.Delay(ThreadFrequencyInMS);
#else
                    System.Threading.Thread.Sleep(ThreadFrequencyInMS);
#endif
                }
            }
            finally
            {
                HTTPManager.Logger.Information("HTTPUpdateDelegator", "Update Thread Ended");
            }
        }

        private void Update()
        {
            if (!IsSetupCalled)
                Setup();

            if (!IsThreaded)
                HTTPManager.OnUpdate();
        }

#if UNITY_EDITOR
#if UNITY_2017_2_OR_NEWER
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange playMode)
        {
            if (playMode == UnityEditor.PlayModeStateChange.EnteredPlayMode)
                UnityEditor.EditorApplication.update -= Update;
            else if (playMode == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                UnityEditor.EditorApplication.update += Update;
        }
#else
        void OnPlayModeStateChanged()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                UnityEditor.EditorApplication.update -= Update;
            else if (!UnityEditor.EditorApplication.isPlaying)
                UnityEditor.EditorApplication.update += Update;
        }

#endif
#endif

        private void OnDisable()
        {
            HTTPManager.Logger.Information("HTTPUpdateDelegator", "OnDisable Called!");

#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
#endif
                OnApplicationQuit();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (HTTPUpdateDelegator.OnApplicationForegroundStateChanged != null)
                HTTPUpdateDelegator.OnApplicationForegroundStateChanged(isPaused);
        }

        private void OnApplicationQuit()
        {
            HTTPManager.Logger.Information("HTTPUpdateDelegator", "OnApplicationQuit Called!");

            if (OnBeforeApplicationQuit != null)
            {
                try
                {
                    if (!OnBeforeApplicationQuit())
                    {
                        HTTPManager.Logger.Information("HTTPUpdateDelegator", "OnBeforeApplicationQuit call returned false, postponing plugin shutdown.");
                        return;
                    }
                }
                catch(System.Exception ex)
                {
                    HTTPManager.Logger.Exception("HTTPUpdateDelegator", string.Empty, ex);
                }
            }

            IsThreadRunning = false;

            if (!IsCreated)
                return;

            IsCreated = false;

            HTTPManager.OnQuit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= Update;
#if UNITY_2017_2_OR_NEWER
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#else
            UnityEditor.EditorApplication.playmodeStateChanged -= OnPlayModeStateChanged;
#endif
#endif
        }
    }
}