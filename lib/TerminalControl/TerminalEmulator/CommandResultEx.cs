/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CommandResultEx.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

using Poderosa.View;
using Poderosa.Forms;
using Poderosa.Document;
using Poderosa.Commands;

namespace Poderosa.Terminal {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ICommandResultProcessor : IAdaptable {
        void StartCommand(AbstractTerminal terminal, string command_text, GLine prompt_line);
        void EndCommand(List<GLine> command_result);
    }

    //Extension Point用
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ICommandResultProcessorMenuItem : IAdaptable {
        string Text {
            get;
        }
        bool IsEnabled(AbstractTerminal terminal);
        ICommandResultProcessor CommandBody {
            get;
        }
    }
}
