using NatSuite.Devices;
using NatSuite.Devices.Outputs;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UtilsModule;
using System;
using System.Collections;
using UnityEngine;

namespace NatDeviceWithOpenCVForUnity.UnityUtils.Helper
{
    /// <summary>
    /// This is called every time there is the current frame image mat available.
    /// The Mat object's type is 'CV_8UC4' or 'CV_8UC3' or 'CV_8UC1' (ColorFormat is determined by the outputColorFormat setting).
    /// </summary>
    /// <param name="mat">The recently captured frame image mat.</param>
    /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
    public delegate void FrameMatAcquiredCallback(Mat mat, long timestamp);

    /// <summary>
    /// NatDeviceCamPreview to mat helper.
    /// v 1.0.3
    /// Depends on NatDevice version 1.2.0 or later.
    /// Depends on OpenCVForUnity version 2.4.4 (WebCamTextureToMatHelper v 1.1.3 or later.
    /// </summary>
    public class NatDeviceCamPreviewToMatHelper : WebCamTextureToMatHelper
    {
        /// <summary>
        /// This will be called whenever the current frame image available is converted to Mat.
        /// The Mat object's type is 'CV_8UC4' or 'CV_8UC3' or 'CV_8UC1' (ColorFormat is determined by the outputColorFormat setting).
        /// You must properly initialize the NatDeviceCamPreviewToMatHelper, 
        /// including calling Play() before this event will begin firing.
        /// </summary>
        public virtual event FrameMatAcquiredCallback frameMatAcquired;


        #region --NatDevice CameraDevice Properties--

        public virtual bool defaultForMediaType => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().defaultForMediaType : default;

        public virtual float exposureBias_device // exposureBias
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().exposureBias : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().exposureBias = value; }
        }

        public virtual (float min, float max) exposureBiasRange
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().exposureBiasRange : default; }
        }

        public virtual (float min, float max) exposureDurationRange
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().exposureDurationRange : default; }
        }

        public virtual CameraDevice.ExposureMode exposureMode
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().exposureMode : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().exposureMode = value; }
        }

        public virtual bool exposurePointSupported => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().exposurePointSupported : default;

        public virtual (float width, float height) fieldOfView
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().fieldOfView : default; }
        }

        public virtual CameraDevice.FlashMode flashMode
        {
            get { return (GetNatDeviceCameraDevice() != null && GetNatDeviceCameraDevice().running) ? GetNatDeviceCameraDevice().flashMode : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().flashMode = value; }
        }

        public virtual bool flashSupported => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().flashSupported : default;

        public virtual CameraDevice.FocusMode focusMode
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().focusMode : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().focusMode = value; }
        }

        public virtual bool focusPointSupported => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().focusPointSupported : default;

        public virtual int frameRate
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().frameRate : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().frameRate = value; }
        }

        public virtual bool frontFacing => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().frontFacing : default;

        public virtual (float min, float max) ISORange
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().ISORange : default; }
        }

        public virtual DeviceLocation location => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().location : default;

        public virtual string name_device => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().name : ""; // name

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

        public virtual bool running => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().running : default;

        public virtual bool torchEnabled
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().torchEnabled : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().torchEnabled = value; }
        }

        public virtual bool torchSupported => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().torchSupported : default;

        public virtual string uniqueID => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().uniqueID : "";

        public virtual bool whiteBalanceLock
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().whiteBalanceLock : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().whiteBalanceLock = value; }
        }

        public virtual bool whiteBalanceLockSupported => GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().whiteBalanceLockSupported : default;

        public virtual (float min, float max) zoomRange
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().zoomRange : default; }
        }

        public virtual float zoomRatio
        {
            get { return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().zoomRatio : default; }
            set { if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().zoomRatio = value; }
        }

        #endregion


        #region --NatDevice CameraDevice Functions--

        public virtual void CapturePhoto(Action<CameraImage> handler)
        {
            if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().CapturePhoto(handler);
        }

        public virtual bool ExposureModeSupported(CameraDevice.ExposureMode exposureMode)
        {
            return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().ExposureModeSupported(exposureMode) : default;
        }

        public virtual bool FocusModeSupported(CameraDevice.FocusMode focusMode)
        {
            return GetNatDeviceCameraDevice() != null ? GetNatDeviceCameraDevice().FocusModeSupported(focusMode) : default;
        }

        public virtual void SetExposureDuration(float duration, float ISO)
        {
            if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().SetExposureDuration(duration, ISO);
        }

        public virtual void SetExposurePoint(float x, float y)
        {
            if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().SetExposurePoint(x, y);
        }

        public virtual void SetFocusPoint(float x, float y)
        {
            if (GetNatDeviceCameraDevice() != null) GetNatDeviceCameraDevice().SetFocusPoint(x, y);
        }

        #endregion


        #region --NatDevice CameraImage Properties--

        public long timestamp { get; private set; }

        public bool verticallyMirrored { get; private set; }

        public Matrix4x4 intrinsics { get; private set; }

        public float exposureBias { get; private set; }

        public float exposureDuration { get; private set; }

        public float ISO { get; private set; }

        public float focalLength { get; private set; }

        public float fNumber { get; private set; }

        public float brightness { get; private set; }

        #endregion


