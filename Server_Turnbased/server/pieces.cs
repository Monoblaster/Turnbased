// brickData dataBlock\tpiece space\tcolor
// create a new turnbasedpiece, this is like a datablock for the pieces

function server_CreatePiece(%name,%brickData,%descriptions,%values,%abilities)
{
    %p = new ScriptObject("TBP" @ %name)
    {
        class = "TurnbasedPiece";

        pieceName = %name;
        brickData = %brickData;
    };

    //sort out the descriptions field
    %c = 0;
    while((%desc = getField(%descriptions,%c)) !$= "")
    {
        %p.desc[%c] = %desc;
        %c++;
    }
    //sort out the values field
    %c = 0;
    while((%field = getField(%values,%c)) !$= "")
    {
        %p.value[getWord(%field,0)] = getWords(%field,1);
        %c++;
    }
    //sort out the abilities field
    %c = 0;
    while((%field = getField(%abilities,%c)) !$= "")
    {
        %p.ability[%c] = %field;
        %c++;
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

//removes an instantiated brick
function PieceInstance::KillPiece(%piece)
{
    %piece.PieceBricks.deleteAll();

    %piece.delete();
}

//builds an instanited piece
function PieceInstance::BuildPiece(%piece,%vector)
{
    %brickData = %piece.pieceData.brickData;

    %piece.PieceBricks.deleteAll();

    %piece.worldVector = %vector;

    //load bricks from the brickdata save
    %brickList = turnbasedLoad(%brickData,%vector,%rotation);

    //setup bricks
    %count = %bricklest.getCount();
    for(%c = 0; %c < %count; %c++)
    {
        %brick = %brickList.getObject(%c);

        %brick.isPlanted = true;

        %brick.plant();
        %brick.setTrusted(true);

        %brick.piece = %piece;

        %piece.pieceBricks.add(%brick);

        %c++;
    }
}

//adds an instantiated piece to the player's temp bricks
function PieceInstance::TempPiece(%piece,%vector)
{
    %brickData = %piece.pieceData.brickData;

    %piece.worldVector = %vector;

    %client = %piece.client;

    %c = 0;
    //load bricks from the brickdata save
    %brickList = turnbasedLoad(%brickData,%vector,%rotation);
    //setup bricks
    %count = %bricklest.getCount();
    for(%c = 0; %c < %count; %c++)
    {
        %brick = %brickList.getObject(%c);
        //make turnbased temp bricks if it doesn't exist
        if(!isObject(%client.turnbasedTempBricks))
        {
            %client.turnbasedTempBricks = new scriptGroup(){};
        } 
        %client.turnbasedTempBricks.add(%brick);

        %c++;
    }
}

function PieceInstance::OnPieceInteract(%piece,%client)
{
    %c = 0;
    while((%desc = %piece.pieceData.desc[%c]) !$= "")
    {
        %client.ChatMessage(%desc);

        %c++;
    }
    //print ability prompt when the client controls a unit.
    if(%client == %piece.client)
    {
        //did it start or are we already running something?
        if(!%client.TBstartAbilityWithInput("AbilitySelect",%piece))
            return;

        %client.chatmessage("<color:00FF00><font:palatinolinotype:14>Press the numpad key to use the ability:");
        %client.chatmessage("<color:00FF00><font:palatinolinotype:14>| 0 : Deselect");

        %c = 0;
        while((%field = %piece.pieceData.ability[%c]) !$= "")
        {
            %client.chatMessage("<color:00FF00><font:palatinolinotype:14>|" SPC %c + 1 SPC ":" SPC getWord(%field,0));

            %c++;
        }
    }
}