using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;

namespace NatDeviceWithOpenCVForUnityExample
{

    /// <summary>
    /// WebCamTexture To Mat Example
    /// An example of converting a WebCamTexture image to OpenCV's Mat format.
    /// </summary>
    public class WebCamTextureToMatExample : ExampleBase<WebCamMatSource>
    {

        Mat frameMatrix, grayMatrix;
        Texture2D texture;

        FpsMonitor fpsMonitor;

        #region --ExampleBase--

        protected override async void Start()
        {
            base.Start();

            fpsMonitor = GetComponent<FpsMonitor>();
            if (fpsMonitor != null)
            {
                fpsMonitor.Add("Name", "WebCamTextureToMatExample");
                fpsMonitor.Add("onFrameFPS", onFrameFPS.ToString("F1"));
                fpsMonitor.Add("drawFPS", drawFPS.ToString("F1"));
                fpsMonitor.Add("width", "");
                fpsMonitor.Add("height", "");
                fpsMonitor.Add("isFrontFacing", "");
                fpsMonitor.Add("orientation", "");
            }

            // Load global camera benchmark settings.
            int width, height, framerate;
            NatDeviceWithOpenCVForUnityExample.CameraConfiguration(out width, out height, out framerate);
            // Create camera source
            cameraSource = new WebCamMatSource(width, height, framerate, useFrontCamera);
            if (!cameraSource.activeCamera)
                cameraSource = new WebCamMatSource(width, height, framerate, !useFrontCamera);
            await cameraSource.StartRunning(OnStart, OnFrame);
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
            // Display texture
            rawImage.texture = texture;
            aspectFitter.aspectRatio = cameraSource.width / (float)cameraSource.height;
            Debug.Log("WebCam camera source started with resolution: " + cameraSource.width + "x" + cameraSource.height + " isFrontFacing: " + cameraSource.isFrontFacing);

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
            ProcessImage(frameMatrix, grayMatrix, imageProcessingType);
            // Convert to Texture2D
            Utils.fastMatToTexture2D(frameMatrix, texture);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            grayMatrix.Dispose();
            frameMatrix.Dispose();
            Texture2D.Destroy(texture);
            texture = null;
            grayMatrix =
            frameMatrix = null;
        }

        #endregion

    }
}