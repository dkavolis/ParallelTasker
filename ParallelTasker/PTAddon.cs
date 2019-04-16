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
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParallelTasker
{
#if !UNITY
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
#endif
    public class PTAddon : Singleton<PTAddon>
    {
        public const string AssetBundleName = "ParallelTasker/Assets/synchronizers.pt";
        public static string RootPath = "GameData";

        public bool ResetOnSceneChange
        {
            get;
            set;
        } = true;

        [SerializeField] private GameObject m_synchronizer;

        internal PTController Controller;
        private Dictionary<PTEventTime, IPTSynchronizer> s_synchronizers;

        public Dictionary<PTEventTime, IPTSynchronizer> Synchronizers
        {
            get
            {
                return s_synchronizers;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            s_synchronizers = new Dictionary<PTEventTime, IPTSynchronizer>();
#if !UNITY
            RootPath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData");
#endif
            SceneManager.activeSceneChanged += OnSceneChange;
        }

        internal void OnSceneChange(Scene current, Scene next)
        {
            // in KSP scene changes are done through an intermediate loading buffer scene
            if (!ResetOnSceneChange)return;
            PTLogger.Debug($"Scene change: {current.name} -> {next.name}");
            Controller?.ResetCurrentTasks();
        }

        private void Start()
        {
            Load();
            Controller = new PTController();
#if DEBUG_SYNCHRONIZATION
            Disposable<List<PTTask>> tasks = ListPool<PTTask>.Instance.BorrowDisposable();
            foreach (var group in PTUtils.GetAllTimePairs())
            {
                var task = new SimpleTask(group, group);
                tasks.Value.Add(ParallelTasker.AddTask(group, group, task.OnInitialize, task.Execute, task.OnFinalize));
            }
            StartCoroutine(WaitAndClearTasks(tasks));
#endif
        }

#if DEBUG_SYNCHRONIZATION
        private static IEnumerator WaitAndClearTasks(Disposable<List<PTTask>> tasks)
        {
            var counter = 0;
            while (counter++ < 20)
            {
                yield return new WaitForEndOfFrame();
            }

            foreach (var task in tasks.Value)
            {
                ParallelTasker.RemoveTask(task.EndTime, task);
            }
            tasks.Dispose();
        }
#endif

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
            m_synchronizer.SetActive(true);

            foreach (var sync in m_synchronizer.GetComponents<IPTSynchronizer>())
            {
                PTLogger.Debug($"Adding {sync} to known synchronizers");
                s_synchronizers.Add(sync.EventTime, sync);
            }
        }

        protected override void OnSingletonDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChange;
            Controller.Dispose();
            base.OnSingletonDestroy();
        }
    }
}
