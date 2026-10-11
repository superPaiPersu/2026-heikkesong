using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmitCapture : MonoBehaviour
{
    public static readonly string[] CameraNames = { "front", "back", "left", "right", "up", "down" };
    public static readonly string[] ViewNames = { "正面", "背面", "左侧", "右侧", "上方", "下方" };

    public static SubmitCapture Ensure()
    {
        SubmitCapture existing = FindAnyObjectByType<SubmitCapture>();
        if (existing != null)
            return existing;
        return new GameObject("SubmitCapture").AddComponent<SubmitCapture>();
    }

    void Start()
    {
        foreach (Camera cam in Collect())
            cam.enabled = true;
    }

    public void Hide()
    {
        foreach (Camera cam in Collect())
            cam.enabled = false;
    }

    public IEnumerator Capture(Action<List<byte[]>> onDone)
    {
        Camera[] cams = Collect();
        List<byte[]> pngs = new List<byte[]>();
        foreach (Camera cam in cams)
        {
            if (cam.targetTexture == null)
                cam.targetTexture = new RenderTexture(256, 256, 16);
            cam.enabled = true;
            yield return new WaitForEndOfFrame();
            pngs.Add(Read(cam.targetTexture));
        }
        onDone?.Invoke(pngs);
    }

    Camera[] Collect()
    {
        List<Camera> list = new List<Camera>();
        for (int i = 0; i < CameraNames.Length; i++)
        {
            GameObject go = GameObject.Find(CameraNames[i]);
            if (go == null)
                continue;
            Camera cam = go.GetComponent<Camera>();
            if (cam != null)
                list.Add(cam);
        }
        return list.ToArray();
    }

    static byte[] Read(RenderTexture source)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;
        Texture2D tex = new Texture2D(tmp.width, tmp.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        tex.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);
        byte[] png = tex.EncodeToPNG();
        Destroy(tex);
        return png;
    }
}
