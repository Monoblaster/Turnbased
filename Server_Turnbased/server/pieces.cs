// brickData dataBlock\tpiece space\tcolor
// create a new turnbasedpiece, this is like a datablock for the pieces

function server_CreatePiece(%name,%brickData,%descriptors)
{
    %p = new ScriptObject("TBP" @ %name)
    {
        class = "TurnbasedPiece";

        pieceName = %name;
        brickData = %brickData;
    };

    //sort out the %descriptors field
    %c = 0;
    while((%name = getField(%descriptors,%c)) !$= "")
    {
        %p.data[%c / 3] = getFields(%descriptors,%c,%c + 2);
        %c += 3;
    }

    $Turnabsed::Server::PieceDataGroup.add(%p);

    return %p;
}

//instantiates a piece
function TurnbasedPiece::NewPiece(%piece,%client,%vector)
{
    //does this client have a piece group?
    if(!isObject(%client.PieceSet))
    {
        //create a new one
        %client.PieceSet = new SimSet(){};
    }

    //create the piece script object and put it into the piece group
    %p = new ScriptObject()
    {
        class = "PieceInstance";

        pieceData = %piece;
        client = %client;
        worldVector = %vector;
    };

    %p.pieceBricks = new SimSet(){};

    %client.PieceSet.add(%p);

    %p.buildPiece(%vector);

    missioncleanup.add(%p);

    return %p;
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
        %brickColor = getField(%brickData,%c + 2);

        %brick = new fxDTSBrick()
        {
            dataBlock = %dataBlock;
            position = %brickPos;

            rotation = "1 0 0 0";
            
            client = %piece.client;

            colorid = %brickColor;
            isPlanted = true;

            piece = %piece;
        };

        //999999
        %brickGroup = mainBrickGroup.getObject(1);
        %brickGroup.add(%brick);

        %brick.setTrusted(1);
        %brick.plant();
        

        %piece.PieceBricks.add(%brick);
        %c += 3;
    }
}

//removes an instantiated brick
function PieceInstance::KillPiece(%piece)
{
    %piece.PieceBricks.deleteAll();

    %piece.delete();
}

//adds an instantiated piece to the player's temp bricks
function PieceInstance::TempPiece(%piece,%vector)
{
    %brickData = %piece.pieceData.brickData;

    %piece.worldVector = %vector;

    %c = 0;
    while((%dataBlock = getField(%brickData,%c)) !$= "")
    {
        %brickPos = vectorAdd(getField(%brickData,%c + 1),%vector);
        %brickColor = getField(%brickData,%c + 2);

        %brick = new fxDTSBrick()
        {
            dataBlock = %dataBlock;
            position = %brickPos;
            
            client = %piece.client;

            colorid = %brickColor;

            piece = %piece;
        };
        
        //make turnbased temp bricks if it doesn't exist
        %player = %piece.client.player;

        if(!isObject(%player.turnbasedTempBricks))
        {
            %player.turnbasedTempBricks = new scriptGroup(){};
        }
            
        %piece.client.player.turnbasedTempBricks.add(%brick);
        %c += 3;
    }
}

function PieceInstance::OnPieceInteract(%piece,%client)
{
    %c = 0;
    while((%field = %piece.pieceData.data[%c]) !$= "")
    {
        %client.ChatMessage(getField(%field,2));

        %c++;
    }

    if(%client == %piece.client)
    {
        %client.ChatMessage("Abilities go here");
    }
}