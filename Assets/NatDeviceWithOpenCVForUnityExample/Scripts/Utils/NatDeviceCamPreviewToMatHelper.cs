using NatSuite.Devices;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace NatDeviceWithOpenCVForUnity.UnityUtils.Helper
{
    /// <summary>
    /// NatDeviceCamPreview to mat helper.
    /// v 1.0.0
    /// Depends on NatDevice version 1.0.0 or later.
    /// Depends on OpenCVForUnity version 2.3.7 or later.
    /// </summary>
    public class NatDeviceCamPreviewToMatHelper : WebCamTextureToMatHelper
    {
        #region --NatDevice CameraDevice Properties--

        public virtual bool running => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().running : default;


        public virtual string uniqueID => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().uniqueID : default;

        public virtual bool frontFacing => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().frontFacing : default;
        
        public virtual bool flashSupported => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().flashSupported : default;

        public virtual bool torchSupported => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().torchSupported : default;

        public virtual bool exposureLockSupported => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().exposureLockSupported : default;

        public virtual bool focusLockSupported => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().focusLockSupported : default;

        public virtual bool whiteBalanceLockSupported => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().whiteBalanceLockSupported : default;

        public virtual (float width, float height) fieldOfView
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().fieldOfView : default; }
        }

        public virtual (float min, float max) exposureRange
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().exposureRange : default; }
        }

        public virtual (float min, float max) zoomRange
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().zoomRange : default; }
        }

        public virtual (int width, int height) previewResolution
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().previewResolution : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().previewResolution = (width: value.width, height: value.height); }
        }

        public virtual (int width, int height) photoResolution
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().photoResolution : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().photoResolution = (width: value.width, height: value.height); }
        }

        public virtual int frameRate
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().frameRate : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().frameRate = value; }
        }

        public virtual float exposureBias
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().exposureBias : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().exposureBias = value; }
        }

        public virtual bool exposureLock
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().exposureLock : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().exposureLock = value; }
        }

        public virtual (float x, float y) exposurePoint
        {
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().exposurePoint = (x: value.x, y: value.y); }
        }

        public virtual FlashMode flashMode
        {
            get { return (GetNatDeviceCameraDevice() != null && GetNatDeviceCameraDevice().running) ? GetNatDeviceCameraDevice().flashMode : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().flashMode = value; }
        }

        public virtual bool focusLock
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().focusLock : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().focusLock = value; }
        }

        public virtual (float x, float y) focusPoint
        {
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().exposurePoint = (x: value.x, y: value.y); }
        }

        public virtual bool torchEnabled
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().torchEnabled : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().torchEnabled = value; }
        }

        public virtual bool whiteBalanceLock
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().whiteBalanceLock : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().whiteBalanceLock = value; }
        }

        public virtual float zoomRatio
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().zoomRatio : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().zoomRatio = value; }
        }

        public virtual FrameOrientation orientation
        {
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().orientation = value; }
        }
        #endregion