#if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID) && !DISABLE_NATDEVICE_API

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
        protected bool didUpdatePreviewPixelBufferInCurrentFrame = false;
        protected CameraDevice cameraDevice;
        protected PixelBufferOutput previewPixelBufferOutput;
        protected int previewWidth;
        protected int previewHeight;

        protected bool didCameraRunningBeforeSuspend = false;

        protected virtual void LateUpdate()
        {
            if (didUpdateThisFrame && !didUpdatePreviewPixelBufferInCurrentFrame)
                didUpdateThisFrame = false;

            didUpdatePreviewPixelBufferInCurrentFrame = false;
        }

        protected virtual IEnumerator OnApplicationPause(bool pauseStatus)
        {
            while (isStartWaiting)
            {
                yield return null;
            }

            if (!hasInitDone)
                yield break;

            // The developer needs to do the camera suspend process oneself so that it is synchronized with the app suspend.
            if (pauseStatus)
            {
                didCameraRunningBeforeSuspend = cameraDevice.running;

                if (cameraDevice.running)
                {
                    cameraDevice.StopRunning();
                    didUpdateThisFrame = false;
                    didUpdatePreviewPixelBufferInCurrentFrame = false;
                }
            }
            else
            {
                if (!cameraDevice.running && didCameraRunningBeforeSuspend)
                {
                    isStartWaiting = true;
                    cameraDevice.StartRunning(OnPixelBufferReceived);
                }
            }
        }

        private void OnPixelBufferReceived(CameraImage image)
        {
            if (previewPixelBufferOutput == null)
                return;

            // Process only when the latest previewResolution and CameraImage size match.
            if (cameraDevice == null || cameraDevice.previewResolution.width != image.width || cameraDevice.previewResolution.height != image.height)
                return;

            timestamp = image.timestamp;
            verticallyMirrored = image.verticallyMirrored;
            float[] intrinsics = image.intrinsics;
            this.intrinsics = (intrinsics != null) ?
                new Matrix4x4(
                    new Vector4(intrinsics[0], intrinsics[3], intrinsics[6]),
                    new Vector4(intrinsics[1], intrinsics[4], intrinsics[7]),
                    new Vector4(intrinsics[2], intrinsics[5], intrinsics[8]),
                    new Vector4())
                : Matrix4x4.zero;
            exposureBias = image.exposureBias ?? -1f;
            exposureDuration = image.exposureDuration ?? -1f;
            ISO = image.ISO ?? -1f;
            focalLength = image.focalLength ?? -1f;
            fNumber = image.fNumber ?? -1f;
            brightness = image.brightness ?? -1f;


            bool firstFrame = !previewPixelBufferOutput.pixelBuffer.IsCreated;
            previewPixelBufferOutput.Update(image);

            if (firstFrame)
            {
                previewWidth = previewPixelBufferOutput.width;
                previewHeight = previewPixelBufferOutput.height;
            }

            if (isStartWaiting)
            {
                isStartWaiting = false;
            }

            didUpdateThisFrame = true;
            didUpdatePreviewPixelBufferInCurrentFrame = true;

            if (hasInitDone && frameMatAcquired != null)
            {
                frameMatAcquired.Invoke(GetMat(), timestamp);
            }
        }


        // Update is called once per frame
        protected override void Update()
        {
            if (hasInitDone)
            {
                // Catch the orientation change of the screen and correct the mat image to the correct direction.
                if (screenOrientation != Screen.orientation)
                {
                    Initialize();
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
            MediaDeviceQuery deviceQuery = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);

            if (!String.IsNullOrEmpty(requestedDeviceName))
            {
                int requestedDeviceIndex = -1;
                if (Int32.TryParse(requestedDeviceName, out requestedDeviceIndex))
                {
                    if (requestedDeviceIndex >= 0 && requestedDeviceIndex < deviceQuery.count)
                    {
                        cameraDevice = deviceQuery[requestedDeviceIndex] as CameraDevice;
                    }
                }
                else
                {
                    for (int cameraIndex = 0; cameraIndex < deviceQuery.count; cameraIndex++)
                    {
                        if (deviceQuery[cameraIndex].uniqueID == requestedDeviceName)
                        {
                            cameraDevice = deviceQuery[cameraIndex] as CameraDevice;
                            break;
                        }
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
                    cameraDevice = deviceQuery[cameraIndex] as CameraDevice;

                    if (cameraDevice != null && cameraDevice.frontFacing == requestedIsFrontFacing)
                    {
                        break;
                    }
                    cameraDevice = null;
                }
            }

            if (cameraDevice == null)
            {
                if (deviceQuery.count > 0)
                {
                    cameraDevice = deviceQuery[0] as CameraDevice;
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
            didUpdatePreviewPixelBufferInCurrentFrame = false;

            previewPixelBufferOutput = new PixelBufferOutput();
            cameraDevice.StartRunning(OnPixelBufferReceived);

            int initFrameCount = 0;
            bool isTimeout = false;

            while (true)
            {
                if (initFrameCount > timeoutFrameCount)
                {
                    isTimeout = true;
                    break;
                }
                else if (didUpdateThisFrame)
                {
                    Debug.Log("NatDeviceCamPreviewToMatHelper:: " + "name:" + cameraDevice.name + " width:" + previewWidth + " height:" + previewHeight + " fps:" + cameraDevice.frameRate
                    + " isFrongFacing:" + cameraDevice.frontFacing);

                    baseMat = new Mat(previewHeight, previewWidth, CvType.CV_8UC4);

                    if (baseColorFormat == outputColorFormat)
                    {
                        frameMat = baseMat;
                    }
                    else
                    {
                        frameMat = new Mat(baseMat.rows(), baseMat.cols(), CvType.CV_8UC(Channels(outputColorFormat)), new Scalar(0, 0, 0, 255));
                    }

                    screenOrientation = Screen.orientation;
                    screenWidth = Screen.width;
                    screenHeight = Screen.height;

                    if (rotate90Degree)
                        rotatedFrameMat = new Mat(frameMat.cols(), frameMat.rows(), CvType.CV_8UC(Channels(outputColorFormat)), new Scalar(0, 0, 0, 255));

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

            cameraDevice.StartRunning(OnPixelBufferReceived);
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
            didUpdatePreviewPixelBufferInCurrentFrame = false;
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
            return hasInitDone ? cameraDevice.name : "";
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
#if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID) && !DISABLE_NATDEVICE_API
            return cameraDevice;
#else
            return null;
#endif
        }

#if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID) && !DISABLE_NATDEVICE_API
        /// <summary>
        /// Gets the mat of the current frame.
        /// The Mat object's type is 'CV_8UC4' or 'CV_8UC3' or 'CV_8UC1' (ColorFormat is determined by the outputColorFormat setting).
        /// Please do not dispose of the returned mat as it will be reused.
        /// </summary>
        /// <returns>The mat of the current frame.</returns>
        public override Mat GetMat()
        {
            if (!hasInitDone || !cameraDevice.running || previewPixelBufferOutput == null)
            {
                return (rotatedFrameMat != null) ? rotatedFrameMat : frameMat;
            }

            if (baseColorFormat == outputColorFormat)
            {
                if (baseMat.IsDisposed)
                {
                    baseMat = new Mat(previewHeight, previewWidth, CvType.CV_8UC4);
                    frameMat = baseMat;
                }

                MatUtils.copyToMat(previewPixelBufferOutput.pixelBuffer, frameMat);
            }
            else
            {
                if (frameMat.IsDisposed)
                {
                    frameMat = new Mat(baseMat.rows(), baseMat.cols(), CvType.CV_8UC(Channels(outputColorFormat)));
                }

                MatUtils.copyToMat(previewPixelBufferOutput.pixelBuffer, baseMat);
                Imgproc.cvtColor(baseMat, frameMat, ColorConversionCodes(baseColorFormat, outputColorFormat));
            }

            FlipMat(frameMat, flipVertical, flipHorizontal);
            if (rotatedFrameMat != null)
            {
                if (rotatedFrameMat.IsDisposed)
                {
                    rotatedFrameMat = new Mat(frameMat.cols(), frameMat.rows(), CvType.CV_8UC(Channels(outputColorFormat)));
                }

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

            if (previewPixelBufferOutput != null)
            {
                previewPixelBufferOutput.Dispose();
                previewPixelBufferOutput = null;
            }

            didUpdateThisFrame = false;
            didUpdatePreviewPixelBufferInCurrentFrame = false;

            if (frameMat != null)
            {
                frameMat.Dispose();
                frameMat = null;
            }
            if (baseMat != null)
            {
                baseMat.Dispose();
                baseMat = null;
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

                frameMatAcquired = null;

                if (cameraDevice != null)
                {
                    if (this != null && this.isActiveAndEnabled)
                        StartCoroutine(_Dispose());
                }

                ReleaseResources();
            }
            else if (hasInitDone)
            {
                frameMatAcquired = null;

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