using System.IO;

namespace UnityEngine.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class UIModelWrap : RawImage
	{
		/// <summary>
		/// render obj
		/// </summary>
		private GameObject modelObject;
		public GameObject ModelObject
		{
			get
			{
				return modelObject;
			}
			set
			{
				modelObject = value;

				if(CheckCullingMask())
				{
					//CheckInVisiblePart();
					EnableCamera();
				}
			}
		}
		/// <summary>
		/// keep render?
		/// </summary>
		public bool isLoop;
		/// <summary>
		/// debug mode?
		/// </summary>
		public bool isDebug;

		protected Camera m_camera;

		private int cullingMask;
		private Color bgColor;
		private RenderTexture tempRt;
		private RenderTextureFormat format;

		protected void Render()
		{
			if(!IsValid())
				return;

			if(!isLoop)
			{
				if(null == m_camera)
					m_camera = UIModelWrapCameraPool.GetCamera(this.GetInstanceID());
			}

			if(null!=tempRt)
				RenderTexture.ReleaseTemporary(tempRt);
			var imgeSize = this.rectTransform.sizeDelta;
			tempRt = RenderTexture.GetTemporary((int)imgeSize.x * 2, (int)imgeSize.y * 2, 24, format); //*2保证质量
			m_camera.targetTexture = tempRt;
			m_camera.Render();
			if(null!=material&&material!=defaultMaterial)
				material.mainTexture = tempRt;
			else
				texture = tempRt;

			if(!isLoop)
			{
				DisableCamera();
			}

			//TODO.刷新
			SetMaterialDirty();
		}

		private bool IsValid()
		{
			return null != modelObject;
		}

		private void EnableCamera()
		{
			if(!IsValid())
			{
				DisableCamera();
				return;
			}

			if(null == m_camera)
				m_camera = UIModelWrapCameraPool.GetCamera(this.GetInstanceID());

			SetCameraParams();
			Render();
		}

		private void DisableCamera()
		{
			if(null!=m_camera)
				UIModelWrapCameraPool.RecycleCamera(m_camera);
			m_camera = null;
		}

		private void DestroyCamera()
		{
			UIModelWrapCameraPool.Resize();
			m_camera = null;
		}

		private void SetCameraParams()
		{
			m_camera.orthographic = true;
			m_camera.cullingMask = cullingMask;
			m_camera.clearFlags = CameraClearFlags.SolidColor;
			m_camera.backgroundColor = bgColor;
			m_camera.forceIntoRenderTexture = true;
			var imgeSize = this.rectTransform.sizeDelta;
			m_camera.aspect = imgeSize.x/imgeSize.y;

			var bounds = GetBounds();
			var camPos = bounds.center;
			camPos.z -= bounds.size.z;
			m_camera.transform.position = camPos;

			//根据模型的大小自适应camera的orthographicSize
			//orthographicSize=Screen height/2
			//这里尽可能的包含模型
			float w = bounds.size.x;
			float h = bounds.size.y;
			if(h >= w)
			{
				float os = h / 2;
				m_camera.orthographicSize = os;
			}
			else
			{
				float os = w / m_camera.aspect / 2;
				m_camera.orthographicSize = os;
			}
		}

		private Bounds GetBounds()
		{
			Bounds bound = new Bounds();
			var renderers = modelObject.transform.GetComponentsInChildren<Renderer>();
			if(null!=renderers)
			{
				int calcNum = 0;
				var center = Vector3.zero;
				foreach(var r in renderers)
				{
					//这里可以添加一些筛选条件
					if(null!=r.GetComponent<ParticleSystem>()) //有些粒子范围很大
						continue;
					calcNum++;
					center += r.bounds.center;
				}
				bound.center = calcNum > 0 ? center / calcNum : center;
				foreach(var r in renderers)
				{
					//这里可以添加一些筛选条件
					if(null!=r.GetComponent<ParticleSystem>())
						continue;
					bound.Encapsulate(r.bounds);
				}
			}
		}

		/// <summary>
		/// 隐藏一些不需要的部分
		/// </summary>
		private bool CheckInVisiblePart()
		{
			//自定义
		}

		private bool CheckCullingMask()
		{
			if(!IsValid())
				return false;

			bool suit = (cullingMask & (1 << modelObject.layer)) != 0;
			if(!suit)
			{
				Debug.Log("modellayer don't suit with camera cullingMask!");
				DisableCamera();
				modelObject = null;
			}

			return suit;
		}

		#region 外部调用
		public void Refresh()
		{
			ModelObject = modelObject;
		}

		public void OnHide()
		{
			if(null!=m_camera&&m_camera.enabled)
				DisableCamera();
		}

		public void Save()
		{
			if(null == tempRt)
				return;

			#if UNITY_EDITOR
				Texture2D tex2d = new Texture2D(tempRt.width, tempRt.height, TextureFormat.ARGB32, false);
				RenderTexture.active = tempRt;
				tex2d.ReadPixels(new Rect(0, 0, tempRt.width, tempRt.height), 0, 0);
				tex2d.Apply();

				byte[] array = tex2d.EncodeToPNG();
				string path = Application.dataPath + "/ModelWrapRt.png";
				FileStream stream = File.Open(path, FileMode.Create);
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(array);
				writer.Close();
				stream.Close();
				stream.Dispose();
			#endif
		}
		#endregion

		#region 系统调用
		void LateUpdate()
		{
			if(isLoop && null!=m_camera)
			{
				SetCameraParams();
				Render();
			}
		}

		protected override void Start()
		{
			base.Start();

			//材质最好使用ui-default-rt
			//if(material.name == "UI-Default-Rt")
			//	material = Instantiate(material);
			bgColor = new Color(0, 0, 0, 0);
			cullingMask = 1 << LayerMask.NameToLayer("UI") | 1 << LayerMask.NameToLayer("UIEffectSpecial");
			format = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32) ? RenderTextureFormat.ARGB32 : RenderTextureFormat.Default;
		}

		protected override void OnEnable()
		{
			EnableCamera();

			base.OnEnable();
		}

		protected override void OnDisable()
		{
			DisableCamera();

			base.OnDisable();
		}

		protected override void OnDestroy()
		{
			if(null!=tempRt)
				RenderTexture.ReleaseTemporary(tempRt);
			DestroyCamera();
			if(material!=defaultMaterial)
				Object.DestroyImmediate(material);

			base.OnDestroy();
		}

		void OnDrawGizmos()
		{
			#if UNITY_EDITOR
				if(IsValid() && isDebug)
				{
					Gizmos.color = Color.yellow;
					var bound = GetBounds();
					Gizmos.DrawCube(bound.center, bound.size);
				}
			#endif
		}
		#endregion
	}
}