using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UtilsModule;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace NatDeviceWithOpenCVForUnityExample
{

    public class WebCamSource : ICameraSource
    {

        #region --Op vars--

        private WebCamDevice cameraDevice;
        private Action startCallback, frameCallback;
        private Color32[] sourceBuffer, uprightBuffer;
        private int requestedWidth, requestedHeight, requestedFramerate;
        private int cameraIndex;
        private bool firstFrame;
        private DeviceOrientation orientation;
        private bool rotate90Degree;

        private WebCamSourceAttachment attachment;
        private class WebCamSourceAttachment : MonoBehaviour
        {
            public Action @delegate;
            void Awake() => DontDestroyOnLoad(this.gameObject);
            void Update() => @delegate?.Invoke();
        }

        #endregion


        #region --Client API--

        public int width { get; private set; }

        public int height { get; private set; }

        public bool isRunning { get { return (activeCamera && !firstFrame) ? activeCamera.isPlaying : false; } }

        public bool isFrontFacing { get { return activeCamera ? cameraDevice.isFrontFacing : false; } }

        public WebCamTexture activeCamera { get; private set; }

        public WebCamSource(int width, int height, int framerate = 30, bool front = false)
        {
            requestedWidth = width;
            requestedHeight = height;
            requestedFramerate = framerate;
#if UNITY_ANDROID && !UNITY_EDITOR
            // Set the requestedFPS parameter to avoid the problem of the WebCamTexture image becoming low light on some Android devices. (Pixel, Pixel 2)
            // https://forum.unity.com/threads/released-opencv-for-unity.277080/page-33#post-3445178
            framerate = front ? 15 : framerate;
#endif
            orientation = (DeviceOrientation)(int)Screen.orientation;

            // Pick camera
            var devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("Camera device does not exist.");
                return;
            }

            for (; cameraIndex < devices.Length; cameraIndex++)
                if (devices[cameraIndex].isFrontFacing == front)
                    break;

            if (cameraIndex == devices.Length)
            {
                Debug.LogError("Camera is null. Consider using " + (front ? "rear" : "front") + " camera.");
                return;
            }

            cameraDevice = devices[cameraIndex];
            activeCamera = new WebCamTexture(cameraDevice.name, requestedWidth, requestedHeight, framerate);
        }

        public void Dispose()
        {
            StopRunning();

            if (activeCamera != null)
            {
                WebCamTexture.Destroy(activeCamera);
                activeCamera = null;
            }
            sourceBuffer = null;
            uprightBuffer = null;
        }

        public Task StartRunning(Action startCallback, Action frameCallback)
        {
            var startTask = new TaskCompletionSource<bool>();

            if (activeCamera == null || (activeCamera != null && activeCamera.isPlaying))
                return startTask.Task;

            this.startCallback = startCallback;
            this.frameCallback = frameCallback;
            firstFrame = true;
            activeCamera.Play();

            attachment = new GameObject("NatDeviceWithOpenCVForUnityExample WebCamSource Helper").AddComponent<WebCamSourceAttachment>();
            attachment.@delegate = () =>
            {
                OnFrame();
            };

            startTask.SetResult(true);
            return startTask.Task;
        }

        public void StopRunning()
        {
            if (activeCamera == null)
                return;

            if (attachment != null)
            {
                attachment.@delegate = default;
                WebCamSourceAttachment.Destroy(attachment.gameObject);
                attachment = default;
            }

            activeCamera.Stop();
        }

        public void CaptureFrame(Mat matrix)
        {
            if (uprightBuffer == null) return;

            if ((int)matrix.total() * (int)matrix.elemSize() != uprightBuffer.Length * 4)
                throw new ArgumentException("matrix and CamSource image need to be the same size.");

            MatUtils.copyToMat(uprightBuffer, matrix);
            Core.flip(matrix, matrix, 0);
        }

        public void CaptureFrame(Color32[] pixelBuffer)
        {
            if (uprightBuffer == null) return;

            if (pixelBuffer.Length != uprightBuffer.Length)
                throw new ArgumentException("pixelBuffer and CamSource image need to be the same size.");

            Array.Copy(uprightBuffer, pixelBuffer, uprightBuffer.Length);
        }

        public void CaptureFrame(byte[] pixelBuffer)
        {
            if (uprightBuffer == null) return;

            if (pixelBuffer.Length != uprightBuffer.Length * 4)
                throw new ArgumentException("pixelBuffer and CamSource image need to be the same size.");

            GCHandle pin = GCHandle.Alloc(uprightBuffer, GCHandleType.Pinned);
            Marshal.Copy(pin.AddrOfPinnedObject(), pixelBuffer, 0, pixelBuffer.Length);
            pin.Free();
        }

        public async Task SwitchCamera()
        {
            if (activeCamera == null)
                return;

            var _isRunning = activeCamera.isPlaying;

            Dispose();

            var devices = WebCamTexture.devices;
            cameraIndex = ++cameraIndex % devices.Length;
            cameraDevice = devices[cameraIndex];

            bool front = cameraDevice.isFrontFacing;
            int framerate = requestedFramerate;
#if UNITY_ANDROID && !UNITY_EDITOR
            // Set the requestedFPS parameter to avoid the problem of the WebCamTexture image becoming low light on some Android devices. (Pixel, Pixel 2)
            // https://forum.unity.com/threads/released-opencv-for-unity.277080/page-33#post-3445178
            framerate = front ? 15 : framerate;
#endif

            activeCamera = new WebCamTexture(cameraDevice.name, requestedWidth, requestedHeight, framerate);
            await StartRunning(startCallback, frameCallback);

            if (!_isRunning)
                StopRunning();
        }

        #endregion


        #region --Operations--

        private void OnFrame()
        {
            if (!activeCamera.isPlaying || !activeCamera.didUpdateThisFrame)
                return;

            // Check buffers
            sourceBuffer = sourceBuffer ?? activeCamera.GetPixels32();
            uprightBuffer = uprightBuffer ?? new Color32[sourceBuffer.Length];
            // Update buffers
            activeCamera.GetPixels32(sourceBuffer);

            if (firstFrame)
            {
                rotate90Degree = false;

#if !UNITY_EDITOR && !(UNITY_STANDALONE || UNITY_WEBGL)
                switch (orientation)
                {
                    case DeviceOrientation.LandscapeLeft:
                    case DeviceOrientation.LandscapeRight:
                        width = activeCamera.width;
                        height = activeCamera.height;
                        break;
                    case DeviceOrientation.Portrait:
                    case DeviceOrientation.PortraitUpsideDown:
                        width = activeCamera.height;
                        height = activeCamera.width;
                        rotate90Degree = true;
                        break;
                }
#else
                width = activeCamera.width;
                height = activeCamera.height;
#endif
            }

            int flipCode = int.MinValue;
            if (cameraDevice.isFrontFacing)
            {
                if (activeCamera.videoRotationAngle == 0 || activeCamera.videoRotationAngle == 90)
                {
                    flipCode = 1;
                }
                else if (activeCamera.videoRotationAngle == 180 || activeCamera.videoRotationAngle == 270)
                {
                    flipCode = 0;
                }
            }
            else
            {
                if (activeCamera.videoRotationAngle == 180 || activeCamera.videoRotationAngle == 270)
                {
                    flipCode = -1;
                }
            }

            if (rotate90Degree && cameraDevice.isFrontFacing)
            {
                if (flipCode == int.MinValue)
                {
                    flipCode = -1;
                }
                else if (flipCode == 0)
                {
                    flipCode = 1;
                }
                else if (flipCode == 1)
                {
                    flipCode = 0;
                }
                else if (flipCode == -1)
                {
                    flipCode = int.MinValue;
                }
            }

            if (flipCode > int.MinValue)
            {
                if (flipCode == 0)
                {
                    FlipVertical(sourceBuffer, activeCamera.width, activeCamera.height, sourceBuffer);
                }
                else if (flipCode == 1)
                {
                    FlipHorizontal(sourceBuffer, activeCamera.width, activeCamera.height, sourceBuffer);
                }
                else if (flipCode == -1)
                {
                    Rotate(sourceBuffer, activeCamera.width, activeCamera.height, sourceBuffer, 2);
                }
            }

            if (rotate90Degree)
            {
                Rotate(sourceBuffer, activeCamera.width, activeCamera.height, uprightBuffer, 1);
            }
            else
            {
                Array.Copy(sourceBuffer, uprightBuffer, sourceBuffer.Length);
            }

            // Invoke client callbacks
            if (firstFrame)
            {
                startCallback();
                firstFrame = false;
            }

            frameCallback();
        }

        #endregion


        #region --Utilities--

        private static void FlipVertical(Color32[] src, int width, int height, Color32[] dst)
        {
            for (var i = 0; i < height / 2; i++)
            {
                var y = i * width;
                var x = (height - i - 1) * width;
                for (var j = 0; j < width; j++)
                {
                    int s = y + j;
                    int t = x + j;
                    Color32 c = src[s];
                    dst[s] = src[t];
                    dst[t] = c;
                }
            }
        }

        private static void FlipHorizontal(Color32[] src, int width, int height, Color32[] dst)
        {
            for (int i = 0; i < height; i++)
            {
                int y = i * width;
                int x = y + width - 1;
                for (var j = 0; j < width / 2; j++)
                {
                    int s = y + j;
                    int t = x - j;
                    Color32 c = src[s];
                    dst[s] = src[t];
                    dst[t] = c;
                }
            }
        }

        private static void Rotate(Color32[] src, int srcWidth, int srcHeight, Color32[] dst, int rotation)
        {
            int i;
            switch (rotation)
            {
                case 0:
                    Array.Copy(src, dst, src.Length);
                    break;
                case 1:
                    // Rotate 90 degrees (CLOCKWISE)
                    i = 0;
                    for (int x = srcWidth - 1; x >= 0; x--)
                    {
                        for (int y = 0; y < srcHeight; y++)
                        {
                            dst[i] = src[x + y * srcWidth];
                            i++;
                        }
                    }
                    break;
                case 2:
                    // Rotate 180 degrees
                    i = src.Length;
                    for (int x = 0; x < i / 2; x++)
                    {
                        Color32 t = src[x];
                        dst[x] = src[i - x - 1];
                        dst[i - x - 1] = t;
                    }
                    break;
                case 3:
                    // Rotate 90 degrees (COUNTERCLOCKWISE)
                    i = 0;
                    for (int x = 0; x < srcWidth; x++)
                    {
                        for (int y = srcHeight - 1; y >= 0; y--)
                        {
                            dst[i] = src[x + y * srcWidth];
                            i++;
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}