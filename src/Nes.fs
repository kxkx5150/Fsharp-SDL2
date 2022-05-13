module NES

open System
open SDL
open System.Runtime.InteropServices


type Nes() =
    let asUint32 (r, g, b) =
        BitConverter.ToUInt32(ReadOnlySpan [| b; g; r; 255uy |])

    let (width, height) = 256, 240
    let mutable frameRate = 0
    let mutable lastTick = 0

    member this.Loop(renderer, texture, frameBuffer: uint32 array, bufferPtr) =
        async {
            while true do
                frameRate <- frameRate + 1
                if (System.Environment.TickCount - lastTick) >= 1000 then
                    printfn "%d cycles/sec" frameRate
                    frameRate <- 0
                    lastTick <- System.Environment.TickCount

                    SDL_UpdateTexture(texture, IntPtr.Zero, bufferPtr, width * 4) |> ignore
                    SDL_RenderClear(renderer) |> ignore
                    SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero) |> ignore
                    SDL_RenderPresent(renderer) |> ignore
        }

    member this.start_loop =
        SDL_Init(SDL_INIT_VIDEO) |> ignore
        let mutable window, renderer = IntPtr.Zero, IntPtr.Zero
        let windowFlags = SDL_WindowFlags.SDL_WINDOW_SHOWN ||| SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS
        SDL_CreateWindowAndRenderer(width, height, windowFlags, &window, &renderer) |> ignore
        let texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ARGB8888, SDL_TEXTUREACCESS_STREAMING, width, height)
        let frameBuffer = Array.create (width * height) (asUint32 (0uy, 0uy, 205uy))
        let bufferPtr = IntPtr ((Marshal.UnsafeAddrOfPinnedArrayElement (frameBuffer, 0)).ToPointer ())
        let mutable keyEvent = Unchecked.defaultof<SDL_KeyboardEvent>

        Async.Start(this.Loop(renderer, texture, frameBuffer, bufferPtr))

        let mutable brk = false
        while not brk do
            let evt = SDL_PollEvent(&keyEvent)
            if (keyEvent.``type`` = SDL_QUIT) then
                brk <- true
            else if keyEvent.keysym.sym = SDLK_ESCAPE then 
                brk <- true

        SDL_DestroyTexture(texture)
        SDL_DestroyRenderer(renderer)
        SDL_DestroyWindow(window)
        SDL_Quit()
