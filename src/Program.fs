open System
open NES

[<EntryPoint>]
[<STAThread>]
let main argv =
    let nes = new Nes()
    nes.start_loop
    0
