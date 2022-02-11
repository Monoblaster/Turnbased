function fxDTSBrick::givePiece(%brick, %pieceName, %client)
{
    %pieceData = ("TBP" @ %pieceName);

    if(isObject(%pieceData))
        %pieceData.NewPiece(%client, %client.player.position);
}
function fxDTSBrick::pieceInteract(%brick, %client)
{
    %piece = %brick.piece;

    if(!isObject(%piece))
        return;
    
    %piece.OnPieceInteract();
}