//ServerCmdShiftBrick(%client, %x, %y, %z)
//ServerCmdRotateBrick(%client, %dir)
//ServerCmdUndoBrick(%client)
//ServerCmdPlantBrick(%client)
//ServerCmdCancelBrick(%client)
package turnbased_input
{
    function ServerCmdShiftBrick(%client, %x, %y, %z)
    {
        %turnbasedTempBricks = %client.player.turnbasedTempBricks;
        %isSelectingAction = %client.player.isSelectingAction;
        
        if(isobject(%turnbasedTempBricks))
        {
            %count = %turnbasedTempBricks.getCount();

            %player = %client.Player;
            %controlObj = %client.getControlObject ();
            
            if (isObject (%controlObj) )
            {

                if(%count > 0)
                {
                    %x = mFloor (%x);
                    %y = mFloor (%y);
                    %z = mFloor (%z);

                    %forwardVec = %controlObj.getForwardVector ();
                    %forwardX = getWord (%forwardVec, 0);
                    %forwardY = getWord (%forwardVec, 1);
                    %forwardZ = getWord (%forwardVec, 2);

                    if (%forwardZ == -1)
                    {
                        %forwardVec = %controlObj.getUpVector ();
                        %forwardX = getWord (%forwardVec, 0);
                        %forwardY = getWord (%forwardVec, 1);
                    }
                    if (%forwardX > 0)
                    {
                        if (%forwardX > mAbs (%forwardY))
                        {
                            
                        }
                        else if (%forwardY > 0)
                        {
                            %newY = %x;
                            %newX = -1 * %y;
                            %x = %newX;
                            %y = %newY;
                        }
                        else 
                        {
                            %newY = -1 * %x;
                            %newX = 1 * %y;
                            %x = %newX;
                            %y = %newY;
                        }
                    }
                    else if (mAbs (%forwardX) > mAbs (%forwardY))
                    {
                        %x *= -1;
                        %y *= -1;
                    }
                    else if (%forwardY > 0)
                    {
                        %newY = %x;
                        %newX = -1 * %y;
                        %x = %newX;
                        %y = %newY;
                    }
                    else 
                    {
                        %newY = -1 * %x;
                        %newX = 1 * %y;
                        %x = %newX;
                        %y = %newY;
                    }
                    %x *= 0.5;
                    %y *= 0.5;
                    %z *= 0.2;

                    for(%i = 0; %i < %count; %i++)
                    {
                        %tempBrick = %turnbasedTempBricks.getObject(%i);

                        shift(%tempBrick,%x,%y,%z);
                    }
                }
                
            }
        }
        else if(%isSelectingAction)
        {
            //give action input
        }
        else
        {
            return Parent::ServerCmdShiftBrick(%client, %x, %y, %z);
        }
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