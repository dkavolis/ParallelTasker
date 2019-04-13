/*
Copyright 2019, Daumantas Kavolis

   This file is part of ParallelTasker.

   ParallelTasker is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   ParallelTasker is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with ParallelTasker.  If not, see <http: //www.gnu.org/licenses/>.

 */

using System.Collections;
using System.IO;
using UnityEngine;

namespace ParallelTasker
{
#if !UNITY
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
#endif
    public class PTAddon : Singleton<PTAddon>
    {
        public const string AssetBundleName = "ParallelTasker/Assets/synchronizers.pt";
        public static string RootPath = "GameData";

        [SerializeField] private GameObject m_synchronizer = null;
        private PTStartSynchronizer m_loopStart = null;
        private PTEndSynchronizer m_loopEnd = null;

        public PTStartSynchronizer StartSynchronizer
        {
            get
            {
                return m_loopStart;
            }
        }

        public PTEndSynchronizer EndSynchronizer
        {
            get
            {
                return m_loopEnd;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
#if !UNITY
            RootPath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData");
#endif

            SetupListener();
        }

        private void SetupListener()
        {
#if !UNITY
            StartCoroutine(DoSetupListener());
#endif
        }

#if !UNITY
        private IEnumerator DoSetupListener()
        {
            while (HighLogic.LoadedScene == GameScenes.LOADING)
                yield return null;
            // have to wait until loaded otherwise this throws NRE
            GameEvents.onLevelWasLoaded.Add(OnSceneChange);
        }

        internal void OnSceneChange(GameScenes scene)
        {
            if (PTController.ResetOnSceneChange)
                PTController.ResetCurrentTasks();
        }
#endif

        private void Start()
        {
            Load();
#if DEBUG_SYNCHRONIZATION
            foreach (var group in PTUtils.GetEnumValues<PTGroup>())
            {
                var task = new SimpleTask(group);
                ParallelTasker.AddTask(group, task.OnInitialize, task.Execute, task.OnFinalize);
            }
#endif
        }

        private void Load()
        {
#if UNITY
            // In unity editor set synchronizer from the inspector
#else
            var path = Path.Combine(RootPath, AssetBundleName);
            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null)
            {
                PTLogger.Error($"Failed to load ParallelTasker asset bundle from {path}.");
                return;
            }

            var prefab = bundle.LoadAsset<GameObject>("Synchronizer");
            m_synchronizer = Instantiate(prefab);
            bundle.Unload(true);
#endif

            if (m_synchronizer == null)
            {
                PTLogger.Error("Could not load synchronizers");
                return;
            }

            m_loopStart = m_synchronizer.GetComponent<PTStartSynchronizer>();
            m_loopEnd = m_synchronizer.GetComponent<PTEndSynchronizer>();
            Setup();
        }

        private void Setup()
        {
            if (m_loopStart != null)
            {
                m_loopStart.OnUpdate += PTController.Instance.UpdateStart;
                m_loopStart.OnLateUpdate += PTController.Instance.LateUpdateStart;
                m_loopStart.OnFixedUpdate += PTController.Instance.FixedUpdateStart;
            }
            else
            {
                PTLogger.Error($"Failed to load {typeof(PTStartSynchronizer)}");
            }

            if (m_loopEnd != null)
            {
                m_loopEnd.OnUpdate += PTController.Instance.UpdateEnd;
                m_loopEnd.OnLateUpdate += PTController.Instance.LateUpdateEnd;
                m_loopEnd.OnFixedUpdate += PTController.Instance.FixedUpdateEnd;
            }
            else
            {
                PTLogger.Error($"Failed to load {typeof(PTEndSynchronizer)}");
            }

            m_synchronizer?.SetActive(true);
        }

        protected override void OnSingletonDestroy()
        {
            var instance = PTController.Instance;
            if (instance != null)
            {
                if (m_loopStart != null)
                {
                    m_loopStart.OnUpdate -= instance.UpdateStart;
                    m_loopStart.OnLateUpdate -= instance.LateUpdateStart;
                    m_loopStart.OnFixedUpdate -= instance.FixedUpdateStart;
                }

                if (m_loopEnd != null)
                {
                    m_loopEnd.OnUpdate -= instance.UpdateEnd;
                    m_loopEnd.OnLateUpdate -= instance.LateUpdateEnd;
                    m_loopEnd.OnFixedUpdate -= instance.FixedUpdateEnd;
                }
            }
            base.OnSingletonDestroy();
        }
    }
}
