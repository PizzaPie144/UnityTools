using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace PizzaPie.Editor.Util
{
    /// <summary>
    /// Contains helper methods for handling Assets
    /// </summary>
    public static class AssetUtil
    {

        /// <summary>
        /// Returns all instances of assets of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folders"></param>
        /// <returns></returns>
        public static List<T> GetAllInstances<T>(string[] folders = null) where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).FullName, folders);

            List<T> instances = new List<T>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                T instance = AssetDatabase.LoadAssetAtPath<T>(path);
                if (instance != null)
                {
                    instances.Add(instance);
                }
            }
            return instances;
        }

        /// <summary>
        /// Return all instances of assets of type T, use only if GetAllInstances doesn't work.
        /// Does not work if type is contained in dll.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List of instances of assets.</returns>
        public static List<T> FindAllAssetsOfType<T>() where T : ScriptableObject
        {
            string typeGUID = "";
            Type TType = typeof(T);

            string typeFullName = TType.FullName;
            var files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

            Regex regex = new Regex(@"\bnamespace\b.*?{", RegexOptions.Singleline);

            string targetScriptFile = "";
            //find the script file of given type
            foreach (var f in files)
            {
                if (f.Replace(".cs", "").EndsWith(TType.Name) || f.Replace(".cs", "").EndsWith(TType.FullName))
                {
                    using (StreamReader sr = new StreamReader(f))
                    {
                        string script = sr.ReadToEnd();
                        var match = regex.Match(script);

                        if ((String.IsNullOrEmpty(TType.Namespace) && !match.Success) ||
                            (match.Success && RemoveSpecialCharacters(ParceNamespaceString(match.Value)).Equals(TType.Namespace)))
                        {
                            targetScriptFile = f;
                            break;
                        }
                    }
                }
            }

            if (targetScriptFile.Equals(""))
            {
                Debug.LogError(TType.Name + ".cs.meta could not be found!");
                return null;
            }

            //find meta file of given script file
            using (StreamReader sr = new StreamReader(targetScriptFile + ".meta"))
            {
                string script = sr.ReadToEnd();
                regex = new Regex(@"\bguid\b.+$", RegexOptions.Multiline);
                Match m = regex.Match(script);
                if (m.Success)
                    typeGUID = regex.Match(script).Value;
                else
                {
                    Debug.LogError("could not find GUID!");
                    return null;
                }

                typeGUID = typeGUID.Replace(" ", "");
                typeGUID = RemoveSpecialCharacters(typeGUID);
                typeGUID = typeGUID.Replace("guid", "");
            }

            var assetFiles = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories);

            List<string> assetPaths = new List<string>();
            List<T> results = new List<T>();

            regex = new Regex(@"\bguid\b.+?,", RegexOptions.Multiline);
            Regex regexPath = new Regex(@"\bAssets\b.+$", RegexOptions.Multiline);

            //find all asset files and compare the GUIDs
            foreach (var assetFile in assetFiles)
            {
                using (StreamReader sr = new StreamReader(assetFile))
                {
                    string a = sr.ReadToEnd();
                    Match match = regex.Match(a);
                    if (match.Success)
                    {
                        string assetTypeGUID = match.Value.Replace(" ", "");
                        assetTypeGUID = assetTypeGUID.Replace("guid", "");
                        assetTypeGUID = RemoveSpecialCharacters(assetTypeGUID);
                        if (assetTypeGUID == typeGUID)
                        {
                            Match m = regexPath.Match(assetFile);
                            if (m.Success)
                            {
                                string relativePath = m.Value;
                                results.Add(AssetDatabase.LoadAssetAtPath<T>(relativePath));
                            }
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Remove all special characters.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if ((str[i] >= '0' && str[i] <= '9')
                    || (str[i] >= 'A' && str[i] <= 'z'
                        || (str[i] == '.' || str[i] == '_')))
                {
                    sb.Append(str[i]);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Parce namespace name out of regex match.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string ParceNamespaceString(string s)
        {
            s = s.Replace("namespace", String.Empty);               //if namespace is present inside the actual name will get removed
            s = s.Remove(s.Length - 1);
            s = s.Replace(" ", String.Empty);
            return s;
        }

        /// <summary>
        /// Create Asset of type T.
        /// </summary>
        /// <typeparam name="T">Derives from ScriptableObject</typeparam>
        /// <param name="path">Path relative to "**/MyProject/Assets/", may start with "Assets/". </param>
        /// <param name="createPath">Create the path folders if not present. </param>
        /// <param name="name">Name of the Asset. </param>
        /// <returns>The new instance.</returns>
        public static T CreateAsset<T>(string path, bool createPath = true, string name = "") where T : ScriptableObject
        {
            if (!path.EndsWith("/"))
                path += '/';

            if (!createPath && !AssetDatabase.IsValidFolder(path.Remove(path.Length - 1)))
                throw new ArgumentException("Invalid path, make sure folder exists or set createPath = true");

            if (createPath)
                CreateFolder(path);

            AssetDatabase.Refresh();

            T instance = ScriptableObject.CreateInstance<T>();

            if (name.Equals(""))
                name = typeof(T).Name;

            int count = 0;

            var files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                string f = file.Replace(path, "");
                if (f.Contains(name) && f.Contains(".asset") && !f.Contains(".meta"))
                    count++;
            }

            if (count != 0)
                name += " " + count.ToString();

            AssetDatabase.CreateAsset(instance, path + name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return instance;
        }

        /// <summary>
        /// Create folder/s based on path if they do not exist.
        /// </summary>
        /// <param name="path">Relative path to "**/MyProject/Assets/", may start with "Assets/".</param>
        /// <code><para>CreateFolder("newFolder/newFolder") </para>
        /// <para>CreateFolder("Assets/newFolder/newFolder") </para></code>
        public static string CreateFolder(string path)
        {
            string[] sep = path.Split('/');
            string p = "Assets/";
            int i = 0;

            if (path.StartsWith("Assets/"))
                i = 1;

            string lastValidPath = "";

            for (; i < sep.Length; i++)
            {
                if (i == sep.Length - 1 && sep[i].Equals(""))
                    continue;

                p = String.Concat(p, sep[i]);
                if (!AssetDatabase.IsValidFolder(p))
                {
                    AssetDatabase.CreateFolder(lastValidPath, sep[i]);
                    lastValidPath = lastValidPath + '/' + sep[i];
                }

                lastValidPath = p;
                p = String.Concat(p, '/');
            }

            return p;
        }
    }
}