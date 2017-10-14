using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBehavior : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private OutputAudioRecorder recorder;

    [SerializeField] private bool startPlaying;
    [SerializeField] private bool redRecButton;

    void Start () {
	}
	
	void Update () {
	    if (startPlaying && audioSource != null)
	    {
	        startPlaying = false;
            audioSource.Play();
	    }
	    if (redRecButton && recorder != null) {
	        redRecButton = false;
            if (recorder.IsRecording)
	            recorder.StopRecording();
            else
	            recorder.StartRecording();
	    }
    }
}
