using System;
using System.Collections.Generic;
using MarkLight;
using MarkLight.ValueConverters;
using UnityEngine;

namespace Marklight.Themes
{
    public class CssThemeLoader
    {
        private static readonly char[] HierarchySplitter = new char[] {' '};

        /// <summary>
        /// Loads Theme CSS and returns Theme or null if failed.
        /// </summary>
        public Theme LoadCss(string css, string cssAssetName) {

            var parser = new CssParser(css);
            List<CssParser.Selectors> selectors;

            try
            {
                selectors = parser.ParseCss();
            }
            catch (CssParseException e)
            {
                Debug.LogErrorFormat("[MarkLight] {0}: Error parsing theme CSS. {1}", cssAssetName, e.Message);
                return null;
            }

            var styleDataList = new List<StyleData>(selectors.Count);
            var styleDataDict = new Dictionary<string, StyleData>();
            var themeProperties = new Dictionary<string, string>();

            var index = 0;
            for (var i = 0; i < selectors.Count; i++)
            {
                var selector = selectors[i];

                // check for theme selector
                if (selector.SelectorList.Count == 1 && selector.SelectorList[0] == "Theme")
                {
                    foreach (var prop in selector.PropertyList)
                    {
                        themeProperties.Add(prop.Name, prop.Value);
                    }
                    continue;
                }

                // get styles
                foreach (var sel in selector.SelectorList)
                {
                    var properties = new List<StyleProperty>();

                    // get properties
                    foreach (var prop in selector.PropertyList)
                    {
                        properties.Add(CreateProperty(sel, prop));
                    }

                    var hierarchy = sel.Split(HierarchySplitter, StringSplitOptions.RemoveEmptyEntries);
                    StyleData prev = null;

                    for (var j = 0; j < hierarchy.Length; j++)
                    {
                        var curr = hierarchy[j];
                        var style = new StyleSelector(curr);
                        var key = sel + ":" + style.LocalSelector;
                        StyleData data;
                        if (!styleDataDict.TryGetValue(key, out data))
                        {
                            data = new StyleData(index++, prev == null ? -1 : prev.Index,
                                                 style.ElementName, style.Id, style.ClassName);

                            styleDataList.Add(data);
                            styleDataDict.Add(key, data);
                        }

                        if (j == hierarchy.Length - 1)
                        {
                            data.Properties.RemoveAll(x => properties.Contains(x));
                            data.Properties.AddRange(properties);
                        }

                        prev = data;
                    }

                }
            }

            if (themeProperties.Count == 0)
            {
                Debug.LogErrorFormat("[MarkLight] {0}: Error parsing theme CSS. Missing Theme selector.", cssAssetName);
                return null;
            }

            var themeName = themeProperties.Get("Name");
            if (themeName == null)
            {
                Debug.LogErrorFormat("[MarkLight] {0}: Error parsing theme CSS. Missing Theme Name.", cssAssetName);
                return null;
            }

            var unitSize = themeProperties.Get("UnitSize");
            var baseDirectory = themeProperties.Get("BaseDirectory");
            var isUnitSizeSet = unitSize != null;
            var isBaseDirectorySet = baseDirectory != null;

            return new Theme(themeName, baseDirectory, ParseUnitSize(unitSize, cssAssetName),
                                    isBaseDirectorySet, isUnitSizeSet, styleDataList.ToArray());
        }

        private static StyleProperty CreateProperty(string selector, CssParser.Property cssProperty) {

            var pathIndex = selector.IndexOf("::", StringComparison.Ordinal);
            if (pathIndex == -1)
                return new StyleProperty(cssProperty.Name, cssProperty.Value);

            var path = selector.Substring(pathIndex + 2);
            var name = cssProperty.Name;
            var state = "";

            var stateIndex = name.IndexOf('-', 0);
            if (stateIndex > 0)
            {
                var stateViewField = name.Substring(stateIndex + 1);
                state = name.Substring(0, stateIndex) + '-';
                name = stateViewField;

                var isSubState = name.StartsWith("-");
                if (isSubState)
                {
                    name = name.Substring(1);
                    state += '-';
                }
            }

            name = state + path + '.' + name;
            return new StyleProperty(name, cssProperty.Value);
        }

        private static Vector3 ParseUnitSize(string unitSize, string cssAssetName) {
            if (string.IsNullOrEmpty(unitSize))
            {
                // use default unit size
                return ViewPresenter.Instance.UnitSize;
            }
            var converter = new Vector3ValueConverter();
            var result = converter.Convert(unitSize);
            if (result.Success)
            {
                return (Vector3) result.ConvertedValue;
            }

            Debug.LogError(string.Format(
                "[MarkLight] {0}: Error parsing theme CSS. Unable to parse UnitSize attribute value \"{1}\".",
                cssAssetName, unitSize));

            return ViewPresenter.Instance.UnitSize;
        }
    }
}