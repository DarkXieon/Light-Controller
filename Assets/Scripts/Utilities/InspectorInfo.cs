//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEditor;
//using UnityEngine;

//namespace LightControls.Utilities
//{
//    [InitializeOnLoad]
//    public class InspectorInfoFinder
//    {
//        private const string path = "Assets/Resources/InspectorInfo";

//        public static InspectorInfo Singleton => singleton;
//        private static InspectorInfo singleton;

//        static InspectorInfoFinder()
//        {
//            singleton = Resources.Load<InspectorInfo>(path);

//            if (singleton == null)
//            {
//                singleton = ScriptableObject.CreateInstance<InspectorInfo>();

//                AssetDatabase.CreateAsset(singleton, path);
//            }

//            singleton.Initialize();
//        }
//    }

//    public class InspectorInfo : ScriptableObject
//    {
//        [Serializable]
//        public class InspectorObjectData
//        {
//            [SerializeField]
//            private List<KeyValuePair<string, object>> data; //Dictionary<string, object> Toggles;

//            public InspectorObjectData()
//            {
//                data = data ?? new List<KeyValuePair<string, object>>(); //new Dictionary<string, object>();
//            }

//            public void EnsureValueWithName<TValue>(string name, TValue value)
//            {
//                if(!data.Select(pair => pair.Key).Contains(name))
//                {
//                    data.Add(new KeyValuePair<string, object>(name, value));
//                }
//            }

//            public TValue GetValueWithName<TValue>(string name)
//            {
//                return (TValue)data.Single(pair => pair.Key == name).Value;
//            }
//        }

//        //public Dictionary<string, InspectorObjectData> Info;

//        [SerializeField]
//        public List<KeyValuePair<string, InspectorObjectData>> serializationList;
        
//        public void Initialize()
//        {
//            serializationList = serializationList ?? new List<KeyValuePair<string, InspectorObjectData>>();
            
//            //Info = serializationList.ToDictionary(pair => pair.Key, pair => pair.Value);
//        }

//        public void OnDestroy()
//        {
//            ScriptableObject singleton = ScriptableObject.CreateInstance<InspectorInfo>();

//            AssetDatabase.CreateAsset(singleton, "Assets/Resources/test");
//        }
        
//        public InspectorObjectData GetOrCreateValue(string name)
//        {
//            if (!serializationList.Select(pair => pair.Key).Contains(name))
//            {
//                serializationList.Add(new KeyValuePair<string, InspectorObjectData>(name, new InspectorObjectData()));
//            }

//            return serializationList.Single(pair => pair.Key == name).Value;
//        }
//    }
//}
