function GameConnection::TBstartTurnbasedInput(%client)
{
    %client.turnbasedInput = true;
}

function GameConnection::TBendTurnbasedInput(%client)
{
    %client.turnbasedInput = false;
}

function GameConnection::TBdoTurnbasedInput(%client,%input)
{
    call("TBdoAbilitySelect",%client,%input);
}

function GameConnection::TBstartAbilitySelect(%client,%piece)
{
    %client.abilitySelectPiece = %piece;
    %client.abilityFunction = "TBdoAbilitySelect";
    %client.startTurnbasedInput(%client);
}

function GameConnection::TBendAbilitySelect(%client)
{
    %client.abilitySelectPiece = "";
    %client.abilityFunction = "";
    %client.endTurnbasedInput(%client);
}

function GameConnection::TBdoAbilitySelect(%client,%input)
{
    if(%input $= "0" || %input >= 1 && %input <= 9)
    {
        %ability = %client.abilitySelectPiece.pieceData.ability[%input];

        if(%ability $= "" && %input != 0)
            return;

        %client.TBendAbilitySelect();

        if(%ability $= "")
        {
            talk("starting" SPC %input);
            //start ability function
        }
    }
        
}

