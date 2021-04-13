//ServerCmdShiftBrick(%client, %x, %y, %z)
//ServerCmdRotateBrick(%client, %dir)
//ServerCmdUndoBrick(%client)
//ServerCmdPlantBrick(%client)
//ServerCmdCancelBrick(%client)

$Turnbased::Sever::InputToNumbers[0,0,-1] = 1;
$Turnbased::Sever::InputToNumbers[-1,0,0] = 2;
$Turnbased::Sever::InputToNumbers[0,0,1] = 3;
$Turnbased::Sever::InputToNumbers[0,1,0] = 4;
$Turnbased::Sever::InputToNumbers[0,0,-3] = 5;
$Turnbased::Sever::InputToNumbers[0,0,3] = "+";
$Turnbased::Sever::InputToNumbers[0,-1,0] = 6;
$Turnbased::Sever::InputToNumbers[-1] = 7;
$Turnbased::Sever::InputToNumbers[1,0,0] = 8;
$Turnbased::Sever::InputToNumbers[1] = 9;

package turnbased_input
{
    function ServerCmdCancelBrick(%client)
    {
         if(%client.turnbasedInput)
            %client.doTurnbasedInput(0);
        else
            Parent::ServerCmdCancelBrick(%client);
    }

    function ServerCmdPlantBrick(%client)
    {
        if(%client.turnbasedInput)
            %client.doTurnbasedInput("Enter");
        else
            Parent::ServerCmdPlantBrick(%client);
    }

    function ServerCmdUndoBrick(%client)
    {
         if(%client.turnbasedInput)
            %client.doTurnbasedInput("Undo");
        else
            Parent::ServerCmdUndoBrick(%client);
    }

    function ServerCmdRotateBrick(%client, %dir)
    {
         if(%client.turnbasedInput)
            %client.doTurnbasedInput($Turnbased::Sever::InputToNumbers[%dir]);
        else
            Parent::ServerCmdRotateBrick(%client, %dir);
    }

    function ServerCmdShiftBrick(%client, %x, %y, %z)
    {
        if(%client.turnbasedInput)
            %client.doTurnbasedInput($Turnbased::Sever::InputToNumbers[%x,%y,%z]);
        else
            Parent::ServerCmdShiftBrick(%client, %x, %y, %z);
            // %count = %turnbasedTempBricks.getCount();

            // %player = %client.Player;
            // %controlObj = %client.getControlObject ();
            
            // if (isObject (%controlObj) )
            // {

            //     if(%count > 0)
            //     {
            //         %x = mFloor (%x);
            //         %y = mFloor (%y);
            //         %z = mFloor (%z);

            //         %forwardVec = %controlObj.getForwardVector ();
            //         %forwardX = getWord (%forwardVec, 0);
            //         %forwardY = getWord (%forwardVec, 1);
            //         %forwardZ = getWord (%forwardVec, 2);

            //         if (%forwardZ == -1)
            //         {
            //             %forwardVec = %controlObj.getUpVector ();
            //             %forwardX = getWord (%forwardVec, 0);
            //             %forwardY = getWord (%forwardVec, 1);
            //         }
            //         if (%forwardX > 0)
            //         {
            //             if (%forwardX > mAbs (%forwardY))
            //             {
                            
            //             }
            //             else if (%forwardY > 0)
            //             {
            //                 %newY = %x;
            //                 %newX = -1 * %y;
            //                 %x = %newX;
            //                 %y = %newY;
            //             }
            //             else 
            //             {
            //                 %newY = -1 * %x;
            //                 %newX = 1 * %y;
            //                 %x = %newX;
            //                 %y = %newY;
            //             }
            //         }
            //         else if (mAbs (%forwardX) > mAbs (%forwardY))
            //         {
            //             %x *= -1;
            //             %y *= -1;
            //         }
            //         else if (%forwardY > 0)
            //         {
            //             %newY = %x;
            //             %newX = -1 * %y;
            //             %x = %newX;
            //             %y = %newY;
            //         }
            //         else 
            //         {
            //             %newY = -1 * %x;
            //             %newX = 1 * %y;
            //             %x = %newX;
            //             %y = %newY;
            //         }
            //         %x *= 0.5;
            //         %y *= 0.5;
            //         %z *= 0.2;

            //         for(%i = 0; %i < %count; %i++)
            //         {
            //             %tempBrick = %turnbasedTempBricks.getObject(%i);

            //             shift(%tempBrick,%x,%y,%z);
            //         }
            //     }
                
            // }
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