module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Model = { Todos: Todo list; Input: string; Ticker: int }
type Seconds = int

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo
    | GetNextPhotoAfterDelay of int
    | GotPhotoDisplay of int
    | StartTimer

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let init () =
    let model = { Todos = []; Input = ""; Ticker = 0 }
    let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos
    model, cmd


// running a timer that spawns a message
// in the timer - do a countdown or a real-time clock
// partition the timer and aggregate to the full time frame
// have a busy time and an off time



let getNext todosApi model (delayAmount : Seconds) =

    fun () ->
        let nextPage =
            model.Ticker + 1
            //match model.Response with
            //| Some response -> response.Page + 1
            //| None -> 1

        async {
            do! Async.Sleep (delayAmount * 1000)

            //let! next = todosApi.getNextPage nextPage
            return nextPage
        }

let update msg model =
    match msg with
    | GotTodos todos -> { model with Todos = todos }, Cmd.none

    | GotConfiguration photoAlbumConfiguration ->

        let cmd =
            Cmd.OfAsync.either (getNext photoApi model 0) () GotPhotoDisplay RetryGetPage
        { model with PhotoAlbumConfiguration = Some photoAlbumConfiguration }, cmd


    | SetInput value -> { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input

        let cmd = Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

        { model with Input = "" }, cmd
    | AddedTodo todo ->
        {
            model with
                Todos = model.Todos @ [ todo ]
        },
        Cmd.none

    | GotPhotoDisplay nextTime ->
        let cmd =
            Cmd.ofMsg (GetNextPhotoAfterDelay (1))
        { model with Ticker = nextTime }, cmd

    | GetNextPhotoAfterDelay delay ->
        let cmd =
            Cmd.OfAsync.perform (getNext todosApi model delay) () GotPhotoDisplay
        model, cmd

open Feliz

let private todoAction model dispatch =
    Html.div [
        prop.className "flex flex-col sm:flex-row mt-4 gap-4"
        prop.children [
            Html.input [
                prop.className
                    "shadow appearance-none border rounded w-full py-2 px-3 outline-none focus:ring-2 ring-teal-300 text-grey-darker"
                prop.value model.Input
                prop.placeholder "What needs to be done?"
                prop.autoFocus true
                prop.onChange (SetInput >> dispatch)
                prop.onKeyPress (fun ev ->
                    if ev.key = "Enter" then
                        dispatch AddTodo)
            ]
            Html.button [
                prop.className
                    "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed"
                prop.disabled (Todo.isValid model.Input |> not)
                prop.onClick (fun _ -> dispatch AddTodo)
                prop.text "Add"
            ]
        ]
    ]

let private todoList model dispatch =
    Html.div [
        prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
        prop.children [
            Html.ol [
                prop.className "list-decimal ml-6"
                prop.children [
                    for todo in model.Todos do
                        Html.li [ prop.className "my-1"; prop.text todo.Description ]
                ]
            ]

            todoAction model dispatch
        ]
    ]

let view model dispatch =
    Html.section [
        prop.className "h-screen w-screen"
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]

        prop.children [
            Html.a [
                prop.href "https://safe-stack.github.io/"
                prop.className "absolute block ml-12 h-12 w-12 bg-teal-300 hover:cursor-pointer hover:bg-teal-400"
                prop.children [ Html.img [ prop.src "/favicon.png"; prop.alt "Logo" ] ]
            ]

            Html.div [
                prop.className "flex flex-col items-center justify-center h-full"
                prop.children [
                    Html.h1 [
                        prop.className "text-center text-5xl font-bold text-white mb-3 rounded-md p-4"
                        prop.text "BillsTomato"
                    ]
                    todoList model dispatch
                ]
            ]
        ]
    ]