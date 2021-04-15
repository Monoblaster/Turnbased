function serverCmdTurnbasedSave(%client,%filename)
{
    talk("egg");
    if($ns[%client.ndSelection,"B",0] $= "")
        return;

    %file = new FileObject(){};
    %file.openForWrite("config/server/TurnbasedSaves/" @ %filename @ ".txt");
    %lineCount = -1;
    %c = 0;
    while($ns[%client.ndSelection,"B",%c] !$= "")
    {
        talk(%brick);
        %line[%lineCount++] = $ns[%client.ndSelection,"D",%c].getName() TAB $ns[%client.ndSelection,"P",%c] TAB $ns[%client.ndSelection,"R",%c] TAB $ns[%client.ndSelection,"CO",%c] TAB $ns[%client.ndSelection,"CF",%c] TAB $ns[%client.ndSelection,"SF",%c] TAB $ns[%client.ndSelection,"PR",%c];
        %c++;
    }
    for(%i = 0; %i <= %lineCount; %i++)
    {
        talk(%line);
        %file.writeLine(%line[%i]);
    }
    %file.close();
    %file.delete();
}
function serverCmdTurnbasedLoad(%client,%filename, %x, %y, %z, %rotation)
{
    talk("egg");
    %vector = vectorAdd(%x SPC %y SPC %z,"0 0 0.2");
    %file = new FileObject(){};
    %success = %file.openForRead("config/server/TurnbasedSaves/" @ %filename @ ".txt");

    if(!%success)
        return;

    talk("ladoing");
    %count = 0;
    %lowestZ = inf;
    //read in bricks
    while(!%file.isEOF())
    {
        %line = %file.readLine();
        %datablock[%c] = getField(%line,0);
        %position[%c] = getField(%line,1);
        %rotation[%c] = getField(%line,2);
        %colorid[%c] = getField(%line,3);
        %colorfx[%c] = getField(%line,4);
        %shapefx[%c] = getField(%line,5);
        %print[%c] = getfield(%line,6);

        %c++;
    }
    //replace z as we want to use lowest brick position
    //create bricks
    while(%c >= 0)
    {
        talk(%datablock[%c].getId() SPC vectorAdd(%position[%c],%vector));
        %brick = new fxDTSBrick()
        {
            dataBlock = %datablock[%c].getId();
            position = vectorAdd(%position[%c],%vector); //add in our place vector
            rotation = %rotation[%c];
            colorid = %colorid[%c];
            colorfx = %colorfx[%c];
            shapefx = %shapefx[%c];
            print = %print[%c];
            isPlanted = true;
        };
        talk(%brick);
        %brick.plant();
        %brick.setTrusted(true);
        missionCleanup.add(%brick);
        %c--;
    }
}