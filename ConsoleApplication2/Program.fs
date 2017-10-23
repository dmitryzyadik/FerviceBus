// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open FSharp.Data
open System
open System.Data
open System.Data.Linq
open FSharp.Data.TypeProviders
open Microsoft.FSharp.Linq
open FSharp.Data.JsonExtensions


let routesId:seq<int> =  seq {for i in 6 .. 18 do yield i} 
let routeUrl:string = "http://www.patp2-nv.ru/monitoring/route/routeinfo.php?a=GetRouteVehicles&id="
type dbSchema1 = SqlDataConnection<"Data Source=SQLBACK-WIN12;Initial Catalog=Dima;Integrated Security=SSPI;uid=BusDriver;pwd=klop;">

let createRouteUrls (url:string) (routesId:seq<int>) : (seq<string>)  =    
        routesId |> Seq.map(fun x -> sprintf "%s"  url + x.ToString())
    

let getListRoutesUrl = createRouteUrls  routeUrl routesId    

let loadRoute (url:string) =
    let valueAsync = 
        JsonValue.Load(url)
    valueAsync

let db = dbSchema1.GetDataContext() 
    
        
let loadRoutes=         
            getListRoutesUrl |> Seq.map(fun x -> loadRoute x)
        
let parse jsonRoute =
        match jsonRoute with
        | JsonValue.Record [| info; data |] -> 
            let num = match info with
                      | s ,d -> d?routeId.AsString()       
            
            match data with
            |  s, d-> 
                //let vehicles = d?vehicles
                [
                for v in d -> new dbSchema1.ServiceTypes.BusLocation(RouteNum = "6", 
                                                                            BusNum = v?public_number.AsString(), 
                                                                            Lat = (float32)(v?lat.AsFloat()), 
                                                                            Lon = (float32)(v?lon.AsFloat()), 
                                                                            DataTime = DateTime.Now)
                ]

[<EntryPoint>]
let main argv =  

    loadRoutes |> Seq.iter(fun x  -> db.BusLocation.InsertAllOnSubmit(parse x))
    
    try
        db.DataContext.SubmitChanges()
    with    
    | exn -> printfn "Exception:\n%s" exn.Message
    printfn "%A" argv
    0 // return an integer exit code
