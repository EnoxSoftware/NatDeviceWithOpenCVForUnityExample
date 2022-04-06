using NatSuite.Devices;
using NatSuite.Devices.Outputs;
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
        private PixelBufferOutput previewPixelBufferOutput;

        #endregion


        #region --Client API--

        public int width { get; private set; }

        public int height { get; private set; }

        public long timestamp { get; private set; }

        public bool verticallyMirrored { get; private set; }

        public Matrix4x4 intrinsics { get; private set; }

        public float exposureBias { get; private set; }

        public float exposureDuration { get; private set; }

        public float ISO { get; private set; }

        public float focalLength { get; private set; }

        public float fNumber { get; private set; }

        public float brightness { get; private set; }


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

            if (previewPixelBufferOutput == null)
                previewPixelBufferOutput = new PixelBufferOutput();

            activeCamera.StartRunning(OnPixelBufferOutputReceived);

            await Task.Yield();
        }

        public void StopRunning()
        {
            if (activeCamera == null || !isRunning)
                return;

            isRunning = false;

            if (previewPixelBufferOutput != null)
            {
                previewPixelBufferOutput.Dispose();
                previewPixelBufferOutput = null;
            }

            if (activeCamera.running)
                activeCamera.StopRunning();
        }

        public void CaptureFrame(Mat matrix)
        {
            if (!isRunning) return;

            if (matrix.width() != previewPixelBufferOutput.width || matrix.height() != previewPixelBufferOutput.height)
                throw new ArgumentException("matrix and CamSource image need to be the same size.");

            MatUtils.copyToMat(previewPixelBufferOutput.pixelBuffer, matrix);
            Core.flip(matrix, matrix, 0);
        }

        public void CaptureFrame(Color32[] pixelBuffer)
        {
            if (!isRunning) return;

            if (pixelBuffer.Length * 4 != previewPixelBufferOutput.pixelBuffer.Length)
                throw new ArgumentException("pixelBuffer and CamSource image need to be the same size.");

            unsafe
            {
                GCHandle pin = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
                Buffer.MemoryCopy(Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(previewPixelBufferOutput.pixelBuffer), (void*)pin.AddrOfPinnedObject(), pixelBuffer.Length * 4, previewPixelBufferOutput.pixelBuffer.Length);
                pin.Free();
            }
        }

        public void CaptureFrame(byte[] pixelBuffer)
        {
            if (!isRunning) return;

            if (pixelBuffer.Length != previewPixelBufferOutput.pixelBuffer.Length)
                throw new ArgumentException("pixelBuffer and CamSource image need to be the same size.");

            previewPixelBufferOutput.pixelBuffer.CopyTo(pixelBuffer);
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

        private void OnPixelBufferOutputReceived(CameraImage image)
        {
            if (previewPixelBufferOutput == null)
                return;

            // Process only when the latest previewResolution and CameraImage size match.
            if (activeCamera == null || activeCamera.previewResolution.width != image.width || activeCamera.previewResolution.height != image.height)
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
                width = previewPixelBufferOutput.width;
                height = previewPixelBufferOutput.height;

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