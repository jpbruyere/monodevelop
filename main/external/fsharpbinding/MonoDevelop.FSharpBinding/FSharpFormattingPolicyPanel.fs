﻿namespace MonoDevelop.FSharp

open Gtk
open MonoDevelop.Components
open MonoDevelop.Ide.Gui.Dialogs

type FSharpFormattingPolicyPanel() =
    inherit MimeTypePolicyOptionsPanel<FSharpFormattingPolicy>()
    let mutable policy = FSharpFormattingPolicy()
    let mutable panel = new FSharpFormattingPolicyPanelWidget()
    override __.CreatePanelWidget() =
        panel <- new FSharpFormattingPolicyPanelWidget()
        panel.Initialize()
        Control.op_Implicit panel

    override __.LoadFrom(p : FSharpFormattingPolicy) =
        policy <- p.Clone()
        panel.SetFormat(policy)

    override __.GetPolicy() =
        panel.CommitPendingChanges ()
        policy
