namespace Website

open WebSharper

[<JavaScript>]
module Client =

    open WebSharper.JavaScript
    open WebSharper.UI
    open WebSharper.UI.Client
    open WebSharper.UI.Html
    open WebSharper.CodeMirror

    [<Require(typeof<CodeMirror.Resources.Modes.Javascript>)>]
    [<Require(typeof<CodeMirror.Resources.Modes.Xml>)>]
    [<Require(typeof<CodeMirror.Resources.Modes.Htmlmixed>)>]
    [<SPAEntryPoint>]
    let Main() =
        let options =
            CodeMirror.Options(
                Mode = "javascript",
                LineNumbers = true,
                ExtraKeys = New [
                    "Ctrl-Space", box(fun cm ->
                        CodeMirror.ShowHint(cm, Hint.JavaScript()))
                ]
            )
        let cm =
            CodeMirror.FromTextArea(
                JS.Document.GetElementById "editor",
                options)
        Console.Log (cm.CharCoords(CharCoords(1, 1), CoordsMode.Page))
        let dial = cm.OpenDialog(Dialog("bar:<input/>"), fun x -> Console.Log(x))
        let resultContainer = Elt.pre [Attr.Class "cm-s-default"] []

        div [] [
            button [
                on.click (fun _ _ ->
                    CodeMirror.RunMode(cm.GetValue(), "javascript",
                        RunModeOutput(resultContainer.Dom)))
            ] [text "Run mode"]
            button [on.click (fun _ _ -> resultContainer.Clear())] [text "Clear"]
            resultContainer
        ]
        |> Doc.RunAppend JS.Document.Body
