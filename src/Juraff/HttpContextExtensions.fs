[<AutoOpenAttribute>]
module Juraff.HttpContextExtensions

open System
open System.IO
open System.Reflection
open System.ComponentModel
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Reflection
open Microsoft.Net.Http.Headers
open Juraff.Common

type HttpContext with

    /// ---------------------------
    /// Dependency management
    /// ---------------------------

    member this.GetService<'T>() =
        this.RequestServices.GetService(typeof<'T>) :?> 'T

    member this.GetLogger<'T>() =
        this.GetService<ILogger<'T>>()

    /// ---------------------------
    /// Common helpers
    /// ---------------------------

    /// ---------------------------
    /// Model binding
    /// ---------------------------

    member this.ReadBodyFromRequest() =
        let body = this.Request.Body
        use reader = new StreamReader(body, true)
        reader.ReadToEndAsync()