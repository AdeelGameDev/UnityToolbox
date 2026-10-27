using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

namespace Adeel.UnityToolbox.Samples
{
    /// <summary>
    /// Renders the rear camera into a <see cref="RawImage"/> and decodes a QR code
    /// on demand into a TextMeshPro label.
    /// </summary>
    /// <remarks>
    /// Requires ZXing.Net (https://github.com/micjahn/ZXing.Net). Hook a UI button
    /// up to <see cref="OnClickScan"/>.
    /// </remarks>
    public class QRCodeScanner : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImageBackground;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private TextMeshProUGUI _textOut;
        [SerializeField] private RectTransform _scanZone;

        private WebCamTexture _cameraTexture;
        private bool _cameraAvailable;

        private void Start()
        {
            SetUpCamera();
        }

        private void Update()
        {
            UpdateCameraRender();
        }

        private void SetUpCamera()
        {
            WebCamDevice[] devices = WebCamTexture.devices;

            if (devices.Length == 0)
            {
                _cameraAvailable = false;
                Debug.LogWarning("QRCodeScanner: no camera device found.", this);
                return;
            }

            int width = _scanZone != null ? Mathf.Max(1, (int)_scanZone.rect.width) : 720;
            int height = _scanZone != null ? Mathf.Max(1, (int)_scanZone.rect.height) : 720;

            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].isFrontFacing)
                {
                    continue;
                }

                _cameraTexture = new WebCamTexture(devices[i].name, width, height);
                break;
            }

            // Fall back to the front camera on devices without a rear one.
            if (_cameraTexture == null)
            {
                _cameraTexture = new WebCamTexture(devices[0].name, width, height);
            }

            _cameraTexture.Play();

            if (_rawImageBackground != null)
            {
                _rawImageBackground.texture = _cameraTexture;
            }

            _cameraAvailable = true;
        }

        private void UpdateCameraRender()
        {
            if (!_cameraAvailable || _cameraTexture == null || !_cameraTexture.isPlaying)
            {
                return;
            }

            if (_aspectRatioFitter != null && _cameraTexture.height > 0)
            {
                _aspectRatioFitter.aspectRatio = (float)_cameraTexture.width / _cameraTexture.height;
            }

            if (_rawImageBackground != null)
            {
                int orientation = -_cameraTexture.videoRotationAngle;

                if (_cameraTexture.videoVerticallyMirrored)
                {
                    orientation += 180;
                }

                _rawImageBackground.rectTransform.localEulerAngles = new Vector3(0f, 0f, orientation);
            }
        }

        /// <summary>Decodes the current camera frame. Wire this to a UI button.</summary>
        public void OnClickScan()
        {
            if (!_cameraAvailable || _cameraTexture == null)
            {
                SetResult("No camera available");
                return;
            }

            try
            {
                IBarcodeReader barcodeReader = new BarcodeReader();
                Result result = barcodeReader.Decode(
                    _cameraTexture.GetPixels32(),
                    _cameraTexture.width,
                    _cameraTexture.height);

                SetResult(result != null ? result.Text : "No QR code found");
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception, this);
                SetResult("Scan failed");
            }
        }

        private void SetResult(string message)
        {
            if (_textOut != null)
            {
                _textOut.text = message;
            }
        }

        private void OnDestroy()
        {
            if (_cameraTexture == null)
            {
                return;
            }

            if (_cameraTexture.isPlaying)
            {
                _cameraTexture.Stop();
            }

            Destroy(_cameraTexture);
        }
    }
}
