using CartoonEngine;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	public class UIModelWrapCameraPool
	{
		class ModelWrapCamera
		{
			internal GameObject camObj;
			public Camera cam{
				get{
					var c = camObj.GetComponent<Camera>();
					if(null == c)
						c = camObj.AddComponent<Camera>();
					return c;
				}
			}
			public int referInstId;
		}

		const int MAXKEEP = 2;
		const int INVALID = -1;
		static GameObject parent;
		static List<ModelWrapCamera> s_pool = new List<ModelWrapCamera>();

		static private void CreateParent()
		{
			if(Application.isPlaying)
			{
				if(null == parent)
				{
					parent = GameObject.Find("ModelWrapCamera");					
					if(null == parent)
					{
						parent = new GameObject("ModelWrapCamera");
						parent.transform.position = Vector3.zero;
						parent.transform.rotation = Quaternion.identity;
						parent.transform.localScale = Vector3.one;
						Object.DontDestroyOnLoad(parent);
					}
				}
			}
		}

		static public Camera GetCamera(int instanceId)
		{
			CreateParent();

			int validIdx = s_pool.FindIndex(c => c.referInstId == 0);
			if(validIdx == INVALID)
			{
				validIdx = s_pool.Count;
				s_pool.Add(new ModelWrapCamera()
				{
					camObj = new GameObject("ModelWrapCamera" + validIdx),
					referInstId = instanceId,
				});
				s_pool[validIdx].camObj.transform.SetParent(null!=parent?parent.transform:null);
			}

			var camera = s_pool[validIdx].cam;
			camera.enabled = true;
			s_pool[validIdx].referInstId = instanceId;
			return camera;
		}

		static public void RecycleCamera(Camera camera)
		{
			int validIdx = s_pool.FindIndex(c => c.cam == camera);
			if(validIdx == INVALID)
			{
				Debug.LogError("Erro to Recycle Camera!");
				return;
			}

			s_pool[validIdx].cam.enabled = false;
			s_pool[validIdx].referInstId = 0;
		}

		static public void Resize()
		{
			int keep = MAXKEEP;
			for(int i = 0; i < s_pool.Count; i++)
			{
				var c = s_pool[i];
				if(0 == c.referInstId)
				{
					keep--;
					if(keep<0)
					{
						Object.DestroyImmediate(c.cam);
						Object.DestroyImmediate(c.camObj);
						s_pool.Remove(c);
					}
				}
			}
		}

		static public void Release()
		{			
			for(int i = 0; i < s_pool.Count; i++)
			{
				var c = s_pool[i];
				Object.DestroyImmediate(c.cam);
				Object.DestroyImmediate(c.camObj);
				s_pool.Remove(c);
			}
		}
	}
}