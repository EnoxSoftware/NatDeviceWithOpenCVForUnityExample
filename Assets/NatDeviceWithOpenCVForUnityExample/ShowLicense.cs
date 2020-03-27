using UnityEngine;
using UnityEngine.SceneManagement;

namespace NatDeviceWithOpenCVForUnityExample
{

    public class ShowLicense : MonoBehaviour
    {

        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("NatDeviceWithOpenCVForUnityExample");
        }
    }
}