#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR && !DISABLE_NATDEVICE_API

        public override float requestedFPS
        {
            get { return _requestedFPS; }
            set
            {
                _requestedFPS = Mathf.Clamp(value, -1f, float.MaxValue);
                if (hasInitDone)
                {
                    Initialize();
                }
            }
        }

        protected bool isStartWaiting = false;
        protected bool didUpdateThisFrame = false;
        protected MediaDeviceQuery deviceQuery;
        protected CameraDevice cameraDevice;
        protected Texture2D preview;

        // Update is called once per frame
        protected override void Update()
        {
            if (hasInitDone)
            {
                // Catch the orientation change of the screen and correct the mat image to the correct direction.
                if (screenOrientation != Screen.orientation && (screenWidth != Screen.width || screenHeight != Screen.height))
                {
                    switch (Screen.orientation)
                    {
                        case ScreenOrientation.LandscapeLeft: cameraDevice.orientation = FrameOrientation.LandscapeLeft; break;
                        case ScreenOrientation.Portrait: cameraDevice.orientation = FrameOrientation.Portrait; break;
                        case ScreenOrientation.LandscapeRight: cameraDevice.orientation = FrameOrientation.LandscapeRight; break;
                        case ScreenOrientation.PortraitUpsideDown: cameraDevice.orientation = FrameOrientation.PortraitUpsideDown; break;
                    }

                    Initialize();
                }
                else
                {
                    screenWidth = Screen.width;
                    screenHeight = Screen.height;
                }
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        protected override void OnDestroy()
        {
            Dispose();

            if (cameraDevice != null && cameraDevice.running)
                cameraDevice.StopRunning();
            cameraDevice = null;
        }

        /// <summary>
        /// Initializes this instance by coroutine.
        /// </summary>
        protected override IEnumerator _Initialize()
        {
            if (hasInitDone)
            {
                ReleaseResources();

                if (onDisposed != null)
                    onDisposed.Invoke();
            }

            isInitWaiting = true;

            while (isStartWaiting)
            {
                yield return null;
            }

            if (cameraDevice != null && cameraDevice.running)
                cameraDevice.StopRunning();
            cameraDevice = null;


            // Checks camera permission state.
            IEnumerator coroutine = hasUserAuthorizedCameraPermission();
            yield return coroutine;

            if (!(bool)coroutine.Current)
            {
                isInitWaiting = false;
                initCoroutine = null;

                if (onErrorOccurred != null)
                    onErrorOccurred.Invoke(ErrorCode.CAMERA_PERMISSION_DENIED);

                yield break;
            }


            // Creates the camera
            deviceQuery = deviceQuery ?? new MediaDeviceQuery(MediaDeviceQuery.Criteria.CameraDevice);

            //var devices = deviceQuery.devices;
            if (!String.IsNullOrEmpty(requestedDeviceName))
            {
                int requestedDeviceIndex = -1;
                if (Int32.TryParse(requestedDeviceName, out requestedDeviceIndex))
                {
                    if (requestedDeviceIndex >= 0 && requestedDeviceIndex < deviceQuery.count)
                    {
                        cameraDevice = deviceQuery.currentDevice as CameraDevice;
                    }
                }
                else
                {
                    for (int cameraIndex = 0; cameraIndex < deviceQuery.count; cameraIndex++)
                    {
                        if (deviceQuery.currentDevice.uniqueID == requestedDeviceName)
                        {
                            cameraDevice = deviceQuery.currentDevice as CameraDevice;
                            break;
                        }
                        deviceQuery.Advance();
                    }
                }
                if (cameraDevice == null)
                    Debug.Log("Cannot find camera device " + requestedDeviceName + ".");
            }
            
            if (cameraDevice == null)
            {
                // Checks how many and which cameras are available on the device
                for (int cameraIndex = 0; cameraIndex < deviceQuery.count; cameraIndex++)
                {
                    cameraDevice = deviceQuery.currentDevice as CameraDevice;

                    if (cameraDevice != null && cameraDevice.frontFacing == requestedIsFrontFacing)
                    {
                        break;
                    }
                    deviceQuery.Advance();
                }
            }
            
            if (cameraDevice == null)
            {
                if (deviceQuery.count > 0)
                {
                    cameraDevice = deviceQuery.currentDevice as CameraDevice;
                }

                if (cameraDevice == null)
                {
                    isInitWaiting = false;
                    initCoroutine = null;

                    if (onErrorOccurred != null)
                        onErrorOccurred.Invoke(ErrorCode.CAMERA_DEVICE_NOT_EXIST);

                    yield break;
                }
            }

            // Set the camera's preview resolution and frameRate
            cameraDevice.previewResolution = (width: requestedWidth, height: requestedHeight);
            cameraDevice.frameRate = (int)requestedFPS;

            // Starts the camera
            isStartWaiting = true;
            didUpdateThisFrame = false;
            bool isError = false;
            cameraDevice.StartRunning().ContinueWith(
                task => {
                    if (task.Exception == null)
                    {
                        preview = task.Result;

                        didUpdateThisFrame = true;
                    }
                    else
                    {
                        isError = true;

                        Debug.LogError(task.Exception.Message);
                    }
                    isStartWaiting = false;
                },
                TaskContinuationOptions.ExecuteSynchronously
                );


            int initFrameCount = 0;
            bool isTimeout = false;

            while (true)
            {
                if (isError)
                {
                    cameraDevice = null;

                    isInitWaiting = false;
                    initCoroutine = null;

                    if (onErrorOccurred != null)
                        onErrorOccurred.Invoke(ErrorCode.UNKNOWN);

                    break;
                }

                if (initFrameCount > timeoutFrameCount)
                {
                    isTimeout = true;
                    break;
                }
                else if (didUpdateThisFrame)
                {

                    Debug.Log("NatDeviceCamPreviewToMatHelper:: " + "UniqueID:" + cameraDevice.uniqueID + " width:" + preview.width + " height:" + preview.height + " fps:" + cameraDevice.frameRate
                    + " isFrongFacing:" + cameraDevice.frontFacing);

                    frameMat = new Mat(preview.height, preview.width, CvType.CV_8UC4);

                    screenOrientation = Screen.orientation;
                    screenWidth = Screen.width;
                    screenHeight = Screen.height;

                    if (rotate90Degree)
                        rotatedFrameMat = new Mat(preview.width, preview.height, CvType.CV_8UC4);

                    isInitWaiting = false;
                    hasInitDone = true;
                    initCoroutine = null;

                    if (onInitialized != null)
                        onInitialized.Invoke();

                    break;
                }
                else
                {
                    initFrameCount++;
                    yield return null;
                }
            }

            if (isTimeout)
            {
                if (cameraDevice.running)
                    cameraDevice.StopRunning();
                cameraDevice = null;

                isInitWaiting = false;
                initCoroutine = null;

                if (onErrorOccurred != null)
                    onErrorOccurred.Invoke(ErrorCode.TIMEOUT);
            }
        }

        /// <summary>
        /// Starts the camera.
        /// </summary>
        public override void Play()
        {
            if (hasInitDone)
                StartCoroutine(_Play());
        }

        protected virtual IEnumerator _Play()
        {
            while (isStartWaiting)
            {
                yield return null;
            }

            if (!hasInitDone || cameraDevice.running) yield break;

            isStartWaiting = true;
            cameraDevice.StartRunning().ContinueWith(
                task => {
                    if (task.Exception == null)
                    {
                        preview = task.Result;

                        didUpdateThisFrame = true;
                    }
                    else
                    {
                        Debug.LogError(task.Exception.Message);
                    }
                    isStartWaiting = false;
                },
                TaskContinuationOptions.ExecuteSynchronously
                );
        }

        /// <summary>
        /// Pauses the active camera.
        /// </summary>
        public override void Pause()
        {
            if (hasInitDone)
                StartCoroutine(_Stop());
        }

        /// <summary>
        /// Stops the active camera.
        /// </summary>
        public override void Stop()
        {
            if (hasInitDone)
                StartCoroutine(_Stop());
        }
        protected virtual IEnumerator _Stop()
        {
            while (isStartWaiting)
            {
                yield return null;
            }

            if (!hasInitDone || !cameraDevice.running) yield break;

            cameraDevice.StopRunning();
            didUpdateThisFrame = false;
        }

        /// <summary>
        /// Indicates whether the active camera is currently playing.
        /// </summary>
        /// <returns><c>true</c>, if the active camera is playing, <c>false</c> otherwise.</returns>
        public override bool IsPlaying()
        {
            return hasInitDone ? cameraDevice.running : false;
        }

        /// <summary>
        /// Indicates whether the active camera device is currently front facng.
        /// </summary>
        /// <returns><c>true</c>, if the active camera device is front facng, <c>false</c> otherwise.</returns>
        public override bool IsFrontFacing()
        {
            return hasInitDone ? cameraDevice.frontFacing : false;
        }

        /// <summary>
        /// Returns the active camera device name.
        /// </summary>
        /// <returns>The active camera device name.</returns>
        public override string GetDeviceName()
        {
            return hasInitDone ? cameraDevice.uniqueID : "";
        }

        /// <summary>
        /// Returns the active camera framerate.
        /// </summary>
        /// <returns>The active camera framerate.</returns>
        public override float GetFPS()
        {
            return hasInitDone ? cameraDevice.frameRate : -1f;
        }

        /// <summary>
        /// Returns the active WebcamTexture.
        /// </summary>
        /// <returns>The active WebcamTexture.</returns>
        public override WebCamTexture GetWebCamTexture()
        {
            return null;
        }

        /// <summary>
        /// Indicates whether the video buffer of the frame has been updated.
        /// </summary>
        /// <returns><c>true</c>, if the video buffer has been updated <c>false</c> otherwise.</returns>
        public override bool DidUpdateThisFrame()
        {
            if (!hasInitDone)
                return false;

            return didUpdateThisFrame;
        }
#endif

        /// <summary>
        /// Returns the NatDevice camera device.
        /// </summary>
        /// <returns>The NatDevice camera device.</returns>
        public virtual CameraDevice GetNatDeviceCameraDevice()
        {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR && !DISABLE_NATDEVICE_API
            return cameraDevice;
#else
            return null;
#endif
        }

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR && !DISABLE_NATDEVICE_API
        /// <summary>
        /// Gets the mat of the current frame.
        /// The Mat object's type is 'CV_8UC4' (RGBA).
        /// </summary>
        /// <returns>The mat of the current frame.</returns>
        public override Mat GetMat()
        {
            if (!hasInitDone || !cameraDevice.running)
            {
                return (rotatedFrameMat != null) ? rotatedFrameMat : frameMat;
            }

            Utils.fastTexture2DToMat(preview, frameMat, false);

            FlipMat(frameMat, flipVertical, flipHorizontal);
            if (rotatedFrameMat != null)
            {
                Core.rotate(frameMat, rotatedFrameMat, Core.ROTATE_90_CLOCKWISE);
                return rotatedFrameMat;
            }
            else
            {
                return frameMat;
            }
        }

        /// <summary>
        /// Flips the mat.
        /// </summary>
        /// <param name="mat">Mat.</param>
        protected override void FlipMat(Mat mat, bool flipVertical, bool flipHorizontal)
        {
            int flipCode = 0;

            if (flipVertical)
            {
                if (flipCode == int.MinValue)
                {
                    flipCode = 0;
                }
                else if (flipCode == 0)
                {
                    flipCode = int.MinValue;
                }
                else if (flipCode == 1)
                {
                    flipCode = -1;
                }
                else if (flipCode == -1)
                {
                    flipCode = 1;
                }
            }

            if (flipHorizontal)
            {
                if (flipCode == int.MinValue)
                {
                    flipCode = 1;
                }
                else if (flipCode == 0)
                {
                    flipCode = -1;
                }
                else if (flipCode == 1)
                {
                    flipCode = int.MinValue;
                }
                else if (flipCode == -1)
                {
                    flipCode = 0;
                }
            }

            if (flipCode > int.MinValue)
            {
                Core.flip(mat, mat, flipCode);
            }
        }

        /// <summary>
        /// To release the resources.
        /// </summary>
        protected override void ReleaseResources()
        {
            isInitWaiting = false;
            hasInitDone = false;

            preview = null;

            didUpdateThisFrame = false;

            if (frameMat != null)
            {
                frameMat.Dispose();
                frameMat = null;
            }
            if (rotatedFrameMat != null)
            {
                rotatedFrameMat.Dispose();
                rotatedFrameMat = null;
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="WebCamTextureToMatHelper"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="WebCamTextureToMatHelper"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="WebCamTextureToMatHelper"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="WebCamTextureToMatHelper"/> so
        /// the garbage collector can reclaim the memory that the <see cref="WebCamTextureToMatHelper"/> was occupying.</remarks>
        public override void Dispose()
        {
            if (colors != null)
                colors = null;

            if (isInitWaiting)
            {

                CancelInitCoroutine();

                if (cameraDevice != null)
                {
                    if (this != null && this.isActiveAndEnabled)
                        StartCoroutine(_Dispose());
                }

                ReleaseResources();
            }
            else if (hasInitDone)
            {
                if (this != null && this.isActiveAndEnabled)
                    StartCoroutine(_Dispose());

                ReleaseResources();

                if (onDisposed != null)
                    onDisposed.Invoke();
            }
        }

        protected virtual IEnumerator _Dispose()
        {
            while (isStartWaiting)
            {
                yield return null;
            }

            if (cameraDevice.running)
                cameraDevice.StopRunning();
            cameraDevice = null;
        }
#endif
    }
}