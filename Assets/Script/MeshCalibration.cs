using UnityEngine;
using System.Collections;

public class MeshCalibration : MonoBehaviour
{

    DepthMesh DepthMesh;

    Vector3 TopLeft;
    Vector3 TopRight;
    Vector3 BottomRight;
    Vector3 BottomLeft;

    int ClickCount = 0;

    // Use this for initialization
    void Start()
    {
        DepthMesh = GetComponent<DepthMesh>();
        ClickCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ProcessClick();
        }
    }

    void ProcessClick()
    {
        //ClickPosition
        Vector3 ClickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 PositionOnMesh = ClickPosition - transform.position;

        //Set Variable
        switch (ClickCount)
        {
            case 0: TopLeft = PositionOnMesh;
                break;
            case 1: TopRight = PositionOnMesh;
                break;
            case 2: BottomRight = PositionOnMesh;
                break;
            case 3: BottomLeft = PositionOnMesh;
                CalculateRectangle();
                ClickCount = -1;
                break;
        }

        ClickCount++;
    }

    void CalculateRectangle()
    {
        Debug.Log(TopLeft);
        Debug.Log(TopRight);
        Debug.Log(BottomRight);
        Debug.Log(BottomLeft);

        DepthMesh.OffsetX = Mathf.FloorToInt(Mathf.Min(TopLeft.x, BottomLeft.x)) + DepthMesh.OffsetX;
        DepthMesh.OffsetY = Mathf.FloorToInt(Mathf.Min(BottomLeft.y, BottomRight.y)) + DepthMesh.OffsetY;
        DepthMesh.Height = Mathf.FloorToInt(Mathf.Max(TopLeft.y, TopRight.y) - Mathf.Min(BottomLeft.y, BottomRight.y));
        DepthMesh.Width = Mathf.FloorToInt(Mathf.Max(TopRight.x, BottomRight.x) - Mathf.Min(TopLeft.x, BottomLeft.x));
    }
}
