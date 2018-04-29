﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventide4
{
    /*
    public enum RenderProperty
    {
        Boundary
    }
    */

    public interface IRenderComponent
    {
        //void setHost(Entity _host);
        Entity host { get; set; }
        Rectangle Boundary();
        //dynamic Request(RenderProperty property);
        void Render();
    }
}