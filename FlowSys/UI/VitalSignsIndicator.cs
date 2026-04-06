using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace GAME.FlowSys
{
    public class VitalSignsIndicator : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI heartRateText;
        public Image waveformImage;
        public RectTransform waveformLine;
        
        [Header("Settings")]
        public float baseHeartRate = 60f;
        public float waveSpeed = 1f;
        public Color ecgLineColor = Color.green;
        public float lineThickness = 2f;
        
        private float currentSignalStrength = 0f;
        private float currentHeartRate = 60f;
        private bool isAnimating = false;
        private Coroutine animationCoroutine;
        
        private Texture2D ecgTexture;
        private int textureWidth = 480;
        private int textureHeight = 150;
        private float scrollOffset = 0f;
        
        private void Awake()
        {
            InitializeECGTexture();
        }
        
        private void InitializeECGTexture()
        {
            if (waveformImage != null)
            {
                ecgTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
                ecgTexture.filterMode = FilterMode.Point;
                ClearTexture();
                
                Sprite ecgSprite = Sprite.Create(ecgTexture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));
                waveformImage.sprite = ecgSprite;
            }
        }
        
        private void ClearTexture()
        {
            Color[] pixels = new Color[textureWidth * textureHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0, 0, 0, 0);
            }
            ecgTexture.SetPixels(pixels);
            ecgTexture.Apply();
        }
        
        public void Show(string indicatorType, float signalStrength)
        {
            currentSignalStrength = signalStrength;
            currentHeartRate = baseHeartRate + (signalStrength * 40f);
            
            gameObject.SetActive(true);
            
            if (!isAnimating)
            {
                if (animationCoroutine != null)
                    StopCoroutine(animationCoroutine);
                animationCoroutine = StartCoroutine(AnimateHeartRate());
            }
        }
        
        public void Hide()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            isAnimating = false;
            gameObject.SetActive(false);
        }
        
        public void UpdateSignalStrength(float newStrength)
        {
            currentSignalStrength = newStrength;
            currentHeartRate = baseHeartRate + (newStrength * 40f);
        }
        
        private IEnumerator AnimateHeartRate()
        {
            isAnimating = true;
            float time = 0f;
            
            while (isAnimating)
            {
                if (heartRateText != null)
                {
                    heartRateText.text = $"{Mathf.RoundToInt(currentHeartRate)} BPM";
                }
                
                DrawECGWaveform(time);
                
                time += Time.deltaTime * waveSpeed * 50f;
                yield return null;
            }
        }
        
        private void DrawECGWaveform(float time)
        {
            if (ecgTexture == null) return;
            
            float beatInterval = 60f / currentHeartRate;
            int pixelsPerBeat = Mathf.RoundToInt(textureWidth * 0.3f);
            
            ShiftTextureLeft(2);
            
            for (int x = textureWidth - 2; x < textureWidth; x++)
            {
                float normalizedTime = ((time + (x - textureWidth + 2) * 0.01f) % beatInterval) / beatInterval;
                float ecgValue = GetECGValue(normalizedTime);
                
                int centerY = textureHeight / 2;
                int y = centerY + Mathf.RoundToInt(ecgValue * (textureHeight * 0.35f));
                
                y = Mathf.Clamp(y, 0, textureHeight - 1);
                
                for (int thickness = -(int)lineThickness; thickness <= (int)lineThickness; thickness++)
                {
                    int drawY = y + thickness;
                    if (drawY >= 0 && drawY < textureHeight)
                    {
                        ecgTexture.SetPixel(x, drawY, ecgLineColor);
                    }
                }
            }
            
            ecgTexture.Apply();
        }
        
        private void ShiftTextureLeft(int pixels)
        {
            Color[] currentPixels = ecgTexture.GetPixels();
            Color[] newPixels = new Color[textureWidth * textureHeight];
            
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth - pixels; x++)
                {
                    newPixels[y * textureWidth + x] = currentPixels[y * textureWidth + (x + pixels)];
                }
                for (int x = textureWidth - pixels; x < textureWidth; x++)
                {
                    newPixels[y * textureWidth + x] = new Color(0, 0, 0, 0);
                }
            }
            
            ecgTexture.SetPixels(newPixels);
        }
        
        private float GetECGValue(float t)
        {
            if (t < 0.1f)
            {
                return Mathf.Sin(t * Mathf.PI * 10f) * 0.15f;
            }
            else if (t < 0.15f)
            {
                float localT = (t - 0.1f) / 0.05f;
                return Mathf.Lerp(0, -0.3f, localT);
            }
            else if (t < 0.2f)
            {
                float localT = (t - 0.15f) / 0.05f;
                return Mathf.Lerp(-0.3f, 1.0f, localT);
            }
            else if (t < 0.25f)
            {
                float localT = (t - 0.2f) / 0.05f;
                return Mathf.Lerp(1.0f, -0.2f, localT);
            }
            else if (t < 0.3f)
            {
                float localT = (t - 0.25f) / 0.05f;
                return Mathf.Lerp(-0.2f, 0.2f, localT);
            }
            else if (t < 0.4f)
            {
                float localT = (t - 0.3f) / 0.1f;
                return Mathf.Lerp(0.2f, 0, localT);
            }
            else
            {
                return 0f;
            }
        }
    }
}
