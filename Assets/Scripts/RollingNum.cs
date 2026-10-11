using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollingNum : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    List<RectTransform> rts = new List<RectTransform>();
    VerticalLayoutGroup vlg;
    int nowNum=0;
    float height=0;
    bool isRolling;
    int queuedNum = -1;
    RectTransform rectTransform;
    bool initialized;
    public float rollSpeed;

    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (initialized)
            return;
        vlg = GetComponent<VerticalLayoutGroup>();
        rectTransform = transform as RectTransform;
        rts.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child != null)
                rts.Add(child);
        }
        Canvas.ForceUpdateCanvases();
        if (rts.Count > 0)
        {
            height = rts[0].rect.height;
            if (height <= 0f)
                height = rts[0].sizeDelta.y;
        }
        initialized = vlg != null && rectTransform != null && rts.Count > 0 && height > 0f;
    }   
    public void SetNum(int num)
    {
        Initialize();
        if (!initialized)
            return;
        int target = Mathf.Abs(num) % rts.Count;
        if (isRolling)
        {
            queuedNum = target;
            return;
        }
        if (target == nowNum)
            return;
        queuedNum = -1;
        isRolling = true;
        StartCoroutine(Roll(target));
    }
    IEnumerator Roll(int targetNum)
    {
        float stepHeight = height + vlg.spacing;
        int offsetStep = (targetNum - nowNum + rts.Count) % rts.Count;
        float offsetFloat = 0f;
        float speed = Mathf.Abs(rollSpeed);

        if (speed <= 0f || stepHeight <= 0f)
        {
            for (int i = 0; i < offsetStep; i++)
                transform.GetChild(0).SetAsLastSibling();
            vlg.padding.top = 0;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            nowNum = targetNum;
            FinishRoll();
            yield break;
        }

        int nowStep = 0;
        while (nowStep < offsetStep)
        {
            offsetFloat += speed * Time.deltaTime;
            while (offsetFloat >= stepHeight && nowStep < offsetStep)
            {
                transform.GetChild(0).SetAsLastSibling();
                offsetFloat -= stepHeight;
                nowStep++;
            }
            vlg.padding.top = -(int)offsetFloat;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            yield return null;
        }
        vlg.padding.top = 0;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        nowNum = targetNum;
        FinishRoll();
    }

    void FinishRoll()
    {
        if (queuedNum >= 0 && queuedNum != nowNum)
        {
            int next = queuedNum;
            queuedNum = -1;
            StartCoroutine(Roll(next));
            return;
        }
        queuedNum = -1;
        isRolling = false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
