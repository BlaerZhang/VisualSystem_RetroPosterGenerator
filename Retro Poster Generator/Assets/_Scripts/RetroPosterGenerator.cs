using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RetroPosterGenerator : MonoBehaviour
{
    public List<Color> colorPalette = new List<Color>();
    public List<TMP_FontAsset> fontAssets = new List<TMP_FontAsset>();
    public List<TMP_FontAsset> fontAssetsItalic = new List<TMP_FontAsset>();
    public List<string> adjStringList;
    public List<string> conjunctionStringList;
    public List<string> locationStringList;
    public List<Sprite> itemList = new List<Sprite>();
    public Image beamPrefab;
    public Image gridPrefab;

    private Image _frame, _background, _foreground, _item, _circle;
    private RectTransform _beamParent;
    private TextMeshProUGUI _title, _subtitleTop, _subtitleBottom, _content, _subcontent, _date;
    private GridLayoutGroup _gridParent;

    [Header("Variables")] 
    public float titleAnchoredPosY;
    private float _titleOriginalAnchoredPosY;
    public float titleRotationZ;
    private float _titleOriginalRotationZ;
    public float contentAnchoredPosY;
    public float beamAngleInterval;
    public float beamWidth;
    public float itemPosY;
    private float _itemOriginalPosY;
    
    void Start()
    {
        _frame = GetComponent<Image>();
        _background = transform.Find("Texture Mask/Background").GetComponent<Image>();
        _beamParent = transform.Find("Texture Mask/Beam Parent").GetComponent<RectTransform>();
        _foreground = transform.Find("Texture Mask/Foreground").GetComponent<Image>();
        _item = transform.Find("Texture Mask/Item").GetComponent<Image>();
        _circle = transform.Find("Texture Mask/Circle").GetComponent<Image>();
        _content = transform.Find("Texture Mask/Content").GetComponent<TextMeshProUGUI>();
        _subcontent = transform.Find("Texture Mask/Content/Subcontent").GetComponent<TextMeshProUGUI>();
        _date = transform.Find("Texture Mask/Content/Date").GetComponent<TextMeshProUGUI>();
        _title = transform.Find("Texture Mask/Title").GetComponent<TextMeshProUGUI>();
        _subtitleTop = transform.Find("Texture Mask/Title/Subtitle Top").GetComponent<TextMeshProUGUI>();
        _subtitleBottom = transform.Find("Texture Mask/Title/Subtitle Bottom").GetComponent<TextMeshProUGUI>();
        _gridParent = transform.Find("Texture Mask/Grid").GetComponent<GridLayoutGroup>();

        _titleOriginalAnchoredPosY = _title.rectTransform.anchoredPosition.y;
        _titleOriginalRotationZ = _title.rectTransform.rotation.z;
        _itemOriginalPosY = _item.rectTransform.anchoredPosition.y;
        
        locationStringList = Resources.Load<TextAsset>("location").text.Split(",").ToList();
        adjStringList = Resources.Load<TextAsset>("adj").text.Split(", ").ToList();
        
        
        Generate();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Generate();
    }

    void Generate()
    {
        //item sprite and pos
        _item.sprite = itemList[Random.Range(0, itemList.Count)];
        _item.SetNativeSize();
        itemPosY = _itemOriginalPosY + Random.Range(-50f, 50f);
        _item.rectTransform.anchoredPosition = Vector2.up * itemPosY;
        
        //circle pos y
        _circle.rectTransform.anchoredPosition = Vector2.up * (itemPosY - 150f);
        
        //foreground pos y
        _foreground.rectTransform.sizeDelta = new Vector2(_foreground.rectTransform.rect.width, itemPosY + 500f);
        
        
        //content fonts
        _content.font = fontAssets[Random.Range(0, fontAssets.Count)];
        _subcontent.font = fontAssets[Random.Range(0, fontAssets.Count)];
        _date.font = _subcontent.font;
        
        GenerateColor();

        //pattern
        ClearPattern();
        switch (Random.Range(0f,1f))
        {
            case > 0.5f:
                GenerateGrid();
                
                //title rotation z
                titleRotationZ = 0;
                _title.rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, titleRotationZ));
                
                //title fonts
                _title.font = fontAssets[Random.Range(0, fontAssets.Count)];
                _subtitleTop.font = fontAssets[Random.Range(0, fontAssets.Count)];
                _subtitleBottom.font = fontAssets[Random.Range(0, fontAssets.Count)];
                
                _circle.enabled = true;
                break;

            case <= 0.5f:
                GenerateBeam();
                
                //title rotation z
                titleRotationZ = Random.Range(5f, 15f);
                _title.rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, titleRotationZ));
                
                //title font
                _title.font = fontAssetsItalic[Random.Range(0, fontAssetsItalic.Count)];
                _subtitleTop.font = fontAssetsItalic[Random.Range(0, fontAssetsItalic.Count)];
                _subtitleBottom.font = _subtitleTop.font;
                
                _circle.enabled = false;
                break;
        }
        
        GenerateFontMaterial(_title);
        GenerateFontMaterial(_subtitleTop);
        GenerateFontMaterial(_subtitleBottom);
        GenerateFontMaterial(_content);
        GenerateFontMaterial(_subcontent);
        GenerateFontMaterial(_date);
        
        GenerateText();
    }

    void GenerateBeam()
    {
        float beamHeight = beamPrefab.rectTransform.rect.height;
        beamWidth = Random.Range(0.1f, 0.2f) * beamHeight;
        beamAngleInterval = GenerateRandomFactorOf360(CalculateTopAngle(beamWidth, beamHeight) + 1, 31);
        for (int i = 0; i < 360 / beamAngleInterval; i++)
        {
            Image beamInstance = Instantiate(beamPrefab, _beamParent);
            beamInstance.rectTransform.anchoredPosition = Vector2.zero;
            beamInstance.rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, beamAngleInterval * i));
            beamInstance.rectTransform.sizeDelta = new Vector2(beamWidth, beamHeight);
            beamInstance.color = _frame.color;
        }
        
        _beamParent.rotation = Quaternion.Euler(Vector3.zero);
        _beamParent.DOLocalRotate(new Vector3(0, 0, beamAngleInterval * (Random.Range(0, 2) * 2 - 1)),
                Random.Range(0.2f, 0.5f))
            .SetEase(Ease.Linear)
            .SetLoops(-1);

        //beam pos y
        _beamParent.anchoredPosition = Vector2.up * (_circle.rectTransform.anchoredPosition.y +
            _circle.rectTransform.rect.height / 2 - _beamParent.rect.height / 2 - 100f);
        
        //title pos y
        titleAnchoredPosY = Random.Range(_item.rectTransform.rect.height + itemPosY + 300f, _background.rectTransform.rect.height - 200f);
        _title.rectTransform.anchoredPosition = new Vector3(0, titleAnchoredPosY, 0);
        
        //content pos y
        contentAnchoredPosY = Random.Range(200f, itemPosY - _content.rectTransform.rect.height - 50);
        _content.rectTransform.anchoredPosition = new Vector3(0, contentAnchoredPosY, 0);

        _foreground.enabled = true;
        _circle.color = _background.color;
    }

    void GenerateGrid()
    {
        float gridSize = Random.Range(150f, 400f);
        _gridParent.cellSize = Vector2.one * gridSize;
        
        int row = Mathf.FloorToInt(_gridParent.GetComponent<RectTransform>().rect.width / gridSize);
        _gridParent.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        _gridParent.constraintCount = row;
        int colum = row;

        for (int i = 0; i < colum * row; i++)
        {
            Image grid = Instantiate(gridPrefab, _gridParent.transform);
            grid.color = Color.black;
        }
        
        //grid pos y
        _gridParent.GetComponent<RectTransform>().anchoredPosition = Vector2.up * (_circle.rectTransform.anchoredPosition.y +
             _circle.rectTransform.rect.height / 2 - _gridParent.GetComponent<RectTransform>().rect.height / 2);
        
        //title pos y
        titleAnchoredPosY =
            Random.Range(
                _background.rectTransform.rect.height / 2 + gridSize * row / 2 + _title.rectTransform.rect.height + 50f,
                _background.rectTransform.rect.height - 200f);
        _title.rectTransform.anchoredPosition = new Vector3(0, titleAnchoredPosY, 0);
        
        //content pos y
        contentAnchoredPosY = Random.Range(100f,
            _background.rectTransform.rect.height / 2 - gridSize * row / 2 +
            _gridParent.GetComponent<RectTransform>().anchoredPosition.y - _content.rectTransform.rect.height - 50);
        _content.rectTransform.anchoredPosition = new Vector3(0, contentAnchoredPosY, 0);

        _foreground.enabled = false;
        _circle.color = _foreground.color;
    }

    void GenerateText()
    {
        _title.text = adjStringList[Random.Range(0,adjStringList.Count)];
        _subtitleTop.text = adjStringList[Random.Range(0,adjStringList.Count)];
        _subtitleBottom.text = $"{adjStringList[Random.Range(0,adjStringList.Count)]}{conjunctionStringList[Random.Range(0,conjunctionStringList.Count)]}{adjStringList[Random.Range(0,adjStringList.Count)]}";
        
        _content.text = $"{adjStringList[Random.Range(0,adjStringList.Count)]}{conjunctionStringList[Random.Range(0,conjunctionStringList.Count)]}{adjStringList[Random.Range(0,adjStringList.Count)]}";
        _subcontent.text = $"{locationStringList[Random.Range(0,locationStringList.Count)]}, {locationStringList[Random.Range(0,locationStringList.Count)]}";
        _date.text = $"{Random.Range(1,32).ToString("D2")}/{Random.Range(1,13).ToString("D2")}/{Random.Range(1959,2025)}";
    }

    void GenerateFontMaterial(TextMeshProUGUI textMeshProUGUI)
    {
        Material fontMat = textMeshProUGUI.fontSharedMaterial;
        fontMat.SetFloat("_OutlineWidth", Random.Range(0, 0.15f));
        if (Random.Range(0f, 1f) > 0.5f)
        {
            fontMat.EnableKeyword("UNDERLAY_ON");
            fontMat.SetColor("_UnderlayColor", new Color(0,0,0,Random.Range(0.5f,1f)));
            fontMat.SetFloat("_UnderlayOffsetX", Random.Range(0.75f, 1f));
            fontMat.SetFloat("_UnderlayOffsetY", Random.Range(-1f, -0.75f));
            fontMat.SetFloat("_UnderlayDilate", Random.Range(0, 0.5f));
        }
    }

    void ClearPattern()
    {
        foreach (Transform beam in _beamParent)
        {
            Destroy(beam.gameObject);
        }
        _beamParent.DOKill();
        
        foreach (Transform grid in _gridParent.transform)
        {
            Destroy(grid.gameObject);
        }
    }

    void GenerateColor()
    {
        Color[] paletteArray = GenerateAnalogousColors(colorPalette[Random.Range(0, colorPalette.Count)]);
        _foreground.color = paletteArray[0];
        _circle.color = paletteArray[0];
        _frame.color = paletteArray[2];

        _title.color = paletteArray[1];
        _subtitleTop.color = paletteArray[1];
        _subtitleBottom.color = paletteArray[1];
        
        _content.color = paletteArray[1];
        _subcontent.color = paletteArray[1];
        _date.color = paletteArray[1];

    }
    
    public float CalculateTopAngle(float baseLength, float height)
    {
        float halfBaseLength = baseLength / 2f;
        float angleRadians = Mathf.Atan2(height, halfBaseLength);
        float angleDegrees = angleRadians * Mathf.Rad2Deg;
        float topAngleDegrees = 180f - 2f * angleDegrees;

        return topAngleDegrees;
    }
    
    public float GenerateRandomFactorOf360(float minInclusive, float maxInclusive)
    {
        float randomNumber;
        do
        {
            randomNumber = Random.Range(minInclusive, maxInclusive);
        }
        while (360 % randomNumber != 0);
        
        return randomNumber;
    }
    
    public Color[] GenerateAnalogousColors(Color baseColor)
    {
        float h, s, l;
        RGBToHSL(baseColor, out h, out s, out l);

        float angle = 30f / 360f; // 30 degree shift on either side
        Color color1 = HSLToRGB((h + angle) % 1.0f, s, l); // Adjust hue by +30 degrees
        Color color2 = HSLToRGB((h - angle + 1.0f) % 1.0f, s, l); // Adjust hue by -30 degrees
        Color color3 = baseColor;

        return new Color[] { color1, color2, color3 };
    }
    
    private void RGBToHSL(Color rgbColor, out float H, out float S, out float L)
    {
        float r = rgbColor.r;
        float g = rgbColor.g;
        float b = rgbColor.b;
        float max = Mathf.Max(r, Mathf.Max(g, b));
        float min = Mathf.Min(r, Mathf.Min(g, b));
        L = (max + min) / 2;

        if (max == min)
        {
            H = S = 0; // achromatic
        }
        else
        {
            float d = max - min;
            S = (L > 0.5) ? d / (2 - max - min) : d / (max + min);
            if (max == r)
            {
                H = (g - b) / d + (g < b ? 6 : 0);
            }
            else if (max == g)
            {
                H = (b - r) / d + 2;
            }
            else
            {
                H = (r - g) / d + 4;
            }
            H /= 6;
        }
    }
    
    private Color HSLToRGB(float h, float s, float l)
    {
        float r, g, b;
        if (s == 0)
        {
            r = g = b = l; // achromatic
        }
        else
        {
            float q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            float p = 2 * l - q;
            r = HueToRGB(p, q, h + 1f / 3f);
            g = HueToRGB(p, q, h);
            b = HueToRGB(p, q, h - 1f / 3f);
        }
        return new Color(r, g, b);
    }
    
    private float HueToRGB(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }
}
