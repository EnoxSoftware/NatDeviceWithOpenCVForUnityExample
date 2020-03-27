using OpenCVForUnity.CoreModule;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NatDeviceWithOpenCVForUnityExample
{

    public interface ICameraSource
    {

        #region --Properties--

        int width { get; }

        int height { get; }

        bool isRunning { get; }

        bool isFrontFacing { get; }

        #endregion


        #region --Operations--

        Task StartRunning(Action startCallback, Action frameCallback);

        void StopRunning();

        void CaptureFrame(Mat matrix);

        void CaptureFrame(Color32[] pixelBuffer);

        void CaptureFrame(byte[] pixelBuffer);

        Task SwitchCamera();

        void Dispose();

        #endregion
    }
}