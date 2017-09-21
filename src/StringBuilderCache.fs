
namespace HCup

    open System
    open System.Threading
    open System.Text

    type StringBuilderCache () =


        [<ThreadStatic>]
        [<DefaultValue>]static val mutable private CachedInstance: StringBuilder

        static member Acquire () =
            let sb = StringBuilderCache.CachedInstance
            if sb |> isNull |> not
            then
                StringBuilderCache.CachedInstance <- null;
                sb.Clear()
            else
                StringBuilder(3000)


        static member Release sb =
            StringBuilderCache.CachedInstance <- sb

        static member GetStringAndRelease sb =
            let result = sb.ToString()
            StringBuilderCache.Release(sb)
            result
