using NatSuite.Devices;
using System.Collections.Generic;
using UnityEngine;

namespace NatDeviceWithOpenCVForUnityExample
{

    /// <summary>
    /// NatDeviceCamPreview Only Example
    /// An example of displaying the preview frame of camera only using NatDevice.
    /// </summary>
    public class NatDeviceCamPreviewOnlyExample : ExampleBase<NatDeviceCamSource>
    {

        Texture2D texture;
        byte[] pixelBuffer;

        FpsMonitor fpsMonitor;

        protected override async void Start()
        {
            base.Start();

            fpsMonitor = GetComponent<FpsMonitor>();
            if (fpsMonitor != null)
            {
                fpsMonitor.Add("Name", "NatDeviceCamPreviewOnlyExample");
                fpsMonitor.Add("onFrameFPS", onFrameFPS.ToString("F1"));
                fpsMonitor.Add("drawFPS", drawFPS.ToString("F1"));
                fpsMonitor.Add("width", "");
                fpsMonitor.Add("height", "");
                fpsMonitor.Add("isFrontFacing", "");
                fpsMonitor.Add("orientation", "");
            }

            // Request camera permissions
            if (!await MediaDeviceQuery.RequestPermissions<CameraDevice>())
            {
                Debug.LogError("User did not grant camera permissions");
                return;
            }

            // Load global camera benchmark settings.
            int width, height, framerate;
            NatDeviceWithOpenCVForUnityExample.CameraConfiguration(out width, out height, out framerate);
            // Create camera source
            cameraSource = new NatDeviceCamSource(width, height, framerate, useFrontCamera);
            if (cameraSource.activeCamera == null)
                cameraSource = new NatDeviceCamSource(width, height, framerate, !useFrontCamera);
            await cameraSource.StartRunning(OnStart, OnFrame);
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Create pixel buffer
            pixelBuffer = new byte[cameraSource.width * cameraSource.height * 4];
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
            var camera = cameraSource.activeCamera;
            if (camera != null)
            {
                cameraProps.Add("exposureBias", camera.exposureBias.ToString());
                cameraProps.Add("exposureLock", camera.exposureLock.ToString());
                cameraProps.Add("exposureLockSupported", camera.exposureLockSupported.ToString());
                cameraProps.Add("exposurePointSupported", camera.exposurePointSupported.ToString());
                cameraProps.Add("exposureRange", camera.exposureRange.max + "x" + camera.exposureRange.min);
                cameraProps.Add("fieldOfView", camera.fieldOfView.width + "x" + camera.fieldOfView.height);
                cameraProps.Add("flashMode", camera.flashMode.ToString());
                cameraProps.Add("flashSupported", camera.flashSupported.ToString());
                cameraProps.Add("focusLock", camera.focusLock.ToString());
                cameraProps.Add("focusLockSupported", camera.focusLockSupported.ToString());
                cameraProps.Add("focusPointSupported", camera.focusPointSupported.ToString());
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
                fpsMonitor.Add("isFrontFacing", cameraSource.isFrontFacing.ToString());
                fpsMonitor.Add("orientation", Screen.orientation.ToString());

                fpsMonitor.boxWidth = 240;
                fpsMonitor.boxHeight = 830;
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
            cameraSource.CaptureFrame(pixelBuffer);
            ProcessImage(pixelBuffer, texture.width, texture.height, imageProcessingType);
            texture.LoadRawTextureData(pixelBuffer);
            texture.Apply();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Texture2D.Destroy(texture);
            texture = null;
            pixelBuffer = null;
        }

        protected virtual async void OnApplicationPause(bool pauseStatus)
        {
            if (cameraSource == null || cameraSource.activeCamera == null)
                return;

            // The developer needs to do the camera suspend process oneself so that it is synchronized with the app suspend.
            if (pauseStatus)
            {
                if (cameraSource.isRunning)
                    cameraSource.StopRunning();
            }
            else
            {
                if (!cameraSource.isRunning)
                    await cameraSource.StartRunning(OnStart, OnFrame);
            }
        }
    }
}