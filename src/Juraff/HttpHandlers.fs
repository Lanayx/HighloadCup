module Juraff.HttpHandlers

open System
open System.Text
open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Primitives
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open FSharp.Core.Printf
open Juraff.Common
open System.Text.RegularExpressions
open Newtonsoft.Json.Linq
open Juraff.Tasks



type HttpHandler = HttpContext -> Task<unit>


/// Sets the HTTP response status code.
let setStatusCode (statusCode : int) =
    fun (ctx : HttpContext) ->
        ctx.Response.StatusCode <- statusCode
        Task.FromResult()


let setHttpHeaderSimple (key : string) (value : obj) =
    fun (ctx : HttpContext) ->
        ctx.Response.Headers.[key] <- StringValues(value.ToString())

/// Writes to the body of the HTTP response and sets the HTTP header Content-Length accordingly.
let setBody (bytes : byte array) =
    fun (ctx : HttpContext) ->
        task {
            ctx.Response.Headers.["Content-Length"] <- StringValues(bytes.Length.ToString())
            return! ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length)
        }

/// Writes a string to the body of the HTTP response and sets the HTTP header Content-Length accordingly.
let setBodyAsString (str : string) =
    Encoding.UTF8.GetBytes str
    |> setBody