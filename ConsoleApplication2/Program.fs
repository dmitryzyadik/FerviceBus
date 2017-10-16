// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open FSharp.Data
open System
open System.Data
open System.Data.Linq
open FSharp.Data.TypeProviders
open Microsoft.FSharp.Linq

let routesId = [6,7,8,9,10,11,12,13,14,15,16,17,18]
type BusRoute = JsonProvider<("http://www.patp2-nv.ru/monitoring/route/routeinfo.php?a=GetRouteVehicles&id=7")>
type dbSchema3 = SqlDataConnection<"Data Source=SQLBACK-WIN12;Initial Catalog=Dima;Integrated Security=SSPI;uid=BusDriver;pwd=klop;">
[<EntryPoint>]
let main argv = 
    
    let id = "6"
    let getRoute id = "http://www.patp2-nv.ru/monitoring/route/routeinfo.php?a=GetRouteVehicles&id=" + id
    let show id = "http://www.patp2-nv.ru/monitoring/route/routeinfo.php?a=Show&id=" + id
    
    let r = 
        BusRoute.AsyncLoad((getRoute id))

    let s = BusRoute.GetSample()

    let db = dbSchema3.GetDataContext()
    
    let newRecord = [ for b in s.Vehicles  -> new dbSchema3.ServiceTypes.BusLocation(RouteNum = id, 
                                                            BusNum = b.PublicNumber, 
                                                            Lan = (float32)b.Lat, 
                                                            Lin = (float32)b.Lon, 
                                                            DataTime = DateTime.Now)
                                                            ]
    
    db.BusLocation.InsertAllOnSubmit(newRecord)
    try
        db.DataContext.SubmitChanges()
    with
    | exn -> printfn "Exception:\n%s" exn.Message
    
    //for  v in s.Vehicles do
        
    
    
    printfn "%A" argv
    0 // return an integer exit code
