using UnityEngine;
using UnityEditor;

namespace CubeWarsTools.Browser
{
    /// <summary>
    /// Centralized UI styles for the Script Browser window.
    /// Keeps the UI consistent and clean.
    /// </summary>
    public static class ScriptBrowserStyles
    {
        private static GUIStyle _header;
        public static GUIStyle Header
        {
            get
            {
                if (_header == null)
                {
                    _header = new GUIStyle(EditorStyles.boldLabel);
                    _header.fontSize = 14;
                    _header.margin = new RectOffset(4, 4, 6, 6);
                }
                return _header;
            }
        }

        private static GUIStyle _subHeader;
        public static GUIStyle SubHeader
        {
            get
            {
                if (_subHeader == null)
                {
                    _subHeader = new GUIStyle(EditorStyles.boldLabel);
                    _subHeader.fontSize = 12;
                    _subHeader.margin = new RectOffset(4, 4, 4, 4);
                }
                return _subHeader;
            }
        }

        private static GUIStyle _listItem;
        public static GUIStyle ListItem
        {
            get
            {
                if (_listItem == null)
                {
                    _listItem = new GUIStyle(EditorStyles.label);
                    _listItem.margin = new RectOffset(8, 4, 2, 2);
                }
                return _listItem;
            }
        }

        private static GUIStyle _selectedListItem;
        public static GUIStyle SelectedListItem
        {
            get
            {
                if (_selectedListItem == null)
                {
                    _selectedListItem = new GUIStyle(EditorStyles.whiteLabel);
                    _selectedListItem.margin = new RectOffset(8, 4, 2, 2);
                }
                return _selectedListItem;
            }
        }

        private static GUIStyle _box;
        public static GUIStyle Box
        {
            get
            {
                if (_box == null)
                {
                    _box = new GUIStyle("box");
                    _box.padding = new RectOffset(8, 8, 8, 8);
                    _box.margin = new RectOffset(4, 4, 4, 4);
                }
                return _box;
            }
        }

        public static void Space(float amount = 8f)
        {
            GUILayout.Space(amount);
        }
    }
}