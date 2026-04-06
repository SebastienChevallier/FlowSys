using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GAME.MissionSystem.Editor
{
    internal static class MissionGraphIMGUIStyleUtility
    {
        private static readonly Dictionary<Color32, Texture2D> TextureCache = new Dictionary<Color32, Texture2D>();
        private static readonly Dictionary<string, GUIStyle> StyleCache = new Dictionary<string, GUIStyle>();

        internal static Texture2D GetOrCreateSolidTexture(Color color)
        {
            Color32 key = (Color32)color;
            if (TextureCache.TryGetValue(key, out Texture2D cachedTexture) && cachedTexture != null)
                return cachedTexture;

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            TextureCache[key] = texture;
            return texture;
        }

        internal static GUIStyle GetButtonStyle(Color backgroundColor, Color textColor)
        {
            string cacheKey = $"button:{(Color32)backgroundColor}:{(Color32)textColor}";
            if (StyleCache.TryGetValue(cacheKey, out GUIStyle cachedStyle) && cachedStyle != null)
                return cachedStyle;

            GUIStyle style = new GUIStyle(GUI.skin.button);
            Texture2D texture = GetOrCreateSolidTexture(backgroundColor);
            style.normal.background = texture;
            style.hover.background = texture;
            style.focused.background = texture;
            style.active.background = texture;
            style.onNormal.background = texture;
            style.onHover.background = texture;
            style.onFocused.background = texture;
            style.onActive.background = texture;
            style.normal.textColor = textColor;
            style.hover.textColor = textColor;
            style.focused.textColor = textColor;
            style.active.textColor = textColor;
            style.onNormal.textColor = textColor;
            style.onHover.textColor = textColor;
            style.onFocused.textColor = textColor;
            style.onActive.textColor = textColor;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            StyleCache[cacheKey] = style;
            return style;
        }

        internal static GUIStyle GetPanelStyle(string styleKind, Color backgroundColor, Color textColor, RectOffset border, RectOffset margin, RectOffset padding)
        {
            string cacheKey = $"{styleKind}:{(Color32)backgroundColor}:{(Color32)textColor}:{border.left},{border.right},{border.top},{border.bottom}:{margin.left},{margin.right},{margin.top},{margin.bottom}:{padding.left},{padding.right},{padding.top},{padding.bottom}";
            if (StyleCache.TryGetValue(cacheKey, out GUIStyle cachedStyle) && cachedStyle != null)
                return cachedStyle;

            GUIStyle style = new GUIStyle();
            Texture2D texture = GetOrCreateSolidTexture(backgroundColor);
            style.normal.background = texture;
            style.hover.background = texture;
            style.focused.background = texture;
            style.active.background = texture;
            style.onNormal.background = texture;
            style.onHover.background = texture;
            style.onFocused.background = texture;
            style.onActive.background = texture;
            style.normal.textColor = textColor;
            style.border = new RectOffset(border.left, border.right, border.top, border.bottom);
            style.margin = new RectOffset(margin.left, margin.right, margin.top, margin.bottom);
            style.padding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom);
            StyleCache[cacheKey] = style;
            return style;
        }

        internal static GUIStyle GetOutlineStyle(int marginLeft, int marginRight, int marginTop, int marginBottom, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom)
        {
            string cacheKey = $"outline:{marginLeft},{marginRight},{marginTop},{marginBottom}:{paddingLeft},{paddingRight},{paddingTop},{paddingBottom}";
            if (StyleCache.TryGetValue(cacheKey, out GUIStyle cachedStyle) && cachedStyle != null)
                return cachedStyle;

            GUIStyle style = new GUIStyle();
            Texture2D texture = GetOrCreateSolidTexture(Color.black);
            style.normal.background = texture;
            style.hover.background = texture;
            style.focused.background = texture;
            style.active.background = texture;
            style.onNormal.background = texture;
            style.onHover.background = texture;
            style.onFocused.background = texture;
            style.onActive.background = texture;
            style.margin = new RectOffset(marginLeft, marginRight, marginTop, marginBottom);
            style.padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
            StyleCache[cacheKey] = style;
            return style;
        }

        internal static GUIStyle GetFoldoutStyle(Color textColor, int fontSize)
        {
            string cacheKey = $"foldout:{(Color32)textColor}:{fontSize}";
            if (StyleCache.TryGetValue(cacheKey, out GUIStyle cachedStyle) && cachedStyle != null)
                return cachedStyle;

            GUIStyle style = new GUIStyle(EditorStyles.foldout);
            style.fontStyle = FontStyle.Bold;
            style.fontSize = fontSize;
            style.normal.textColor = textColor;
            style.onNormal.textColor = textColor;
            style.focused.textColor = textColor;
            style.onFocused.textColor = textColor;
            style.hover.textColor = textColor;
            style.onHover.textColor = textColor;
            StyleCache[cacheKey] = style;
            return style;
        }

        internal static bool DrawSettingsButton(string label, bool isDelete, float width, bool isInteractive = true)
        {
            MissionFlowGraphSettings settings = MissionFlowGraphSettings.Instance;
            Color backgroundColor = isDelete
                ? (settings != null ? settings.deleteButtonColor : new Color(0.8f, 0.2f, 0.2f, 1f))
                : (settings != null ? settings.addButtonColor : new Color(0.2f, 0.7f, 0.2f, 1f));
            Color textColor = isDelete
                ? (settings != null ? settings.deleteButtonTextColor : Color.white)
                : (settings != null ? settings.addButtonTextColor : Color.white);

            Color previousBackgroundColor = GUI.backgroundColor;
            Color previousContentColor = GUI.contentColor;
            bool previousEnabled = GUI.enabled;
            GUIStyle style = GetButtonStyle(backgroundColor, textColor);

            GUI.backgroundColor = backgroundColor;
            GUI.contentColor = textColor;
            GUI.enabled = isInteractive;
            bool clicked = GUILayout.Button(label, style, GUILayout.Width(width));
            GUI.enabled = previousEnabled;
            GUI.backgroundColor = previousBackgroundColor;
            GUI.contentColor = previousContentColor;
            return clicked;
        }
    }
}
