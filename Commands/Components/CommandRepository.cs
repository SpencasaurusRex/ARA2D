﻿using System.Collections.Generic;
using MoonSharp.Interpreter;
using Nez;

namespace ARA2D.Commands.Components
{
    public class CommandRepository : Component
    {
        public Dictionary<string, Closure> Commands;
        public Script Script;

        public CommandRepository()
        {
            Commands = new Dictionary<string, Closure>();
            Script = new Script();
        }
    }
}