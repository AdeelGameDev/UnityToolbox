# QR Code Scanner

Scans QR codes from the device camera into a TextMeshPro label.

**Requires [ZXing.Net](https://github.com/micjahn/ZXing.Net)** (`com.micjahn.zxing` /
the `zxing.unity.dll` build) and TextMeshPro.

## Setup

1. Import ZXing.Net into your project.
2. Build a UI canvas containing:
   - a `RawImage` for the camera feed,
   - an `AspectRatioFitter` on the same object,
   - a `RectTransform` marking the scan area,
   - a `TextMeshProUGUI` for the result.
3. Copy `QRCodeScanner.cs` into your project (outside `Editor/`).
4. Drag the four references onto the component.
5. Call `OnClickScan()` from a button, or `Scan()` from your own flow.

## Notes

- The script picks the first rear-facing camera and sizes the texture to the scan
  area.
- If no camera is present, the component logs a warning and stays idle instead of
  throwing a `NullReferenceException` — the previous version crashed here.
- Decoding runs on the main thread in this sample. For smooth UI during scanning,
  move the `BarcodeReader.Decode` call to a worker thread and hand the result back
  on the main thread.
- Grant camera permission on Android/iOS before the first scan.
