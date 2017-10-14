﻿using System;
using System.IO;
using UnityEngine;

public class OutputAudioRecorder : MonoBehaviour {
    public const string DEFAULT_FILENAME = "record";
    public const string FILE_EXTENSION = ".wav";

    public bool IsRecording {  get { return recOutput; } }

    private int bufferSize;
    private int numBuffers;
    private int outputRate;
    private int headerSize = 44; //default for uncompressed wav
    private String fileName;
    private bool recOutput = false;

    private FileStream fileStream;

    void Awake() {
        outputRate = AudioSettings.outputSampleRate;
    }

    void Start() {
        AudioSettings.GetDSPBufferSize(out bufferSize, out numBuffers);
    }

    public void StartRecording(string recordFileName = null) {
        if (string.IsNullOrEmpty(recordFileName)) {
            fileName = DEFAULT_FILENAME + FILE_EXTENSION;
        }
        else
        {
            //currently it works only for .wav files so be sure the name is correct
            fileName = Path.GetFileNameWithoutExtension(recordFileName) + FILE_EXTENSION;
        }
#if UNITY_EDITOR
        Debug.Log("StartRecording " + fileName);
#endif
        if (!recOutput) {
            StartWriting(fileName);
            recOutput = true;
        }
        else {
            Debug.LogError("Recording is in progress already");
        }
    }

    public void StopRecording() {
#if UNITY_EDITOR
        Debug.Log("StopRecording");
#endif
        recOutput = false;
        WriteHeader();
    }


    private void StartWriting(String name) {
        fileStream = new FileStream(Application.dataPath + "/" + name, FileMode.Create);
        var emptyByte = new byte();
        for (int i = 0; i < headerSize; i++) //preparing the header
        {
            fileStream.WriteByte(emptyByte);
        }
    }

    private void OnAudioFilterRead(float[] data, int channels) {
        if (recOutput) {
            ConvertAndWrite(data); //audio data is interlaced
        }
    }

    private void ConvertAndWrite(float[] dataSource) {

        var intData = new Int16[dataSource.Length];
        //converting in 2 steps : float[] to Int16[], //then Int16[] to Byte[]

        var bytesData = new Byte[dataSource.Length * 2];
        //bytesData array is twice the size of
        //dataSource array because a float converted in Int16 is 2 bytes.

        var rescaleFactor = 32767; //to convert float to Int16

        for (var i = 0; i < dataSource.Length; i++) {
            intData[i] = (Int16)(dataSource[i] * rescaleFactor);
            var byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    private void WriteHeader() {

        fileStream.Seek(0, SeekOrigin.Begin);

        var riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        var chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        var wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        var fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        var subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 two = 2;
        UInt16 one = 1;

        var audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        var numChannels = BitConverter.GetBytes(two);
        fileStream.Write(numChannels, 0, 2);

        var sampleRate = BitConverter.GetBytes(outputRate);
        fileStream.Write(sampleRate, 0, 4);

        var byteRate = BitConverter.GetBytes(outputRate * 4);

        fileStream.Write(byteRate, 0, 4);

        UInt16 four = 4;
        var blockAlign = BitConverter.GetBytes(four);
        fileStream.Write(blockAlign, 0, 2);

        UInt16 sixteen = 16;
        var bitsPerSample = BitConverter.GetBytes(sixteen);
        fileStream.Write(bitsPerSample, 0, 2);

        var dataString = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(dataString, 0, 4);

        var subChunk2 = BitConverter.GetBytes(fileStream.Length - headerSize);
        fileStream.Write(subChunk2, 0, 4);

        fileStream.Close();
    }
}
