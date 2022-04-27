using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {
    private Text _fpsText;

    public float updateInterval = 0.5f;

    private float _accum = 0; // FPS accumulated over the interval
    private int _frames = 0; // Frames drawn over the interval
    private float _timeleft; // Left time for current interval

    void Awake() {
        _fpsText = GetComponent<Text>();
    }

    void Start() {
        _timeleft = updateInterval;
    }

    void Update() {
        _timeleft -= Time.deltaTime;
        _accum += Time.timeScale / Time.deltaTime;
        ++_frames;

        // Interval ended - update GUI text and start new interval
        if (_timeleft <= 0.0) {
            // display two fractional digits (f2 format)
            float fps = _accum / _frames;
            string format = System.String.Format("{0:F0} FPS", fps);
            _fpsText.text = format;

            _timeleft = updateInterval;
            _accum = 0.0F;
            _frames = 0;
        }
    }
}