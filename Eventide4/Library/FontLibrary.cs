﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Linq;

// TODO: Currently each Font object retains its own copy of the specified SpriteFont file.
//   This is wasteful if there are multiple font objects using the same spritefont.
//   Ideally, multiple font objects will not use the same spritefont.

namespace Eventide4.Library
{
    public class FontLibrary : Library<string, Font>
    {
        ContentManager contentManager;

        public FontLibrary()
        {
            contentManager = GlobalServices.GlobalContent; // For now, I'm considering all fonts global (permanent) content.
        }

        protected override Font Load(string path)
        {
            Pathfinder pathfinder = Pathfinder.Find(path, "fonts", Pathfinder.FileType.xml);
            if (pathfinder.Path == null)
            {
                // Load fallback asset.
                pathfinder = Pathfinder.Find("system:default", "fonts", Pathfinder.FileType.xml);
            }
            Font font = XmlHelper<Font>.Load(pathfinder.Path);

            Pathfinder.SetCurrentPath(pathfinder);
            Pathfinder pathfinder2 = Pathfinder.Find(font.FontFile, "fonts", Pathfinder.FileType.xnb);
            if (pathfinder2.Path == null)
            {
                // Load fallback asset.
                pathfinder2 = Pathfinder.Find("system:Pixellari_16px", "fonts", Pathfinder.FileType.xnb);
            }
            contentManager.RootDirectory = pathfinder2.ContentPath;
            font.SetFont(contentManager.Load<SpriteFont>(pathfinder2.ContentFile));
            Pathfinder.ClearCurrentPath();

            return font;
        }
    }
}
