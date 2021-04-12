// create a new turnbasedpiece, this is like a datablock for the pieces
function server_CreatePiece(%name,%brickData,%descriptors)
{
    %p = new ScriptObject()
    {
        class = "TurnbasedPiece";
        name = %name;

        brickData = %brickData;
    };

    //sort out the %descriptors field
    %c = 0;
    while((%name = getField(%descriptors,%c)) !$= "")
    {
        %p.setField(%name, getField(%descriptors,%c + 1));
        %c += 2;
    }

    return %p;
}

//instantiates a piece
function TurnbasedPiece::NewPiece(%piece,%client,%vector)
{
    //does this client have a piece group?
    if(!isObject(%client.PieceGroup))
    {
        //create a new one
        %client.PieceGroup = new ScriptGroup(){};
    }

    //create the piece script object and put it into the piece group
    %p = new ScriptObject()
    {
        class = "PieceInstance";

        pieceData = %piece;
        client = %client;
        worldVector = %vector;
        
    };

    %p.pieceBricks = new ScriptGroup(){};

    %client.PieceGroup.add(%p);

    %p.buildPiece(%vector);
}

//builds an instanited piece
function PieceInstance::BuildPiece(%piece,%vector)
{
    %brickData = %piece.pieceData.brickData;

    %piece.PieceBricks.deleteAll();

    %piece.worldVector = %vector;

    %c = 0;
    while((%dataBlock = getField(%brickData,%c)) !$= "")
    {
        %brickPos = vectorAdd(getField(%brickData,%c + 1),%vector);

        %brick = new fxdtsbrick()
        {
            dataBlock = %dataBlock;
            position = %brickPos;

            isPlanted = true;
        };

        %piece.PieceBricks.add(%brick);
        %c += 2;
    }
}

//removes an instantiated brick
function PieceInstance::KillPiece(%piece)
{
    
}
