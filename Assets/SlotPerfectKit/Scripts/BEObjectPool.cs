using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///-----------------------------------------------------------------------------------------
///   Namespace:      BE
///   Class:          BEObjectPool
///   Description:    The cost of instantiate & destory is too high,
///                   we instantiate many object at the start time,
///                   and activate or deactivate object at runtime
///   Usage :
///   Author:         BraveElephant inc.
///   Version: 		  v1.0 (2015-08-30)
///-----------------------------------------------------------------------------------------
namespace BE {
	public class BEObjectPool : MonoBehaviour {
		private static BEObjectPool 	_instance;
		public 	static BEObjectPool 	instance {
			get {
				if (!_instance) {
					_instance = GameObject.FindObjectOfType(typeof(BEObjectPool)) as BEObjectPool;
					if (!_instance) {
						GameObject container = new GameObject("ObjectPool");
						_instance = container.AddComponent(typeof(BEObjectPool)) as BEObjectPool;
						DontDestroyOnLoad(_instance);
					}
				}

				return _instance;
			}
		}

		[System.Serializable]
		public class PoolItem {
			public GameObject 	prefab;
			public int 			size;

			public PoolItem(GameObject _prefab, int _size) {
				prefab = _prefab;
				size = _size;
			}
		}

		Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();
		Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();

		public List<PoolItem> 	Pools=new List<PoolItem>();

		void Awake() {
			CreateStartupPools();
		}

		void Start() {
		}

		public static void AddPoolItem(GameObject _prefab, int _size) {
			if(instance.Pools.Find(x => (x.prefab == _prefab)) == null) {
				instance.Pools.Add (new PoolItem(_prefab, _size));
			}
		}
		public static void CreateStartupPools() {
			var pools = instance.Pools;
			if (pools != null && pools.Count > 0)
				for (int i = 0; i < pools.Count; ++i)
					CreatePool(pools[i].prefab, pools[i].size);
		}
		public static void CreatePool(GameObject prefab, int initialPoolSize) {
			if (prefab != null && !instance.pooledObjects.ContainsKey(prefab)) {
				var list = new List<GameObject>();
				instance.pooledObjects.Add(prefab, list);

				if (initialPoolSize > 0) {
					bool active = prefab.activeSelf;
					prefab.SetActive(false);
					Transform trParent = instance.transform;
					while (list.Count < initialPoolSize) {
						var obj = (GameObject)Object.Instantiate(prefab);
						obj.transform.SetParent (trParent);
						list.Add(obj);
					}
					prefab.SetActive(active);
				}
			}
		}
		public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position) {
			return Spawn(prefab, parent, position, Quaternion.identity);
		}
		public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation) {
			return Spawn(prefab, null, position, rotation);
		}
		public static GameObject Spawn(GameObject prefab, Transform parent) {
			return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
		}
		public static GameObject Spawn(GameObject prefab, Vector3 position) {
			return Spawn(prefab, null, position, Quaternion.identity);
		}
		public static GameObject Spawn(GameObject prefab) {
			return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
		}
		public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation) {
			List<GameObject> list;
			Transform tr;
			GameObject obj;
			if (instance.pooledObjects.TryGetValue(prefab, out list)) {
				obj = null;
				if (list.Count > 0) {
					while (obj == null && list.Count > 0) {
						obj = list[0];
						list.RemoveAt(0);
					}
					if (obj != null) {
						tr = obj.transform;
						tr.SetParent(parent);
						tr.localPosition = position;
						tr.localRotation = rotation;
						obj.SetActive(true);
						instance.spawnedObjects.Add(obj, prefab);
						//Debug.Log ("Spawn:"+prefab.name);
						return obj;
					}
				}
				obj = (GameObject)Object.Instantiate(prefab);
				tr = obj.transform;
				tr.SetParent(parent);
				tr.localPosition = position;
				tr.localRotation = rotation;
				instance.spawnedObjects.Add(obj, prefab);
				Debug.Log ("SpawnInstantiate:"+prefab.name);
				return obj;
			}
			else {
				obj = (GameObject)Object.Instantiate(prefab);
				tr = obj.GetComponent<Transform>();
				tr.SetParent(parent);
				tr.localPosition = position;
				tr.localRotation = rotation;
				Debug.Log ("SpawnInstantiate2:"+prefab.name);;
				return obj;
			}
		}
		public static void Unspawn(GameObject obj) {
			//Debug.Log ("Unspawn:"+obj.name);;
			GameObject prefab;
			if (instance.spawnedObjects.TryGetValue(obj, out prefab))
				Unspawn(obj, prefab);
			else {
				if(Application.isEditor) Object.DestroyImmediate(obj);
				else Object.Destroy(obj);
			}
		}
		static void Unspawn(GameObject obj, GameObject prefab) {
			instance.pooledObjects[prefab].Add(obj);
			instance.spawnedObjects.Remove(obj);
			obj.transform.SetParent(instance.transform);
			obj.transform.position = new Vector3(10000,10000,10000);
			obj.SetActive(false);
		}
	}
}
