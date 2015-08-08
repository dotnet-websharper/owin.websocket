﻿namespace WebSharper.Owin.WebSocket

/// Which JSON encoding to use when sending messages through the websocket.
type JsonEncoding =
    /// Verbose encoding that includes extra type information.
    /// This is the same encoding used by WebSharper RPC.
    | Typed = 0
    /// Readable and external API-friendly encoding that drops
    /// some information regarding subtypes.
    /// This is the same encoding used by WebSharper Sitelets
    /// and by the WebSharper.Json.Serialize family of functions.
    | Readable = 1

/// A WebSockets endpoint.
[<Sealed>]
type Endpoint<'S2C, 'C2S> =

    /// Create a websockets endpoint for a given base URL and path.
    /// Call this on the server side and pass it down to the client.
    static member Create
        : url: string
        * route: string
        * ?encoding: JsonEncoding
        -> Endpoint<'S2C, 'C2S>

    /// Create a websockets endpoint for a given full URL.
    /// Call this to connect to an external websocket from the client.
    static member CreateRemote
        : url : string
        * ?encoding: JsonEncoding
        -> Endpoint<'S2C, 'C2S>

/// WebSocket server.
module Server =
    open global.Owin.WebSocket

    /// Messages received by the server.
    type Message<'C2S> =
        | Message of 'C2S
        | Error of exn
        | Close

    /// A client to which you can post messages.
    [<Class>]
    type WebSocketClient<'S2C, 'C2S> =
        member Connection : WebSocketConnection
        member PostAsync : 'S2C -> Async<unit>
        member Post : 'S2C -> unit

    type Agent<'S2C, 'C2S> = WebSocketClient<'S2C, 'C2S> -> Message<'C2S> -> unit

/// WebSocket client.
module Client =

    /// Messages received by the client.
    type Message<'S2C> =
        | Message of 'S2C
        | Error
        | Open
        | Close

    /// A server to which you can post messages.
    [<Class>]
    type WebSocketServer<'S2C, 'C2S> =
        member Connection : WebSharper.JavaScript.WebSocket
        member Post : 'C2S -> unit

    type Agent<'S2C, 'C2S> = WebSocketServer<'S2C, 'C2S> -> Message<'S2C> -> unit

    /// Connect to a websocket server.
    val FromWebSocket : ws: WebSharper.JavaScript.WebSocket -> agent: Agent<'S2C, 'C2S> -> JsonEncoding -> Async<WebSocketServer<'S2C, 'C2S>>

    /// Connect to a websocket server.
    val Connect : endpoint: Endpoint<'S2C, 'C2S> -> agent: Agent<'S2C, 'C2S> -> Async<WebSocketServer<'S2C, 'C2S>>

[<AutoOpen>]
module Extensions =
    open WebSharper.Owin

    type WebSharperOptions<'T when 'T: equality> with
        /// Serve websockets on the given endpoint.
        member WithWebSocketServer : endPoint: Endpoint<'S2C, 'C2S> * agent: Server.Agent<'S2C, 'C2S> -> WebSharperOptions<'T>