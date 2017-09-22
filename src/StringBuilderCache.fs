
namespace HCup

    open System
    open System.Threading
    open System.Text

    type SbSize =
    | Small = 0
    | Big = 1

    type StringBuilderCache () =


        [<ThreadStatic>]
        [<DefaultValue>]static val mutable private SmallCachedInstance: StringBuilder

        [<ThreadStatic>]
        [<DefaultValue>]static val mutable private BigCachedInstance: StringBuilder

        static member Acquire sbSize =
            if sbSize = SbSize.Big
            then
                let sbb =StringBuilderCache.BigCachedInstance
                if sbb |> isNull |> not
                then
                    StringBuilderCache.BigCachedInstance <- null
                    sbb.Clear()
                else
                    StringBuilder(3000)
            else
                let sbs =StringBuilderCache.SmallCachedInstance
                if sbs |> isNull |> not
                then
                    StringBuilderCache.SmallCachedInstance <- null
                    sbs.Clear()
                else
                    StringBuilder(300)

        static member Release sb sbSize =
            if sbSize = SbSize.Big
            then StringBuilderCache.BigCachedInstance <- sb
            else StringBuilderCache.SmallCachedInstance <- sb

        static member GetStringAndRelease sb sbSize =
            let result = sb.ToString()
            StringBuilderCache.Release sb sbSize
            result
