//ServerCmdShiftBrick(%client, %x, %y, %z)
//ServerCmdRotateBrick(%client, %dir)
//ServerCmdUndoBrick(%client)
//ServerCmdPlantBrick(%client)
//ServerCmdCancelBrick(%client)

$Turnbased::Server::InputToNumbers[0,0,-1] = 1;
$Turnbased::Server::InputToNumbers[-1,0,0] = 2;
$Turnbased::Server::InputToNumbers[0,0,1] = 3;
$Turnbased::Server::InputToNumbers[0,1,0] = 4;
$Turnbased::Server::InputToNumbers[0,0,-3] = 5;
$Turnbased::Server::InputToNumbers[0,0,3] = "+";
$Turnbased::Server::InputToNumbers[0,-1,0] = 6;
$Turnbased::Server::InputToNumbers[-1] = 7;
$Turnbased::Server::InputToNumbers[1,0,0] = 8;
$Turnbased::Server::InputToNumbers[1] = 9;

$Turnbased::Server::NumbersToInput[1] = "0 0 -1";
$Turnbased::Server::NumbersToInput[2] = "-1 0 0";
$Turnbased::Server::NumbersToInput[3] = "0 0 1";
$Turnbased::Server::NumbersToInput[4] = "0 1 0";
$Turnbased::Server::NumbersToInput[5] = "0 0 -3";
$Turnbased::Server::NumbersToInput["+"] = "0 0 3";
$Turnbased::Server::NumbersToInput[6] = "0 -1 0";
$Turnbased::Server::NumbersToInput[7] = "-1";
$Turnbased::Server::NumbersToInput[8] = "1 0 0";
$Turnbased::Server::NumbersToInput[9] = "1";

package turnbased_input
{
    function ServerCmdCancelBrick(%client)
    {
         if(%client.turnbasedInput)
            %client.TBdoTurnbasedInput(0);
        else
            Parent::ServerCmdCancelBrick(%client);
    }

    function ServerCmdPlantBrick(%client)
    {
        if(%client.turnbasedInput)
            %client.TBdoTurnbasedInput("Enter");
        else
            Parent::ServerCmdPlantBrick(%client);
    }

    function ServerCmdUndoBrick(%client)
    {
         if(%client.turnbasedInput)
            %client.TBdoTurnbasedInput("Undo");
        else
            Parent::ServerCmdUndoBrick(%client);
    }

    function ServerCmdRotateBrick(%client, %dir)
    {
         if(%client.turnbasedInput)
            %client.TBdoTurnbasedInput($Turnbased::Server::InputToNumbers[%dir]);
        else
            Parent::ServerCmdRotateBrick(%client, %dir);
    }

    function ServerCmdShiftBrick(%client, %x, %y, %z)
    {
        if(%client.turnbasedInput)
            %client.TBdoTurnbasedInput($Turnbased::Server::InputToNumbers[%x,%y,%z]);
        else
            Parent::ServerCmdShiftBrick(%client, %x, %y, %z);
    }

    function fxDTSBrick::onActivate(%obj,%player,%client,%pos,%vec)
    {
        //is this part of a piece?
        if(isObject(%piece = %obj.piece))
        {
            %piece.OnPieceInteract(%client);
        }
        Parent::onActivate(%obj,%player,%client,%pos,%vec);
    }
};

deactivatepackage("turnbased_input");
activatepackage("turnbased_input");