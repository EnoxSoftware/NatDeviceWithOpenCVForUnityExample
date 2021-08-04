using NatSuite.Devices;
using NatSuite.Sharing;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace NatDeviceWithOpenCVForUnityExample
{

    /// <summary>
    /// Integration With NatShare Example
    /// An example of the native sharing and save to the camera roll using NatShare.
    /// </summary>
    public class IntegrationWithNatShareExample : ExampleBase<NatDeviceCamSource>
    {
        public Toggle applyComicFilterToggle;

        Mat frameMatrix;
        Texture2D texture;
        ComicFilter comicFilter;

        FpsMonitor fpsMonitor;

        string exampleTitle = "";
        string exampleSceneTitle = "";
        string settingInfo1 = "";
        Scalar textColor = new Scalar(255, 255, 255, 255);
        Point textPos = new Point();

        protected override async void Start()
        {
            base.Start();

            fpsMonitor = GetComponent<FpsMonitor>();
            if (fpsMonitor != null)
            {
                fpsMonitor.Add("Name", "IntegrationWithNatShareExample");
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
            // Create comic filter
            comicFilter = new ComicFilter();

            exampleTitle = "[NatDeviceWithOpenCVForUnity Example] (" + NatDeviceWithOpenCVForUnityExample.GetNatDeviceVersion() + ")";
            exampleSceneTitle = "- Integration With NatShare Example";
        }

        protected override void OnStart()
        {
            settingInfo1 = "- resolution: " + cameraSource.width + "x" + cameraSource.height;

            // Create matrix
            if (frameMatrix != null)
                frameMatrix.Dispose();
            frameMatrix = new Mat(cameraSource.height, cameraSource.width, CvType.CV_8UC4);
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

            if (fpsMonitor != null)
            {
                fpsMonitor.Add("width", cameraSource.width.ToString());
                fpsMonitor.Add("height", cameraSource.height.ToString());
                fpsMonitor.Add("isFrontFacing", cameraSource.isFrontFacing.ToString());
                fpsMonitor.Add("orientation", Screen.orientation.ToString());
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
            cameraSource.CaptureFrame(frameMatrix);

            if (applyComicFilterToggle.isOn)
                comicFilter.Process(frameMatrix, frameMatrix);

            textPos.x = 5;
            textPos.y = frameMatrix.rows() - 50;
            Imgproc.putText(frameMatrix, exampleTitle, textPos, Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 1, Imgproc.LINE_AA, false);
            textPos.y = frameMatrix.rows() - 30;
            Imgproc.putText(frameMatrix, exampleSceneTitle, textPos, Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 1, Imgproc.LINE_AA, false);
            textPos.y = frameMatrix.rows() - 10;
            Imgproc.putText(frameMatrix, settingInfo1, textPos, Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 1, Imgproc.LINE_AA, false);

            // Convert to Texture2D
            Utils.fastMatToTexture2D(frameMatrix, texture, true, 0, false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (frameMatrix != null)
                frameMatrix.Dispose();
            frameMatrix = null;
            Texture2D.Destroy(texture);
            texture = null;
            comicFilter.Dispose();
            comicFilter = null;
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

        public async void OnShareButtonClick()
        {
            var mes = "";

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            try
            {
                SharePayload payload = new SharePayload();
                payload.AddText("User shared image! [NatDeviceWithOpenCVForUnity Example](" + NatDeviceWithOpenCVForUnityExample.GetNatDeviceVersion() + ")");
                payload.AddImage(texture);
                var success = await payload.Commit();

                mes = $"Successfully shared items: {success}";
            }
            catch (ApplicationException e)
            {
                mes = e.Message;
            }
#else
            mes = "NatShare Error: SharePayload is not supported on this platform";
            await Task.Delay(100);
#endif

            Debug.Log(mes);
            if (fpsMonitor != null) fpsMonitor.Toast(mes);
        }

        public async void OnSaveToCameraRollButtonClick()
        {
            var mes = "";

 #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            try
            {
                SavePayload payload = new SavePayload("NatDeviceWithOpenCVForUnityExample");
                payload.AddImage(texture);
                var success = await payload.Commit();

                mes = $"Successfully saved items: {success}";
            }
            catch (ApplicationException e)
            {
                mes = e.Message;
            }
#else
            mes = "NatShare Error: SavePayload is not supported on this platform";
            await Task.Delay(100);
#endif

            Debug.Log(mes);
            if (fpsMonitor != null) fpsMonitor.Toast(mes);
        }
    }
}