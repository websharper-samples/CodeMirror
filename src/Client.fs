namespace Samples

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Templating
open WebSharper.UI.Client
open WebSharper.CodeMirror

/// A CodeMirror 6 playground: pick a language, edit the snippet, and toggle
/// between the light and dark themes.
[<JavaScript>]
module Client =

    type Template = Templating.Template<"wwwroot/index.html", ClientLoad.FromDocument>

    type Snippet =
        {
            Id: string
            Name: string
            Extension: obj
            Code: string
        }

    let private snippets =
        [|
            {
                Id = "javascript"
                Name = "JavaScript"
                Extension = box (CodeMirror.Javascript())
                Code =
                    """const range = (n) => [...Array(n).keys()];

const squares = range(10)
  .map((x) => x * x)
  .filter((x) => x % 2 === 0);

console.log(squares);"""
            }
            {
                Id = "typescript"
                Name = "TypeScript"
                Extension = box (CodeMirror.Javascript(JavaScriptConfig(Typescript = true)))
                Code =
                    """interface Point {
    x: number;
    y: number;
}

const distance = (a: Point, b: Point): number =>
    Math.hypot(a.x - b.x, a.y - b.y);

console.log(distance({ x: 0, y: 0 }, { x: 3, y: 4 }));"""
            }
            {
                Id = "python"
                Name = "Python"
                Extension = box (CodeMirror.Python())
                Code =
                    """def fib(n):
    a, b = 0, 1
    for _ in range(n):
        a, b = b, a + b
    return a

print([fib(i) for i in range(10)])"""
            }
            {
                Id = "html"
                Name = "HTML"
                Extension = box (CodeMirror.Html())
                Code =
                    """<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>Hello</title>
  </head>
  <body>
    <h1 class="title">Hello, world!</h1>
  </body>
</html>"""
            }
            {
                Id = "css"
                Name = "CSS"
                Extension = box (CodeMirror.Css())
                Code =
                    """.card {
  display: flex;
  padding: 1rem 1.25rem;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.07);
}"""
            }
            {
                Id = "json"
                Name = "JSON"
                Extension = box (CodeMirror.Json())
                Code =
                    """{
  "name": "websharper",
  "version": "10.1.9",
  "keywords": ["fsharp", "web", "spa"],
  "stable": true
}"""
            }
            {
                Id = "sql"
                Name = "SQL"
                Extension = box (CodeMirror.Sql())
                Code =
                    """SELECT c.name, COUNT(o.id) AS orders
FROM customers AS c
JOIN orders AS o ON o.customer_id = c.id
WHERE o.placed_at >= '2026-01-01'
GROUP BY c.name
ORDER BY orders DESC;"""
            }
            {
                Id = "markdown"
                Name = "Markdown"
                Extension = box (CodeMirror.Markdown())
                Code =
                    """# WebSharper

Write **F#** in the browser with *WebSharper*.

- Reactive UIs
- Static typing
- No JavaScript required"""
            }
        |]

    [<SPAEntryPoint>]
    let Main () =
        let language = Var.Create "javascript"
        let dark = Var.Create false

        let host = ref (None: Dom.Element option)
        let editor = ref (None: EditorView option)

        let create () =
            match host.Value with
            | None -> ()
            | Some h ->
                match editor.Value with
                | Some view -> view.Destroy()
                | None -> ()
                h.InnerHTML <- ""

                match snippets |> Array.tryFind (fun s -> s.Id = language.Value) with
                | None -> ()
                | Some snippet ->
                    let extensions = ResizeArray<obj>()
                    extensions.Add(box CodeMirror.BasicSetup)
                    extensions.Add(snippet.Extension)
                    if dark.Value then
                        extensions.Add(box CodeMirror.OneDark)

                    let view =
                        EditorView(
                            EditorViewConfig(
                                Doc = snippet.Code,
                                Parent = h,
                                Extensions = extensions.ToArray()))

                    editor.Value <- Some view

        let languageButton (snippet: Snippet) =
            Doc.Button snippet.Name [
                attr.``class`` "lang-btn"
                Attr.DynamicClassPred "is-active" (language.View |> View.Map (fun l -> l = snippet.Id))
            ] (fun () ->
                language.Value <- snippet.Id
                create ())

        let languages =
            Doc.Concat [
                for snippet in snippets -> languageButton snippet
            ]

        let theme =
            dark.View
            |> View.Map (fun d -> if d then "Light theme" else "Dark theme")

        let editorHost =
            div [
                on.afterRender (fun el ->
                    host.Value <- Some el
                    create ())
            ] []

        Template()
            .Languages(languages)
            .Theme(theme)
            .Editor(editorHost)
            .ToggleTheme(fun _ ->
                dark.Value <- not dark.Value
                create ())
            .Bind()
