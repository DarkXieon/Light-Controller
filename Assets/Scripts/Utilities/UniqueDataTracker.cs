#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static LightControls.Utilities.PairTracker;

namespace LightControls.Utilities
{
    [Serializable]
    public class PairTracker : ISerializationCallbackReceiver
    {
        [Serializable]
        public class Pair<TData>
            where TData : SerializedData
        {
            public string GUID;
            public TData Data;

            public Pair(string guid, TData data)
            {
                GUID = guid;
                Data = data;
            }
        }

        [Serializable]
        public class StagePair : Pair<StageData>
        {
            public StagePair(string guid, StageData data)
                : base(guid, data)
            {
            }
        }

        [Serializable]
        public class WarningLoggerPair : Pair<WarningLoggerData>
        {
            public WarningLoggerPair(string guid, WarningLoggerData data) 
                : base(guid, data)
            {
            }
        }

        public PairTracker()
        {
            Init();
        }
        
        private void Init()
        {
            data = data ?? new List<Pair<SerializedData>>();
            stageData = stageData ?? new List<StagePair>();
            warningLoggerData = warningLoggerData ?? new List<WarningLoggerPair>();
        }

        private List<Pair<SerializedData>> data;

        [SerializeField] private List<StagePair> stageData;
        [SerializeField] private List<WarningLoggerPair> warningLoggerData;

        public TValue GetOrCreateData<TValue>(string guid)
            where TValue : SerializedData, new()
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(guid));

            TValue value;

            if (!HasKey(guid))
            {
                value = new TValue();

                data.Add(new Pair<SerializedData>(guid, value));
            }
            else
            {
                value = (TValue)GetPair(guid).Data;
            }

            return value;
        }

        public void RemoveData(string guid)
        {
            data.Remove(GetPair(guid));
        }

        private bool HasKey(string guid)
        {
            return data.Select(pair => pair.GUID).Contains(guid);
        }

        private Pair<SerializedData> GetPair(string guid)
        {
            return data.Single(pair => pair.GUID == guid);
        }

        public void OnAfterDeserialize()
        {
            Init();

            var castedStageData = stageData
                .Select(pair => new Pair<SerializedData>(pair.GUID, pair.Data))
                .ToList();

            var castedWarningLoggerData = warningLoggerData
                .Select(pair => new Pair<SerializedData>(pair.GUID, pair.Data))
                .ToList();

            data = castedStageData
                .Concat(castedWarningLoggerData)
                .ToList();
        }

        public void OnBeforeSerialize()
        {
            var castedStageData = data
                .Where(pair => pair.Data.GetType() == typeof(StageData))
                .Select(pair => new StagePair(pair.GUID, (StageData)pair.Data))
                .ToList();

            var castedWarningLoggerData = data
                .Where(pair => pair.Data.GetType() == typeof(WarningLoggerPair))
                .Select(pair => new WarningLoggerPair(pair.GUID, (WarningLoggerData)pair.Data))
                .ToList();

            stageData = castedStageData;
            warningLoggerData = castedWarningLoggerData;
        }
    }

    public class UniqueDataTracker : ScriptableObject
    {
        private const string path = "Assets/Resources/UniqueDataTracker.asset";

        public static UniqueDataTracker Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = AssetDatabase.LoadAssetAtPath<UniqueDataTracker>(path);

                    if (singleton == null)
                    {
                        singleton = ScriptableObject.CreateInstance<UniqueDataTracker>();

                        AssetDatabase.CreateAsset(singleton, path);
                    }

                    singleton.Initialize();
                }

                return singleton;
            }
        }

        private static UniqueDataTracker singleton;
        
        public PairTracker Data;

        public void Initialize()
        {
            Data = Data ?? new PairTracker();
        }
    }

    [Serializable]
    public abstract class SerializedData { }

    [Serializable]
    public class WarningLoggerData : SerializedData
    {
        public enum VerbosityLevel
        {
            None,
            Low,
            Medium,
            High
        }

        public VerbosityLevel Level;

        public Color LowLevelColor;
        public Color MediumLevelColor;
        public Color HighLevelColor;
    }

    [Serializable]
    public class StageData : SerializedData
    { 
        public string Name = "(name your stage. . .)";
        public bool Collapsed = false;
    }
}

#endif