using NatSuite.Devices;
using System;
using System.Threading.Tasks;
using UnityEngine;
using OpenCVForUnity.UtilsModule;
using OpenCVForUnity.CoreModule;

#if OPENCV_USE_UNSAFE_CODE && UNITY_2018_2_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
#endif

namespace NatDeviceWithOpenCVForUnityExample
{

    public class NatDeviceCamSource : ICameraSource
    {

        #region --Op vars--

        private Action startCallback, frameCallback;
        private int requestedWidth, requestedHeight, requestedFramerate;
        private bool firstFrame;
        private MediaDeviceQuery deviceQuery;

        private NatDeviceCamSourceAttachment attachment;
        private class NatDeviceCamSourceAttachment : MonoBehaviour
        {
            public Action @delegate;
            void Awake() => DontDestroyOnLoad(this.gameObject);
            void Update() => @delegate?.Invoke();
        }

        #endregion


        #region --Client API--

        public int width { get; private set; }

        public int height { get; private set; }

        public bool isRunning { get; private set; }

        public bool isFrontFacing { get { return activeCamera != null ? activeCamera.frontFacing : false; } }

        public Texture2D preview { get; private set; }

        public ICameraDevice activeCamera { get; private set; }

        public NatDeviceCamSource(int width, int height, int framerate = 30, bool front = false)
        {
            requestedWidth = width;
            requestedHeight = height;
            requestedFramerate = framerate;

            // Create a device query for device cameras
            // Use `GenericCameraDevice` so we also capture WebCamTexture cameras
            deviceQuery = new MediaDeviceQuery(MediaDeviceQuery.Criteria.GenericCameraDevice);

            // Pick camera
            if (deviceQuery.devices.Length == 0)
            {
                Debug.LogError("Camera device does not exist.");
                return;
            }

            for (var i = 0; i < deviceQuery.devices.Length; i++)
            {
                activeCamera = deviceQuery.currentDevice as ICameraDevice;
                if (activeCamera.frontFacing == front) break;
                deviceQuery.Advance();
            }

            if (activeCamera == null || activeCamera.frontFacing != front)
            {
                Debug.LogError("Camera is null. Consider using " + (front ? "rear" : "front") + " camera.");
                activeCamera = null;
                return;
            }

            activeCamera.previewResolution = (width: requestedWidth, height: requestedHeight);
            activeCamera.frameRate = requestedFramerate;
        }

        public void Dispose()
        {
            StopRunning();

            activeCamera = null;
            if (preview != null)
            {
                Texture2D.Destroy(preview);
                preview = null;
            }
        }

        public async Task StartRunning(Action startCallback, Action frameCallback)
        {
            if (activeCamera == null || isRunning)
                return;

            this.startCallback = startCallback;
            this.frameCallback = frameCallback;

            preview = await activeCamera.StartRunning();
            width = preview.width;
            height = preview.height;
            isRunning = true;
            firstFrame = true;

            attachment = new GameObject("NatDeviceWithOpenCVForUnityExample NatDeviceCamSource Helper").AddComponent<NatDeviceCamSourceAttachment>();
            attachment.@delegate = () => {
                if (firstFrame)
                {
                    startCallback();
                    firstFrame = false;
                }
                else
                {
                    frameCallback();
                }
            };
        }

        public void StopRunning()
        {
            if (activeCamera == null || !isRunning)
                return;

            if (attachment != null)
            {
                attachment.@delegate = default;
                NatDeviceCamSourceAttachment.Destroy(attachment.gameObject);
                attachment = default;
            }

            activeCamera.StopRunning();
            isRunning = false;
        }

        public void CaptureFrame(Mat matrix)
        {
            if (preview == null) return;

#if OPENCV_USE_UNSAFE_CODE && UNITY_2018_2_OR_NEWER
            unsafe
            {
                var ptr = (IntPtr)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(preview.GetRawTextureData<byte>());
                MatUtils.copyToMat(ptr, matrix);
            }
            Core.flip(matrix, matrix, 0);
#else
            MatUtils.copyToMat(preview.GetRawTextureData(), matrix);
            Core.flip(matrix, matrix, 0);
#endif
        }

        public void CaptureFrame(Color32[] pixelBuffer)
        {
            if (preview == null) return;

#if OPENCV_USE_UNSAFE_CODE && UNITY_2018_2_OR_NEWER
            unsafe
            {
                NativeArray<Color32> rawTextureData = preview.GetRawTextureData<Color32>();
                int size = UnsafeUtility.SizeOf<Color32>() * rawTextureData.Length;
                Color32* srcAddr = (Color32*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(rawTextureData);

                fixed (Color32* dstAddr = pixelBuffer)
                {
                    UnsafeUtility.MemCpy(dstAddr, srcAddr, size);
                }
            }
#else
            byte[] rawTextureData = preview.GetRawTextureData();
            GCHandle pin = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
            Marshal.Copy(rawTextureData, 0, pin.AddrOfPinnedObject(), rawTextureData.Length);
            pin.Free();
#endif
        }

        public void CaptureFrame(byte[] pixelBuffer)
        {
            if (preview == null) return;

#if OPENCV_USE_UNSAFE_CODE && UNITY_2018_2_OR_NEWER
            unsafe
            {
                NativeArray<byte> rawTextureData = preview.GetRawTextureData<byte>();
                int size = UnsafeUtility.SizeOf<byte>() * rawTextureData.Length;
                byte* srcAddr = (byte*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(rawTextureData);

                fixed (byte* dstAddr = pixelBuffer)
                {
                    UnsafeUtility.MemCpy(dstAddr, srcAddr, size);
                }
            }
#else
            byte[] rawTextureData = preview.GetRawTextureData();
            Buffer.BlockCopy(rawTextureData, 0, pixelBuffer, 0, rawTextureData.Length);
#endif
        }

        public async Task SwitchCamera()
        {
            if (activeCamera == null)
                return;

            var _isRunning = isRunning;

            Dispose();

            deviceQuery.Advance();
            activeCamera = deviceQuery.currentDevice as ICameraDevice;

            activeCamera.previewResolution = (width: requestedWidth, height: requestedHeight);
            activeCamera.frameRate = requestedFramerate;

            await StartRunning(startCallback, frameCallback);

            if (!_isRunning)
                StopRunning();
        }

        #endregion
    }
}