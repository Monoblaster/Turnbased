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

    %ability = %client.abilityFunction;
    //call the abilitys end function if it exists
    if(isFunction("GameConnection","TBend" @ %ability))
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
    %piece = %client.abilitySelectPiece;

    %piece.TempPiece(%piece.worldVector);
}

function GameConnection::TBendBasicMove(%client)
{
    %tempBrick = %client.turnbasedTempBricks.getObject(0);
    %position = vectorSub(%tempBrick.position,"0 0 0.3");

    %client.turnbasedTempBricks.deleteAll();

    %client.AbilitySelectPiece.buildPiece(%position);
}

function GameConnection::TBdoBasicMove(%client,%input)
{
    %newInput = $Turnbased::Server::NumbersToInput[%input];

    if(%newInput !$= "")
        %input = %newInput;

    %player = %client.Player;

    %turnbasedTempBricks = %client.turnbasedTempBricks;
    %count = %turnbasedTempBricks.getCount();

    %controlObj = %client.getControlObject ();

    if(getWordCount(%input) == 3)
    {
        //shift
        if (isObject (%controlObj) )
        {

            %x = mFloor (getWord(%input,0));
            %y = mFloor (getWord(%input,1));
            %z = mFloor (getWord(%input,2));

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
    else if(%input $= "Enter")
    {
        //place
        %client.TBendAbilityWithInput();
    }
    else
    {
        //rotate
        %dir = mFloor (%input);
        %player = %client.Player;

        for(%i = 0; %i < %count; %i++) 
        {
            %tempbrick = %turnbasedTempBricks.getObject(%i);

            %brickTrans = %tempBrick.getTransform ();
            %x = getWord (%brickTrans, 0);
            %y = getWord (%brickTrans, 1);
            %z = getWord (%brickTrans, 2);
            %brickAngle = getWord (%brickTrans, 6);
            %vectorDir = getWord (%brickTrans, 5);
            %forwardVec = %player.getForwardVector ();
            %forwardX = getWord (%forwardVec, 0);
            %forwardY = getWord (%forwardVec, 1);
            if (%tempBrick.angleID % 2 == 0)
            {
                %shiftX = 0.25;
                %shiftY = 0.25;
            }
            else 
            {
                %shiftX = -0.25;
                %shiftY = -0.25;
            }
            if (%tempBrick.getDataBlock ().brickSizeX % 2 == %tempBrick.getDataBlock ().brickSizeY % 2)
            {
                %shiftX = 0;
                %shiftY = 0;
            }
            if (%forwardX > 0)
            {
                if (%forwardX > mAbs (%forwardY))
                {
                    
                }
                else if (%forwardY > 0)
                {
                    %x += %shiftX;
                }
                else 
                {
                    %y -= %shiftY;
                    %x -= %shiftX;
                }
            }
            else if (mAbs (%forwardX) > mAbs (%forwardY))
            {
                %x += %shiftX;
                %y -= %shiftY;
            }
            else if (%forwardY > 0)
            {
                %x += %shiftX;
            }
            else 
            {
                %y -= %shiftY;
                %x -= %shiftX;
            }
            if (%vectorDir == -1)
            {
                %brickAngle += $pi;
            }
            %brickAngle /= $piOver2;
            %brickAngle = mFloor (%brickAngle + 0.1);
            %brickAngle += %dir;
            if (%brickAngle > 4)
            {
                %brickAngle -= 4;
            }
            if (%brickAngle <= 0)
            {
                %brickAngle += 4;
            }
            %tempBrick.setTransform (%x SPC %y SPC %z @ " 0 0 1 " @ %brickAngle * $piOver2);
        }      
        
    }
}
