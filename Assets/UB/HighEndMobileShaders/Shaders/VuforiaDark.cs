#if VUFORIA_DARK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

namespace UB
{
    public class VuforiaDark : MonoBehaviour, IVideoBackgroundEventHandler
    {         
#region PRIVATE_MEMBERS
        private float mTextureRatioX = 0.0f;
        private float mTextureRatioY = 0.0f;

        private float mViewportSizeX = 0.0f;
        private float mViewportSizeY = 0.0f;

        private float mViewportOrigX = 0.0f;
        private float mViewportOrigY = 0.0f;

        private float mScreenWidth = 0.0f;
        private float mScreenHeight = 0.0f;

        private float mPrefixX = 0.0f;
        private float mPrefixY = 0.0f;

        private float mInversionMultiplierX = 0.0f;
        private float mInversionMultiplierY = 0.0f;

        private Texture mVideoBackgroundTexture = null;

        private bool mVideoBgConfigChanged = false;
#endregion //PRIVATE_MEMBERS


#region MONOBEHAVIOUR_METHODS
        void Start()
        {
            // register for the OnVideoBackgroundConfigChanged event at the VuforiaBehaviour
            VuforiaARController.Instance.RegisterVideoBgEventHandler(this);
        }

        void Update()
        {
            if (mVideoBgConfigChanged && VuforiaRenderer.Instance.IsVideoBackgroundInfoAvailable())
            {
                UpdateVideoTexture();
                mVideoBgConfigChanged = false;
            }

            Material mat = GetComponent<Renderer>().material;
            mat.SetFloat("_TextureRatioX", mTextureRatioX);
            mat.SetFloat("_TextureRatioY", mTextureRatioY);
            mat.SetFloat("_ViewportSizeX", mViewportSizeX);
            mat.SetFloat("_ViewportSizeY", mViewportSizeY);
            mat.SetFloat("_ViewportOrigX", mViewportOrigX);
            mat.SetFloat("_ViewportOrigY", mViewportOrigY);
            mat.SetFloat("_ScreenWidth", mScreenWidth);
            mat.SetFloat("_ScreenHeight", mScreenHeight);
            mat.SetFloat("_PrefixX", mPrefixX);
            mat.SetFloat("_PrefixY", mPrefixY);
            mat.SetFloat("_InversionMultiplierX", mInversionMultiplierX);
            mat.SetFloat("_InversionMultiplierY", mInversionMultiplierY);
        }

        void OnDestroy()
        {
            // unregister for the OnVideoBackgroundConfigChanged event at the VuforiaBehaviour
            VuforiaARController.Instance.UnregisterVideoBgEventHandler(this);
        }

#endregion //MONOBEHAVIOUR_METHODS



#region PRIVATE_METHODS
        private void UpdateVideoTexture()
        {
            if (mVideoBackgroundTexture != VuforiaRenderer.Instance.VideoBackgroundTexture)
            {
                mVideoBackgroundTexture = VuforiaRenderer.Instance.VideoBackgroundTexture;
                GetComponent<Renderer>().material.mainTexture = mVideoBackgroundTexture;
            }

            VuforiaRenderer.VideoTextureInfo textureInfo = VuforiaRenderer.Instance.GetVideoTextureInfo();
            float ratioX = (float)textureInfo.imageSize.x / (float)textureInfo.textureSize.x;
            float ratioY = (float)textureInfo.imageSize.y / (float)textureInfo.textureSize.y;
            SetTextureRatio(ratioX, ratioY);
        }

        private void SetTextureRatio(float ratioX, float ratioY)
        {
            mTextureRatioX = ratioX;
            mTextureRatioY = ratioY;
            SetViewportParameters();
        }

        private void SetViewportParameters()
        {
            var vuforiaBehaviour = VuforiaARController.Instance;

            // update viewport size
            Rect viewport = vuforiaBehaviour.GetVideoBackgroundRectInViewPort();
            mViewportOrigX = viewport.xMin;
            mViewportOrigY = viewport.yMin;
            mViewportSizeX = viewport.xMax - viewport.xMin;
            mViewportSizeY = viewport.yMax - viewport.yMin;
            // update screen size
            mScreenWidth = Screen.width;
            mScreenHeight = Screen.height;

            bool isMirrored = vuforiaBehaviour.VideoBackGroundMirrored == VuforiaRenderer.VideoBackgroundReflection.ON;

            // determine for which orientation the shaders should be set up:
            switch (VuforiaRuntimeUtilities.ScreenOrientation)
            {
                case ScreenOrientation.Portrait:
                    SetParametersForPortraitNormal(isMirrored);
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    SetParametersForPortraitUpsideDown(isMirrored);
                    break;
                case ScreenOrientation.LandscapeLeft:
                    SetParametersForLandscapeLeft(isMirrored);
                    break;
                case ScreenOrientation.LandscapeRight:
                    SetParametersForLandscapeRight(isMirrored);
                    break;
            }
        }

        private void SetParametersForPortraitNormal(bool isMirrored)
        {
            Shader.DisableKeyword("PORTRAIT_OFF");
            Shader.EnableKeyword("PORTRAIT_ON");

            if (isMirrored)
            {
                mPrefixX = 0.0f;
                mPrefixY = 1.0f;

                mInversionMultiplierX = 1.0f;
                mInversionMultiplierY = -1.0f;
            }
            else
            {
                mPrefixX = 1.0f;
                mPrefixY = 1.0f;

                mInversionMultiplierX = -1.0f;
                mInversionMultiplierY = -1.0f;
            }
        }

        private void SetParametersForPortraitUpsideDown(bool isMirrored)
        {
            Shader.DisableKeyword("PORTRAIT_OFF");
            Shader.EnableKeyword("PORTRAIT_ON");

            if (isMirrored)
            {
                mPrefixX = 1.0f;
                mPrefixY = 0.0f;

                mInversionMultiplierX = -1.0f;
                mInversionMultiplierY = 1.0f;
            }
            else
            {
                mPrefixX = 0.0f;
                mPrefixY = 0.0f;

                mInversionMultiplierX = 1.0f;
                mInversionMultiplierY = 1.0f;
            }
        }

        private void SetParametersForLandscapeLeft(bool isMirrored)
        {
            Shader.DisableKeyword("PORTRAIT_ON");
            Shader.EnableKeyword("PORTRAIT_OFF");

            if (isMirrored)
            {
                mPrefixX = 1.0f;
                mPrefixY = 1.0f;

                mInversionMultiplierX = -1.0f;
                mInversionMultiplierY = -1.0f;
            }
            else
            {
                mPrefixX = 0.0f;
                mPrefixY = 1.0f;

                mInversionMultiplierX = 1.0f;
                mInversionMultiplierY = -1.0f;
            }
        }

        private void SetParametersForLandscapeRight(bool isMirrored)
        {
            Shader.DisableKeyword("PORTRAIT_ON");
            Shader.EnableKeyword("PORTRAIT_OFF");

            if (isMirrored)
            {
                mPrefixX = 0.0f;
                mPrefixY = 0.0f;

                mInversionMultiplierX = 1.0f;
                mInversionMultiplierY = 1.0f;
            }
            else
            {
                mPrefixX = 1.0f;
                mPrefixY = 0.0f;

                mInversionMultiplierX = -1.0f;
                mInversionMultiplierY = 1.0f;
            }
        }
#endregion //PRIVATE_METHODS


#region PUBLIC_METHODS
        public void OnVideoBackgroundConfigChanged()
        {
            mVideoBgConfigChanged = true;
        }
#endregion //PUBLIC_METHODS
    }
}
#endif