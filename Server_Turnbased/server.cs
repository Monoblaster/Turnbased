exec("./server/pieces.cs");

exec("./server/package.cs");

exec("./server/event.cs");



function server_TurnbasedInitialize()
{
    if($Turnbased::Server:Initialized)
        return;
    
    $Turnabsed::Server::PieceDataGroup = new ScriptGroup(){};

    server_CreatePiece("Warrior","brick1x1Data\t0 0 0.3\t34" TAB "brick1x1fPrintData\t0 0 0.7\t34","Health\t8\t<color:00FF00>Warrior : 8" TAB "Attack\t4\t<color:FFFFFF>Attack : 4" TAB "Range\t1\t<color:FFFFFF>Range : 1" TAB "Movement\t4\t<color:FFFFFF>Movement : 4");

    registerOutputEvent("fxDTSBrick", "givePiece", "string 100 100", true);
    registerOutputEvent("fxDTSBrick", "pieceInteract", "", true);
}

server_TurnbasedInitialize();
$Turnbased::Server:Initialized = 1;