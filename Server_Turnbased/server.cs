exec("./server/pieces.cs");

exec("./server/package.cs");

exec("./server/event.cs");

exec("./server/ability.cs");

exec("./server/loading.cs");



function server_TurnbasedInitialize()
{
    if($Turnbased::Server:Initialized)
        return;
    
    $Turnabsed::Server::PieceDataGroup = new ScriptGroup(){};

    new ScriptObject("TurnbasedLoader"){};

    server_CreatePiece(
        "Warrior"
    ,   
        "brick1x1Data\t0 0 0.3\t34" TAB
        "brick1x1fPrintData\t0 0 0.7\t34"
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

    registerOutputEvent("fxDTSBrick", "givePiece", "string 100 100", true);
    registerOutputEvent("fxDTSBrick", "pieceInteract", "", true);
}

server_TurnbasedInitialize();
$Turnbased::Server:Initialized = 1;