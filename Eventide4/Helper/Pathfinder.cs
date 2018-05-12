﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventide4
{
    public class Pathfinder
    {
        public enum FileType
        {
            xml,
            image,
            xnb,
            custom
        }

        string _namespace;
        string collection;
        List<string> sub;
        string file;
        FileType type;
        string ext;
        string path;
        public string Path { get { return path; } set { path = value; } }

        public Pathfinder()
        {
        }

        public Pathfinder(string _namespace, string collection, List<string> sub, string file, Pathfinder.FileType type)
        {
            this._namespace = _namespace;
            this.collection = collection;
            this.sub = new List<string>(sub); // make a duplicate instead of copying the reference
            this.file = file;
            this.type = type;
            //this.ext = ext;
        }

        void FindPath()
        {
            // TODO: Make sure all variables only include _0-9A-Za-z
            string[] ext;
            switch (type)
            {
                case FileType.xml:
                    ext = new string[] { "xml" };
                    break;
                case FileType.image:
                    ext = new string[] { "xnb", "png", "jpg" };
                    break;
                case FileType.custom:
                    ext = new string[] { this.ext };
                    break;
                default:
                    ext = new string[] { "xnb" };
                    break;
            }
            string path1;
            string path2;
            path1 = System.IO.Path.Combine(_namespace, collection, System.IO.Path.Combine(sub.ToArray()), file);
            foreach (string folder in GlobalServices.ExtensionDirectories)
            {
                for (int i = 0; i < ext.Length; i++)
                {
                    path2 = System.IO.Path.Combine(folder, path1 + "." + ext[i]);
/*#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Searching " + path2);
#endif*/
                    if (File.Exists(path2))
                    {
                        this.ext = ext[i];
                        path = path2;
                        return;
                    }
                }
            }
            for (int i = 0; i < ext.Length; i++)
            {
                path2 = System.IO.Path.Combine(GlobalServices.ContentDirectory, path1 + "." + ext[i]);
/*#if DEBUG
                System.Diagnostics.Debug.WriteLine("Searching " + path2);
#endif*/
                if (File.Exists(path2))
                {
                    this.ext = ext[i];
                    path = path2;
                    return;
                }
            }
            path = null;
        }

        // Example: Pathfinder.Find("Eventide:common/ball", "sprites", Pathfinder.image).Path
        public static Pathfinder Find(string locator, string collection, FileType type, Pathfinder current = null, string ext = null)
        {
            Pathfinder pathfinder = new Pathfinder();
            pathfinder.collection = collection;
            pathfinder.type = type;
            pathfinder.ext = ext;
            //string currentNamespace;
            //List<string> currentPath;
            if (current == null)
            {
                pathfinder._namespace = "core";
                pathfinder.sub = new List<string>();
            }
            else
            {
                pathfinder._namespace = current._namespace;
                pathfinder.sub = new List<string>(current.sub); // make a duplicate instead of copying the reference
            }

            for (int i = 0; i < locator.Length; i++)
            {
                if (locator[i] == '_') continue;
                if (locator[i] == '/') continue;
                if (locator[i] == ':') continue;
                if (locator[i] < '0')
                {
                    // error
                }
                if (locator[i] <= '9') continue;
                if (locator[i] < 'A')
                {
                    // error
                }
                if (locator[i] <= 'Z') continue;
                if (locator[i] < 'a')
                {
                    // error
                }
                if (locator[i] <= 'z') continue;
                // error
            }

            string[] parts = locator.Split(':');
            if (parts.Length == 2)
            {
                pathfinder._namespace = parts[0];
            }
            else if (parts.Length != 1)
            {
                // error
            }
            string[] parts2 = parts[parts.Length - 1].Split('/');
            if (parts2.Length <= 0)
            {
                // error
            }
            pathfinder.file = parts2[parts2.Length - 1];
            if (parts2[0].Equals("")) pathfinder.sub = new List<string>();
            if (parts2.Length > 1)
            {
                for (int i = 0; i < parts2.Length - 1; i++)
                {
                    if (parts2[i].Equals(".."))
                    {
                        if (pathfinder.sub.Count < 1)
                        {
                            // error
                        }
                        pathfinder.sub.RemoveAt(pathfinder.sub.Count - 1);
                    }
                    else if (!parts2[i].Equals(""))
                    {
                        pathfinder.sub.Add(parts2[i]);
                    }
                }
            }
            /*
            for (int i = 0; i < ext.Length; i++)
            {
                if (File.Exists(Path.Combine(GlobalServices.ContentDirectory, )))
            }
            */
            pathfinder.FindPath();
            return pathfinder;
        }
    }
}
