
namespace HCup

    open System
    open System.Threading
    open System.Text


    type StringBuilderCache () =


        [<ThreadStatic>]
        [<DefaultValue>]static val mutable private CachedInstance: StringBuilder

        static member Acquire sbSize =
            let sbs =StringBuilderCache.CachedInstance
            if sbs |> isNull |> not
            then
                StringBuilderCache.CachedInstance <- null
                sbs.Clear()
            else
                StringBuilder(3000)

        static member Release sb =
            StringBuilderCache.CachedInstance <- sb

        static member GetStringAndRelease sb =
            let result = sb.ToString()
            StringBuilderCache.Release sb
            result
