using NatSuite.Devices;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UtilsModule;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace NatDeviceWithOpenCVForUnityExample
{

    public class NatDeviceCamSource : ICameraSource
    {

        #region --Op vars--

        private Action startCallback, frameCallback;
        private int requestedWidth, requestedHeight, requestedFramerate;
        private MediaDeviceQuery deviceQuery;
        private byte[] previewPixelBuffer;

        #endregion


        #region --Client API--

        public int width { get; private set; }

        public int height { get; private set; }

        public bool isRunning { get; private set; }

        public bool isFrontFacing { get { return activeCamera != null ? activeCamera.frontFacing : false; } }

        public CameraDevice activeCamera { get; private set; }

        public NatDeviceCamSource(int width, int height, int framerate = 30, bool front = false)
        {
            requestedWidth = width;
            requestedHeight = height;
            requestedFramerate = framerate;

            // Create a device query for device cameras
            deviceQuery = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);

            // Pick camera
            if (deviceQuery.count == 0)
            {
                Debug.LogError("Camera device does not exist.");
                return;
            }

            for (var i = 0; i < deviceQuery.count; i++)
            {
                activeCamera = deviceQuery.current as CameraDevice;
                if (activeCamera != null && activeCamera.frontFacing == front) break;
                deviceQuery.Advance();
                activeCamera = null;
            }

            if (activeCamera == null)
            {
                Debug.LogError("Camera is null. Consider using " + (front ? "rear" : "front") + " camera.");
                return;
            }

            activeCamera.previewResolution = (width: requestedWidth, height: requestedHeight);
            activeCamera.frameRate = requestedFramerate;
        }

        public void Dispose()
        {
            StopRunning();

            activeCamera = null;
        }

        public async Task StartRunning(Action startCallback, Action frameCallback)
        {
            if (activeCamera == null || isRunning)
                return;

            this.startCallback = startCallback;
            this.frameCallback = frameCallback;

            activeCamera.StartRunning(OnPixelBufferReceived);
            await Task.Yield();
        }

        public void StopRunning()
        {
            if (activeCamera == null || !isRunning)
                return;

            isRunning = false;
            previewPixelBuffer = null;

            activeCamera.StopRunning();
        }

        public void CaptureFrame(Mat matrix)
        {
            if (previewPixelBuffer == null) return;

            MatUtils.copyToMat(previewPixelBuffer, matrix);
            Core.flip(matrix, matrix, 0);
        }

        public void CaptureFrame(Color32[] pixelBuffer)
        {
            if (previewPixelBuffer == null) return;

            GCHandle pin = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
            Marshal.Copy(previewPixelBuffer, 0, pin.AddrOfPinnedObject(), previewPixelBuffer.Length);
            pin.Free();
        }

        public void CaptureFrame(byte[] pixelBuffer)
        {
            if (previewPixelBuffer == null) return;

            Buffer.BlockCopy(previewPixelBuffer, 0, pixelBuffer, 0, previewPixelBuffer.Length);
        }

        public async Task SwitchCamera()
        {
            if (activeCamera == null)
                return;

            var _isRunning = isRunning;

            Dispose();

            deviceQuery.Advance();
            activeCamera = deviceQuery.current as CameraDevice;

            activeCamera.previewResolution = (width: requestedWidth, height: requestedHeight);
            activeCamera.frameRate = requestedFramerate;

            await StartRunning(startCallback, frameCallback);

            if (!_isRunning)
                StopRunning();
        }

        #endregion


        #region --Operations--

        private void OnPixelBufferReceived(byte[] pixelBuffer, int width, int height, long timestamp)
        {
            bool firstFrame = previewPixelBuffer == null;

            if (firstFrame)
            {
                previewPixelBuffer = pixelBuffer;
                this.width = width;
                this.height = height;

                isRunning = true;

                startCallback();
            }
            else
            {
                frameCallback();
            }
        }

        #endregion
    }
}