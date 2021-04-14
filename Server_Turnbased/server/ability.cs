function GameConnection::TBstartAbilityWithInput(%client,%ability,%piece)
{
    //we don't want turnbaed input to be true if we want to start a new one
    %started = !%client.turnbasedInput;

    if(%started)
    {
        %client.turnbasedInput = true;

        %client.abilitySelectPiece = %piece;
        %client.abilityFunction = %ability;

        //call the abilitys start function if it exists
        if(isFunction("GameConnection","TBstart" @ %ability))
            %client.call("TBstart" @ %ability);
    }

    return %started;
}
function GameConnection::TBendAbilityWithInput(%client)
{
    %client.turnbasedInput = false;

    //call the abilitys end function if it exists
    if(isFunction(%client,"TBend" @ %ability))
        %client.call("TBend" @ %ability);
}

function GameConnection::TBdoTurnbasedInput(%client,%input)
{
   
    %abilityFunction = %client.abilityFunction;

    %client.call("TBdo" @ %abilityFunction,%input);
}

function GameConnection::TBdoAbilitySelect(%client,%input)
{
    if(%input $= "0" || %input >= 1 && %input <= 9)
    {
        %ability = %client.abilitySelectPiece.pieceData.ability[%input - 1];

        if(%ability $= "" && %input != 0)
            return;

        %client.TBendAbilityWithInput();

        if(%ability !$= "")
        {
            //start ability function
            %abilityFunction = getWord(%ability,1);
            %abilityPiece = %client.abilitySelectPiece;

            %client.TBstartAbilityWithInput(%abilityFunction,%abilityPiece);
        }
    }
        
}

function GameConnection::TBstartBasicMove(%client)
{
    
    
}

function GameConnection::TBendBasicMove(%client)
{
    
    
}

function GameConnection::TBdoBasicMove(%client,%input)
{
    %turnbasedTempBricks = %client.

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
