exec("./server/pieces.cs");

exec("./server/maps.cs");

exec("./server/package.cs");

exec("./server/event.cs");

exec("./server/ability.cs");

exec("./server/brickData.cs");


function server_TurnbasedInitialize()
{
    
    $Turnabsed::Server::PieceDataGroup = new ScriptGroup(){};
    $Turnabsed::Server::MapDataGroup = new ScriptGroup(){};

    //pieces
    server_CreatePiece(
        "Warrior"
    ,   
        "Warrior"
    ,
        "<color:00FF00>Warrior : 8" TAB
        "<color:FFFFFF>Attack : 4" TAB
        "<color:FFFFFF>Range : 1" TAB
        "<color:FFFFFF>Movement : 4"
    ,
        "Health 8" TAB
        "Attack 4" TAB
        "Range 1" TAB
        "Movement 4"
    ,
        "Move basicMove" TAB
        "Attack basicAttack"
    );
    //maps
    server_CreateMap(
        "Default"
    ,   
        "Default"
    );

    registerOutputEvent("fxDTSBrick", "givePiece", "string 100 100", true);
}

if(!$Turnbased::Server:Initialized)
{
    server_TurnbasedInitialize();
    $Turnbased::Server:Initialized = 1;
}