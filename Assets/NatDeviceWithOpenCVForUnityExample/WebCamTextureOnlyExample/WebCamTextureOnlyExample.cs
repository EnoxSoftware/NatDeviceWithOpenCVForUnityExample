using UnityEngine;

namespace NatDeviceWithOpenCVForUnityExample
{

    /// <summary>
    /// WebCamTexture Only Example
    /// An example of displaying the preview frame of camera only using WebCamTexture API.
    /// </summary>
    public class WebCamTextureOnlyExample : ExampleBase<WebCamSource>
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
                fpsMonitor.Add("Name", "WebCamTextureOnlyExample");
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
            cameraSource = new WebCamSource(width, height, framerate, useFrontCamera);
            if (!cameraSource.activeCamera)
                cameraSource = new WebCamSource(width, height, framerate, !useFrontCamera);
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
    }
}