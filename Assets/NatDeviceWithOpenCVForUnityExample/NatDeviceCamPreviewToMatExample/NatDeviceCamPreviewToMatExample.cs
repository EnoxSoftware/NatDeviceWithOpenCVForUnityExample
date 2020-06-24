using NatSuite.Devices;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UtilsModule;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if OPENCV_USE_UNSAFE_CODE && UNITY_2018_2_OR_NEWER
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
#endif

namespace NatDeviceWithOpenCVForUnityExample
{

    /// <summary>
    /// NatDeviceCamPreview To Mat Example
    /// An example of converting a NatDevice camera preview image to OpenCV's Mat format.
    /// </summary>
    public class NatDeviceCamPreviewToMatExample : ExampleBase<NatDeviceCamSource>
    {

        public enum MatCaptureMethod
        {
            GetRawTextureData_ByteArray,
            GetRawTextureData_NativeArray,
        }

        [Header("OpenCV")]
        public MatCaptureMethod matCaptureMethod = MatCaptureMethod.GetRawTextureData_ByteArray;
        public Dropdown matCaptureMethodDropdown;

        Mat frameMatrix;
        Mat grayMatrix;
        Texture2D texture;

        FpsMonitor fpsMonitor;

        #region --ExampleBase--

        protected override async void Start()
        {
            base.Start();

#if !UNITY_STANDALONE_WIN && !UNITY_EDITOR
            // Request camera permissions
            if (!await MediaDeviceQuery.RequestPermissions<CameraDevice>())
            {
                Debug.LogError("User did not grant camera permissions");
                return;
            }
#endif

            // Load global camera benchmark settings.
            int width, height, framerate;
            NatDeviceWithOpenCVForUnityExample.CameraConfiguration(out width, out height, out framerate);
            // Create camera source
            cameraSource = new NatDeviceCamSource(width, height, framerate, useFrontCamera);
            if (cameraSource.activeCamera == null)
                cameraSource = new NatDeviceCamSource(width, height, framerate, !useFrontCamera);
            await cameraSource.StartRunning(OnStart, OnFrame);
            // Update UI
            matCaptureMethodDropdown.value = (int)matCaptureMethod;

            fpsMonitor = GetComponent<FpsMonitor>();
            if (fpsMonitor != null)
            {
                fpsMonitor.Add("Name", "NatDeviceCamPreviewToMatExample");
                fpsMonitor.Add("onFrameFPS", onFrameFPS.ToString("F1"));
                fpsMonitor.Add("drawFPS", drawFPS.ToString("F1"));
                fpsMonitor.Add("width", "");
                fpsMonitor.Add("height", "");
                fpsMonitor.Add("orientation", "");
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Create matrices
            if (frameMatrix != null)
                frameMatrix.Dispose();
            frameMatrix = new Mat(cameraSource.height, cameraSource.width, CvType.CV_8UC4);
            if (grayMatrix != null)
                grayMatrix.Dispose();
            grayMatrix = new Mat(cameraSource.height, cameraSource.width, CvType.CV_8UC1);
            // Create texture
            if (texture != null)
                Texture2D.Destroy(texture);
            texture = new Texture2D(
                cameraSource.width,
                cameraSource.height,
                TextureFormat.RGBA32,
                false,
                false
            );
            // Display preview
            rawImage.texture = texture;
            aspectFitter.aspectRatio = cameraSource.width / (float)cameraSource.height;
            Debug.Log("NatDevice camera source started with resolution: " + cameraSource.width + "x" + cameraSource.height + " isFrontFacing: " + cameraSource.isFrontFacing);
            // Log camera properties
            var cameraProps = new Dictionary<string, string>();
            var camera = cameraSource.activeCamera as CameraDevice;
            if (camera != null)
            {
                cameraProps.Add("exposureBias", camera.exposureBias.ToString());
                cameraProps.Add("exposureLock", camera.exposureLock.ToString());
                cameraProps.Add("exposureLockSupported", camera.exposureLockSupported.ToString());
                cameraProps.Add("exposureRange", camera.exposureRange.max + "x" + camera.exposureRange.min);
                cameraProps.Add("fieldOfView", camera.fieldOfView.width + "x" + camera.fieldOfView.height);
                cameraProps.Add("flashMode", camera.flashMode.ToString());
                cameraProps.Add("flashSupported", camera.flashSupported.ToString());
                cameraProps.Add("focusLock", camera.focusLock.ToString());
                cameraProps.Add("focusLockSupported", camera.focusLockSupported.ToString());
                cameraProps.Add("frameRate", camera.frameRate.ToString());
                cameraProps.Add("frontFacing", camera.frontFacing.ToString());
                cameraProps.Add("photoResolution", camera.photoResolution.width + "x" + camera.photoResolution.height);
                cameraProps.Add("previewResolution", camera.previewResolution.width + "x" + camera.previewResolution.height);
                cameraProps.Add("running", camera.running.ToString());
                cameraProps.Add("torchEnabled", camera.torchEnabled.ToString());
                cameraProps.Add("torchSupported", camera.torchSupported.ToString());
                cameraProps.Add("uniqueID", camera.uniqueID.ToString());
                cameraProps.Add("whiteBalanceLock", camera.whiteBalanceLock.ToString());
                cameraProps.Add("whiteBalanceLockSupported", camera.whiteBalanceLockSupported.ToString());
                cameraProps.Add("zoomRange", camera.zoomRange.max + "x" + camera.zoomRange.min);
                cameraProps.Add("zoomRatio", camera.zoomRatio.ToString());
            }
            Debug.Log("# Active Camera Properties #####################");
            foreach (string key in cameraProps.Keys)
                Debug.Log(key + ": " + cameraProps[key]);
            Debug.Log("#######################################");

            if (fpsMonitor != null)
            {
                fpsMonitor.Add("width", cameraSource.width.ToString());
                fpsMonitor.Add("height", cameraSource.height.ToString());
                fpsMonitor.Add("orientation", Screen.orientation.ToString());

                fpsMonitor.boxWidth = 240;
                fpsMonitor.boxHeight = 800;
                fpsMonitor.LocateGUI();

                foreach (string key in cameraProps.Keys)
                    fpsMonitor.Add(key, cameraProps[key]);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (updateCount == 0)
            {
                if (fpsMonitor != null)
                {
                    fpsMonitor.Add("onFrameFPS", onFrameFPS.ToString("F1"));
                    fpsMonitor.Add("drawFPS", drawFPS.ToString("F1"));
                    fpsMonitor.Add("orientation", Screen.orientation.ToString());
                }
            }
        }

        protected override void UpdateTexture()
        {
            // Get the matrix
            switch (matCaptureMethod)
            {
                case MatCaptureMethod.GetRawTextureData_ByteArray:
                    MatUtils.copyToMat(cameraSource.preview.GetRawTextureData(), frameMatrix);
                    Core.flip(frameMatrix, frameMatrix, 0);
                    break;
                case MatCaptureMethod.GetRawTextureData_NativeArray:

#if OPENCV_USE_UNSAFE_CODE && UNITY_2018_2_OR_NEWER
                    // non-memory allocation.
                    unsafe
                    {
                        var ptr = (IntPtr)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(cameraSource.preview.GetRawTextureData<byte>());
                        MatUtils.copyToMat(ptr, frameMatrix);
                    }
                    Core.flip(frameMatrix, frameMatrix, 0);
#else
                    MatUtils.copyToMat(cameraSource.preview.GetRawTextureData(), frameMatrix);
                    Core.flip(frameMatrix, frameMatrix, 0);
                    Imgproc.putText(frameMatrix, "NativeArray<T> GetRawTextureData() method can be used from Unity 2018.2 or later.", new Point(5, frameMatrix.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar(255, 255, 255, 255), 1, Imgproc.LINE_AA, false);
#endif
                    break;
            }

            ProcessImage(frameMatrix, grayMatrix, imageProcessingType);

            // Convert to Texture2D
            Utils.fastMatToTexture2D(frameMatrix, texture);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (frameMatrix != null)
                frameMatrix.Dispose();
            if (grayMatrix != null)
                grayMatrix.Dispose();
            frameMatrix =
            grayMatrix = null;
            Texture2D.Destroy(texture);
            texture = null;
        }

#endregion


#region --UI Callbacks--

        public void OnMatCaptureMethodDropdownValueChanged(int result)
        {
            matCaptureMethod = (MatCaptureMethod)result;
        }

#endregion

    }
}