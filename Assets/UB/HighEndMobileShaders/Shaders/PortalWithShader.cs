
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;

namespace UB
{
    [ExecuteInEditMode]
    public class PortalWithShader : EffectBase
    {
        public Camera MainCamera = null;
        public Camera PortalCamera = null;
        public Transform Source = null;
        public Transform Destination = null;

        [Range(1, 100)]
        public int Iterations = 3;
        public float CustomFloatParam2 = 3;
        public Shader Shader;
        private Material _material;
        public bool DisablePixelLights = true;
        public int TextureSize = 256;

        public LayerMask ReflectLayers = -1;

        private Hashtable _reflectionCameras = new Hashtable(); // Camera -> Camera table

        private RenderTexture _reflectionTexture1;
        private RenderTexture _reflectionTexture2;
        private RenderTexture _reflectionTexture3;

        private int _oldReflectionTextureSize;


        public static Quaternion QuaternionFromMatrix(Matrix4x4 m) { return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); }
        public static Vector4 PosToV4(Vector3 v) { return new Vector4(v.x, v.y, v.z, 1.0f); }
        public static Vector3 ToV3(Vector4 v) { return new Vector3(v.x, v.y, v.z); }

        public static Vector3 ZeroV3 = new Vector3(0.0f, 0.0f, 0.0f);
        public static Vector3 OneV3 = new Vector3(1.0f, 1.0f, 1.0f);

        public void OnWillRenderObject()
        {
            var rend = GetComponent<Renderer>();
            RenderMe(rend.sharedMaterials.Where(a => a != null).ToArray());
        }

        void RenderMe(Material[] materials)
        {
            // Safeguard from recursive draws       
            if (InsideRendering)
                return;
            InsideRendering = true;

            //use current camera if not assigned
            if (PortalCamera == null)
            {
                PortalCamera = Camera.current;
            }

            if (_material)
            {
                DestroyImmediate(_material);
                _material = null;
            }
            if (Shader)
            {
                _material = new Material(Shader);
                _material.hideFlags = HideFlags.HideAndDontSave;

                if (_material.HasProperty("_CustomFloatParam2"))
                {
                    _material.SetFloat("_CustomFloatParam2", CustomFloatParam2);
                }
            }

            // Optionally disable pixel lights for reflection
            int oldPixelLightCount = QualitySettings.pixelLightCount;
            if (DisablePixelLights)
                QualitySettings.pixelLightCount = 0;

            //var mainCamOldPosition = m_Camera.transform.position;
            //var mainCamOldRotation = m_Camera.transform.rotation;
            //Camera cam = m_Camera;

            ////setup
            Camera reflectionCamera;
            CreateMirrorObjects(PortalCamera, out reflectionCamera);

            UpdateCameraModes(PortalCamera, reflectionCamera);

            reflectionCamera.cullingMask = ~(1 << 4) & ReflectLayers.value; // never render water layer
            reflectionCamera.targetTexture = _reflectionTexture1;

            Matrix4x4 destinationFlipRotation = Matrix4x4.TRS(ZeroV3, Quaternion.AngleAxis(180.0f, Vector3.up), OneV3);
            Matrix4x4 sourceInvMat = destinationFlipRotation * Source.worldToLocalMatrix;

            // Calculate translation and rotation of MainCamera in Source space
            Vector3 cameraPositionInSourceSpace = ToV3(sourceInvMat * PosToV4(MainCamera.transform.position));
            Quaternion cameraRotationInSourceSpace = QuaternionFromMatrix(sourceInvMat) * MainCamera.transform.rotation;

            // Transform Portal Camera to World Space relative to Destination transform,
            // matching the Main Camera position/orientation
            PortalCamera.transform.position = Destination.TransformPoint(cameraPositionInSourceSpace);
            PortalCamera.transform.rotation = Destination.rotation * cameraRotationInSourceSpace;

            // Calculate clip plane for portal (for culling of objects inbetween destination camera and portal)
            Vector4 clipPlaneWorldSpace = new Vector4(Destination.forward.x, Destination.forward.y, Destination.forward.z, Vector3.Dot(Destination.position, -Destination.forward));
            Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(PortalCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

            // Update projection based on new clip plane
            // Note: http://aras-p.info/texts/obliqueortho.html and http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
            reflectionCamera.projectionMatrix = MainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);

            reflectionCamera.transform.position = PortalCamera.transform.position;
            reflectionCamera.transform.rotation = PortalCamera.transform.rotation;

            reflectionCamera.Render();


            //hold texture1 unchanged!!
            Graphics.Blit(_reflectionTexture1, _reflectionTexture2);

            if (Shader != null && _material != null)
            {
                for (int i = 1; i <= Iterations; i++)
                {
                    if (_material.HasProperty("_CustomFloatParam1"))
                        _material.SetFloat("_CustomFloatParam1", i);
                    if (i % 2 == 1) //a little hack to copy textures in order from 1 to 2 than 2 to 1 and so :)
                    {
                        Graphics.Blit(_reflectionTexture2, _reflectionTexture3, _material);
                    }
                    else
                    {
                        Graphics.Blit(_reflectionTexture3, _reflectionTexture2, _material);
                    }
                }
            }

            foreach (Material mat in materials)
            {
                if (mat.HasProperty("_ReflectionTex"))
                {
                    if (Shader)
                    {
                        //Debug.Log("setting ref text for " + GetInstanceID());
                        if (Iterations % 2 == 1) //again a hack:)
                        {
                            mat.SetTexture("_ReflectionTex", _reflectionTexture3);
                        }
                        else
                        {
                            mat.SetTexture("_ReflectionTex", _reflectionTexture2);
                        }
                    }
                    else
                    {
                        mat.SetTexture("_ReflectionTex", _reflectionTexture1);
                    }
                }
            }

            // Restore pixel light count
            if (DisablePixelLights)
                QualitySettings.pixelLightCount = oldPixelLightCount;

            InsideRendering = false;
        }


