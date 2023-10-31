using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SplatterManager : MonoBehaviour
{
    [SerializeField] private Vector2 renderTexSize;
    [SerializeField] private RenderTexture rt;
    [SerializeField] private RenderTexture rtCalc;
    [SerializeField] private List<RenderTexture> rtPlayer = new List<RenderTexture>();

    [SerializeField] private List<Color> playerColors = new List<Color>();
    [SerializeField] private List<Color> calcColors = new List<Color>();

    [SerializeField] private List<Texture2D> splatterTextures = new List<Texture2D>();
    [SerializeField] private List<Texture2D> calcTextures = new List<Texture2D>();

    [SerializeField] private Texture2D _coneSplatter;
    [SerializeField] private Texture2D _coneCalc;

    [SerializeField] private float _splatterSize;

    public enum SplatterType
    {
        small,
        medium,
        big,
        cone
    }

    void Start()
    {
        //Screen.SetResolution(1920, 1080, true);

        ClearTextures();
    }

    public void ClearTextures()
    {
        ClearOutRenderTexture(rt);
        ClearOutRenderTexture(rtCalc);
        foreach (RenderTexture crt in rtPlayer) ClearOutRenderTexture(crt);
    }

    public void DrawSplatter(Vector2 pos, int player, SplatterType type, float rotation = 0)
    {
        pos = Vector2.Scale(pos, renderTexSize);

        pos *= 1 / 1.4f;
        pos += renderTexSize * 0.2f / 1.4f;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, renderTexSize.x, renderTexSize.y, 0);
        RenderTexture.active = rt;

        int splatterIndex = Random.Range(0, splatterTextures.Count);
        Texture2D splatterTex = splatterTextures[splatterIndex];
        Texture2D calcTex = calcTextures[splatterIndex];
        float size = _splatterSize;

        switch (type)
        {
            case SplatterType.small:
                size = _splatterSize;
                splatterTex = splatterTextures[splatterIndex];
                calcTex = calcTextures[splatterIndex];
                break;

            case SplatterType.medium:
                size = _splatterSize * 1.5f;
                splatterTex = splatterTextures[splatterIndex];
                calcTex = calcTextures[splatterIndex];
                break;

            case SplatterType.big:
                size = _splatterSize * 2.5f;
                splatterTex = splatterTextures[splatterIndex];
                calcTex = calcTextures[splatterIndex];
                break;

            case SplatterType.cone:
                size = _splatterSize;
                splatterTex = rotateTexture(_coneSplatter, rotation);
                calcTex = _coneCalc;
                pos += new Vector2(size / 2, size / 2);
                break;
        }
        
        RenderColor(rt, splatterTex, pos, playerColors[player], size);
        RenderColor(rtCalc, calcTex, pos, calcColors[player], size);

        RenderTexture.active = null;
        GL.PopMatrix();
    }

    private void ClearOutRenderTexture(RenderTexture renderTexture)
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;
    }

    private void RenderColor(RenderTexture crt, Texture2D tex, Vector2 pos, Color col, float size)
    {
        RenderTexture.active = crt;
        Graphics.DrawTexture(new Rect(pos.x - size/2, pos.y - size/2, size, size), tex, new Rect(0, 0, 1, 1), 0, 0, 0, 0, col, null);
        RenderTexture.active = null;
    }

    Texture2D rotateTexture(Texture2D tex, float angle)
    {
        Debug.Log("rotating");
        Texture2D rotImage = new Texture2D(tex.width, tex.height);
        int x, y;
        float x1, y1, x2, y2;

        int w = tex.width;
        int h = tex.height;
        float x0 = rot_x(angle, -w / 2.0f, -h / 2.0f) + w / 2.0f;
        float y0 = rot_y(angle, -w / 2.0f, -h / 2.0f) + h / 2.0f;

        float dx_x = rot_x(angle, 1.0f, 0.0f);
        float dx_y = rot_y(angle, 1.0f, 0.0f);
        float dy_x = rot_x(angle, 0.0f, 1.0f);
        float dy_y = rot_y(angle, 0.0f, 1.0f);


        x1 = x0;
        y1 = y0;

        for (x = 0; x < tex.width; x++)
        {
            x2 = x1;
            y2 = y1;
            for (y = 0; y < tex.height; y++)
            {
                //rotImage.SetPixel (x1, y1, Color.clear);          

                x2 += dx_x;//rot_x(angle, x1, y1);
                y2 += dx_y;//rot_y(angle, x1, y1);
                rotImage.SetPixel((int)Mathf.Floor(x), (int)Mathf.Floor(y), getPixel(tex, x2, y2));
            }

            x1 += dy_x;
            y1 += dy_y;

        }

        rotImage.Apply();
        return rotImage;
    }

    private Color getPixel(Texture2D tex, float x, float y)
    {
        Color pix;
        int x1 = (int)Mathf.Floor(x);
        int y1 = (int)Mathf.Floor(y);

        if (x1 > tex.width || x1 < 0 ||
           y1 > tex.height || y1 < 0)
        {
            pix = Color.clear;
        }
        else
        {
            pix = tex.GetPixel(x1, y1);
        }

        return pix;
    }

    private float rot_x(float angle, float x, float y)
    {
        float cos = Mathf.Cos(angle / 180.0f * Mathf.PI);
        float sin = Mathf.Sin(angle / 180.0f * Mathf.PI);
        return (x * cos + y * (-sin));
    }
    private float rot_y(float angle, float x, float y)
    {
        float cos = Mathf.Cos(angle / 180.0f * Mathf.PI);
        float sin = Mathf.Sin(angle / 180.0f * Mathf.PI);
        return (x * sin + y * cos);
    }

    [ContextMenu("evaluate")]
    public Vector2 CalculateWinner()
    {
        Texture2D tex = new Texture2D(1920, 1080);

        Rect rectReadPicture = new Rect(0, 0, 1920, 1080);

        RenderTexture.active = rtCalc;

        // Read pixels
        tex.ReadPixels(rectReadPicture, 0, 0);
        tex.Apply();

        RenderTexture.active = null; // added to avoid errors 


        int redCnt = 0;
        int greenCnt = 0;

        foreach(Color col in tex.GetPixels())
        {
            if (col.r >= 0.99f) redCnt++;
            else if (col.g >= 0.99f) greenCnt++;
        }

        //Debug.Log("red: " + redCnt + " // green: " + greenCnt);

        if (greenCnt > redCnt) return new Vector2((float)redCnt/(redCnt+greenCnt), (float)greenCnt /(redCnt+greenCnt));
        else return new Vector2((float)redCnt / (redCnt + greenCnt), (float)greenCnt / (redCnt + greenCnt));
    }
}
