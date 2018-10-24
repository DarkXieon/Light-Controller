using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleScript : MonoBehaviour
{
    public Transform Transform;
    public float MinPercentage;

    private float maxXScale;
    private float maxYScale;
    private float minXScale;
    private float minYScale;
    private float currentXScale;
    private float currentYScale;

    private bool shrinking = true;

    public void Awake()
    {
        maxXScale = Transform.localScale.x;
        maxYScale = Transform.localScale.z;

        minXScale = maxXScale / MinPercentage;
        minYScale = maxYScale / MinPercentage;

        currentXScale = maxXScale;
        currentYScale = maxYScale;
    }

    public void Update()
    {
        if(shrinking)
        {
            currentXScale = Mathf.Clamp(currentXScale - 1, minXScale, maxXScale);
            currentYScale = Mathf.Clamp(currentYScale - 1, minYScale, maxXScale);

            shrinking = currentXScale == minXScale && currentYScale == minYScale
                ? !shrinking
                : shrinking;
        }
        else
        {
            currentXScale = Mathf.Clamp(currentXScale + 1, minXScale, maxXScale);
            currentYScale = Mathf.Clamp(currentYScale + 1, minYScale, maxXScale);

            shrinking = currentXScale == maxXScale && currentYScale == maxYScale
                ? !shrinking
                : shrinking;
        }

        Transform.localScale = new Vector3(currentXScale, Transform.localScale.y, currentYScale);
    }
}