        // Cleanup all the objects we possibly have created
        void OnDisable()
        {
            if (_reflectionTexture1)
            {
                DestroyImmediate(_reflectionTexture1);
                _reflectionTexture1 = null;
            }
            if (_reflectionTexture2)
            {
                DestroyImmediate(_reflectionTexture2);
                _reflectionTexture2 = null;
            }
            if (_reflectionTexture3)
            {
                DestroyImmediate(_reflectionTexture3);
                _reflectionTexture3 = null;
            }
            foreach (DictionaryEntry kvp in _reflectionCameras)
                DestroyImmediate(((Camera)kvp.Value).gameObject);
            _reflectionCameras.Clear();
        }


        private void UpdateCameraModes(Camera src, Camera dest)
        {
            if (dest == null)
                return;
            // set camera to clear the same way as current camera
            dest.clearFlags = src.clearFlags;//CameraClearFlags.SolidColor; // src.clearFlags;
            dest.backgroundColor = src.backgroundColor;
            if (src.clearFlags == CameraClearFlags.Skybox)
            {
                Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
                Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
                if (!sky || !sky.material)
                {
                    mysky.enabled = false;
                }
                else
                {
                    mysky.enabled = true;
                    mysky.material = sky.material;
                }
            }
            // update other values to match current camera.
            // even if we are supplying custom camera&projection matrices,
            // some of values are used elsewhere (e.g. skybox uses far plane)
            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.orthographic = src.orthographic;
            dest.fieldOfView = src.fieldOfView;
            dest.aspect = src.aspect;
            dest.orthographicSize = src.orthographicSize;
            dest.renderingPath = src.renderingPath;
        }

        // On-demand create any objects we need
        private void CreateMirrorObjects(Camera currentCamera, out Camera reflectionCamera)
        {
            //reflectionCamera = null;

            // Reflection render texture
            if (!_reflectionTexture1 || !_reflectionTexture2 || !_reflectionTexture3 ||
                _oldReflectionTextureSize != TextureSize)
            {
                if (_reflectionTexture1)
                    DestroyImmediate(_reflectionTexture1);
                _reflectionTexture1 = new RenderTexture(TextureSize, TextureSize, 24);
                _reflectionTexture1.name = "__MirrorReflection1" + GetInstanceID();
                _reflectionTexture1.isPowerOfTwo = true;
                _reflectionTexture1.hideFlags = HideFlags.DontSave;

                if (_reflectionTexture2)
                    DestroyImmediate(_reflectionTexture2);
                _reflectionTexture2 = new RenderTexture(TextureSize, TextureSize, 24);
                _reflectionTexture2.name = "__MirrorReflection2" + GetInstanceID();
                _reflectionTexture2.isPowerOfTwo = true;
                _reflectionTexture2.hideFlags = HideFlags.DontSave;

                if (_reflectionTexture3)
                    DestroyImmediate(_reflectionTexture3);
                _reflectionTexture3 = new RenderTexture(TextureSize, TextureSize, 24);
                _reflectionTexture3.name = "__MirrorReflection3" + GetInstanceID();
                _reflectionTexture3.isPowerOfTwo = true;
                _reflectionTexture3.hideFlags = HideFlags.DontSave;

                _oldReflectionTextureSize = TextureSize;
            }

            // Camera for reflection
            reflectionCamera = _reflectionCameras[currentCamera] as Camera;
            if (!reflectionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
            {
                GameObject go = new GameObject("Mirror Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
                reflectionCamera = go.GetComponent<Camera>();
                reflectionCamera.enabled = false;
                reflectionCamera.transform.position = transform.position;
                reflectionCamera.transform.rotation = transform.rotation;
                reflectionCamera.gameObject.AddComponent<FlareLayer>();
                go.hideFlags = HideFlags.HideAndDontSave;
                _reflectionCameras[currentCamera] = reflectionCamera;
            }
        }
    }
}
